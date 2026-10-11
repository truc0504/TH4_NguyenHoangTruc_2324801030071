# Bài thực hành 4 — Cài đặt môi trường Microservices

**Sinh viên:** Nguyễn Hoàng Trúc — **MSSV:** 2324801030071  
**Repository:** https://github.com/truc0504/TH4_NguyenHoangTruc_2324801030071

## Phạm vi bài nộp

Bài thực hành 4 yêu cầu tổ chức source, cài MediatR, bổ sung các thư mục được phát và build hệ thống. Theo Task 1, các dịch vụ mới chỉ cần khung project. Các host hiện có endpoint `/health` và đăng ký MediatR; **chưa triển khai nghiệp vụ bán bảo hiểm, đăng nhập, chat, thanh toán hay tìm kiếm**. Giao diện Vue/Blazor được giữ từ source giảng viên; build được không đồng nghĩa các luồng nghiệp vụ đã hoạt động.

## Tổ chức source

Mỗi dịch vụ có ba project: `*.Api` (định nghĩa giao tiếp/DTO), tên dịch vụ (ASP.NET Core host/triển khai), `*.Test` (kiểm thử). Có Product, Pricing, Offer, Policy, Auth, Chat, Payment, PolicySearch, Document và Dashboard. Tổng cộng 32 project .NET, gồm 30 project của 10 dịch vụ, AgentPortalApiGateway và BlazorWasmClient.

Các thư mục `eureka-server`, `postgres`, `scripts`, `Web` xuất hiện trong Solution Explorer dưới dạng Solution Items. BlazorWasmClient và AgentPortalApiGateway là project thực trong solution. Các DTO bổ sung đáp ứng tham chiếu còn thiếu trong source Blazor.

## Build và kiểm thử

Yêu cầu .NET SDK 8 trở lên, Node 22 trở lên và Internet khi restore lần đầu. Mở `InsuranceMicroservices.sln` bằng Visual Studio 2022 hoặc chạy tại thư mục này:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

Script build toàn bộ .NET, chạy test và build Vue; log được lưu vào `artifacts/`. Có thể dùng `-SkipWeb` hoặc `-SkipTests` khi chỉ muốn kiểm tra một phần. `scripts/build.bat` là lối vào tương đương cho Windows; Linux dùng `sh scripts/build.sh`.

Đã kiểm tra trên máy hiện tại:

- Build Release .NET: thành công, 0 warning, 0 error; xem `artifacts/build-dotnet.log`.
- 10/10 bài kiểm tra đăng ký MediatR thành công; xem `artifacts/test.log` và `artifacts/test-results/`.
- Vue: xem `artifacts/build-web.log`; source cũ có cảnh báo kích thước bundle và dữ liệu Browserslist cũ.
- Docker Compose: hai file cấu hình đã qua `docker compose ... config --quiet`. Container chưa chạy do Windows cần khởi động lại sau khi cài WSL.
- Eureka chưa được build/chạy thực tế; Dockerfile sẽ build Gradle khi chạy hạ tầng.
- Chưa có backup CSDL thật cho đến khi PostgreSQL chạy thành công.

MediatR 12.4.1 được đăng ký qua `AddMediatR(config => config.RegisterServicesFromAssemblyContaining<Program>())`, thay cho cú pháp Startup cũ trong đề. Gateway dùng Steeltoe Eureka 3.4.0 để xử lý cảnh báo NuGet của bản 3.2.7: https://github.com/advisories/GHSA-j8ph-6fxj-g533 .

Vue đổi node-sass sang Dart Sass, dùng đường dẫn Node trực tiếp để hỗ trợ thư mục `TH_KT&TKPM`, tắt worker pool cũ và dùng wrapper chờ build promise hoàn tất trước khi thoát trên Node mới. `.npmrc` bật `legacy-peer-deps` cho các plugin Vue CLI 3; đã kiểm tra cài mới bằng `npm ci` rồi build thành công. Không tắt cảnh báo kích thước bundle.

## Hạ tầng và CSDL

GitHub Desktop 3.6.7 đã được cài thành công qua winget từ installer chính thức. Để mở repo trong ứng dụng, dùng File → Add local repository và chọn thư mục chứa solution này. Source đã được đẩy bằng Git lên nhánh `main` của repository ở đầu trang; quyền cộng tác của giảng viên chưa được xác nhận.

Docker Desktop đã có trên máy. Lệnh cài WSL đã báo thành công nhưng yêu cầu **khởi động lại Windows** mới có hiệu lực. Nếu Docker vẫn báo lỗi sau khi khởi động lại, mở PowerShell với quyền Administrator và kiểm tra `wsl --status`, `wsl --version`; cập nhật bằng `wsl --update` nếu cần. Không xóa/reset dữ liệu Docker.

