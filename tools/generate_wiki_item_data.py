"""Generate アイテムデータ section for WIKI.md"""
import re
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

ELEMENT_NAMES = {0: "物理", 1: "火", 2: "氷", 3: "雷", 4: "光", 5: "闇"}
CATEGORY_NAMES = {0: "薬", 1: "巻物", 2: "本", 3: "杖", 6: "その他"}
THROW_DISTANCE = 10
RARITY_NAMES = {0: "Common", 1: "Uncommon", 2: "Rare", 3: "Epic", 4: "Legendary"}

FLAG_NAMES = {
    "CannotAct": "行動不能",
    "CannotMove": "移動不能",
    "Confused": "混乱",
    "Clairvoyant": "透視",
    "Blind": "盲目",
    "NarrowVision": "視力低下",
    "OverDrive": "オーバードライブ",
    "AllConditionProof": "全状態異常耐性",
    "Hard": "硬質",
    "ExplosionProof": "爆発耐性",
    "Heavy": "スーパーアーマー",
    "SecureHold": "手放さず",
    "CurseProof": "呪い耐性",
    "Negotiator": "値切り",
    "IsAffectedByTrap": "帯電",
    "AutoIdentify": "自動識別",
    "RandomTeleport": "気まぐれワープ",
    "RandomExplosion": "気まぐれ爆発",
    "BookMaster": "魔法書マスター",
    "WandMaster": "杖マスター",
    "PotionMaster": "ポーションマスター",
    "CurseIdentify": "呪い識別",
    "AdjacentAttackGuard": "隣接ダメージ半減",
    "FullHpCritical": "満タンクリティカル",
    "StealEmpower": "盗むたび攻撃強化",
}

FLAG_DESC = {
    "Clairvoyant": "視界が広がる",
    "OverDrive": "時間停止状態になり、自分だけ連続行動できる",
    "AllConditionProof": "全状態異常に耐性",
    "Hard": "クリティカル以外のダメージが1に軽減",
    "ExplosionProof": "爆発ダメージを軽減",
    "Heavy": "攻撃を受けても行動が中断されにくい",
    "SecureHold": "装備中のアイテムを落としにくい",
    "CurseProof": "呪いを受けにくい",
    "Negotiator": "ショップの価格が安くなる",
    "IsAffectedByTrap": "罠の影響を受けやすい",
    "AutoIdentify": "拾ったアイテムを自動鑑定",
    "RandomTeleport": "ランダムにワープする",
    "RandomExplosion": "ランダムに爆発する",
    "BookMaster": "魔法書の効果が強化",
    "WandMaster": "杖の効果が強化",
    "PotionMaster": "ポーションの効果が強化",
    "CurseIdentify": "拾った時点で呪い状態も判明",
    "AdjacentAttackGuard": "隣接マスからのダメージ半減",
    "FullHpCritical": "HP満タン時の攻撃が必ずクリティカル",
    "StealEmpower": "盗むたび攻撃力が上昇",
}


def pct(value):
    return f"{float(value) * 100:.0f}%"


def decode_unicode_name(raw: str) -> str:
    name = raw.strip().strip('"')
    if "\\u" in name:
        return name.encode("ascii").decode("unicode_escape")
    return name


def decode_asset_name(raw: str) -> str:
    return decode_unicode_name(raw)


def parse_refs(text: str) -> dict:
    refs = {}
    ref_section = text.split("references:", 1)
    if len(ref_section) < 2:
        return refs
    body = ref_section[1]
    if "RefIds:" not in body:
        return refs
    body = body.split("RefIds:", 1)[1]
    current_rid = None
    current_class = None
    data_lines = []
    in_data = False

    def flush():
        nonlocal current_rid, current_class, data_lines, in_data
        if current_rid is None:
            return
        data_text = "\n".join(data_lines).rstrip()
        refs[current_rid] = {"class": current_class, "data": data_text}
        current_rid = None
        current_class = None
        data_lines = []
        in_data = False

    for line in body.splitlines():
        m = re.match(r"\s*- rid: (\d+)", line)
        if m:
            flush()
            current_rid = m.group(1)
            continue
        m = re.match(r"\s+type: \{class: (\w+)", line)
        if m and current_rid:
            current_class = m.group(1)
            continue
        if re.match(r"\s+data:\s*$", line) and current_rid:
            in_data = True
            continue
        if in_data:
            if re.match(r"\s+- rid: \d+", line):
                flush()
                m = re.match(r"\s+- rid: (\d+)", line)
                current_rid = m.group(1)
                continue
            data_lines.append(line)
    flush()
    return refs


