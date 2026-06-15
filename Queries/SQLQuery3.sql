USE HungryHubDB;
GO

-- Favourite restaurants
CREATE TABLE Favourites (
    FavouriteId  INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL,
    RestaurantId INT NOT NULL,
    RestaurantName  NVARCHAR(100) NOT NULL,
    RestaurantCuisine NVARCHAR(100),
    RestaurantIcon  NVARCHAR(10),
    AddedAt      DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_UserRestaurant UNIQUE (UserId, RestaurantId)
);
GO

-- Restaurant ratings
CREATE TABLE Ratings (
    RatingId     INT IDENTITY(1,1) PRIMARY KEY,
    UserId       INT NOT NULL,
    OrderId      INT NOT NULL,
    RestaurantId INT NOT NULL,
    Stars        INT NOT NULL CHECK (Stars BETWEEN 1 AND 5),
    Comment      NVARCHAR(500),
    RatedAt      DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId)  REFERENCES Users(UserId),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT UQ_UserOrder UNIQUE (UserId, OrderId)
);
GO

-- Notifications
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL,
    Title          NVARCHAR(150) NOT NULL,
    Message        NVARCHAR(500) NOT NULL,
    Icon           NVARCHAR(10) DEFAULT N'🔔',
    IsRead         BIT DEFAULT 0,
    CreatedAt      DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO