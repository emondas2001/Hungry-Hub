USE HungryHubDB;
GO

CREATE TABLE Payments (
    PaymentId       INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL,
    UserId          INT NOT NULL,
    Amount          DECIMAL(10,2) NOT NULL,
    PaymentMethod   NVARCHAR(30) NOT NULL,
    TransactionId   NVARCHAR(100),
    AccountNumber   NVARCHAR(50),
    Status          NVARCHAR(20) DEFAULT 'Pending',
    PaidAt          DATETIME NULL,
    CreatedAt       DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (OrderId)
        REFERENCES Orders(OrderId)
        ON DELETE CASCADE,
    FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
);
GO

ALTER TABLE Orders
ADD PaymentMethod  NVARCHAR(30)  DEFAULT 'Cash',
    PaymentStatus  NVARCHAR(20)  DEFAULT 'Pending',
    TransactionId  NVARCHAR(100) NULL;
GO