def ref_data_dict(data_text: str) -> dict:
    result = {}
    for line in data_text.splitlines():
        m = re.match(r"\s+(\w+):\s*(.*)$", line)
        if not m:
            continue
        key, val = m.group(1), m.group(2).strip()
        if val == "":
            continue
        if val in ("0", "1") and key.startswith(("Apply", "Contains", "Can", "Is", "Has")):
            result[key] = val == "1"
        else:
            try:
                if "." in val:
                    result[key] = float(val)
                else:
                    result[key] = int(val)
            except ValueError:
                result[key] = decode_unicode_name(val.strip('"'))
    return result


def get_ref(refs, rid):
    if rid is None or rid == "-2":
        return None
    return refs.get(str(rid))


def parse_skill_block(text: str, prefix: str) -> dict | None:
    pattern = rf"{re.escape(prefix)}:\n((?:    .*\n)*)"
    m = re.search(pattern, text)
    if not m:
        return None
    block = m.group(1)
    pos = re.search(r"<Position>k__BackingField:\n\s+rid: (\d+|-2)", block)
    area = re.search(r"<Area>k__BackingField:\n\s+rid: (\d+|-2)", block)
    effects = re.findall(r"<Effects>k__BackingField:\n(?:\s+- rid: (\d+|-2)\n)+", block)
    effect_rids = re.findall(r"- rid: (\d+|-2)", block.split("<Effects>k__BackingField:")[1].split("<Repeats>")[0]) if "<Effects>k__BackingField:" in block else []
    skill = {
        "position_rid": pos.group(1) if pos else None,
        "area_rid": area.group(1) if area else None,
        "effect_rids": [r for r in effect_rids if r != "-2"],
    }
    for key, cast in [
        ("repeats", "Repeats"),
        ("probability", "ProbabilityOfSuccess"),
        ("cost", "Cost"),
        ("rush", "RushDistance"),
        ("backstep", "BackStepDistance"),
        ("charge", "ChargeTurn"),
        ("cooltime", "CoolTime"),
    ]:
        m2 = re.search(rf"<{cast}>k__BackingField: ([\d.]+)", block)
        if m2:
            skill[key] = float(m2.group(1))
    return skill


def area_target_text(area_ref, position_ref, context: str) -> str:
    if not area_ref:
        return "対象"
    cls = area_ref["class"]
    data = ref_data_dict(area_ref.get("data", ""))
    pos_cls = position_ref["class"] if position_ref else None
    if pos_cls == "ProjectileImpact":
        data_pos = ref_data_dict(position_ref.get("data", "")) if position_ref else {}
        if data_pos.get("IsPiercing"):
            if cls == "CircleArea":
                return f"射線上の各対象とその周囲{int(data.get('Radius', 0))}マス"
            return "射線上の対象すべて"
        if cls == "CircleArea":
            return f"命中地点とその周囲{int(data.get('Radius', 0))}マス"
        return "命中地点"
    if pos_cls == "NearByCharacter":
        return "近くの敵"
    if context == "投擲":
        return "投擲先"
    if cls == "SelfArea":
        return "その場"
    if cls == "LineArea":
        return f"前方{int(data.get('Length', 1))}マス"
    if cls == "FanArea":
        return f"前{int(data.get('Radius', 1))}マス（扇形）"
    if cls == "CircleArea":
        radius = int(data.get("Radius", 1))
        ignore = data.get("CanIgnoreWalls", False)
        contains_self = data.get("ContainsSelf", True)
        if radius >= 8 and not contains_self and ignore:
            return f"マップ全体（半径{radius}マス・壁越し）"
        return f"周囲{radius}マス"
    return cls


