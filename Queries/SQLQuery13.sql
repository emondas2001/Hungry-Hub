USE HungryHubDB;
GO

-- Restaurant owner accounts
CREATE TABLE RestaurantOwners (
    OwnerId      INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId INT NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(300) NOT NULL,
    Phone        NVARCHAR(20),
    IsActive     BIT DEFAULT 1,
    CreatedAt    DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RestaurantId)
        REFERENCES Restaurants(RestaurantId)
        ON DELETE CASCADE
);
GO

-- Restaurant activity log (admin monitoring)
CREATE TABLE RestaurantActivityLogs (
    LogId        INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId INT NOT NULL,
    OwnerId      INT NOT NULL,
    Action       NVARCHAR(200) NOT NULL,
    Details      NVARCHAR(500),
    CreatedAt    DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RestaurantId)
        REFERENCES Restaurants(RestaurantId)
        ON DELETE CASCADE
);
GO