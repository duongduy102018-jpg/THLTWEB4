HUONG DAN DIA CHI SQL CHO CHECKOUT

Project da duoc sua de Checkout lay Tinh/Thanh pho, Quan/Huyen, Phuong/Xa truc tiep tu SQL qua 3 bang:
- Provinces
- Districts
- Wards

Code KHONG con tu seed du lieu dia chi mau trong Program.cs nua.

Cach chay:
1) Chay migration tao bang:
   Package Manager Console: Update-Database
   Hoac Terminal: dotnet ef database update

2) Import du lieu dia chi vao SQL Server.
   Co the dung file database/VietnamAddress_Seed.sql de test nhanh giao dien.
   Luu y: file nay chi la du lieu mau/tuong trung, khong phai danh sach day du that cua tat ca phuong xa.
   Neu can day du that, hay thay bang file SQL dia chi Viet Nam day du va insert vao 3 bang tren.

3) Neu da lo chay du lieu mau va muon xoa de nap lai du lieu that:
   Chay database/ClearAddressData.sql truoc, sau do import lai file SQL day du.

Lien ket bang:
- Districts.ProvinceId -> Provinces.Id
- Wards.DistrictId -> Districts.Id

Checkout goi cac API:
- /Cart/GetProvinces
- /Cart/GetDistricts?provinceId=...
- /Cart/GetWards?districtId=...