def position_prefix(position_ref, context: str) -> str:
    if not position_ref:
        return ""
    cls = position_ref["class"]
    data = ref_data_dict(position_ref.get("data", ""))
    if cls == "ProjectileImpact":
        if data.get("IsPiercing"):
            return f"射程{THROW_DISTANCE}マスの貫通攻撃を放ち、"
        return f"射程{THROW_DISTANCE}マスの攻撃を放ち、"
    if cls == "NearByCharacter":
        return "曲射で近くの敵を狙い、"
    return ""


def describe_attack(data_text: str) -> str:
    data = ref_data_dict(data_text)
    powers = []
    for m in re.finditer(r"_element: (\d+)\n\s+_power: (\d+)", data_text):
        powers.append(f"{ELEMENT_NAMES.get(int(m.group(1)), m.group(1))}{m.group(2)}")
    attack = f"攻撃[{ '/'.join(powers) if powers else '?' }]"
    crit = data.get("_criticalRate", 0)
    if crit and float(crit) > 0:
        attack += f"（クリティカル{pct(crit)}）"
    return attack


def describe_absorb(data_text: str) -> str:
    data = ref_data_dict(data_text)
    powers = []
    for m in re.finditer(r"_element: (\d+)\n\s+_power: (\d+)", data_text):
        powers.append(f"{ELEMENT_NAMES.get(int(m.group(1)), m.group(1))}{m.group(2)}")
    rate = data.get("_rate", 1)
    return f"攻撃[{'/'.join(powers) if powers else '?'}]（与ダメの{pct(rate)}吸収）"


def describe_effect(ref, refs, depth=0) -> list[str]:
    if not ref or depth > 4:
        return []
    cls = ref["class"]
    data_text = ref.get("data", "")
    data = ref_data_dict(data_text)
    lines = []
    if cls == "AttackEffect":
        lines.append(describe_attack(data_text))
    elif cls == "HealEffect":
        lines.append(f"{int(data.get('_power', 0))}HP回復")
    elif cls == "AddConditionEffect":
        cond = re.search(r'_name: "?([^"\n]+)"?', data_text)
        name = decode_unicode_name(cond.group(1)) if cond else "状態"
        prob = data.get("_probabilityOfSuccess", 1)
        lines.append(f"[{name}]を付与（{pct(prob)}）")
    elif cls == "AbsorbsEffect":
        lines.append(describe_absorb(data_text))
    elif cls == "BlowAwayEffect":
        lines.append(f"{int(data.get('_distance', 1))}マス吹き飛ばす")
    elif cls == "BreakEffect":
        targets = []
        for key, label in [
            ("ApplyToCharacter", "キャラクター"),
            ("ApplyToItem", "アイテム"),
            ("ApplyToMoney", "お金"),
            ("ApplyToTrap", "罠"),
            ("ApplyToChest", "宝箱"),
            ("ApplyToStatue", "石像"),
        ]:
            if data.get(key):
                targets.append(label)
        lines.append(f"{'・'.join(targets) if targets else '対象'}を破壊")
    elif cls == "DigEffect":
        lines.append("壁を掘る")
    elif cls == "TeleportEffect":
        lines.append("対象をテレポート")
    elif cls == "TeleportToAreaEffect":
        lines.append("対象に向かってテレポート")
    elif cls == "ClearConditionEffect":
        lines.append("全状態異常を解除")
    elif cls == "ForgetEffect":
        lines.append("知識を消去")
    elif cls == "CurseItemEffect":
        lines.append("対象の持つアイテムに呪い付与")
    elif cls == "RemoveUpgradeEffect":
        lines.append("対象の持つアイテムの強化を解除")
    elif cls == "DropItemEffect":
        lines.append("対象の持つアイテムを落とさせる")
    elif cls == "SpawnFireEffect":
        lines.append("炎を生成")
    elif cls == "SpawnIceEffect":
        lines.append("水上なら氷を生成")
    elif cls == "SpawnTrapEffect":
        trap = re.search(r'_name: "?([^"\n]+)"?', data_text)
        trap_name = decode_unicode_name(trap.group(1)) if trap else "罠"
        count = int(data.get("_count", 1))
        lines.append(f"{trap_name}を{count}個設置")
    elif cls == "SpawnCharacterEffect":
        char = re.search(r'Name: "?([^"\n]+)"?', data_text)
        char_name = decode_unicode_name(char.group(1)) if char else "キャラ"
        count = int(data.get("_count", 1))
        lines.append(f"{char_name}を{count}体召喚")
    elif cls == "SpawnRandomCharacterEffect":
        count = int(data.get("_count", 1))
        lines.append(f"ランダムに{count}体召喚")
    elif cls == "PercentageDamageEffect":
        rate = data.get("_damageRate", 0)
        lines.append(f"最大HPの{pct(rate)}の爆発ダメージ")
    elif cls == "AlertEffect":
        lines.append("警報を鳴らす")
    elif cls == "AffectionIncreaseEffect":
        lines.append(f"好感度を{int(data.get('_power', 0))}上昇")
    elif cls == "RandomEffect":
        lines.append("ランダム効果（以下のいずれか）")
        for rid_m in re.finditer(r"- rid: (\d+)", data_text):
            sub = get_ref(refs, rid_m.group(1))
            for sub_line in describe_effect(sub, refs, depth + 1):
                lines.append(f"  - {sub_line}")
    else:
        lines.append(cls)
    return lines


