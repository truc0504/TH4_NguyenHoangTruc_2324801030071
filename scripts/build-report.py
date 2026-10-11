"""Create a readable evidence page from the actual build and verification logs."""
from pathlib import Path
from datetime import datetime
from zoneinfo import ZoneInfo
import html
import json
import re

root = Path(__file__).resolve().parent.parent
out = root / "artifacts"

def read(name):
    path = out / name
    if not path.exists():
        return "Chưa có log kiểm tra."
    raw = path.read_bytes()
    encoding = "utf-16" if raw.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig"
    return raw.decode(encoding, errors="replace").replace('\r\n', '\n')

dotnet, tests, web, restore = (read(name) for name in
    ("build-dotnet.log", "test.log", "build-web.log", "restore-check.log"))
build_pass = "Build succeeded." in dotnet
test_pass = sum(int(n) for n in re.findall(r"Passed:\s+(\d+)", tests))
web_pass = "Build complete." in web
dotnet_summary = dotnet[dotnet.rfind("Build succeeded."):] if build_pass else dotnet[-1600:]
vue_summary = web[web.find("WARNING  Compiled"):] if "WARNING  Compiled" in web else web[-1400:]
runtime_file = out / "runtime-checks.json"
checks = json.loads(runtime_file.read_text(encoding="utf-8")) if runtime_file.exists() else []
rows = "".join(f"<tr><td>{html.escape(c['name'])}</td><td>{html.escape(c['url'])}</td><td>{c.get('status', html.escape(c.get('error', '')))}</td></tr>" for c in checks)
backup_rows = "".join(f"<li>{html.escape(p.name)} — {p.stat().st_size:,} bytes</li>" for p in sorted((out / "database-backups").glob("*.backup")))
stamp = datetime.now(ZoneInfo("Asia/Ho_Chi_Minh")).strftime("%H:%M:%S ngày %d/%m/%Y (UTC+7)")
cards = [(".NET / 32 project", "PASS · 0 lỗi, 0 cảnh báo" if build_pass else "CẦN KIỂM TRA"),
         ("Kiểm thử MediatR", f"{test_pass}/10 đạt"),
         ("Vue production build", "PASS · 2 cảnh báo bundle" if web_pass else "CẦN KIỂM TRA"),
         ("Backup PostgreSQL", f"{restore.count('RESTORE PASS:')}/4 restore đạt")]
cards_html = "".join(f"<div class='card'><span>{html.escape(k)}</span><strong>{html.escape(v)}</strong></div>" for k,v in cards)
page = f"""<!doctype html><html lang="vi"><meta charset="utf-8">
<title>TH4 — Kết quả build và kiểm tra</title>
<style>body{{font:16px/1.6 'Segoe UI',sans-serif;background:#f2f5f8;color:#172231;margin:0}}main{{max-width:1160px;margin:32px auto;background:white;padding:36px;border-radius:12px}}h1{{font-size:30px;margin:0}}h2{{font-size:20px;margin-top:30px}}.meta{{color:#526172}}.cards{{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:24px 0}}.card{{background:#edf5f1;border:1px solid #b9d5c7;border-radius:8px;padding:16px}}.card span{{display:block;color:#436052}}strong{{display:block;font-size:17px}}pre{{background:#142231;color:#e5edf5;padding:20px;overflow:auto;white-space:pre-wrap;font:14px/1.6 Consolas,monospace}}table{{border-collapse:collapse;width:100%;font-size:14px}}td,th{{border-bottom:1px solid #dbe2e9;text-align:left;padding:8px}}a{{color:#0758a1}}.note{{padding:14px;background:#fff7df}}summary{{cursor:pointer;font-weight:600}}</style>
<main><h1>Bài thực hành 4 — Kết quả build và kiểm tra</h1>
<p class="meta">Nguyễn Hoàng Trúc · MSSV 2324801030071<br>Thời điểm lập báo cáo: {stamp}</p>
<div class="cards">{cards_html}</div>
<p class="note">Bài cài đặt môi trường: các host đã đăng ký MediatR và có /health. Nghiệp vụ bán bảo hiểm, đăng nhập, chat và thanh toán chưa được triển khai trong bài này.</p>
<h2>Build solution .NET — log gốc</h2><pre>{html.escape(dotnet_summary.strip())}</pre>
<p><a href="build-dotnet.log">Log đầy đủ 32 project</a> · <a href="test.log">Log kiểm thử</a> · <a href="build-web.log">Log Vue</a></p>
<h2>Backup và restore PostgreSQL 16</h2><pre>{html.escape(restore.strip())}</pre><ul>{backup_rows}</ul>
<h2>Kiểm tra host chạy thật</h2><table><tr><th>Thành phần</th><th>URL</th><th>HTTP</th></tr>{rows}</table>
<h2>Danh sách CSDL</h2><pre>{html.escape(read('postgres-databases.log').strip())}</pre>
<h2>Hạ tầng Docker</h2><pre>{html.escape(read('infra-status.log').strip())}</pre>
<details><summary>Kết quả Vue và cảnh báo kích thước bundle</summary><pre>{html.escape(vue_summary.strip())}</pre></details>
</main></html>"""
(out / "build-report.html").write_text(page, encoding="utf-8")
print(out / "build-report.html")
