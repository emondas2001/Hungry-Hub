USE HungryHubDB;
GO

-- Update existing items that have no icon
UPDATE MenuItems SET Icon = N'🍛'
WHERE RestaurantId = 1 AND Name = 'Mutton Kacchi';

UPDATE MenuItems SET Icon = N'🍚'
WHERE RestaurantId = 1 AND Name = 'Beef Tehari';

UPDATE MenuItems SET Icon = N'🍗'
WHERE RestaurantId = 1 AND Name = 'Chicken Roast';

UPDATE MenuItems SET Icon = N'🥛'
WHERE RestaurantId = 1 AND Name = 'Borhani';

UPDATE MenuItems SET Icon = N'🍮'
WHERE RestaurantId = 1 AND Name = 'Firni';

UPDATE MenuItems SET Icon = N'🍕'
WHERE RestaurantId = 2 AND Name LIKE '%Pizza%';

UPDATE MenuItems SET Icon = N'🥖'
WHERE RestaurantId = 2 AND Name = 'Garlic Bread';

UPDATE MenuItems SET Icon = N'🍔'
WHERE RestaurantId = 3 AND Name LIKE '%Burger%';

UPDATE MenuItems SET Icon = N'🍟'
WHERE RestaurantId = 3 AND Name LIKE '%Fries%';

UPDATE MenuItems SET Icon = N'🍚'
WHERE RestaurantId = 4 AND Name = 'Fried Rice';

UPDATE MenuItems SET Icon = N'🥡'
WHERE RestaurantId = 4 AND Name LIKE '%Chicken%';

UPDATE MenuItems SET Icon = N'🥢'
WHERE RestaurantId = 4 AND Name LIKE '%Spring%';

UPDATE MenuItems SET Icon = N'🥟'
WHERE RestaurantId = 4 AND Name LIKE '%Dim%';

UPDATE MenuItems SET Icon = N'🍜'
WHERE RestaurantId = 4 AND Name LIKE '%Soup%';

UPDATE MenuItems SET Icon = N'🍣'
WHERE RestaurantId = 6 AND Name LIKE '%Nigiri%';

UPDATE MenuItems SET Icon = N'🍱'
WHERE RestaurantId = 6 AND Name LIKE '%Roll%';

UPDATE MenuItems SET Icon = N'🫘'
WHERE RestaurantId = 6 AND Name = 'Edamame';

UPDATE MenuItems SET Icon = N'🍜'
WHERE RestaurantId = 6 AND Name LIKE '%Soup%';
GO