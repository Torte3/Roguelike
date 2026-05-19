"""Update WeaponPrefix assets from balance table."""
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
prefix_dir = root / "Assets/Database/WeaponPrefix"

PREFIXES = {
    "名工の": {"power": 1.0, "feature": 1, "usage": 1.25, "upgrade": 4, "cursed": 0},
    "堅牢な": {"power": 0.7, "feature": 0, "usage": 2.0, "upgrade": 0, "cursed": 0},
    "大きな": {"power": 1.0, "feature": 2, "usage": 1.0, "upgrade": 0, "cursed": 0},
    "巨大な": {"power": 0.8, "feature": 4, "usage": 0.8, "upgrade": 0, "cursed": 0},
    "未完の": {"power": 0.7, "feature": 0, "usage": 1.0, "upgrade": 12, "cursed": 0},
    "硬い": {"power": 1.0, "feature": 0, "usage": 1.5, "upgrade": 0, "cursed": 0},
    "禍々しい": {"power": 1.3, "feature": 2, "usage": 0.6, "upgrade": 0, "cursed": 1},
    "継ぎ接ぎの": {"power": 0.6, "feature": 6, "usage": 0.5, "upgrade": 0, "cursed": 0},
    "良質な": {"power": 1.0, "feature": 0, "usage": 1.0, "upgrade": 6, "cursed": 0},
    "血染めの": {"power": 1.8, "feature": 0, "usage": 0.5, "upgrade": 0, "cursed": 1},
    "鋭い": {"power": 1.5, "feature": 0, "usage": 1.0, "upgrade": 0, "cursed": 0},
}

ROLES = {
    "名工の": "万能型",
    "堅牢な": "超耐久型",
    "大きな": "軽い合成ベース",
    "巨大な": "本格合成ベース",
    "未完の": "大投資型",
    "硬い": "素直な耐久型",
    "禍々しい": "癖の強い混合型",
    "継ぎ接ぎの": "極端な能力枠ロマン",
    "良質な": "素直な強化投資型",
    "血染めの": "短命高火力型",
    "鋭い": "素直な火力型",
}


def decode_name(raw: str) -> str:
    name = raw.strip().strip('"')
    if "\\u" in name:
        return name.encode("ascii").decode("unicode_escape")
    return name


def update_field(text: str, field: str, value) -> str:
    pattern = rf"^  {field}: .+$"
    replacement = f"  {field}: {value}"
    return re.sub(pattern, replacement, text, count=1, flags=re.M)


for asset in sorted(prefix_dir.glob("*.asset")):
    text = asset.read_text(encoding="utf-8")
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    if not name_m:
        continue
    name = decode_name(name_m.group(1))
    if name not in PREFIXES:
        print(f"SKIP: {name}")
        continue
    s = PREFIXES[name]
    text = update_field(text, "PowerMagnification", s["power"])
    text = update_field(text, "FeatureLimitAdditional", s["feature"])
    text = update_field(text, "UsageLimitMagnification", s["usage"])
    text = update_field(text, "AdditionalUpgradeLimit", s["upgrade"])
    text = update_field(text, "IsCursed", s["cursed"])
    # Fix Name field to match filename
    text = update_field(text, "Name", f'"{name}"')
    asset.write_text(text, encoding="utf-8")
    print(f"OK: {name} ({ROLES[name]})")
