using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Data;
using System.Text;

namespace CateringEcommerce.Tests.Helpers
{
    /// <summary>
    /// Shared factory and builder helpers for unit tests.
    /// All builder methods return objects with valid defaults
    /// so tests only override what they care about.
    /// </summary>
    public static class TestFixtures
    {
        // ── CreateOrderDto builders ────────────────────────────────────────────

        /// <summary>Returns a valid COD order 48 hours in the future.</summary>
        public static CreateOrderDto BuildValidOrderDto(long cateringId = 1L) => new CreateOrderDto
        {
            CateringId = cateringId,
            EventDate = DateTime.UtcNow.AddHours(48),
            EventTime = "18:00",
            EventType = "Wedding",
            EventLocation = "Test Venue, Mumbai",
            GuestCount = 100,
            DeliveryAddress = "123 Test Street, Mumbai",
            ContactPerson = "Test User",
            ContactPhone = "9876543210",
            ContactEmail = "test@example.com",
            BaseAmount = 50_000m,
            TaxAmount = 9_000m,
            TotalAmount = 59_000m,
            PaymentMethod = "COD",
            EnableSplitPayment = false,
            OrderItems = new List<CreateOrderItemDto>
            {
                BuildPackageItem()
            }
        };

        /// <summary>Returns an order with split payment enabled (40/60).</summary>
        public static CreateOrderDto BuildSplitPaymentOrderDto(long cateringId = 1L)
        {
            var dto = BuildValidOrderDto(cateringId);
            dto.EnableSplitPayment = true;
            dto.PaymentMethod = "Online";
            dto.PreBookingAmount = 23_600m;   // 40% of 59,000
            dto.PostEventAmount = 35_400m;    // 60% of 59,000
            return dto;
        }

        /// <summary>Returns an order with BankTransfer payment and the given proof file.</summary>
        public static CreateOrderDto BuildBankTransferOrderDto(IFormFile proof, long cateringId = 1L)
        {
            var dto = BuildValidOrderDto(cateringId);
            dto.PaymentMethod = "BankTransfer";
            dto.PaymentProof = proof;
            return dto;
        }

        public static CreateOrderItemDto BuildPackageItem(long packageId = 10L) => new CreateOrderItemDto
        {
            ItemType = "Package",
            ItemId = packageId,
            ItemName = "Premium Package",
            Quantity = 1,
            UnitPrice = 500m,
            TotalPrice = 50_000m
        };

        public static CreateOrderItemDto BuildFoodItem(long foodId = 20L) => new CreateOrderItemDto
        {
            ItemType = "FoodItem",
            ItemId = foodId,
            ItemName = "Biryani",
            Quantity = 100,
            UnitPrice = 150m,
            TotalPrice = 15_000m
        };

        // ── OrderDto builder ───────────────────────────────────────────────────

        public static OrderDto BuildOrderDto(long orderId = 1001L, long cateringId = 1L) => new OrderDto
        {
            OrderId = orderId,
            OrderNumber = "ORD-20260320-001",
            CateringId = cateringId,
            CateringName = "Test Caterers",
            EventDate = DateTime.UtcNow.AddDays(2),
            EventTime = "18:00",
            EventLocation = "Test Venue, Mumbai",
            GuestCount = 100,
            TotalAmount = 59_000m,
            PaymentStatus = "Pending",
            OrderStatus = "Pending",
            ContactPerson = "Test User",
        };

        // ── DataTable helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns a DataTable that simulates an active + verified catering row
        /// (used to stub IDatabaseHelper.ExecuteAsync for IsCateringActiveAsync).
        /// </summary>
        public static DataTable ActiveCateringDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("c_isactive", typeof(bool));
            dt.Columns.Add("c_verified_by_admin", typeof(bool));
            dt.Rows.Add(true, true);
            return dt;
        }

        /// <summary>Returns a DataTable simulating an inactive catering row.</summary>
        public static DataTable InactiveCateringDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("c_isactive", typeof(bool));
            dt.Columns.Add("c_verified_by_admin", typeof(bool));
            dt.Rows.Add(false, true);
            return dt;
        }

        /// <summary>Returns an empty DataTable (catering not found).</summary>
        public static DataTable EmptyDataTable() => new DataTable();

        /// <summary>Returns a DataTable with COUNT(*) = 1 (item exists).</summary>
        public static DataTable CountOneDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("", typeof(int));
            dt.Rows.Add(1);
            return dt;
        }

        /// <summary>Returns a DataTable with COUNT(*) = 0 (item not found).</summary>
        public static DataTable CountZeroDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("", typeof(int));
            dt.Rows.Add(0);
            return dt;
        }

        // ── IFormFile helper ───────────────────────────────────────────────────

        /// <summary>Creates a mock IFormFile for a small JPEG (1 KB) with valid JPEG magic bytes.</summary>
        public static IFormFile BuildMockJpegFile(string fileName = "proof.jpg", long sizeBytes = 1024)
        {
            // JPEG magic bytes: FF D8 FF E0, padded to requested size
            var jpegHeader = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
            var content = new byte[Math.Max((int)sizeBytes, jpegHeader.Length)];
            Buffer.BlockCopy(jpegHeader, 0, content, 0, jpegHeader.Length);
            var stream = new MemoryStream(content);
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(sizeBytes);
            mock.Setup(f => f.ContentType).Returns("image/jpeg");
            mock.Setup(f => f.OpenReadStream()).Returns(stream);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }

        /// <summary>Creates a mock IFormFile that exceeds 10 MB.</summary>
        public static IFormFile BuildOversizedFile() =>
            BuildMockJpegFile("big.jpg", 11 * 1024 * 1024);
    }
}
