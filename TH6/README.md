# Bài thực hành 4 — Công ty bảo hiểm / Cài đặt môi trường

Sinh viên: **Nguyễn Hoàng Trúc — 2324801030071**. Repository: [TH4_NguyenHoangTruc_2324801030071](https://github.com/truc0504/TH4_NguyenHoangTruc_2324801030071).

## Task 1 — Tổ chức source

Mỗi microservice có ba project: `*.Api` định nghĩa giao tiếp, project cùng tên dịch vụ triển khai/host và `*.Test` kiểm thử. Tám dịch vụ theo phụ lục gồm Auth, Chat, Pricing, Policy, PolicySearch, Payment, Product và Document. Có thêm Offer/Dashboard để tương thích với source frontend và cấu hình Docker phát kèm. Solution gồm **32 project .NET 8**: 10 bộ project dịch vụ, AgentPortalApiGateway và BlazorWasmClient.

Source được quản lý bằng GitHub; giảng viên `caotxuan` đã có quyền cộng tác trên repository. GitHub Desktop đã được cài trên máy.

## Task 2 — Nội dung đã thực hiện

- Thêm MediatR 12.4.1 và đăng ký DI trong các host .NET 8; AuthService sử dụng `AddAuthService` trong `ServiceRegistration.cs`.
- Bổ sung sáu thư mục mẫu: `eureka-server`, `postgres`, `scripts`, `Web`, `BlazorWasmClient`, `AgentPortalApiGateway`. Project .NET có trong solution; hạ tầng/frontend được hiển thị qua Solution Items.
- Bổ sung API contracts còn thiếu để Blazor biên dịch; giữ routing, JWT/CORS và UI từ source mẫu.
- Chạy PostgreSQL 16, RabbitMQ management, Elasticsearch 8.9.2 và Eureka bằng Docker Compose; dùng named volumes.
- Tạo lab_user và bốn CSDL; xuất backup Custom, thử restore cả bốn vào CSDL kiểm tra riêng.
- Thay image openjdk:8-jre-slim không còn trên registry bằng eclipse-temurin:8-jre; build Eureka bằng Gradle/Java 8.
- Sửa Vue CLI cũ để build trên Node hiện tại bằng Dart Sass và wrapper build; giữ cảnh báo kích thước bundle.

Phạm vi bài là **cài đặt môi trường và code ban đầu**. Các host có `/health` và MediatR; chưa triển khai nghiệp vụ bán bảo hiểm, đăng nhập, SignalR chat, tìm kiếm hoặc thanh toán. Frontend biên dịch/chạy được nhưng chưa có các API nghiệp vụ để thực hiện các luồng đó.

## Kết quả thực tế ngày 11/10/2026

| Kiểm tra | Kết quả | Minh chứng |
|---|---|---|
| Build solution Release | Thành công, 0 lỗi, 0 cảnh báo | artifacts/build-dotnet.log |
| Kiểm thử MediatR | 10/10 đạt | artifacts/test.log |
| Build Vue production | Thành công; 2 cảnh báo kích thước bundle | artifacts/build-web.log |
| Build Eureka | Thành công | artifacts/build-eureka.log |
| PostgreSQL/RabbitMQ/Elasticsearch/Eureka | Đã chạy; HTTP/SQL kiểm tra thành công | artifacts/infra-checks.json, artifacts/infra-status.log |
| 10 host, gateway và Blazor | HTTP 200 | artifacts/runtime-checks.json |
| Backup/restore PostgreSQL | Đủ 4 backup; 4 restore thành công | artifacts/database-backups/, artifacts/restore-check.log |

Ảnh tại `artifacts/build-results.png` được kết xuất từ log thật bằng Pillow, không phải ảnh chụp màn hình. Trang báo cáo là `artifacts/build-report.html`. Quyền điều khiển Visual Studio và mở trang báo cáo bằng trình duyệt đều bị từ chối trong phiên thực hiện; ảnh chụp màn hình cửa sổ build cần bổ sung thủ công hoặc sau khi cấp quyền.

## Build lại

Yêu cầu .NET SDK 8 trở lên, Node/npm và Internet cho lần restore đầu. Mở InsuranceMicroservices.sln bằng Visual Studio 2022 hoặc chạy từ repo:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\build.ps1
```

Script build .NET, chạy kiểm thử và build Vue; log nằm trong artifacts. Dùng -SkipWeb hoặc -SkipTests để kiểm tra riêng một phần. Linux dùng `sh scripts/build.sh`.

## Chạy và dừng

Mở Docker Desktop trước:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\infra.ps1 -Action up
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\infra.ps1 -Action status
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\start-local.ps1
```

start-local.ps1 chạy các host và Blazor dưới nền, lưu PID/log ở artifacts/runtime. Pricing dùng **5041** trên Windows vì port 5040 bị dịch vụ hệ thống chiếm. Gateway có route /health tới ProductService để kiểm tra routing; route nghiệp vụ vẫn giữ JWT.

| Thành phần | URL/cổng |
|---|---|
| Auth health | http://localhost:6060/health |
| Product health | http://localhost:5030/health |
| Pricing health | http://localhost:5041/health |
| Gateway health | http://localhost:8099/health |
| Blazor | http://localhost:5081 |
| Eureka | http://localhost:8761 |
| RabbitMQ UI | http://localhost:15672 |
| RabbitMQ AMQP | localhost:5672 |
| Elasticsearch | http://localhost:9200 |
| PostgreSQL | localhost:5432 |

Tài khoản lab: PostgreSQL superuser postgres/lab_pass; PostgreSQL ứng dụng và RabbitMQ lab_user/lab_pass. Elasticsearch tắt security trong môi trường lab; các cổng Docker chỉ bind localhost.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\stop-local.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\infra.ps1 -Action down
```

Lệnh down giữ named volumes. Để chạy ứng dụng bằng container, dừng host cục bộ trước rồi dùng `docker compose -f scripts/app.yml up -d --build`. app.yml dùng network của infra nên chạy infra trước. Phiên này kiểm tra runtime bằng host cục bộ; không build/chạy toàn bộ image ứng dụng.

## CSDL và backup

postgres/createdatabases.sql tạo owner lab_user và bốn CSDL:

- lab_netmicro_payments
- lab_netmicro_jobs
- lab_netmicro_policy
- lab_netmicro_pricing

SQL tự chạy khi volume rỗng khởi tạo lần đầu. CSDL giai đoạn này chưa có bảng nghiệp vụ. Backup tạo từ PostgreSQL thật; pg_restore --list và restore thử đều thành công.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\backup-databases.ps1
```

File Custom backup và SHA256 nằm ở artifacts/database-backups. Restore bằng pgAdmin vào CSDL đích trống (Format: Custom), hoặc:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\restore-databases.ps1 -Database lab_netmicro_policy
```

## Đọc source mẫu

| Thư mục | Vai trò và điểm đọc |
|---|---|
| eureka-server | Java/Spring service registry; build.gradle, application.yml, Dockerfile |
| postgres | PostgreSQL, tài khoản và bốn CSDL; Dockerfile và SQL |
| scripts | Compose; build, chạy, backup và restore |
| Web | Vue 2 SPA; package.json, router.js, components, .env-example |
| BlazorWasmClient | .NET 8 client; Program.cs, Components, appsettings.json, ProjectReference |
| AgentPortalApiGateway | Ocelot routing, JWT, CORS; Program.cs, appsettings và ocelot JSON |

## Sản phẩm nộp

1. Source trên GitHub ở đầu README; giảng viên đã có quyền truy cập.
2. Bốn file .backup trong artifacts/database-backups.
3. Ảnh kết xuất kết quả build artifacts/build-results.png, kèm log gốc. Đề yêu cầu ảnh chụp màn hình: cần bổ sung ảnh cửa sổ build trực tiếp trước khi nộp.

Gói ZIP dành cho Elearning được đặt cạnh thư mục repo. Trang nộp: https://elearning.tdmu.edu.vn/mod/assign/view.php?id=1176085 . Trình duyệt từ chối quyền truy cập trang này trong phiên thực hiện nên chưa tải file lên hoặc xác nhận nộp Elearning.
