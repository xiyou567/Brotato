# -*- coding: utf-8 -*-
"""删掉 Assets 下所有 .cs 里的注释。

带状态机，逐个字符扫：字符串、逐字字符串 @"..."、字符字面量 '...' 里的
"//"、"/*" 都是普通字符，不能当注释。块注释里的换行保留，行号不会漂。

先整树备份到 _backup_scenes/comments_backup/。
"""
import os
import shutil
import sys

ROOT = r"D:\游戏create\Brotato"
ASSETS = os.path.join(ROOT, "Assets")
BACKUP = os.path.join(ROOT, "_backup_scenes", "comments_backup")


def strip_comments(src):
    out = []
    i = 0
    n = len(src)
    while i < n:
        c = src[i]

        # 逐字字符串 @"..."，里面只有 "" 才是转义的引号
        if c == "@" and i + 1 < n and src[i + 1] == '"':
            out.append('@"')
            i += 2
            while i < n:
                if src[i] == '"':
                    if i + 1 < n and src[i + 1] == '"':
                        out.append('""')
                        i += 2
                        continue
                    out.append('"')
                    i += 1
                    break
                out.append(src[i])
                i += 1
            continue

        # 普通字符串
        if c == '"':
            out.append('"')
            i += 1
            while i < n:
                ch = src[i]
                if ch == "\\":
                    out.append(src[i:i + 2])
                    i += 2
                    continue
                if ch == '"':
                    out.append('"')
                    i += 1
                    break
                out.append(ch)
                i += 1
            continue

        # 字符字面量，比如 '"' 或者 '/'
        if c == "'":
            out.append("'")
            i += 1
            while i < n:
                ch = src[i]
                if ch == "\\":
                    out.append(src[i:i + 2])
                    i += 2
                    continue
                if ch == "'":
                    out.append("'")
                    i += 1
                    break
                out.append(ch)
                i += 1
            continue

        # 行注释
        if c == "/" and i + 1 < n and src[i + 1] == "/":
            while i < n and src[i] != "\n":
                i += 1
            continue

        # 块注释：里面的换行照抄，保证行数不变
        if c == "/" and i + 1 < n and src[i + 1] == "*":
            i += 2
            while i < n and not (src[i] == "*" and i + 1 < n and src[i + 1] == "/"):
                if src[i] == "\n":
                    out.append("\n")
                i += 1
            i += 2
            continue

        out.append(c)
        i += 1

    return "".join(out)


def tidy(text):
    """去掉行尾空白；连续空行压成一个"""
    crlf = text.count("\r\n") > text.count("\n") / 2
    lines = text.replace("\r\n", "\n").split("\n")
    lines = [ln.rstrip() for ln in lines]

    result = []
    blank = 0
    for ln in lines:
        if ln == "":
            blank += 1
            if blank > 1:
                continue
        else:
            blank = 0
        result.append(ln)

    # 开头空行和结尾多余空行去掉
    while result and result[0] == "":
        result.pop(0)
    while result and result[-1] == "":
        result.pop()

    out = "\n".join(result) + "\n"
    if crlf:
        out = out.replace("\n", "\r\n")
    return out


def main():
    files = []
    for dirpath, _dirnames, filenames in os.walk(ASSETS):
        for fn in filenames:
            if fn.endswith(".cs"):
                files.append(os.path.join(dirpath, fn))
    files.sort()

    changed = 0
    removed_lines = 0
    for path in files:
        rel = os.path.relpath(path, ROOT)

        with open(path, "rb") as f:
            raw = f.read()
        bom = raw.startswith(b"\xef\xbb\xbf")
        text = raw.decode("utf-8-sig")

        stripped = tidy(strip_comments(text))

        # 备份
        dest = os.path.join(BACKUP, rel)
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        shutil.copy2(path, dest)

        if stripped != text:
            changed += 1
            removed_lines += len(text.split("\n")) - len(stripped.split("\n"))

        data = stripped.encode("utf-8")
        if bom:
            data = b"\xef\xbb\xbf" + data
        with open(path, "wb") as f:
            f.write(data)

    print("扫到 %d 个 .cs" % len(files))
    print("改动 %d 个，减少 %d 行" % (changed, removed_lines))
    print("备份在 %s" % BACKUP)


if __name__ == "__main__":
    main()
