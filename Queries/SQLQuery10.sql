USE HungryHubDB;
GO

-- ── ORDER SPLIT ──────────────────────────────
CREATE TABLE SplitOrders (
    SplitOrderId   INT IDENTITY(1,1) PRIMARY KEY,
    OrderId        INT NOT NULL,
    CreatorUserId  INT NOT NULL,
    TotalAmount    DECIMAL(10,2) NOT NULL,
    SplitType      NVARCHAR(20)  NOT NULL,
    Status         NVARCHAR(20)  DEFAULT 'Active',
    CreatedAt      DATETIME      DEFAULT GETDATE(),
    FOREIGN KEY (OrderId)
        REFERENCES Orders(OrderId)
        ON DELETE CASCADE,
    FOREIGN KEY (CreatorUserId)
        REFERENCES Users(UserId)
);
GO

CREATE TABLE SplitParticipants (
    ParticipantId  INT IDENTITY(1,1) PRIMARY KEY,
    SplitOrderId   INT NOT NULL,
    UserId         INT NULL,
    Name           NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(100),
    AmountOwed     DECIMAL(10,2) NOT NULL,
    IsPaid         BIT           DEFAULT 0,
    PaidAt         DATETIME      NULL,
    FOREIGN KEY (SplitOrderId)
        REFERENCES SplitOrders(SplitOrderId)
        ON DELETE CASCADE
);
GO

-- ── PRE-ORDER / SPECIAL EVENTS ───────────────
CREATE TABLE PreOrders (
    PreOrderId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    RestaurantId    INT NOT NULL,
    RestaurantName  NVARCHAR(100),
    EventName       NVARCHAR(200) NOT NULL,
    EventDate       DATETIME      NOT NULL,
    EventAddress    NVARCHAR(300) NOT NULL,
    GuestCount      INT           NOT NULL,
    SpecialRequests NVARCHAR(500),
    TotalAmount     DECIMAL(10,2) DEFAULT 0,
    AdvanceAmount   DECIMAL(10,2) DEFAULT 0,
    Status          NVARCHAR(30)  DEFAULT 'Pending',
    AdminNote       NVARCHAR(500),
    CreatedAt       DATETIME      DEFAULT GETDATE(),
    FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
);
GO

CREATE TABLE PreOrderItems (
    PreOrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    PreOrderId     INT NOT NULL,
    ItemName       NVARCHAR(100) NOT NULL,
    Quantity       INT           NOT NULL,
    UnitPrice      DECIMAL(10,2) NOT NULL,
    SubTotal       DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (PreOrderId)
        REFERENCES PreOrders(PreOrderId)
        ON DELETE CASCADE
);
GO

-- ── SUBSCRIPTION MEAL PLANS ──────────────────
CREATE TABLE MealPlans (
    PlanId         INT IDENTITY(1,1) PRIMARY KEY,
    PlanName       NVARCHAR(100) NOT NULL,
    Description    NVARCHAR(500),
    MealsPerWeek   INT           NOT NULL,
    PricePerWeek   DECIMAL(10,2) NOT NULL,
    IsActive       BIT           DEFAULT 1,
    CreatedAt      DATETIME      DEFAULT GETDATE()
);
GO

CREATE TABLE UserSubscriptions (
    SubscriptionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL,
    PlanId         INT NOT NULL,
    RestaurantId   INT NOT NULL,
    RestaurantName NVARCHAR(100),
    StartDate      DATE          NOT NULL,
    EndDate        DATE          NOT NULL,
    DeliveryTime   NVARCHAR(20)  DEFAULT '12:00 PM',
    DeliveryAddr   NVARCHAR(300) NOT NULL,
    Status         NVARCHAR(20)  DEFAULT 'Active',
    TotalPaid      DECIMAL(10,2) DEFAULT 0,
    CreatedAt      DATETIME      DEFAULT GETDATE(),
    FOREIGN KEY (UserId)
        REFERENCES Users(UserId),
    FOREIGN KEY (PlanId)
        REFERENCES MealPlans(PlanId)
);
GO

-- Seed default meal plans
INSERT INTO MealPlans
    (PlanName, Description,
     MealsPerWeek, PricePerWeek, IsActive)
VALUES
('Basic Plan',
 '5 meals per week, Mon-Fri lunch delivery',
 5, 1200, 1),
('Standard Plan',
 '10 meals per week, Mon-Fri lunch and dinner',
 10, 2200, 1),
('Premium Plan',
 '14 meals per week, daily lunch and dinner',
 14, 2800, 1),
('Weekend Plan',
 '4 meals per week, Sat-Sun lunch and dinner',
 4, 900, 1);
GO