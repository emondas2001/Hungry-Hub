USE HungryHubDB;
GO

-- Food menu items
CREATE TABLE MenuItems (
    MenuItemId   INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId INT NOT NULL,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(255),
    Price        DECIMAL(10,2) NOT NULL,
    Category     NVARCHAR(50),
    Icon         NVARCHAR(10),
    IsAvailable  BIT DEFAULT 1
);
GO

-- Orders header
CREATE TABLE Orders (
    OrderId        INT IDENTITY(1,1) PRIMARY KEY,
    UserId         INT NOT NULL,
    RestaurantId   INT NOT NULL,
    RestaurantName NVARCHAR(100),
    TotalAmount    DECIMAL(10,2) NOT NULL,
    DeliveryFee    DECIMAL(10,2) NOT NULL,
    GrandTotal     DECIMAL(10,2) NOT NULL,
    Status         NVARCHAR(30) DEFAULT 'Pending',
    DeliveryAddress NVARCHAR(255),
    OrderNote      NVARCHAR(500),
    OrderDate      DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- Order line items
CREATE TABLE OrderItems (
    OrderItemId  INT IDENTITY(1,1) PRIMARY KEY,
    OrderId      INT NOT NULL,
    MenuItemId   INT NOT NULL,
    ItemName     NVARCHAR(100) NOT NULL,
    Quantity     INT NOT NULL,
    UnitPrice    DECIMAL(10,2) NOT NULL,
    SubTotal     DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE
);
GO

-- Seed menu items for each restaurant
INSERT INTO MenuItems (RestaurantId, Name, Description, Price, Category, Icon) VALUES
-- Kacchi Bhai (1)
(1,'Mutton Kacchi','Slow-cooked mutton with aromatic basmati rice',320,'Main',N'🍛'),
(1,'Beef Tehari','Spiced beef with fragrant rice',280,'Main',N'🍚'),
(1,'Chicken Roast','Half chicken roasted with spices',250,'Main',N'🍗'),
(1,'Borhani','Traditional fermented yoghurt drink',50,'Drinks',N'🥛'),
(1,'Firni','Classic rice pudding dessert',80,'Dessert',N'🍮'),
-- Pizza Palace (2)
(2,'Margherita Pizza','Classic tomato, mozzarella, basil',380,'Pizza',N'🍕'),
(2,'BBQ Chicken Pizza','Smoky BBQ sauce with grilled chicken',450,'Pizza',N'🍕'),
(2,'Pepperoni Pizza','Loaded with premium pepperoni',480,'Pizza',N'🍕'),
(2,'Garlic Bread','Toasted with herb butter',120,'Sides',N'🥖'),
(2,'Pepsi Can','Chilled 330ml',60,'Drinks',N'🥤'),
-- Burger Nation (3)
(3,'Classic Beef Burger','Double patty with cheese and sauce',280,'Burgers',N'🍔'),
(3,'Crispy Chicken Burger','Fried chicken fillet with slaw',260,'Burgers',N'🍔'),
(3,'Veggie Burger','Garden patty with fresh veggies',220,'Burgers',N'🍔'),
(3,'Loaded Fries','Fries with cheese sauce and jalapeños',150,'Sides',N'🍟'),
(3,'Milkshake','Chocolate or vanilla 400ml',180,'Drinks',N'🥤'),
-- Dragon Garden (4)
(4,'Fried Rice','Wok-tossed egg fried rice',200,'Rice',N'🍚'),
(4,'Kung Pao Chicken','Spicy stir-fried chicken with peanuts',320,'Main',N'🥡'),
(4,'Spring Rolls','Crispy vegetable spring rolls x4',140,'Starters',N'🥢'),
(4,'Dim Sum Basket','Steamed dumplings x6',180,'Starters',N'🥟'),
(4,'Hot & Sour Soup','Classic Chinese soup bowl',120,'Soups',N'🍜'),
-- Sushi Zen (6)
(6,'Salmon Nigiri','Fresh salmon over seasoned rice x2',320,'Sushi',N'🍣'),
(6,'California Roll','Crab, avocado, cucumber roll x8',380,'Rolls',N'🍱'),
(6,'Spicy Tuna Roll','Tuna with sriracha sauce x8',420,'Rolls',N'🍱'),
(6,'Edamame','Salted steamed soybeans',120,'Starters',N'🫘'),
(6,'Miso Soup','Traditional Japanese soup',100,'Soups',N'🍜');
GO