# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

import os
from PIL import Image, ImageDraw

S = 1024  # supersample for smooth edges

def rounded(draw, box, radius, fill):
    draw.rounded_rectangle(box, radius=radius, fill=fill)

big = Image.new("RGBA", (S, S), (0, 0, 0, 0))
d = ImageDraw.Draw(big)

bg = (16, 35, 59, 255)
accent = (56, 189, 248, 255)
ink = (230, 237, 243, 255)
muted = (110, 130, 155, 255)

rounded(d, (0, 0, S - 1, S - 1), radius=int(S * 0.16), fill=bg)
# inner border
d.rounded_rectangle((int(S*0.035), int(S*0.035), S-int(S*0.035), S-int(S*0.035)),
                    radius=int(S*0.13), outline=(31, 92, 145, 255), width=int(S*0.012))

# detection lens
cx, cy, r = int(S*0.5), int(S*0.415), int(S*0.185)
w = int(S*0.052)
d.ellipse((cx-r, cy-r, cx+r, cy+r), outline=accent, width=w)
d.ellipse((cx-int(S*0.055), cy-int(S*0.055), cx+int(S*0.055), cy+int(S*0.055)), fill=accent)

# baseline
by = int(S*0.70)
d.rounded_rectangle((int(S*0.16), by, int(S*0.84), by+int(S*0.055)),
                    radius=int(S*0.027), fill=ink)
# baseline ticks
for tx in (0.28, 0.5, 0.72):
    x = int(S*tx)
    d.rounded_rectangle((x-int(S*0.012), by+int(S*0.075), x+int(S*0.012), by+int(S*0.155)),
                        radius=int(S*0.012), fill=muted)

out_dir = r"C:\Users\MECHREVO\Desktop\MUMA\lab"
os.makedirs(out_dir, exist_ok=True)
png_path = os.path.join(out_dir, "LabBaseline.png")
ico_path = os.path.join(out_dir, "LabBaseline.ico")
big.resize((256,256), Image.LANCZOS).save(png_path)
big.resize((256,256), Image.LANCZOS).save(ico_path, format="ICO",
    sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
print("png", png_path, os.path.getsize(png_path))
print("ico", ico_path, os.path.getsize(ico_path))
