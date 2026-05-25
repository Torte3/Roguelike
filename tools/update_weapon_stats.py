"""Update weapon asset stats from balance table."""
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
weapon_dir = root / "Assets/Database/ItemData/武器"

RARITY = {"Common": 0, "Uncommon": 1, "Rare": 2, "Epic": 3, "Legendary": 4}

WEAPONS = {
    "こんぼう": {"rarity": "Common", "power": 3, "feature_limit": 2, "usage_limit": 70, "upgrade_limit": 10},
    "ダガー": {"rarity": "Common", "power": 4, "feature_limit": 3, "usage_limit": 55, "upgrade_limit": 10},
    "ジャベリン": {"rarity": "Common", "power": 5, "feature_limit": 3, "usage_limit": 40, "upgrade_limit": 10},
    "ロングボウ": {"rarity": "Common", "power": 5, "feature_limit": 4, "usage_limit": 40, "upgrade_limit": 10},
    "シャベル": {"rarity": "Uncommon", "power": 4, "feature_limit": 2, "usage_limit": 35, "upgrade_limit": 15},
    "スティレット": {"rarity": "Uncommon", "power": 4, "feature_limit": 3, "usage_limit": 45, "upgrade_limit": 15},
    "トライデント": {"rarity": "Uncommon", "power": 5, "feature_limit": 3, "usage_limit": 35, "upgrade_limit": 15},
    "バトルアックス": {"rarity": "Uncommon", "power": 6, "feature_limit": 2, "usage_limit": 50, "upgrade_limit": 15},
    "スリングショット": {"rarity": "Uncommon", "power": 4, "feature_limit": 3, "usage_limit": 35, "upgrade_limit": 15},
    "クロスボウ": {"rarity": "Uncommon", "power": 5, "feature_limit": 3, "usage_limit": 30, "upgrade_limit": 15},
    "魔法のほうき": {"rarity": "Uncommon", "power": 3, "feature_limit": 3, "usage_limit": 50, "upgrade_limit": 15},
    "必中の剣": {"rarity": "Rare", "power": 6, "feature_limit": 3, "usage_limit": 45, "upgrade_limit": 20},
    "ヘヴィハンマー": {"rarity": "Rare", "power": 8, "feature_limit": 3, "usage_limit": 30, "upgrade_limit": 20},
    "モーニングスター": {"rarity": "Rare", "power": 8, "feature_limit": 3, "usage_limit": 30, "upgrade_limit": 20},
    "ロングスピア": {"rarity": "Rare", "power": 7, "feature_limit": 3, "usage_limit": 35, "upgrade_limit": 20},
    "ロングソード": {"rarity": "Rare", "power": 7, "feature_limit": 5, "usage_limit": 50, "upgrade_limit": 20},
    "破壊の斧": {"rarity": "Rare", "power": 10, "feature_limit": 1, "usage_limit": 35, "upgrade_limit": 20},
    "ソウルイーター": {"rarity": "Epic", "power": 5, "feature_limit": 3, "usage_limit": 25, "upgrade_limit": 25},
    "竜巻の剣": {"rarity": "Epic", "power": 5, "feature_limit": 3, "usage_limit": 25, "upgrade_limit": 25},
    "連撃刀": {"rarity": "Epic", "power": 4, "feature_limit": 2, "usage_limit": 30, "upgrade_limit": 25},
    "爆発の弓": {"rarity": "Epic", "power": 7, "feature_limit": 3, "usage_limit": 10, "upgrade_limit": 25},
}


def decode_name(raw: str) -> str:
    name = raw.strip().strip('"')
    if "\\u" in name:
        return name.encode("ascii").decode("unicode_escape")
    return name


def update_field(text: str, field: str, value) -> str:
    pattern = rf"^  {field}: .+$"
    replacement = f"  {field}: {value}"
    if re.search(pattern, text, re.M):
        return re.sub(pattern, replacement, text, count=1, flags=re.M)
    return text


for asset in sorted(weapon_dir.rglob("*.asset")):
    text = asset.read_text(encoding="utf-8")
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    if not name_m:
        continue
    name = decode_name(name_m.group(1))
    if name not in WEAPONS:
        print(f"SKIP (not in table): {name}")
        continue

    stats = WEAPONS[name]
    text = update_field(text, "_rarity", RARITY[stats["rarity"]])
    text = update_field(text, "Power", stats["power"])
    text = update_field(text, "FeatureLimit", stats["feature_limit"])
    text = update_field(text, "UsageLimit", stats["usage_limit"])
    text = update_field(text, "UpgradeLimit", stats["upgrade_limit"])
    asset.write_text(text, encoding="utf-8")
    print(f"OK: {name} -> {stats['rarity']} P{stats['power']} FL{stats['feature_limit']} U{stats['usage_limit']} UL{stats['upgrade_limit']}")
