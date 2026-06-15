USE HungryHubDB;
GO

CREATE TABLE Coupons (
    CouponId       INT IDENTITY(1,1) PRIMARY KEY,
    Code           NVARCHAR(20)  NOT NULL UNIQUE,
    Title          NVARCHAR(100) NOT NULL,
    Description    NVARCHAR(300),
    DiscountType   NVARCHAR(10)  NOT NULL,
    DiscountValue  DECIMAL(10,2) NOT NULL,
    MinOrderAmount DECIMAL(10,2) DEFAULT 0,
    MaxDiscount    DECIMAL(10,2) DEFAULT 0,
    UsageLimit     INT           DEFAULT 100,
    UsedCount      INT           DEFAULT 0,
    StartDate      DATETIME      NOT NULL,
    ExpiryDate     DATETIME      NOT NULL,
    IsActive       BIT           DEFAULT 1,
    CreatedAt      DATETIME      DEFAULT GETDATE()
);
GO

CREATE TABLE CouponUsages (
    UsageId   INT IDENTITY(1,1) PRIMARY KEY,
    CouponId  INT NOT NULL,
    UserId    INT NOT NULL,
    OrderId   INT NOT NULL,
    Discount  DECIMAL(10,2) NOT NULL,
    UsedAt    DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CouponId)
        REFERENCES Coupons(CouponId)
        ON DELETE CASCADE,
    FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    FOREIGN KEY (OrderId)
        REFERENCES Orders(OrderId)
        ON DELETE CASCADE
);
GO

-- Seed sample coupons
INSERT INTO Coupons
(Code, Title, Description,
 DiscountType, DiscountValue,
 MinOrderAmount, MaxDiscount,
 UsageLimit, StartDate, ExpiryDate, IsActive)
VALUES
('HUNGRY1ST',
 'First Order Special',
 'Get 20% off on your first order!',
 'Percent', 20, 100, 150,
 500,
 GETDATE(), DATEADD(month,3,GETDATE()), 1),

('SAVE50',
 'Flat ৳50 Off',
 'Get ৳50 off on orders above ৳300',
 'Flat', 50, 300, 50,
 200,
 GETDATE(), DATEADD(month,1,GETDATE()), 1),

('WEEKEND20',
 'Weekend Feast',
 '20% off every weekend, max ৳100 discount',
 'Percent', 20, 200, 100,
 300,
 GETDATE(), DATEADD(month,2,GETDATE()), 1),

('BIRYANI30',
 'Biryani Lover',
 '৳30 flat discount on any biryani order',
 'Flat', 30, 150, 30,
 100,
 GETDATE(), DATEADD(month,2,GETDATE()), 1);
GO