def describe_item_effect(ref, refs) -> str:
    if not ref:
        return "（効果なし）"
    cls = ref["class"]
    data_text = ref.get("data", "")
    mapping = {
        "Repair": "対象アイテムの使用回数を回復（修理）",
        "Identify": "対象アイテムを識別（名前・効果・呪い状態を判明）",
        "UpgradeItem": "対象アイテムを強化",
        "CurseItem": "対象アイテムに呪いを付与",
        "UnleashCurse": "対象アイテムの呪いを解く",
        "DuplicateItem": "対象アイテムを複製",
    }
    if cls in mapping:
        return mapping[cls]
    if cls == "ChangeItem":
        item = re.search(r'_name: "?([^"\n]+)"?', data_text)
        target = decode_unicode_name(item.group(1)) if item else "?"
        return f"対象アイテムを「{target}」に変化"
    return cls


def describe_inventory_effect(ref, refs) -> str:
    if not ref:
        return "（効果なし）"
    cls = ref["class"]
    data_text = ref.get("data", "")
    mapping = {
        "RepairAll": "所持アイテムすべての使用回数を回復",
        "CurseAll": "所持アイテムすべてに呪いを付与",
    }
    if cls in mapping:
        return mapping[cls]
    if cls == "ChangeItemAll":
        item = re.search(r'_name: "?([^"\n]+)"?', data_text)
        target = decode_unicode_name(item.group(1)) if item else "?"
        return f"所持アイテムすべてを「{target}」に変化"
    return cls


def describe_condition(ref) -> str:
    if not ref:
        return ""
    cls = ref["class"]
    data_text = ref.get("data", "")
    data = ref_data_dict(data_text)
    if cls == "FlagCondition":
        flag = re.search(r"stringValue: (\w+)", data_text)
        key = flag.group(1) if flag else ""
        name = FLAG_NAMES.get(key, key)
        desc = FLAG_DESC.get(key, "常時効果")
        return f"{name}（{desc}）"
    if cls == "NaturalHeal":
        return f"自然治癒（ワールドターンごとにHP+{data.get('Power', 0)}）"
    if cls == "AddMaxHp":
        return f"最大HP+{int(data.get('AddValue', 0))}"
    if cls == "AddAttackMultiplier":
        elem = ELEMENT_NAMES.get(int(data.get("Element", 0)), "?")
        return f"{elem}攻撃倍率+{pct(data.get('AddedMultiplier', 0))}"
    if cls == "AddResistanceMultiplier":
        elem = ELEMENT_NAMES.get(int(data.get("Element", 0)), "?")
        return f"{elem}被ダメージ-{pct(data.get('AddedResistanceMultiplier', 0))}"
    if cls == "AddConditionResistanceMultiplier":
        cond = re.search(r'_name: "?([^"\n]+)"?', data_text)
        name = decode_unicode_name(cond.group(1)) if cond else "状態"
        return f"{name}被付与確率-{pct(data.get('ResistanceRate', 0))}"
    return cls


