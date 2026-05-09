-- ===============================================
-- Razorpay Webhook System
-- Reuses t_sys_payment_transactions for transaction tracking.
-- ===============================================

CREATE TABLE IF NOT EXISTS t_sys_payment_webhook_log (
    c_id bigserial PRIMARY KEY,
    c_event_type varchar(100),
    c_payment_id varchar(100),
    c_order_id varchar(100),
    c_payload jsonb,
    c_signature text,
    c_is_valid boolean NOT NULL DEFAULT false,
    c_error_message text,
    c_processing_status varchar(50) NOT NULL DEFAULT 'received',
    c_created_at timestamp NOT NULL DEFAULT current_timestamp
);

CREATE INDEX IF NOT EXISTS idx_payment_webhook_log_created_at
ON t_sys_payment_webhook_log (c_created_at DESC);

CREATE INDEX IF NOT EXISTS idx_payment_webhook_log_payment_id
ON t_sys_payment_webhook_log (c_payment_id);

CREATE INDEX IF NOT EXISTS idx_payment_webhook_log_order_id
ON t_sys_payment_webhook_log (c_order_id);

CREATE INDEX IF NOT EXISTS idx_payment_webhook_log_event_type
ON t_sys_payment_webhook_log (c_event_type);

CREATE INDEX IF NOT EXISTS idx_payment_webhook_log_is_valid
ON t_sys_payment_webhook_log (c_is_valid);

-- Extend existing canonical transaction table for Razorpay webhook traceability.
ALTER TABLE IF EXISTS t_sys_payment_transactions
ADD COLUMN IF NOT EXISTS c_event_type varchar(100);

ALTER TABLE IF EXISTS t_sys_payment_transactions
ADD COLUMN IF NOT EXISTS c_razorpay_payment_id varchar(100);

ALTER TABLE IF EXISTS t_sys_payment_transactions
ADD COLUMN IF NOT EXISTS c_razorpay_order_id varchar(100);

ALTER TABLE IF EXISTS t_sys_payment_transactions
ADD COLUMN IF NOT EXISTS c_webhook_log_id bigint;

ALTER TABLE IF EXISTS t_sys_payment_transactions
ADD COLUMN IF NOT EXISTS c_updated_at timestamp;

UPDATE t_sys_payment_transactions
SET c_razorpay_payment_id = c_gateway_paymentid
WHERE c_razorpay_payment_id IS NULL
  AND c_gateway_paymentid IS NOT NULL;

UPDATE t_sys_payment_transactions
SET c_razorpay_order_id = c_gateway_orderid
WHERE c_razorpay_order_id IS NULL
  AND c_gateway_orderid IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_payment_transactions_gateway_paymentid
ON t_sys_payment_transactions (c_gateway_paymentid)
WHERE c_gateway_paymentid IS NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_payment_transactions_razorpay_payment_id
ON t_sys_payment_transactions (c_razorpay_payment_id)
WHERE c_razorpay_payment_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_transactions_event_type
ON t_sys_payment_transactions (c_event_type);

CREATE INDEX IF NOT EXISTS idx_payment_transactions_webhook_log_id
ON t_sys_payment_transactions (c_webhook_log_id);
