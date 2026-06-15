USE HungryHubDB;
GO

UPDATE Orders
SET PaymentMethod = 'Cash'
WHERE PaymentMethod IS NULL;

UPDATE Orders
SET PaymentStatus = 'Pending'
WHERE PaymentStatus IS NULL;

UPDATE Orders
SET TransactionId = ''
WHERE TransactionId IS NULL;

UPDATE Orders
SET OrderNote = ''
WHERE OrderNote IS NULL;

UPDATE Orders
SET DeliveryAddress = ''
WHERE DeliveryAddress IS NULL;
GO