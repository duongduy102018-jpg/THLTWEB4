INSERT INTO Provinces (Code, Name, SortOrder)
VALUES 
('P001', N'TP. Hồ Chí Minh', 1),
('P002', N'Hà Nội', 2),
('P003', N'Đà Nẵng', 3);

INSERT INTO Districts (Code, Name, ProvinceId)
VALUES 
('P001D001', N'Quận 1', 1),
('P001D002', N'Quận Bình Thạnh', 1),
('P001D003', N'Thành phố Thủ Đức', 1),

('P002D001', N'Quận Hoàn Kiếm', 2),
('P002D002', N'Quận Cầu Giấy', 2),
('P002D003', N'Quận Đống Đa', 2),

('P003D001', N'Quận Hải Châu', 3),
('P003D002', N'Quận Thanh Khê', 3),
('P003D003', N'Quận Sơn Trà', 3);

INSERT INTO Wards (Code, Name, DistrictId)
VALUES 
('P001D001W001', N'Phường Bến Nghé', 1),
('P001D001W002', N'Phường Bến Thành', 1),
('P001D001W003', N'Phường Nguyễn Thái Bình', 1),

('P001D002W001', N'Phường 1', 2),
('P001D002W002', N'Phường 2', 2),
('P001D002W003', N'Phường 25', 2),

('P001D003W001', N'Phường Linh Trung', 3),
('P001D003W002', N'Phường Hiệp Bình Chánh', 3),
('P001D003W003', N'Phường An Phú', 3),

('P002D001W001', N'Phường Hàng Bạc', 4),
('P002D001W002', N'Phường Tràng Tiền', 4),
('P002D001W003', N'Phường Hàng Đào', 4),

('P002D002W001', N'Phường Dịch Vọng', 5),
('P002D002W002', N'Phường Nghĩa Tân', 5),
('P002D002W003', N'Phường Trung Hòa', 5),

('P002D003W001', N'Phường Láng Hạ', 6),
('P002D003W002', N'Phường Ô Chợ Dừa', 6),
('P002D003W003', N'Phường Văn Chương', 6),

('P003D001W001', N'Phường Hải Châu 1', 7),
('P003D001W002', N'Phường Hải Châu 2', 7),
('P003D001W003', N'Phường Thạch Thang', 7),

('P003D002W001', N'Phường An Khê', 8),
('P003D002W002', N'Phường Chính Gián', 8),
('P003D002W003', N'Phường Tân Chính', 8),

('P003D003W001', N'Phường An Hải Bắc', 9),
('P003D003W002', N'Phường Mân Thái', 9),
('P003D003W003', N'Phường Thọ Quang', 9);

SELECT * FROM Provinces;
SELECT * FROM Districts;
SELECT * FROM Wards;