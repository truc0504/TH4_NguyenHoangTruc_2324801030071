"""Render actual log excerpts as an image; this is not a UI screenshot."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json
import re

root = Path(__file__).resolve().parent.parent
out = root / 'artifacts'

def read(name):
    raw = (out / name).read_bytes()
    return raw.decode('utf-16' if raw.startswith((b'\xff\xfe', b'\xfe\xff')) else 'utf-8-sig', errors='replace')

dotnet, tests, vue = map(read, ['build-dotnet.log', 'test.log', 'build-web.log'])
assert 'Build succeeded.' in dotnet
assert 'Build complete.' in vue
passed = sum(map(int, re.findall(r'Passed:\s+(\d+)', tests)))
restored = read('restore-check.log').count('RESTORE PASS:')
runtime = json.loads((out / 'runtime-checks.json').read_text(encoding='utf-8'))
infra = json.loads((out / 'infra-checks.json').read_text(encoding='utf-8'))
image = Image.new('RGB', (1440, 1260), '#f1f5f9')
draw = ImageDraw.Draw(image)
regular = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 25)
title = ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf', 38)
mono = ImageFont.truetype('C:/Windows/Fonts/consola.ttf', 23)
draw.rounded_rectangle((34, 30, 1406, 1230), radius=14, fill='white')
draw.text((64, 60), 'TH4 — Kết quả build và kiểm tra', font=title, fill='#17324a')
draw.text((64, 119), 'Nguyễn Hoàng Trúc · 2324801030071 · 11/10/2026', font=regular, fill='#42586a')
draw.text((64, 166), 'ẢNH KẾT XUẤT TỪ LOG THẬT — không phải ảnh chụp cửa sổ Visual Studio', font=regular, fill='#986414')
y = 229
summary = [
    '.NET Release / 32 project: 0 lỗi, 0 cảnh báo',
    f'Kiểm thử MediatR: {passed}/10 đạt',
    'Vue production build: thành công, 2 cảnh báo kích thước bundle',
    f'PostgreSQL: {restored}/4 backup restore thành công',
    f'Host dịch vụ + Gateway + Blazor: {sum(c.get("status") == 200 for c in runtime)}/{len(runtime)} HTTP 200',
    f'Hạ tầng: {sum(c.get("status") == 200 for c in infra)}/{len(infra)} HTTP kiểm tra đạt'
]
for line in summary:
    draw.text((64, y), line, font=regular, fill='#245f42')
    y += 40
draw.text((64, 505), 'Trích nguyên văn build-dotnet.log:', font=regular, fill='#17324a')
draw.rounded_rectangle((64, 554, 1376, 763), radius=8, fill='#142333')
excerpt = dotnet[dotnet.rfind('Build succeeded.'):].split('Workload updates')[0].strip()
draw.multiline_text((88, 574), excerpt, font=mono, fill='#e6edf5', spacing=7)
draw.text((64, 800), 'Trích nguyên văn restore-check.log:', font=regular, fill='#17324a')
draw.rounded_rectangle((64, 851, 1376, 1062), radius=8, fill='#142333')
for index, line in enumerate(read('restore-check.log').strip().splitlines()):
    line = line.split(' -> ')[0]
    draw.text((88, 876 + index * 39), line, font=mono, fill='#e6edf5')
draw.text((64, 1100), 'Log đầy đủ, thông tin hạ tầng và backup nằm trong thư mục artifacts.', font=regular, fill='#42586a')
draw.text((64, 1150), 'Môi trường + code ban đầu; chưa triển khai các luồng nghiệp vụ bảo hiểm.', font=regular, fill='#42586a')
image.save(out / 'build-results.png')
print(out / 'build-results.png')
