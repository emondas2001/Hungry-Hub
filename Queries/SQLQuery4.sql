USE HungryHubDB;
GO

-- Restaurants stored in DB (replaces seed data)
CREATE TABLE Restaurants (
    RestaurantId   INT IDENTITY(1,1) PRIMARY KEY,
    Name           NVARCHAR(100) NOT NULL,
    Cuisine        NVARCHAR(100) NOT NULL,
    Address        NVARCHAR(255) NOT NULL,
    Phone          NVARCHAR(20),
    Description    NVARCHAR(500),
    DeliveryFee    DECIMAL(10,2) DEFAULT 20,
    DeliveryTime   INT           DEFAULT 30,
    MinOrder       DECIMAL(10,2) DEFAULT 0,
    Rating         DECIMAL(3,1)  DEFAULT 0,
    ImageUrl       NVARCHAR(10)  DEFAULT N'🍽️',
    Tag            NVARCHAR(30)  DEFAULT '',
    IsOpen         BIT           DEFAULT 1,
    IsActive       BIT           DEFAULT 1,
    CreatedAt      DATETIME      DEFAULT GETDATE()
);
GO

-- Restaurant opening/closing hours
CREATE TABLE RestaurantHours (
    HoursId        INT IDENTITY(1,1) PRIMARY KEY,
    RestaurantId   INT NOT NULL,
    DayOfWeek      INT NOT NULL,  -- 0=Sun 1=Mon...6=Sat
    OpenTime       TIME,
    CloseTime      TIME,
    IsClosed       BIT DEFAULT 0,
    FOREIGN KEY (RestaurantId)
        REFERENCES Restaurants(RestaurantId)
        ON DELETE CASCADE
);
GO

-- Seed existing 12 restaurants into DB
INSERT INTO Restaurants
(Name, Cuisine, Address, Phone, Description,
 DeliveryFee, DeliveryTime, Rating, ImageUrl,
 Tag, IsOpen, IsActive)
VALUES
('Kacchi Bhai',    'Bangladeshi',   'GEC Circle, Chattogram',   '+8801711000001', 'Famous for authentic mutton kacchi biryani',   20, 30, 4.8, N'🍛', 'Popular',  1, 1),
('Pizza Palace',   'Italian',       'Agrabad, Chattogram',       '+8801711000002', 'Authentic Italian pizzas baked fresh daily',    30, 25, 4.5, N'🍕', 'Trending', 1, 1),
('Burger Nation',  'Fast Food',     'Muradpur, Chattogram',      '+8801711000003', 'Juicy burgers made with premium ingredients',   15, 20, 4.3, N'🍔', 'Popular',  1, 1),
('Dragon Garden',  'Chinese',       'Nasirabad, Chattogram',     '+8801711000004', 'Traditional Chinese cuisine with a modern twist',25, 35, 4.6, N'🥡', 'New',      1, 1),
('Spice Route',    'Indian',        'Oxygen, Chattogram',        '+8801711000005', 'Rich Indian curries and tandoor specialties',   20, 40, 4.4, N'🍜', '',         0, 1),
('Sushi Zen',      'Japanese',      'CDA Avenue, Chattogram',    '+8801711000006', 'Fresh sushi and Japanese fusion dishes',        40, 45, 4.7, N'🍣', 'Trending', 1, 1),
('Shawarma House', 'Middle Eastern','Halishahar, Chattogram',    '+8801711000007', 'Authentic shawarma and Middle Eastern grills',  15, 20, 4.2, N'🌯', 'Popular',  1, 1),
('The Cake Studio','Desserts',      'Khulshi, Chattogram',       '+8801711000008', 'Artisan cakes and premium desserts',            20, 30, 4.9, N'🎂', 'New',      1, 1),
('Tandoori Nights','Indian',        'Pahartali, Chattogram',     '+8801711000009', 'Smoky tandoori and North Indian classics',      20, 35, 4.3, N'🍗', '',         1, 1),
('Green Bowl',     'Healthy',       'Bayazid, Chattogram',       '+8801711000010', 'Healthy salads, bowls and smoothies',           15, 25, 4.5, N'🥗', 'New',      1, 1),
('Noodle Bar',     'Thai',          'Dampara, Chattogram',       '+8801711000011', 'Thai noodles and Southeast Asian flavors',      20, 30, 4.4, N'🍝', '',         0, 1),
('Momo Corner',    'Nepali',        'Lalkhan Bazar, Chattogram', '+8801711000012', 'Handmade momos and Nepali street food',         10, 20, 4.6, N'🥟', 'Popular',  1, 1);
GO

-- Seed default hours for all restaurants (9AM-11PM)
INSERT INTO RestaurantHours
(RestaurantId, DayOfWeek, OpenTime, CloseTime, IsClosed)
SELECT
    r.RestaurantId,
    d.DayNum,
    '09:00:00',
    '23:00:00',
    0
FROM Restaurants r
CROSS JOIN (
    SELECT 0 AS DayNum UNION SELECT 1 UNION SELECT 2
    UNION SELECT 3 UNION SELECT 4 UNION SELECT 5
    UNION SELECT 6
) d;
GO