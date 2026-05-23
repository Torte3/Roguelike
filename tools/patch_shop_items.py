#!/usr/bin/env python3
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
asset = ROOT / "Assets/Database/DungeonBluePrintData/Dungeon.asset"
snippet = subprocess.check_output(
    ["python", str(ROOT / "tools/get_shop_guids.py")],
    text=True,
    encoding="utf-8",
).rstrip() + "\n"

text = asset.read_text(encoding="utf-8")
pattern = r"    ShopItems:\n      items:.*?\n        Weight: \d+\n"
if not re.search(pattern, text, re.S):
    raise SystemExit("ShopItems block not found")
text = re.sub(pattern, snippet, text, count=1, flags=re.S)
asset.write_text(text, encoding="utf-8", newline="\n")
print("Patched Dungeon.asset ShopItems")