def describe_spawn_skill(skill, refs, context: str, include_prob: bool = True) -> str:
    if not skill:
        return "（なし）"
    pos = get_ref(refs, skill.get("position_rid"))
    area = get_ref(refs, skill.get("area_rid"))
    target = area_target_text(area, pos, context)
    prefix = position_prefix(pos, context)
    effects = []
    for rid in skill.get("effect_rids", []):
        for line in describe_effect(get_ref(refs, rid), refs):
            effects.append(line)
    if not effects:
        return f"{prefix}{target}（効果なし）"
    body = "／".join(effects)
    repeats = int(skill.get("repeats", 1))
    if repeats > 1:
        body += f"（{repeats}回）"
    prob = skill.get("probability")
    extras = []
    if include_prob and prob is not None:
        extras.append(f"成功率{pct(prob)}")
    if skill.get("cost", 0) > 0:
        extras.append(f"消費HP{int(skill['cost'])}")
    if skill.get("rush", 0) > 0:
        extras.append(f"攻撃前{int(skill['rush'])}マス前進")
    if skill.get("backstep", 0) > 0:
        extras.append(f"攻撃後{int(skill['backstep'])}マス後退")
    charge = skill.get("charge", 0)
    if charge and charge > 0:
        extras.append(f"発動に{int(charge) + 1}ターン")
    if skill.get("cooltime", 0) > 0:
        extras.append(f"再使用まで{int(skill['cooltime'])}ターン")
    extra = f"（{'・'.join(extras)}）" if extras else ""
    if prefix:
        return f"{prefix}{target}に{body}{extra}"
    return f"{target}に{body}{extra}"


