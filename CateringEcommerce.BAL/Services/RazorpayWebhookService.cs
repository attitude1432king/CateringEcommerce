using System.Text.Json;
using CateringEcommerce.BAL.Base.User;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Payment;
using CateringEcommerce.Domain.Models.Payment;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.BAL.Services
{
    public class RazorpayWebhookService : IRazorpayWebhookService
    {
        private readonly IRazorpaySignatureVerifier _signatureVerifier;
        private readonly IRazorpayWebhookRepository _webhookRepository;
        private readonly PaymentStageService _paymentStageService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RazorpayWebhookService> _logger;

        public RazorpayWebhookService(
            IRazorpaySignatureVerifier signatureVerifier,
            IRazorpayWebhookRepository webhookRepository,
            PaymentStageService paymentStageService,
            IOrderRepository orderRepository,
            ILogger<RazorpayWebhookService> logger)
        {
            _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
            _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
            _paymentStageService = paymentStageService ?? throw new ArgumentNullException(nameof(paymentStageService));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RazorpayWebhookProcessingResult> ProcessAsync(RazorpayWebhookRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RawBody))
            {
                var logId = await LogAsync(request, null, false, "Empty payload", "invalid");
                return RazorpayWebhookProcessingResult.Invalid(400, "Empty payload.", logId);
            }

            if (string.IsNullOrWhiteSpace(request.Signature))
            {
                var logId = await LogAsync(request, null, false, "Missing X-Razorpay-Signature header", "invalid");
                return RazorpayWebhookProcessingResult.Invalid(400, "Missing signature.", logId);
            }

            if (!_signatureVerifier.VerifyWebhookSignature(request.RawBody, request.Signature))
            {
                var logId = await LogAsync(request, null, false, "Invalid Razorpay webhook signature", "invalid");
                _logger.LogWarning("Invalid Razorpay webhook signature. WebhookLogId: {WebhookLogId}", logId);
                return RazorpayWebhookProcessingResult.Invalid(401, "Invalid signature.", logId);
            }

            RazorpayWebhookPaymentData? paymentData;
            try
            {
                paymentData = ExtractPaymentData(request.RawBody);
            }
            catch (Exception ex)
            {
                var logId = await LogAsync(request, null, true, ex.Message, "failed");
                return RazorpayWebhookProcessingResult.Invalid(400, "Invalid webhook payload.", logId);
            }

            var webhookLogId = await LogAsync(request, paymentData, true, null, "received");

            try
            {
                var result = await ProcessEventAsync(request, paymentData, webhookLogId);
                await _webhookRepository.UpdateWebhookLogAsync(webhookLogId, result.Status);
                result.WebhookLogId = webhookLogId;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Razorpay webhook processing failed. Event: {EventType}, PaymentId: {PaymentId}", paymentData.EventType, paymentData.PaymentId);
                await _webhookRepository.UpdateWebhookLogAsync(webhookLogId, "failed", ex.Message);
                return RazorpayWebhookProcessingResult.Success("failed", "Webhook logged for manual reconciliation.", webhookLogId);
            }
        }

        private async Task<RazorpayWebhookProcessingResult> ProcessEventAsync(
            RazorpayWebhookRequest request,
            RazorpayWebhookPaymentData paymentData,
            long webhookLogId)
        {
            if (string.IsNullOrWhiteSpace(paymentData.PaymentId))
            {
                return RazorpayWebhookProcessingResult.Success("skipped", "Webhook did not include a payment ID.", webhookLogId);
            }

            if (paymentData.EventType == "payment.captured" && await _webhookRepository.IsPaymentSuccessfulAsync(paymentData.PaymentId))
            {
                return RazorpayWebhookProcessingResult.Success("already_processed", "Payment was already processed.", webhookLogId);
            }

            if (!paymentData.ApplicationOrderId.HasValue || paymentData.ApplicationOrderId.Value <= 0)
            {
                return RazorpayWebhookProcessingResult.Success("skipped", "Webhook did not include application order metadata.", webhookLogId);
            }

            switch (paymentData.EventType)
            {
                case "payment.captured":
                    await UpsertTransactionAsync(request, paymentData, webhookLogId, "SUCCESS");
                    await MarkPaymentStageSuccessAsync(paymentData);
                    return RazorpayWebhookProcessingResult.Success("processed", "Payment captured webhook processed.", webhookLogId);

                case "payment.failed":
                    await UpsertTransactionAsync(request, paymentData, webhookLogId, "FAILED");
                    return RazorpayWebhookProcessingResult.Success("processed", "Payment failed webhook recorded.", webhookLogId);

                case "payment.authorized":
                    await UpsertTransactionAsync(request, paymentData, webhookLogId, "AUTHORIZED");
                    return RazorpayWebhookProcessingResult.Success("processed", "Payment authorized webhook recorded.", webhookLogId);

                case "order.paid":
                    await UpsertTransactionAsync(request, paymentData, webhookLogId, "SUCCESS");
                    await MarkPaymentStageSuccessAsync(paymentData);
                    return RazorpayWebhookProcessingResult.Success("processed", "Order paid webhook synced.", webhookLogId);

                default:
                    return RazorpayWebhookProcessingResult.Success("ignored", $"Unsupported Razorpay event: {paymentData.EventType}", webhookLogId);
            }
        }

        private async Task UpsertTransactionAsync(
            RazorpayWebhookRequest request,
            RazorpayWebhookPaymentData paymentData,
            long webhookLogId,
            string status)
        {
            await _webhookRepository.UpsertPaymentTransactionAsync(new RazorpayPaymentTransactionUpsert
            {
                PaymentId = paymentData.PaymentId!,
                RazorpayOrderId = paymentData.RazorpayOrderId,
                OrderId = paymentData.ApplicationOrderId!.Value,
                StageType = paymentData.StageType,
                Amount = paymentData.Amount,
                Currency = paymentData.Currency,
                Status = status,
                EventType = paymentData.EventType,
                PaymentMethod = paymentData.PaymentMethod,
                Signature = request.Signature,
                WebhookLogId = webhookLogId,
                Payload = request.RawBody
            });
        }

        private async Task MarkPaymentStageSuccessAsync(RazorpayWebhookPaymentData paymentData)
        {
            if (string.IsNullOrWhiteSpace(paymentData.StageType))
            {
                throw new InvalidOperationException("Webhook payment notes are missing stage_type.");
            }

            var alreadyProcessed = await _paymentStageService.IsPaymentAlreadyProcessedAsync(paymentData.PaymentId!);
            if (alreadyProcessed)
            {
                return;
            }

            var processPaymentDto = new ProcessPaymentStageDto
            {
                OrderId = paymentData.ApplicationOrderId!.Value,
                StageType = paymentData.StageType,
                PaymentMethod = paymentData.PaymentMethod ?? "Online",
                PaymentGateway = "Razorpay",
                RazorpayOrderId = paymentData.RazorpayOrderId,
                RazorpayPaymentId = paymentData.PaymentId,
                TransactionId = paymentData.PaymentId
            };

            await _paymentStageService.ProcessPaymentStageAsync(processPaymentDto);

            if (paymentData.StageType == "PreBooking" || paymentData.StageType == "Full")
            {
                await _orderRepository.UpdateOrderStatusAsync(
                    paymentData.ApplicationOrderId.Value,
                    "Confirmed",
                    $"Order confirmed after successful {paymentData.StageType} payment via Razorpay webhook (Payment ID: {paymentData.PaymentId})");
            }
        }

        private async Task<long> LogAsync(
            RazorpayWebhookRequest request,
            RazorpayWebhookPaymentData? paymentData,
            bool isValid,
            string? errorMessage,
            string processingStatus)
        {
            return await _webhookRepository.CreateWebhookLogAsync(new RazorpayWebhookLogEntry
            {
                EventType = paymentData?.EventType,
                PaymentId = paymentData?.PaymentId,
                OrderId = paymentData?.RazorpayOrderId ?? paymentData?.ApplicationOrderId?.ToString(),
                Payload = string.IsNullOrWhiteSpace(request.RawBody) ? "{}" : request.RawBody,
                Signature = request.Signature,
                IsValid = isValid,
                ErrorMessage = errorMessage,
                ProcessingStatus = processingStatus
            });
        }

        private static RazorpayWebhookPaymentData ExtractPaymentData(string rawBody)
        {
            using var document = JsonDocument.Parse(rawBody);
            var root = document.RootElement;
            var eventType = GetString(root, "event");

            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new InvalidOperationException("Webhook payload is missing event.");
            }

            var paymentEntity = TryGetPaymentEntity(root);

            if (paymentEntity.ValueKind == JsonValueKind.Undefined)
            {
                return new RazorpayWebhookPaymentData { EventType = eventType };
            }

            var notes = TryGetProperty(paymentEntity, "notes");
            var orderIdFromNotes = GetString(notes, "order_id");

            return new RazorpayWebhookPaymentData
            {
                EventType = eventType,
                PaymentId = GetString(paymentEntity, "id"),
                RazorpayOrderId = GetString(paymentEntity, "order_id"),
                ApplicationOrderId = long.TryParse(orderIdFromNotes, out var appOrderId) ? appOrderId : null,
                StageType = GetString(notes, "stage_type"),
                Amount = PaiseToRupees(GetLong(paymentEntity, "amount")),
                Currency = GetString(paymentEntity, "currency") ?? "INR",
                PaymentStatus = GetString(paymentEntity, "status"),
                PaymentMethod = GetString(paymentEntity, "method")
            };
        }

        private static JsonElement TryGetPaymentEntity(JsonElement root)
        {
            var payment = TryGetProperty(TryGetProperty(root, "payload"), "payment");
            var entity = TryGetProperty(payment, "entity");
            return entity.ValueKind == JsonValueKind.Undefined ? payment : entity;
        }

        private static JsonElement TryGetProperty(JsonElement element, string propertyName)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value))
            {
                return value;
            }

            return default;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            var value = TryGetProperty(element, propertyName);
            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        private static long GetLong(JsonElement element, string propertyName)
        {
            var value = TryGetProperty(element, propertyName);
            return value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number) ? number : 0;
        }

        private static decimal PaiseToRupees(long amountInPaise)
        {
            return amountInPaise / 100m;
        }
    }
}
