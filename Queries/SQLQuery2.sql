USE HungryHubDB;
GO

CREATE TABLE Admins (
    AdminId       INT IDENTITY(1,1) PRIMARY KEY,
    FirstName     NVARCHAR(50)  NOT NULL,
    LastName      NVARCHAR(50)  NOT NULL,
    Email         NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber   NVARCHAR(20)  NOT NULL,
    Gender        NVARCHAR(10)  NOT NULL,
    DateOfBirth   DATE          NOT NULL,
    PasswordHash  NVARCHAR(256) NOT NULL,
    IsActive      BIT           DEFAULT 1,
    CreatedAt     DATETIME      DEFAULT GETDATE()
);
GO