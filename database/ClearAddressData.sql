-- Xoa toan bo du lieu dia chi de import lai du lieu that
DELETE FROM Wards;
DELETE FROM Districts;
DELETE FROM Provinces;

-- Reset identity neu dung SQL Server
DBCC CHECKIDENT ('Wards', RESEED, 0);
DBCC CHECKIDENT ('Districts', RESEED, 0);
DBCC CHECKIDENT ('Provinces', RESEED, 0);
