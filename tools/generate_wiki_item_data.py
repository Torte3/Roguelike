"""Generate アイテムデータ section for WIKI.md"""
import re
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
db = root / "Assets/Database"
dungeon = (db / "DungeonBluePrintData/Dungeon.asset").read_text(encoding="utf-8")
mapgraph = (db / "DungeonBluePrintData/MapGraph.asset").read_text(encoding="utf-8")

guid_to_name = {}
for meta in db.rglob("*.meta"):
    if "ItemData" not in str(meta):
        continue
    t = meta.read_text(encoding="utf-8", errors="ignore")
    m = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", t, re.M)
    if m:
        guid_to_name[m.group(1)] = meta.with_suffix("").stem

def block(name, next_names):
    i = dungeon.find(name + ":")
    if i < 0:
        return ""
    end = len(dungeon)
    for n in next_names:
        j = dungeon.find("\n  " + n + ":", i + 1)
        if j != -1 and j < end:
            end = j
    return dungeon[i:end]

sections = {
    "Potions": block("Potions", ["Scrolls", "Books", "Wands", "DirectWeapons", "RangedWeapons", "Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "Scrolls": block("Scrolls", ["Books", "Wands", "DirectWeapons", "RangedWeapons", "Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "Books": block("Books", ["Wands", "DirectWeapons", "RangedWeapons", "Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "Wands": block("Wands", ["DirectWeapons", "RangedWeapons", "Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "DirectWeapons": block("DirectWeapons", ["RangedWeapons", "Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "RangedWeapons": block("RangedWeapons", ["Artifacts", "Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "Artifacts": block("Artifacts", ["Others", "ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "Others": block("Others", ["ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "ChestItems": block("ChestItems", ["ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "ChestDirectWeapons": block("ChestDirectWeapons", ["ChestRangedWeapons", "ChestArtifacts", "ShopItems"]),
    "ChestRangedWeapons": block("ChestRangedWeapons", ["ChestArtifacts", "ShopItems"]),
    "ChestArtifacts": block("ChestArtifacts", ["ShopItems"]),
    "ShopItems": block("ShopItems", ["SpawnItem", "Traps"]),
}

def guids(text):
    return re.findall(r"guid:\s*([0-9a-f]{32})", text)

def names(gs):
    return [guid_to_name[g] for g in gs if g in guid_to_name]

def uniq(seq):
    s, o = set(), []
    for x in seq:
        if x in s:
            continue
        s.add(x)
        o.append(x)
    return o

pools = {"normal": set(), "chest": set(), "shop": set(), "boss": set()}
for k in ["Potions", "Scrolls", "Books", "Wands", "DirectWeapons", "RangedWeapons", "Artifacts", "Others"]:
    pools["normal"].update(uniq(names(guids(sections[k]))))
for k in ["ChestItems", "ChestDirectWeapons", "ChestRangedWeapons", "ChestArtifacts"]:
    pools["chest"].update(uniq(names(guids(sections[k]))))
pools["shop"].update(uniq(names(guids(sections["ShopItems"]))))
for m in re.finditer(
    r"_bossReward:\n((?:\s+- .*\n|\s+_item:.*\n|\s+_directWeapon:.*\n|\s+_rangedWeapon:.*\n|\s+_artifact:.*\n)*)",
    mapgraph,
):
    pools["boss"].update(uniq(names(guids(m.group(1)))))

def cat(name):
    if "ポーション" in name:
        return (0, name)
    if "巻物" in name:
        return (1, name)
    if "魔法書" in name:
        return (2, name)
    if "杖" in name and "指輪" not in name:
        return (3, name)
    weapons = ["剣", "斧", "弓", "ボウ", "槍", "ソード", "ダガー", "ハンマー", "こんぼう", "シャベル", "ジャベリン", "スティレット", "トライデント", "アックス", "スピア", "刀", "ほうき", "ショット", "イーター"]
    if any(x in name for x in weapons):
        return (4, name)
    if "指輪" in name:
        return (5, name)
    return (6, name)

all_items = sorted(set().union(*pools.values()), key=cat)

FEATURE_DESC = {
    "TwoRangeAttack": "攻撃範囲が直線2マスになる（通常は1マス）",
    "FanAttack": "攻撃範囲が扇形になる",
    "SpinAttack": "攻撃範囲が周囲（回転攻撃）になる",
    "Lunge": "攻撃前に1マス前進する",
    "BackStep": "攻撃後に1マス後退する",
    "ChargeAttack": "威力1.5倍・発動に1ターンの溜めが必要",
    "ArcingShot": "曲射で近くの敵を狙う（通常の直線射撃ではない）",
    "Piercing": "貫通射撃（射線上の複数対象に当たる）",
    "Explosive": "命中地点と周囲1マスに爆発範囲攻撃",
    "DoubleAttack": "効果が2回発動する",
    "TripleAttack": "効果が3回発動する",
    "Knockback": "対象を1マス吹き飛ばす",
    "Critical": "クリティカル率+25%（最大4重複）。命中率は75%に低下",
    "Dig": "壁や床を掘れる",
    "BreakTrap": "トラップを破壊できる",
    "Absorbing": "攻撃の代わりにHP吸収（吸収率+25%、最大4重複）",
    "GuaranteedHit": "命中率100%",
    "EnhanceThrow": "投擲時の威力1.5倍（近接武器のみ）",
    "Paralysis": "麻痺を付与（基礎5%、状態異常強化で上昇）",
    "Blind": "盲目を付与（基礎10%）",
    "Confusion": "混乱を付与（基礎10%）",
    "Sleep": "睡眠を付与（基礎5%）",
    "Poison": "毒を付与（基礎20%）",
    "Slowness": "鈍足を付与（基礎10%）",
    "Restraint": "拘束を付与（基礎10%）",
    "EnhanceAbnormalCondition": "状態異常付与率が1段階強化（最大4重複）",
    "Fire": "攻撃に火属性（威力の半分が火、残りが物理）",
    "Ice": "攻撃に氷属性（威力の半分が氷、残りが物理）",
    "Thunder": "攻撃に雷属性（威力の半分が雷、残りが物理）",
    "Light": "攻撃に光属性（威力の半分が光、残りが物理）",
    "Dark": "攻撃に闇属性（威力の半分が闇、残りが物理）",
    "EnhanceDurability": "使用回数の減少確率-20%（最大5重複）",
    "Artistic": "売値が2倍",
}

FEATURE_NAME = {
    "TwoRangeAttack": "2マス攻撃",
    "FanAttack": "扇型攻撃",
    "SpinAttack": "回転攻撃",
    "Lunge": "突進",
    "BackStep": "バックステップ",
    "ChargeAttack": "溜め攻撃",
    "ArcingShot": "曲射",
    "Piercing": "貫通",
    "Explosive": "爆発",
    "DoubleAttack": "2回攻撃",
    "TripleAttack": "3回攻撃",
    "Knockback": "吹き飛ばし",
    "Critical": "クリティカル",
    "Dig": "掘る",
    "BreakTrap": "トラップを破壊",
    "Absorbing": "吸収",
    "GuaranteedHit": "必中",
    "EnhanceThrow": "投擲強化",
    "Paralysis": "麻痺",
    "Blind": "盲目",
    "Confusion": "混乱",
    "Sleep": "眠り",
    "Poison": "毒",
    "Slowness": "鈍足",
    "Restraint": "拘束",
    "EnhanceAbnormalCondition": "状態異常付与率強化",
    "Fire": "火",
    "Ice": "氷",
    "Thunder": "雷",
    "Light": "光",
    "Dark": "闇",
    "EnhanceDurability": "耐久強化",
    "Artistic": "美術品",
}

RARITY_NAMES = {0: "Common", 1: "Uncommon", 2: "Rare", 3: "Epic", 4: "Legendary"}

weapon_dir = db / "ItemData/武器"
weapons = []

def decode_asset_name(raw: str) -> str:
    name = raw.strip().strip('"')
    if "\\u" in name:
        return name.encode("ascii").decode("unicode_escape")
    return name

for asset in sorted(weapon_dir.rglob("*.asset")):
    text = asset.read_text(encoding="utf-8")
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    name = decode_asset_name(name_m.group(1)) if name_m else asset.stem
    kind = "近接" if asset.parent.name == "DirectWeapon" else "射撃"
    power_m = re.search(r"Power: (\d+)", text)
    power = int(power_m.group(1)) if power_m else None
    limit_m = re.search(r"FeatureLimit: (\d+)", text)
    feature_limit = int(limit_m.group(1)) if limit_m else None
    usage_m = re.search(r"UsageLimit: (\d+)", text)
    usage_limit = int(usage_m.group(1)) if usage_m else None
    rarity_m = re.search(r"_rarity: (\d+)", text)
    rarity = RARITY_NAMES.get(int(rarity_m.group(1)), "?") if rarity_m else "?"
    upgrade_m = re.search(r"UpgradeLimit: (\d+)", text)
    upgrade_limit = int(upgrade_m.group(1)) if upgrade_m else None
    feats = re.findall(r"stringValue: (\w+)", text)
    feat_names = [FEATURE_NAME.get(f, f) for f in feats]
    weapons.append(
        {
            "name": name,
            "kind": kind,
            "power": power,
            "rarity": rarity,
            "feature_limit": feature_limit,
            "usage_limit": usage_limit,
            "upgrade_limit": upgrade_limit,
            "features": feat_names,
            "feature_keys": feats,
        }
    )

lines = []
lines.append("## アイテムデータ")
lines.append("")
lines.append("データソース: `Assets/Database/DungeonBluePrintData/Dungeon.asset`（出現プール）、`MapGraph.asset`（ボス報酬）、`Assets/Database/ItemData/`（武器定義）。")
lines.append("")

lines.append("### 出現プール一覧")
lines.append("")
lines.append("マスターデータ上、各アイテムがどのプールに登録されているか。○ = 登録あり。")
lines.append("")
lines.append("- **通常**: フロアに落ちるアイテム（`SpawnItem` カテゴリ重み付き抽選）")
lines.append("- **宝箱**: 宝箱の中身（`ChestItems` 等。フロアの `WeaponChanceInChest` により通常武器プールから武器が出る場合あり）")
lines.append("- **ボス**: ボス撃破後の報酬宝箱（`MapGraph` の `_bossReward`）")
lines.append("- **店**: ショップの品揃え（`ShopItems`）")
lines.append("")
lines.append("| アイテム | 通常 | 宝箱 | ボス | 店 |")
lines.append("|---|---|:---:|:---:|:---:|")
for item in all_items:
    n = "○" if item in pools["normal"] else ""
    c = "○" if item in pools["chest"] else ""
    b = "○" if item in pools["boss"] else ""
    s = "○" if item in pools["shop"] else ""
    lines.append(f"| {item} | {n} | {c} | {b} | {s} |")
lines.append("")

lines.append("### 武器一覧")
lines.append("")
lines.append("各武器の基礎ステータスと初期付与能力。合成で能力上限まで追加可能。")
lines.append("")
lines.append("| 武器 | 種別 | レア度 | 攻撃力 | 能力上限 | 使用回数 | 強化上限 | 固有能力 |")
lines.append("|---|---|---|---:|---:|---:|---:|---|")
rarity_order = {"Common": 0, "Uncommon": 1, "Rare": 2, "Epic": 3, "Legendary": 4}
for w in sorted(weapons, key=lambda x: (rarity_order.get(x["rarity"], 99), x["name"])):
    feats = "、".join(w["features"]) if w["features"] else "（なし）"
    lines.append(
        f"| {w['name']} | {w['kind']} | {w['rarity']} | {w['power']} | {w['feature_limit']} | {w['usage_limit']} | {w['upgrade_limit']} | {feats} |"
    )
lines.append("")

lines.append("### 武器能力（ItemFeature）一覧")
lines.append("")
lines.append("武器に付与できる特殊能力。ゲーム内表示名と、コード上の効果説明。")
lines.append("")

feature_groups = [
    ("攻撃範囲・形態（近接）", ["TwoRangeAttack", "FanAttack", "SpinAttack"]),
    ("攻撃形態（射撃）", ["ArcingShot", "Piercing", "Explosive"]),
    ("攻撃補助", ["Lunge", "BackStep", "ChargeAttack", "DoubleAttack", "TripleAttack", "Knockback", "Critical", "Dig", "BreakTrap", "Absorbing", "GuaranteedHit", "EnhanceThrow"]),
    ("状態異常付与", ["Paralysis", "Blind", "Confusion", "Sleep", "Poison", "Slowness", "Restraint", "EnhanceAbnormalCondition"]),
    ("属性", ["Fire", "Ice", "Thunder", "Light", "Dark"]),
    ("その他", ["EnhanceDurability", "Artistic"]),
]

for group_name, keys in feature_groups:
    lines.append(f"#### {group_name}")
    lines.append("")
    lines.append("| 能力名 | 説明 |")
    lines.append("|---|---|")
    for k in keys:
        lines.append(f"| {FEATURE_NAME[k]} | {FEATURE_DESC[k]} |")
    lines.append("")

lines.append("### 武器接頭辞（プレフィックス）")
lines.append("")
lines.append("武器名の前に付く修飾語。能力（ItemFeature）とは別枠で、武器の基礎性能や上限値を補正する。")
lines.append("")
lines.append("- **付与タイミング**: 宝箱の中身が武器枠になったとき、`WeaponPrefixes` から1つ抽選される。")
lines.append("- **対象**: 近接武器・射撃武器。通常ドロップ、店売り、ボス固定報酬、シャイニー敵の所持武器には通常付かない。")
lines.append("- **現状の出現**: 各 `FloorData` の `WeaponChanceInChest` が 0 の場合、宝箱から接頭辞付き武器は出ない。")
lines.append("- **呪い付き**: 接頭辞の `IsCursed` が有効な場合、その接頭辞が付いた武器は生成時に呪われる。")
lines.append("- **表示名**: `接頭辞名 + 武器名` で表示される（例: `鋭いロングソード`）。")
lines.append("")
PREFIX_ROLES = {
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

lines.append("| 接頭辞 | 威力倍率 | 能力上限追加 | 使用回数倍率 | 強化上限追加 | 呪い付き | 役割 |")
lines.append("|---|---:|---:|---:|---:|:---:|:---:|")

prefix_dir = db / "WeaponPrefix"
for asset in sorted(prefix_dir.glob("*.asset")):
    text = asset.read_text(encoding="utf-8")
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    name = decode_asset_name(name_m.group(1)) if name_m else asset.stem
    pm = re.search(r"PowerMagnification: ([\d.]+)", text)
    fl = re.search(r"FeatureLimitAdditional: (\d+)", text)
    um = re.search(r"UsageLimitMagnification: ([\d.]+)", text)
    au = re.search(r"AdditionalUpgradeLimit: (\d+)", text)
    cursed = re.search(r"IsCursed: ([01])", text)
    cursed_text = "○" if cursed and cursed.group(1) == "1" else ""
    role = PREFIX_ROLES.get(name, "")
    lines.append(
        f"| {name} | {pm.group(1) if pm else '?'} | {fl.group(1) if fl else '?'} | {um.group(1) if um else '?'} | {au.group(1) if au else '?'} | {cursed_text} | {role} |"
    )
lines.append("")

lines.append("---")
lines.append("")

out = "\n".join(lines)
(root / "tools/wiki_item_data_section.md").write_text(out, encoding="utf-8")
print(f"Wrote {len(lines)} lines")