def parse_consumable_asset(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    refs = parse_refs(text)
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    name = decode_asset_name(name_m.group(1)) if name_m else path.stem
    category = int(re.search(r"Category: (\d+)", text).group(1)) if re.search(r"Category: (\d+)", text) else 6
    rarity = int(re.search(r"_rarity: (\d+)", text).group(1)) if re.search(r"_rarity: (\d+)", text) else 0
    usage = int(re.search(r"UsageLimit: (\d+)", text).group(1)) if re.search(r"UsageLimit: (\d+)", text) else 0
    effect_type = int(re.search(r"EffectType: (\d+)", text).group(1)) if re.search(r"EffectType: (\d+)", text) else 0
    use_on_death = re.search(r"UseOnDeath: 1", text) is not None
    spawn_use = re.search(r"SpawnEffectsOnUse: 1", text) is not None
    spawn_throw = re.search(r"SpawnEffectsOnThrow: 1", text) is not None
    same_skill = re.search(r"IsSameSkill: 1", text) is not None
    merge_feats = [FEATURE_NAME.get(m.group(1), m.group(1)) for m in re.finditer(r"stringValue: (\w+)", text.split("_featuresToMergeWeapon:")[1].split("references:")[0])] if "_featuresToMergeWeapon:" in text else []
    item_effect_rid = re.search(r"ItemEffect:\n\s+rid: (\d+|-2)", text)
    inventory_effect_rid = re.search(r"InventoryEffect:\n\s+rid: (\d+|-2)", text)
    skill_use = parse_skill_block(text, "SkillOnUse")
    skill_throw = parse_skill_block(text, "SkillOnThrow")
    return {
        "name": name,
        "category": category,
        "rarity": RARITY_NAMES.get(rarity, "?"),
        "usage_limit": usage,
        "effect_type": effect_type,
        "use_on_death": use_on_death,
        "spawn_use": spawn_use,
        "spawn_throw": spawn_throw,
        "same_skill": same_skill,
        "merge_features": merge_feats,
        "item_effect": get_ref(refs, item_effect_rid.group(1)) if item_effect_rid else None,
        "inventory_effect": get_ref(refs, inventory_effect_rid.group(1)) if inventory_effect_rid else None,
        "skill_use": skill_use,
        "skill_throw": skill_throw,
        "refs": refs,
    }


def parse_artifact_asset(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    refs = parse_refs(text)
    name_m = re.search(r'm_Name: "?([^"\n]+)"?', text)
    name = decode_asset_name(name_m.group(1)) if name_m else path.stem
    rarity = int(re.search(r"_rarity: (\d+)", text).group(1)) if re.search(r"_rarity: (\d+)", text) else 0
    synth = int(re.search(r"SynthesisSlotLimit: (\d+)", text).group(1)) if re.search(r"SynthesisSlotLimit: (\d+)", text) else 0
    has_passive = re.search(r"HasBuiltInPassive: 1", text) is not None
    display = re.search(r'_displayName: "?([^"\n]*)"?', text)
    display_name = decode_unicode_name(display.group(1)) if display and display.group(1) else ""
    cond_rids = re.findall(r"BuiltInPassiveConditionBundle:[\s\S]*?_conditions:\n((?:\s+- rid: \d+\n)+)", text)
    conditions = []
    if cond_rids:
        for rid in re.findall(r"- rid: (\d+)", cond_rids[0]):
            desc = describe_condition(get_ref(refs, rid))
            if desc:
                conditions.append(desc)
    return {
        "name": name,
        "rarity": RARITY_NAMES.get(rarity, "?"),
        "synthesis_slots": synth,
        "display_name": display_name,
        "conditions": conditions,
        "has_passive": has_passive,
    }


def format_consumable_entry(item: dict) -> list[str]:
    lines = []
    cat = CATEGORY_NAMES.get(item["category"], "その他")
    lines.append(f"##### {item['name']}")
    lines.append("")
    lines.append(f"- **カテゴリ**: {cat}")
    lines.append(f"- **レア度**: {item['rarity']}")
    if item["usage_limit"] > 0:
        lines.append(f"- **使用回数**: {item['usage_limit']}")
    else:
        lines.append("- **使用回数**: 無限（使用不可アイテム）")
    if cat in ("本", "巻物"):
        lines.append("- **識字**: 必要（盲目時は読めない）")
    if cat == "杖":
        lines.append("- **識別**: 使用しても杖の種類は判明しない")
    if item["use_on_death"]:
        lines.append("- **死亡時**: 所持していれば死亡時に自動使用")
    effect_type = item["effect_type"]
    if effect_type == 1:
        if item["same_skill"] and item["spawn_use"] and item["spawn_throw"]:
            use_desc = describe_spawn_skill(item["skill_use"], item["refs"], "使用", include_prob=False)
            throw_prob = item["skill_throw"].get("probability") if item["skill_throw"] else None
            use_prob = item["skill_use"].get("probability") if item["skill_use"] else None
            lines.append(f"- **使用/投擲**: {use_desc}")
            lines.append(
                f"  - 成功率: 使用{pct(use_prob) if use_prob is not None else '?'}／投擲{pct(throw_prob) if throw_prob is not None else '?'}"
            )
        else:
            if item["spawn_use"]:
                lines.append(f"- **使用**: {describe_spawn_skill(item['skill_use'], item['refs'], '使用')}")
            if item["spawn_throw"]:
                lines.append(f"- **投擲**: {describe_spawn_skill(item['skill_throw'], item['refs'], '投擲')}")
            elif item["spawn_use"]:
                lines.append("- **投擲**: 不可")
    elif effect_type == 2:
        lines.append(f"- **使用**: 所持/足元のアイテムを対象に「{describe_item_effect(item['item_effect'], item['refs'])}」")
        lines.append("- **投擲**: 不可")
    elif effect_type == 3:
        lines.append(f"- **使用**: {describe_inventory_effect(item['inventory_effect'], item['refs'])}")
        lines.append("- **投擲**: 不可")
    elif effect_type == 0:
        lines.append("- **使用**: 不可（換金・素材など）")
        lines.append("- **投擲**: 不可")
    if item["merge_features"]:
        lines.append(f"- **武器合成**: {'、'.join(item['merge_features'])} を付与可能")
    else:
        lines.append("- **武器合成**: 不可")
    lines.append("")
    return lines


def format_artifact_entry(item: dict) -> list[str]:
    lines = []
    lines.append(f"##### {item['name']}")
    lines.append("")
    lines.append("- **カテゴリ**: アーティファクト（指輪）")
    lines.append(f"- **レア度**: {item['rarity']}")
    lines.append("- **使用**: 不可（装備中の常時効果のみ）")
    if item["conditions"]:
        passive = "／".join(item["conditions"])
        if item["display_name"]:
            passive = f"{item['display_name']} — {passive}"
        lines.append(f"- **常時効果**: {passive}")
    else:
        lines.append("- **常時効果**: なし（合成用ベース）")
    if item["synthesis_slots"] > 0:
        lines.append(f"- **合成枠**: {item['synthesis_slots']}（他アーティファクトの効果を合成可能）")
    else:
        lines.append("- **合成枠**: 0")
    lines.append("")
    return lines


def load_catalog_items():
    item_dir = db / "ItemData"
    consumables = []
    artifacts = []
    skip = {"ItemMarketPriceTable.asset", "Placeholders.asset"}
    for asset in sorted(item_dir.rglob("*.asset")):
        if asset.name in skip or "武器" in asset.as_posix():
            continue
        text = asset.read_text(encoding="utf-8")
        if "ArtifactData" in text or "guid: 8d05bbe6caa05a74e8a0d30dc3973d2c" in text:
            artifacts.append(parse_artifact_asset(asset))
        else:
            consumables.append(parse_consumable_asset(asset))
    return consumables, artifacts


def append_catalog_sections(lines: list[str], consumables: list[dict], artifacts: list[dict]):
    rarity_order = {"Common": 0, "Uncommon": 1, "Rare": 2, "Epic": 3, "Legendary": 4}
    sections = [
        ("薬一覧", 0),
        ("巻物一覧", 1),
        ("本一覧", 2),
        ("杖一覧", 3),
        ("その他アイテム一覧", 6),
    ]
    lines.append("### 全アイテム詳細")
    lines.append("")
    lines.append("各アイテムの効果・特徴。データは `Assets/Database/ItemData/` の ScriptableObject 定義に基づく。")
    lines.append("")
    for title, cat_id in sections:
        items = [i for i in consumables if i["category"] == cat_id]
        items.sort(key=lambda x: (rarity_order.get(x["rarity"], 99), x["name"]))
        lines.append(f"#### {title}")
        lines.append("")
        if not items:
            lines.append("（なし）")
            lines.append("")
            continue
        for item in items:
            lines.extend(format_consumable_entry(item))
    lines.append("#### 指輪（アーティファクト）一覧")
    lines.append("")
    artifacts.sort(key=lambda x: (rarity_order.get(x["rarity"], 99), x["name"]))
    for item in artifacts:
        lines.extend(format_artifact_entry(item))


weapon_dir = db / "ItemData/武器"


def main():
    weapons = []

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
    lines.append("- **宝箱**: 宝箱の中身（`AllChestItems` = `ChestItems` + 宝箱用武器・指輪プール）")
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

    consumables, artifacts = load_catalog_items()
    append_catalog_sections(lines, consumables, artifacts)

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
    lines.append("- **付与タイミング**: モンスターハウスの宝箱のみ。`WeaponPrefixes` から1つ抽選される。")
    lines.append("- **対象**: 近接武器・射撃武器。通常宝箱・通常ドロップ、店売り、ボス固定報酬、シャイニー敵の所持武器には通常付かない。")
    lines.append("- **呪い付き**: 接頭辞の `IsCursed` が有効な場合、その接頭辞が付いた武器は生成時に呪われる。")
    lines.append("- **表示名**: `接頭辞名 + 武器名` で表示される（例: `鋭いロングソード`）。")
    lines.append("")
    prefix_roles = {
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
        role = prefix_roles.get(name, "")
        lines.append(
            f"| {name} | {pm.group(1) if pm else '?'} | {fl.group(1) if fl else '?'} | {um.group(1) if um else '?'} | {au.group(1) if au else '?'} | {cursed_text} | {role} |"
        )
    lines.append("")

    lines.append("---")
    lines.append("")

    out = "\n".join(lines)
    (root / "tools/wiki_item_data_section.md").write_text(out, encoding="utf-8")

    wiki_path = root / "WIKI.md"
    wiki_text = wiki_path.read_text(encoding="utf-8")
    start = wiki_text.index("## アイテムデータ")
    if "## 敵データ" in wiki_text:
        end = wiki_text.index("## 敵データ")
    else:
        end = wiki_text.index("## 未確定")
    wiki_path.write_text(wiki_text[:start] + out + wiki_text[end:], encoding="utf-8")
    print(f"Wrote {len(lines)} lines to wiki section and WIKI.md")


if __name__ == "__main__":
    main()