Sau khi máy khởi động lại và Docker Desktop báo Engine running:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\infra.ps1 -Action up
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\infra.ps1 -Action status
```

Hạ tầng gồm PostgreSQL 16, RabbitMQ management, Elasticsearch 8.9.2 và Eureka. Dữ liệu được lưu ở named volumes, không phụ thuộc đường dẫn `/tmp` của Linux trên máy Windows. Port chỉ bind localhost.

| Thành phần | Địa chỉ | Tài khoản lab |
|---|---|---|
| PostgreSQL | localhost:5432 | postgres / lab_pass |
| PostgreSQL ứng dụng | localhost:5432 | lab_user / lab_pass |
| RabbitMQ UI | http://localhost:15672 | lab_user / lab_pass |
| RabbitMQ AMQP | localhost:5672 | lab_user / lab_pass |
| Elasticsearch | http://localhost:9200 | Security tắt trong môi trường lab |
| Eureka | http://localhost:8761 | Không có |

Mật khẩu trong source là giá trị mẫu cho máy thực hành. Script `postgres/createdatabases.sql` tạo 4 CSDL và đặt `lab_user` làm owner: `lab_netmicro_payments`, `lab_netmicro_jobs`, `lab_netmicro_policy`, `lab_netmicro_pricing`. Script tự chạy khi volume PostgreSQL được khởi tạo lần đầu. Không chạy lại trực tiếp trên cluster đã có các tên này.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\backup-databases.ps1
```

Script dùng `pg_dump -Fc`, copy file nhị phân bằng Docker Compose (không dùng redirect PowerShell cho binary), kiểm tra bằng `pg_restore --list`, xuất 4 file `.backup` và checksum tại `artifacts/database-backups/`. Các CSDL ở giai đoạn này chưa có bảng nghiệp vụ; backup là minh chứng cài đặt môi trường.

Restore bằng pgAdmin (Restore, Format: Custom) hoặc script vào CSDL đích trống:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\restore-databases.ps1 -Database lab_netmicro_policy
```

## Chạy khung dịch vụ

```powershell
dotnet run --project ProductService
dotnet run --project BlazorWasmClient
docker compose -f scripts/app.yml up -d --build
```

Hai lệnh `dotnet run` dùng riêng khi muốn chạy cục bộ; không chạy cùng container đang dùng cùng port. Product health: http://localhost:5030/health . Blazor: http://localhost:5081 . Vue qua container: http://localhost:8080 . Gateway: http://localhost:8099 . Gateway giữ các route bảo mật từ source phát, dùng địa chỉ dịch vụ cố định cho môi trường khung; Eureka chạy độc lập để đáp ứng phần cài môi trường. Chưa có đăng ký Eureka của các host nghiệp vụ hoặc test tích hợp qua gateway.

## Đọc các thư mục phát kèm

- `eureka-server`: Spring Boot/Gradle chạy service registry trên port 8761; Dockerfile build Java trước khi tạo image runtime.
- `postgres`: Dockerfile và SQL khởi tạo tài khoản, bốn CSDL.
- `scripts`: cấu hình Compose hạ tầng và ứng dụng, script build, backup, restore.
- `Web`: Vue 2 SPA gồm sản phẩm, hợp đồng, dashboard, chat, đăng nhập; dùng biến `VUE_APP_*` trong `.env-example` để chọn backend.
- `BlazorWasmClient`: client .NET 8/Blazorise tương đương, lưu phiên trong SessionStorage, sử dụng các DTO từ project Api; endpoint cấu hình tại `wwwroot/appsettings.json`.
- `AgentPortalApiGateway`: Ocelot, JWT bearer, CORS và cache; route tại `ocelot.json`/`ocelot.docker.json`.

## Sản phẩm nộp bài

1. Source trên repo GitHub ở đầu trang. Mời tài khoản giảng viên theo email `caotxuan@gmail.com` trong Settings → Collaborators → Add people (cần tài khoản sở hữu repo và có thể phải xác thực lại).
2. **Sau khi PostgreSQL chạy:** nộp đủ 4 file `.backup` trong `artifacts/database-backups/` lên elearning.
3. Ảnh kết quả build: `artifacts/build-results.jpg` là ảnh chụp trang hiển thị log gốc `artifacts/build-report.html`, không phải ảnh cửa sổ Output. Nếu cần ảnh Output nguyên bản, mở solution trong Visual Studio, chọn Build → Build Solution, hiển thị Output/Error List và chụp màn hình. Công cụ chụp Visual Studio trên máy này bị timeout khi thử lấy ảnh.

Không coi phần hạ tầng, backup, quyền truy cập giảng viên hoặc nộp elearning là hoàn thành chỉ vì source build được. Hạn trong đề: trước 18:00 ngày 11/10/2026, giờ Việt Nam.
