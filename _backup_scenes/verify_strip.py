# -*- coding: utf-8 -*-
"""校验：拿备份里的原始文件重新跑一遍剥离，结果应当和磁盘上的当前文件逐字节相同。
不同就说明写盘时编码/换行/BOM 出了问题。同时统计引号配平，防字符串被截断。
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from strip_comments import strip_comments, tidy, ROOT, ASSETS, BACKUP

bad = []
quote_bad = []
n = 0

for dirpath, _d, filenames in os.walk(ASSETS):
    for fn in filenames:
        if not fn.endswith(".cs"):
            continue
        n += 1
        path = os.path.join(dirpath, fn)
        rel = os.path.relpath(path, ROOT)
        origin = os.path.join(BACKUP, rel)

        with open(origin, "rb") as f:
            raw = f.read()
        bom = raw.startswith(b"\xef\xbb\xbf")
        expect = tidy(strip_comments(raw.decode("utf-8-sig"))).encode("utf-8")
        if bom:
            expect = b"\xef\xbb\xbf" + expect

        with open(path, "rb") as f:
            actual = f.read()

        if expect != actual:
            bad.append(rel)

        text = actual.decode("utf-8-sig")
        if text.count('"') % 2 != 0 or text.count("'") % 2 != 0:
            quote_bad.append(rel)

print("检查 %d 个文件" % n)
print("与脚本输出不一致: %d %s" % (len(bad), bad[:5]))
print("引号数为奇数(可能截断了字符串): %d %s" % (len(quote_bad), quote_bad[:5]))
