USE HungryHubDB;
GO

ALTER TABLE Orders
ADD CouponCode     NVARCHAR(20)  NULL,
    DiscountAmount DECIMAL(10,2) DEFAULT 0;
GO

UPDATE Orders
SET DiscountAmount = 0
WHERE DiscountAmount IS NULL;
GO