Checkout đã được đổi sang đọc địa chỉ từ SQL qua 3 bảng: Provinces, Districts, Wards.

Cách dùng:
1. Chạy migration mới:
   dotnet ef database update

2. Chạy project:
   dotnet run

3. Nếu database đã có sẵn dữ liệu cũ, có thể chạy file database/VietnamAddress_Seed.sql trong SQL Server để nạp lại dữ liệu mẫu.

Ghi chú:
- Project đã có hàm SeedAddressDataAsync trong Program.cs, nên nếu bảng Provinces đang trống thì hệ thống tự nạp dữ liệu khi chạy.
- Nếu muốn dữ liệu hành chính chuẩn mới nhất, có thể import bộ SQL công khai thanglequoc/vietnamese-provinces-database vào 3 bảng này.
