USE HungryHubDB;
GO

-- Set defaults for any existing NULL values
UPDATE Orders
SET PaymentMethod = 'Cash'
WHERE PaymentMethod IS NULL;

UPDATE Orders
SET PaymentStatus = 'Pending'
WHERE PaymentStatus IS NULL;

UPDATE Orders
SET TransactionId = ''
WHERE TransactionId IS NULL;
GO

-- Make sure columns have default constraints
ALTER TABLE Orders
ALTER COLUMN PaymentMethod
    NVARCHAR(30) NOT NULL;
GO

ALTER TABLE Orders
ALTER COLUMN PaymentStatus
    NVARCHAR(20) NOT NULL;
GO