# -*- coding: utf-8 -*-
"""修正版：跨仓库全量 C#/XAML 相似度扫描。"""
import hashlib, os, re
from itertools import product

A = r"D:\Code\CatClawMusic"
B = r"D:\Code\CatClawVideo"

# 只按根目录排除，避免误伤同名子项目（如 CatClawMusic.Data）
EX_ROOT = {
    A: {"release", "release-assets", "design", "docs", "tools", "tests", "Data", "META-INF", "androidx", "vitrum-src", ".git"},
    B: {"release", "docs", "tools", "Tools", "JavaBridge", "prototype", "samples", "sources", ".git"},
}

def collect(root):
    out = {}
    for dirpath, dirnames, filenames in os.walk(root):
        rel_parent = os.path.relpath(dirpath, root)
        top = rel_parent.split(os.sep)[0] if rel_parent != "." else ""
        if top in EX_ROOT[root]:
            dirnames[:] = []
            continue
        for d in list(dirnames):
            if d in ("obj", "bin", ".git"):
                dirnames.remove(d)
        for f in filenames:
            if f.endswith((".cs", ".xaml")):
                p = os.path.join(dirpath, f)
                rel = os.path.relpath(p, root)
                try:
                    with open(p, "r", encoding="utf-8-sig", errors="replace") as fh:
                        out[rel] = fh.read()
                except OSError:
                    pass
    return out

def normalize(text):
    text = re.sub(r"//[^\n]*", "", text)
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"<!--.*?-->", "", text, flags=re.S)
    text = re.sub(r"\s+", " ", text)
    for a, b in [("CatClawMusic","APP"),("CatClawVideo","APP"),("猫爪音乐","NAME"),("猫爪影视","NAME"),
                 ("com.catclaw.music","PKG"),("com.catclaw.video","PKG"),("catclaw-music","REPO"),("catclaw-video","REPO")]:
        text = text.replace(a, b)
    return text.strip()

def toks(text):
    return frozenset(re.findall(r"[A-Za-z_][A-Za-z0-9_.]*", text))

fa, fb = collect(A), collect(B)
na = {r: normalize(c) for r, c in fa.items()}
nb = {r: normalize(c) for r, c in fb.items()}
ta = {r: toks(n) for r, n in na.items()}
tb = {r: toks(n) for r, n in nb.items()}

print(f"Music 文件数: {len(fa)}  Video 文件数: {len(fb)}")
print()
print("=== 通道1：归一化后哈希完全一致 ===")
ha = {}
for r, n in na.items():
    if len(n) < 60: continue
    ha.setdefault(hashlib.md5(n.encode()).hexdigest(), []).append(r)
hits = []
for h, rs in ha.items():
    rv = nb.get
    # 找 Video 里是否有同样哈希
    for r in rs:
        pass
hb = {}
for r, n in nb.items():
    if len(n) < 60: continue
    hb.setdefault(hashlib.md5(n.encode()).hexdigest(), []).append(r)
for h, rs in ha.items():
    if h in hb:
        for rm in rs:
            for rv in hb[h]:
                print(f"  相同  M:{rm}  <->  V:{rv}")

print()
print("=== 通道2：Video 每个文件 vs Music 全部文件的最高 Jaccard（>0.35 才列出）===")
pairs = []
for rv, tv in tb.items():
    if len(nb[rv]) < 60: continue
    best = []
    for rm, tm in ta.items():
        if len(na[rm]) < 60: continue
        u = len(tv | tm)
        if u == 0: continue
        j = len(tv & tm) / u
        if j > 0.35:
            best.append((j, rm))
    best.sort(reverse=True)
    for j, rm in best[:2]:
        lm = len(na[rm].split()); lv = len(nb[rv].split())
        pairs.append((j, rm, rv, lm, lv))
pairs.sort(reverse=True)
for j, rm, rv, lm, lv in pairs:
    flag = "高" if j >= 0.8 else ("中" if j >= 0.55 else "")
    print(f"  {j:5.1%} {flag}  M:{rm}({lm})  <->  V:{rv}({lv})")
