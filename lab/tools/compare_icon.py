# ---------------------------------------------------------------------------
# Quasar 检测评估实验台 · 实验室基线样本（Quasar-LabBaseline）
# 作者：陈森（Chen Sen）  https://github.com/chendashi666
# 本文件由陈森创作：禁止盗卖，禁止商业用途。
# 上游 Quasar 代码版权归 MaxXor 及 Quasar 贡献者所有（MIT License）。
# ---------------------------------------------------------------------------

from PIL import Image, ImageChops
import sys

ref = Image.open(r"C:\Users\MECHREVO\Desktop\MUMA\lab\LabBaseline.ico")
ref.size = (32, 32)
ref = ref.convert("RGBA")

ext = Image.open(r"C:\Users\MECHREVO\Desktop\MUMA\dist\LabBaseline\_icon_check.png").convert("RGBA")
if ext.size != (32, 32):
    ext = ext.resize((32, 32), Image.LANCZOS)

diff = ImageChops.difference(ref, ext)
bbox = diff.getbbox()
hist = diff.histogram()
total = sum(hist[i] for i in range(len(hist)) if i > 0)
print("extracted_size", ext.size)
print("reference_size", ref.size)
print("difference_bbox", bbox)
print("nonzero_channel_values", total)
mean = sum(i * hist[i] for i in range(256)) / float(32*32*4)
print("mean_abs_diff", round(mean, 3))
print("MATCH" if mean < 6.0 else "MISMATCH")
