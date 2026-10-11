# Task 1 — Tổ chức source code

Nguyễn Hoàng Trúc — 2324801030071

Solution gồm 8 microservice theo phụ lục đề bài: Auth, Chat, Pricing, Policy, PolicySearch, Payment, Product, Document.
Mỗi dịch vụ có 3 project: định nghĩa API (.Api), triển khai (tên dịch vụ), kiểm thử (.Test).
Tổng cộng 24 project .NET 8, chỉ tạo khung tên project, chưa viết code nghiệp vụ hoặc test.
Các project .Test dùng xUnit Test Project, tham chiếu project triển khai tương ứng. Chưa viết bài test; không chạy test trong Task 1.
Không bao gồm Offer, Dashboard, frontend, gateway, MediatR hoặc cấu hình hạ tầng của Task 2.
Mở InsuranceMicroservices.sln bằng Visual Studio 2022 để kiểm tra cấu trúc.
Build bằng Visual Studio (Build → Build Solution) hoặc lệnh `dotnet build InsuranceMicroservices.sln -c Debug -m:1`.
Đã kiểm tra build Debug ngày 11/10/2026: 24 project, 0 lỗi, 0 cảnh báo. Log thực tế lưu tại build-task1.log.
Repository: https://github.com/truc0504/TH4_NguyenHoangTruc_2324801030071 . Nhánh main được chỉnh về bản Task 1; chưa xác nhận quyền truy cập của giảng viên.
Mời giảng viên theo email caotxuan@gmail.com tại GitHub Settings → Collaborators → Add people bằng tài khoản sở hữu repo.
Bản đã làm sang Task 2 được giữ riêng tại ../Archive_Task2_Before_Task1; không nằm trong solution hoặc gói ZIP Task 1.
