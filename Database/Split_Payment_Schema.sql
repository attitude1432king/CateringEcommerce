-- ===============================================
-- Split Payment Workflow Schema
-- Advance Booking with Escrow Management
-- ===============================================

-- ===============================================
-- 1. Payment Transactions Table (Enhanced)
-- ===============================================
-- Tracks all payment transactions for orders
CREATE TABLE IF NOT EXISTS t_sys_payment_transactions (
    c_transactionid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_userid BIGINT NOT NULL,
    c_cateringownerid BIGINT NOT NULL,

    -- Payment Details
    c_transactiontype VARCHAR(50) NOT NULL, -- ADVANCE, FINAL, REFUND, COMMISSION
    c_amount DECIMAL(18,2) NOT NULL,
    c_paymentmethod VARCHAR(50), -- UPI, CARD, NETBANKING, CASH, EMI
    c_paymentgateway VARCHAR(50), -- RAZORPAY, PAYTM, PHONEPE

    -- Gateway Details
    c_gateway_transactionid VARCHAR(200),
    c_gateway_orderid VARCHAR(200),
    c_gateway_paymentid VARCHAR(200),
    c_gateway_signature VARCHAR(500),

    -- Payment Status
    c_paymentstatus VARCHAR(50) NOT NULL, -- PENDING, SUCCESS, FAILED, REFUNDED
    c_statusreason VARCHAR(500),

    -- EMI Details (if applicable)
    c_is_emi BOOLEAN DEFAULT FALSE,
    c_emi_tenure INTEGER, -- Number of months
    c_emi_bank VARCHAR(100),
    c_emi_rate DECIMAL(5,2),
    c_emi_amount DECIMAL(18,2),

    -- Timestamps
    c_initiateddate TIMESTAMP DEFAULT NOW(),
    c_completeddate TIMESTAMP,
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    -- Metadata
    c_metadata TEXT, -- JSON for additional info
    c_ipaddress VARCHAR(50),

    CONSTRAINT fk_payment_txn_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_payment_txn_user FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT fk_payment_txn_owner FOREIGN KEY (c_cateringownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_payment_txn_order ON t_sys_payment_transactions(c_orderid);
CREATE INDEX IF NOT EXISTS ix_payment_txn_status ON t_sys_payment_transactions(c_paymentstatus);
CREATE INDEX IF NOT EXISTS ix_payment_txn_gateway ON t_sys_payment_transactions(c_gateway_transactionid);

-- ===============================================
-- 2. Order Payment Summary Table
-- ===============================================
-- Tracks overall payment status for each order
CREATE TABLE IF NOT EXISTS t_sys_order_payment_summary (
    c_paymentsummaryid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL UNIQUE,

    -- Total Amount Breakdown
    c_totalamount DECIMAL(18,2) NOT NULL,
    c_advancepercentage DECIMAL(5,2) DEFAULT 30.00, -- 30-40%
    c_advanceamount DECIMAL(18,2) NOT NULL,
    c_finalamount DECIMAL(18,2) NOT NULL,

    -- Payment Status
    c_advancepaid BOOLEAN DEFAULT FALSE,
    c_advancepaiddate TIMESTAMP,
    c_finalpaid BOOLEAN DEFAULT FALSE,
    c_finalpaiddate TIMESTAMP,
    c_paymentcompleted BOOLEAN DEFAULT FALSE,

    -- Escrow Management
    c_escrowstatus VARCHAR(50), -- HELD, RELEASED_TO_VENDOR, REFUNDED
    c_escrowamount DECIMAL(18,2),
    c_escrowreleaseddate TIMESTAMP,

    -- Commission
    c_commissionrate DECIMAL(5,2),
    c_commissionamount DECIMAL(18,2),
    c_commissionpaid BOOLEAN DEFAULT FALSE,

    -- Vendor Payout
    c_vendorpayoutstatus VARCHAR(50), -- PENDING, ADVANCE_RELEASED, FINAL_RELEASED, COMPLETED
    c_vendoradvancereleased BOOLEAN DEFAULT FALSE,
    c_vendoradvanceamount DECIMAL(18,2),
    c_vendoradvancereleaseddate TIMESTAMP,
    c_vendorfinalpayout DECIMAL(18,2),
    c_vendorfinalpayoutdate TIMESTAMP,

    -- Metadata
    c_paymentmode VARCHAR(50), -- SPLIT, FULL_ADVANCE, FULL_CASH
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_payment_summary_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid)
);

CREATE INDEX IF NOT EXISTS ix_payment_summary_order ON t_sys_order_payment_summary(c_orderid);
CREATE INDEX IF NOT EXISTS ix_payment_summary_escrow ON t_sys_order_payment_summary(c_escrowstatus);

-- ===============================================
-- 3. Escrow Ledger Table
-- ===============================================
-- Tracks all escrow transactions
CREATE TABLE IF NOT EXISTS t_sys_escrow_ledger (
    c_escrowid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_transactionid BIGINT,

    -- Transaction Details
    c_transactiontype VARCHAR(50) NOT NULL, -- CREDIT, DEBIT, HOLD, RELEASE
    c_amount DECIMAL(18,2) NOT NULL,
    c_balance DECIMAL(18,2), -- Running balance

    -- Related Entities
    c_fromentity VARCHAR(50), -- CUSTOMER, ADMIN, VENDOR
    c_toentity VARCHAR(50),

    -- Status
    c_status VARCHAR(50) NOT NULL, -- PENDING, COMPLETED, CANCELLED
    c_statusreason VARCHAR(500),

    -- Approval
    c_requiresapproval BOOLEAN DEFAULT FALSE,
    c_approvedby BIGINT,
    c_approveddate TIMESTAMP,

    -- Metadata
    c_description VARCHAR(500),
    c_createddate TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_escrow_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_escrow_transaction FOREIGN KEY (c_transactionid) REFERENCES t_sys_payment_transactions(c_transactionid)
);

CREATE INDEX IF NOT EXISTS ix_escrow_order ON t_sys_escrow_ledger(c_orderid);
CREATE INDEX IF NOT EXISTS ix_escrow_status ON t_sys_escrow_ledger(c_status);

-- ===============================================
-- 4. Vendor Payout Requests Table
-- ===============================================
CREATE TABLE IF NOT EXISTS t_sys_vendor_payout_requests (
    c_payoutrequestid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_orderid BIGINT NOT NULL,
    c_cateringownerid BIGINT NOT NULL,

    -- Request Details
    c_requesttype VARCHAR(50) NOT NULL, -- ADVANCE, FINAL
    c_requestamount DECIMAL(18,2) NOT NULL,
    c_requeststatus VARCHAR(50) NOT NULL, -- PENDING, APPROVED, REJECTED, PROCESSED

    -- Bank Details
    c_bankname VARCHAR(200),
    c_accountnumber VARCHAR(50),
    c_ifsccode VARCHAR(20),
    c_accountholdername VARCHAR(200),

    -- Processing
    c_processedby BIGINT,
    c_processeddate TIMESTAMP,
    c_transactionreference VARCHAR(200),
    c_statusreason VARCHAR(500),

    -- Timestamps
    c_requesteddate TIMESTAMP DEFAULT NOW(),
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP,

    CONSTRAINT fk_payout_order FOREIGN KEY (c_orderid) REFERENCES t_sys_orders(c_orderid),
    CONSTRAINT fk_payout_owner FOREIGN KEY (c_cateringownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);

CREATE INDEX IF NOT EXISTS ix_payout_order ON t_sys_vendor_payout_requests(c_orderid);
CREATE INDEX IF NOT EXISTS ix_payout_owner ON t_sys_vendor_payout_requests(c_cateringownerid);
CREATE INDEX IF NOT EXISTS ix_payout_status ON t_sys_vendor_payout_requests(c_requeststatus);

-- ===============================================
-- 5. EMI Plans Table
-- ===============================================
CREATE TABLE IF NOT EXISTS t_sys_emi_plans (
    c_emiplanid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_bankname VARCHAR(100) NOT NULL,
    c_bankcode VARCHAR(50),

    -- Plan Details
    c_minordervalue DECIMAL(18,2) NOT NULL,
    c_maxordervalue DECIMAL(18,2),
    c_tenure INTEGER NOT NULL, -- Months: 3, 6, 9, 12, etc.
    c_interestrate DECIMAL(5,2), -- Annual interest rate
    c_processingfee DECIMAL(18,2),

    -- Configuration
    c_isactive BOOLEAN DEFAULT TRUE,
    c_displayorder INTEGER DEFAULT 0,
    c_termsandconditions TEXT,

    -- Metadata
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP
);

-- ===============================================
-- 6. Payment Gateway Configuration Table
-- ===============================================
CREATE TABLE IF NOT EXISTS t_sys_payment_gateway_config (
    c_configid BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    c_gatewayname VARCHAR(50) NOT NULL UNIQUE,

    -- API Configuration
    c_apikey VARCHAR(500),
    c_apisecret VARCHAR(500),
    c_merchantid VARCHAR(200),
    c_webhookurl VARCHAR(500),
    c_redirecturl VARCHAR(500),

    -- Settings
    c_isenabled BOOLEAN DEFAULT TRUE,
    c_istest BOOLEAN DEFAULT FALSE,
    c_priority INTEGER DEFAULT 0,

    -- Supported Features
    c_supports_upi BOOLEAN DEFAULT TRUE,
    c_supports_card BOOLEAN DEFAULT TRUE,
    c_supports_netbanking BOOLEAN DEFAULT TRUE,
    c_supports_emi BOOLEAN DEFAULT TRUE,

    -- Metadata
    c_createddate TIMESTAMP DEFAULT NOW(),
    c_modifieddate TIMESTAMP
);

-- ===============================================
-- Insert Default EMI Plans
-- ===============================================
INSERT INTO t_sys_emi_plans (c_bankname, c_bankcode, c_minordervalue, c_tenure, c_interestrate, c_processingfee, c_isactive)
VALUES
    ('HDFC Bank', 'HDFC', 10000, 3, 12.00, 199, TRUE),
    ('HDFC Bank', 'HDFC', 10000, 6, 13.00, 299, TRUE),
    ('HDFC Bank', 'HDFC', 15000, 9, 14.00, 399, TRUE),
    ('HDFC Bank', 'HDFC', 20000, 12, 15.00, 499, TRUE),
    ('ICICI Bank', 'ICICI', 10000, 3, 11.50, 199, TRUE),
    ('ICICI Bank', 'ICICI', 10000, 6, 12.50, 299, TRUE),
    ('ICICI Bank', 'ICICI', 15000, 9, 13.50, 399, TRUE),
    ('ICICI Bank', 'ICICI', 20000, 12, 14.50, 499, TRUE),
    ('SBI Bank', 'SBI', 10000, 3, 11.00, 149, TRUE),
    ('SBI Bank', 'SBI', 10000, 6, 12.00, 249, TRUE),
    ('SBI Bank', 'SBI', 15000, 9, 13.00, 349, TRUE),
    ('SBI Bank', 'SBI', 20000, 12, 14.00, 449, TRUE),
    ('Axis Bank', 'AXIS', 10000, 3, 12.50, 199, TRUE),
    ('Axis Bank', 'AXIS', 10000, 6, 13.50, 299, TRUE),
    ('Axis Bank', 'AXIS', 15000, 9, 14.50, 399, TRUE),
    ('Axis Bank', 'AXIS', 20000, 12, 15.50, 499, TRUE)
ON CONFLICT DO NOTHING;

-- ===============================================
-- Insert Default Payment Gateway Config
-- ===============================================
INSERT INTO t_sys_payment_gateway_config (c_gatewayname, c_isenabled, c_supports_upi, c_supports_card, c_supports_netbanking, c_supports_emi)
VALUES
    ('RAZORPAY', TRUE, TRUE, TRUE, TRUE, TRUE),
    ('PAYTM', TRUE, TRUE, TRUE, TRUE, FALSE),
    ('PHONEPE', TRUE, TRUE, TRUE, TRUE, FALSE)
ON CONFLICT (c_gatewayname) DO NOTHING;

