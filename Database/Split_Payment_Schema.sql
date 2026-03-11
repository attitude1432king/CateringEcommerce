-- ===============================================
-- Split Payment Workflow Schema
-- Advance Booking with Escrow Management
-- ===============================================

USE CateringDB;
GO

-- ===============================================
-- 1. Payment Transactions Table (Enhanced)
-- ===============================================
-- Tracks all payment transactions for orders
CREATE TABLE t_sys_payment_transactions (
    c_transactionid BIGINT PRIMARY KEY IDENTITY(1,1),
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
    c_is_emi BIT DEFAULT 0,
    c_emi_tenure INT, -- Number of months
    c_emi_bank VARCHAR(100),
    c_emi_rate DECIMAL(5,2),
    c_emi_amount DECIMAL(18,2),

    -- Timestamps
    c_initiateddate DATETIME DEFAULT GETDATE(),
    c_completeddate DATETIME,
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME,

    -- Metadata
    c_metadata NVARCHAR(MAX), -- JSON for additional info
    c_ipaddress VARCHAR(50),

    CONSTRAINT FK_PaymentTxn_Order FOREIGN KEY (c_orderid) REFERENCES t_sys_order(c_orderid),
    CONSTRAINT FK_PaymentTxn_User FOREIGN KEY (c_userid) REFERENCES t_sys_user(c_userid),
    CONSTRAINT FK_PaymentTxn_Owner FOREIGN KEY (c_cateringownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);
GO

CREATE INDEX IX_PaymentTxn_Order ON t_sys_payment_transactions(c_orderid);
CREATE INDEX IX_PaymentTxn_Status ON t_sys_payment_transactions(c_paymentstatus);
CREATE INDEX IX_PaymentTxn_Gateway ON t_sys_payment_transactions(c_gateway_transactionid);
GO

-- ===============================================
-- 2. Order Payment Summary Table
-- ===============================================
-- Tracks overall payment status for each order
CREATE TABLE t_sys_order_payment_summary (
    c_paymentsummaryid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_orderid BIGINT NOT NULL UNIQUE,

    -- Total Amount Breakdown
    c_totalamount DECIMAL(18,2) NOT NULL,
    c_advancepercentage DECIMAL(5,2) DEFAULT 30.00, -- 30-40%
    c_advanceamount DECIMAL(18,2) NOT NULL,
    c_finalamount DECIMAL(18,2) NOT NULL,

    -- Payment Status
    c_advancepaid BIT DEFAULT 0,
    c_advancepaiddate DATETIME,
    c_finalpaid BIT DEFAULT 0,
    c_finalpaiddate DATETIME,
    c_paymentcompleted BIT DEFAULT 0,

    -- Escrow Management
    c_escrowstatus VARCHAR(50), -- HELD, RELEASED_TO_VENDOR, REFUNDED
    c_escrowamount DECIMAL(18,2),
    c_escrowreleaseddate DATETIME,

    -- Commission
    c_commissionrate DECIMAL(5,2),
    c_commissionamount DECIMAL(18,2),
    c_commissionpaid BIT DEFAULT 0,

    -- Vendor Payout
    c_vendorpayoutstatus VARCHAR(50), -- PENDING, ADVANCE_RELEASED, FINAL_RELEASED, COMPLETED
    c_vendoradvancereleased BIT DEFAULT 0,
    c_vendoradvanceamount DECIMAL(18,2),
    c_vendoradvancereleaseddate DATETIME,
    c_vendorfinalpayout DECIMAL(18,2),
    c_vendorfinalpayoutdate DATETIME,

    -- Metadata
    c_paymentmode VARCHAR(50), -- SPLIT, FULL_ADVANCE, FULL_CASH
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME,

    CONSTRAINT FK_PaymentSummary_Order FOREIGN KEY (c_orderid) REFERENCES t_sys_order(c_orderid)
);
GO

CREATE INDEX IX_PaymentSummary_Order ON t_sys_order_payment_summary(c_orderid);
CREATE INDEX IX_PaymentSummary_Escrow ON t_sys_order_payment_summary(c_escrowstatus);
GO

-- ===============================================
-- 3. Escrow Ledger Table
-- ===============================================
-- Tracks all escrow transactions
CREATE TABLE t_sys_escrow_ledger (
    c_escrowid BIGINT PRIMARY KEY IDENTITY(1,1),
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
    c_requiresapproval BIT DEFAULT 0,
    c_approvedby BIGINT,
    c_approveddate DATETIME,

    -- Metadata
    c_description VARCHAR(500),
    c_createddate DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Escrow_Order FOREIGN KEY (c_orderid) REFERENCES t_sys_order(c_orderid),
    CONSTRAINT FK_Escrow_Transaction FOREIGN KEY (c_transactionid) REFERENCES t_sys_payment_transactions(c_transactionid)
);
GO

CREATE INDEX IX_Escrow_Order ON t_sys_escrow_ledger(c_orderid);
CREATE INDEX IX_Escrow_Status ON t_sys_escrow_ledger(c_status);
GO

-- ===============================================
-- 4. Vendor Payout Requests Table
-- ===============================================
CREATE TABLE t_sys_vendor_payout_requests (
    c_payoutrequestid BIGINT PRIMARY KEY IDENTITY(1,1),
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
    c_processeddate DATETIME,
    c_transactionreference VARCHAR(200),
    c_statusreason VARCHAR(500),

    -- Timestamps
    c_requesteddate DATETIME DEFAULT GETDATE(),
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME,

    CONSTRAINT FK_Payout_Order FOREIGN KEY (c_orderid) REFERENCES t_sys_order(c_orderid),
    CONSTRAINT FK_Payout_Owner FOREIGN KEY (c_cateringownerid) REFERENCES t_sys_catering_owner(c_ownerid)
);
GO

CREATE INDEX IX_Payout_Order ON t_sys_vendor_payout_requests(c_orderid);
CREATE INDEX IX_Payout_Owner ON t_sys_vendor_payout_requests(c_cateringownerid);
CREATE INDEX IX_Payout_Status ON t_sys_vendor_payout_requests(c_requeststatus);
GO

-- ===============================================
-- 5. EMI Plans Table
-- ===============================================l
CREATE TABLE t_sys_emi_plans (
    c_emiplanid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_bankname VARCHAR(100) NOT NULL,
    c_bankcode VARCHAR(50),

    -- Plan Details
    c_minordervalue DECIMAL(18,2) NOT NULL,
    c_maxordervalue DECIMAL(18,2),
    c_tenure INT NOT NULL, -- Months: 3, 6, 9, 12, etc.
    c_interestrate DECIMAL(5,2), -- Annual interest rate
    c_processingfee DECIMAL(18,2),

    -- Configuration
    c_isactive BIT DEFAULT 1,
    c_displayorder INT DEFAULT 0,
    c_termsandconditions NVARCHAR(MAX),

    -- Metadata
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME
);
GO

-- ===============================================
-- 6. Payment Gateway Configuration Table
-- ===============================================
CREATE TABLE t_sys_payment_gateway_config (
    c_configid BIGINT PRIMARY KEY IDENTITY(1,1),
    c_gatewayname VARCHAR(50) NOT NULL UNIQUE,

    -- API Configuration
    c_apikey VARCHAR(500),
    c_apisecret VARCHAR(500),
    c_merchantid VARCHAR(200),
    c_webhookurl VARCHAR(500),
    c_redirecturl VARCHAR(500),

    -- Settings
    c_isenabled BIT DEFAULT 1,
    c_istest BIT DEFAULT 0,
    c_priority INT DEFAULT 0,

    -- Supported Features
    c_supports_upi BIT DEFAULT 1,
    c_supports_card BIT DEFAULT 1,
    c_supports_netbanking BIT DEFAULT 1,
    c_supports_emi BIT DEFAULT 1,

    -- Metadata
    c_createddate DATETIME DEFAULT GETDATE(),
    c_modifieddate DATETIME
);
GO

-- ===============================================
-- Insert Default EMI Plans
-- ===============================================
INSERT INTO t_sys_emi_plans (c_bankname, c_bankcode, c_minordervalue, c_tenure, c_interestrate, c_processingfee, c_isactive)
VALUES
    ('HDFC Bank', 'HDFC', 10000, 3, 12.00, 199, 1),
    ('HDFC Bank', 'HDFC', 10000, 6, 13.00, 299, 1),
    ('HDFC Bank', 'HDFC', 15000, 9, 14.00, 399, 1),
    ('HDFC Bank', 'HDFC', 20000, 12, 15.00, 499, 1),
    ('ICICI Bank', 'ICICI', 10000, 3, 11.50, 199, 1),
    ('ICICI Bank', 'ICICI', 10000, 6, 12.50, 299, 1),
    ('ICICI Bank', 'ICICI', 15000, 9, 13.50, 399, 1),
    ('ICICI Bank', 'ICICI', 20000, 12, 14.50, 499, 1),
    ('SBI Bank', 'SBI', 10000, 3, 11.00, 149, 1),
    ('SBI Bank', 'SBI', 10000, 6, 12.00, 249, 1),
    ('SBI Bank', 'SBI', 15000, 9, 13.00, 349, 1),
    ('SBI Bank', 'SBI', 20000, 12, 14.00, 449, 1),
    ('Axis Bank', 'AXIS', 10000, 3, 12.50, 199, 1),
    ('Axis Bank', 'AXIS', 10000, 6, 13.50, 299, 1),
    ('Axis Bank', 'AXIS', 15000, 9, 14.50, 399, 1),
    ('Axis Bank', 'AXIS', 20000, 12, 15.50, 499, 1);
GO

-- ===============================================
-- Insert Default Payment Gateway Config
-- ===============================================
INSERT INTO t_sys_payment_gateway_config (c_gatewayname, c_isenabled, c_supports_upi, c_supports_card, c_supports_netbanking, c_supports_emi)
VALUES
    ('RAZORPAY', 1, 1, 1, 1, 1),
    ('PAYTM', 1, 1, 1, 1, 0),
    ('PHONEPE', 1, 1, 1, 1, 0);
GO

-- ===============================================
-- Add columns to existing Orders table (if not exist)
-- ===============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('t_sys_order') AND name = 'c_paymentmode')
BEGIN
    ALTER TABLE t_sys_order ADD c_paymentmode VARCHAR(50) DEFAULT 'SPLIT'; -- SPLIT, FULL_ADVANCE, FULL_CASH
    ALTER TABLE t_sys_order ADD c_advancepercentage DECIMAL(5,2) DEFAULT 30.00;
    ALTER TABLE t_sys_order ADD c_advanceamount DECIMAL(18,2);
    ALTER TABLE t_sys_order ADD c_finalamount DECIMAL(18,2);
    ALTER TABLE t_sys_order ADD c_escrowstatus VARCHAR(50) DEFAULT 'PENDING';
END
GO

PRINT 'Split Payment Schema created successfully!';
