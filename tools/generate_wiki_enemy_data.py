"""Generate 敵データ section for WIKI.md"""
from __future__ import annotations

import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from generate_wiki_item_data import (  # noqa: E402
    FLAG_DESC,
    FLAG_NAMES,
    db,
    decode_asset_name,
    decode_unicode_name,
    describe_spawn_skill,
    get_ref,
    parse_refs,
    parse_skill_block,
    pct,
    ref_data_dict,
    root,
)

ENEMY_DIR = db / "EnemyData"
TABLE_DIR = db / "DungeonBluePrintData/EnemyTable"
MAPGRAPH_PATH = db / "DungeonBluePrintData/MapGraph.asset"

GROUP_NAMES = {0: "人間", 1: "モンスター", 2: "中立", 3: "追放者"}
AGGRESSION_NAMES = {
    0: "誰にでも攻撃",
    1: "味方を避ける",
    2: "味方には攻撃しない",
    3: "中立を避ける",
    4: "中立には攻撃しない",
    5: "攻撃しない",
}
MOVE_SPEED_NAMES = {0: "1/4", 1: "1/2", 2: "通常", 3: "2倍", 4: "4倍"}
MOVE_TYPE_NAMES = {0: "動かない", 1: "徘徊", 2: "追跡", 3: "逃走"}

FLAG_STAT_ORDER = [
    "CannotAct",
    "CannotMove",
    "Confused",
    "Clairvoyant",
    "Blind",
    "NarrowVision",
    "OverDrive",
    "AllConditionProof",
    "Hard",
    "ExplosionProof",
    "Heavy",
    "SecureHold",
    "CurseProof",
    "Negotiator",
    "IsAffectedByTrap",
    "AutoIdentify",
    "RandomTeleport",
    "RandomExplosion",
    "BookMaster",
    "WandMaster",
    "PotionMaster",
    "CurseIdentify",
    "AdjacentAttackGuard",
    "FullHpCritical",
    "StealEmpower",
    "KillHeal",
]

TABLE_LABELS = {
    "Tutorial": "Tutorial",
    "Cave": "洞窟",
    "Forest": "森",
    "Snow": "雪山",
    "Dungeon": "Dungeon",
    "Desert": "砂漠",
    "Volcano": "火山",
    "Infinite": "Infinite",
}


def build_guid_map(folder_name: str) -> dict[str, str]:
    guid_to_name: dict[str, str] = {}
    for meta in db.rglob("*.meta"):
        if folder_name not in str(meta):
            continue
        text = meta.read_text(encoding="utf-8", errors="ignore")
        m = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", text, re.M)
        if m:
            guid_to_name[m.group(1)] = meta.with_suffix("").stem
    return guid_to_name


def guids(text: str) -> list[str]:
    return re.findall(r"guid:\s*([0-9a-f]{32})", text)


def parse_flags(raw: str) -> list[str]:
    raw = raw.strip()
    if not raw:
        return []
    names: list[str] = []
    for i in range(0, len(raw), 8):
        chunk = raw[i : i + 8]
        if len(chunk) < 8:
            continue
        index = int.from_bytes(bytes.fromhex(chunk), "little")
        if index >= len(FLAG_STAT_ORDER):
            continue
        key = FLAG_STAT_ORDER[index]
        label = FLAG_NAMES.get(key, key)
        desc = FLAG_DESC.get(key)
        names.append(f"{label}（{desc}）" if desc else label)
    return names


def parse_inline_skill(block: str) -> dict | None:
    pos = re.search(r"<Position>k__BackingField:\n\s+rid: (\d+|-2)", block)
    area = re.search(r"<Area>k__BackingField:\n\s+rid: (\d+|-2)", block)
    if not pos:
        return None
    effect_rids = []
    if "<Effects>k__BackingField:" in block:
        effect_section = block.split("<Effects>k__BackingField:")[1].split("<Repeats>")[0]
        effect_rids = [r for r in re.findall(r"- rid: (\d+|-2)", effect_section) if r != "-2"]
    skill = {
        "position_rid": pos.group(1),
        "area_rid": area.group(1) if area else None,
        "effect_rids": effect_rids,
    }
    for key, field in [
        ("repeats", "Repeats"),
        ("probability", "ProbabilityOfSuccess"),
        ("cost", "Cost"),
        ("rush", "RushDistance"),
        ("backstep", "BackStepDistance"),
        ("charge", "ChargeTurn"),
        ("cooltime", "CoolTime"),
    ]:
        m = re.search(rf"<{field}>k__BackingField: ([\d.]+)", block)
        if m:
            skill[key] = float(m.group(1))
    log_m = re.search(r'<Log>k__BackingField: "?(.*?)"?\s*$', block, re.M)
    if log_m and log_m.group(1):
        skill["log"] = decode_unicode_name(log_m.group(1))
    return skill


def parse_enemy_skills(text: str) -> list[tuple[int, dict]]:
    skills: list[tuple[int, dict]] = []
    for m in re.finditer(r"- Skill:\n((?:      .*\n)*?)    Priority: (\d+)", text):
        skill = parse_inline_skill(m.group(1))
        if skill:
            skills.append((int(m.group(2)), skill))
    skills.sort(key=lambda x: x[0])
    return [skill for _, skill in skills]


def describe_character_type(text: str, refs: dict) -> str:
    rid_m = re.search(r"CharacterType:\n\s+rid: (\d+)", text)
    if not rid_m:
        return "不明"
    ref = get_ref(refs, rid_m.group(1))
    if not ref:
        return "不明"
    cls = ref["class"]
    data = ref_data_dict(ref.get("data", ""))
    if cls == "Human":
        texture = data.get("TextureName", "")
        return f"人間（{texture}）" if texture else "人間"
    return cls


def describe_behavior(text: str) -> str:
    m = re.search(r"Behavior:\n((?:    .*\n)*?)(?=  MoveSpeed:)", text)
    if not m:
        return "不明"
    block = m.group(1)

    def field_int(name: str, default: int = 0) -> int:
        x = re.search(rf"    {name}: (\d+)", block)
        return int(x.group(1)) if x else default

    def field_bool(name: str) -> bool:
        x = re.search(rf"    {name}: (\d+)", block)
        return bool(x and x.group(1) == "1")

    parts = []
    if field_bool("wanderAround"):
        parts.append("通常時は徘徊")
    default = MOVE_TYPE_NAMES.get(field_int("Default", 2), "?")
    parts.append(f"索敵時の基本行動: {default}")
    if field_bool("ChaseLeader"):
        parts.append("リーダー（プレイヤー）を追跡対象にする")
    if field_bool("PrioritizeEnemiesOverLeaders"):
        parts.append("リーダーより他の敵を優先")
    if field_bool("UseTopBound"):
        dist = re.search(r"distanceTopBound: ([\d.]+)", block)
        top = MOVE_TYPE_NAMES.get(field_int("greaterThanTopBound", 2), "?")
        parts.append(f"距離{dist.group(1) if dist else '?'}マス以上なら{top}")
    if field_bool("UseBottomBound"):
        dist = re.search(r"distanceBottomBound: ([\d.]+)", block)
        bottom = MOVE_TYPE_NAMES.get(field_int("lessThanBottomBound", 2), "?")
        parts.append(f"距離{dist.group(1) if dist else '?'}マス以下なら{bottom}")
    return "／".join(parts)


def parse_dict_multipliers(text: str, field_name: str) -> list[str]:
    m = re.search(rf"{field_name}:\n((?:    .*\n)*?)(?=  [A-Za-z_]|$)", text, re.M)
    if not m or "data: []" in m.group(1):
        return []
    entries = []
    for key, val in re.findall(r"first: (\d+)\n\s+second: ([\d.]+)", m.group(1)):
        from generate_wiki_item_data import ELEMENT_NAMES

        elem = ELEMENT_NAMES.get(int(key), key)
        entries.append(f"{elem}×{val}")
    return entries


def parse_enemy_asset(path: Path, item_guid_to_name: dict[str, str]) -> dict:
    text = path.read_text(encoding="utf-8")
    refs = parse_refs(text)
    name_m = re.search(r"^  Name: \"?(.*?)\"?\s*$", text, re.M)
    name = decode_unicode_name(name_m.group(1)) if name_m else path.stem
    rel = path.relative_to(ENEMY_DIR)
    category = rel.parts[0] if len(rel.parts) > 1 else "その他"

    flags_m = re.search(r"^  Flags: (.*)$", text, re.M)
    flags = parse_flags(flags_m.group(1) if flags_m else "")

    drop_rate_m = re.search(r"DropItemRate: ([\d.]+)", text)
    drop_items = []
    if drop_rate_m and float(drop_rate_m.group(1)) > 0:
        for guid in guids(text.split("DropItemTable:", 1)[1].split("references:", 1)[0]):
            if guid in item_guid_to_name:
                drop_items.append(item_guid_to_name[guid])

    skills = parse_enemy_skills(text)
    last_skill = None
    if re.search(r"HasLastSkill: 1", text):
        last_skill = parse_skill_block(text, "LastSkill")

    return {
        "name": name,
        "category": category,
        "path": str(rel),
        "group": GROUP_NAMES.get(int(re.search(r"Group: (\d+)", text).group(1)), "?"),
        "character_type": describe_character_type(text, refs),
        "is_boss": re.search(r"IsBoss: 1", text) is not None,
        "hp": int(re.search(r"Hp: (\d+)", text).group(1)),
        "aggression": AGGRESSION_NAMES.get(int(re.search(r"Aggression: (\d+)", text).group(1)), "?"),
        "behavior": describe_behavior(text),
        "move_speed": MOVE_SPEED_NAMES.get(int(re.search(r"MoveSpeed: (\d+)", text).group(1)), "?"),
        "flags": flags,
        "is_flying": re.search(r"IsFlying: 1", text) is not None,
        "can_through_walls": re.search(r"CanThroughWalls: 1", text) is not None,
        "can_mimic": re.search(r"CanMimic: 1", text) is not None,
        "can_pick_up": re.search(r"CanPickUp: 1", text) is not None,
        "can_use_item": re.search(r"CanUseItem: 1", text) is not None,
        "can_receive_gift": re.search(r"CanReceivePlayerGift: 1", text) is not None,
        "attack_multiplier": float(re.search(r"AttackMultiplier: ([\d.]+)", text).group(1)),
        "element_attack": parse_dict_multipliers(text, "ElementAttackMultiplier"),
        "element_damage_rate": parse_dict_multipliers(text, "ElementDamageRateMultiplier"),
        "skills": skills,
        "last_skill": last_skill,
        "drop_rate": float(drop_rate_m.group(1)) if drop_rate_m else 0.0,
        "drop_items": drop_items,
        "refs": refs,
    }


def load_enemy_tables(enemy_guid_to_name: dict[str, str]) -> dict[str, set[str]]:
    pools: dict[str, set[str]] = {key: set() for key in TABLE_LABELS}
    for asset in sorted(TABLE_DIR.glob("*.asset")):
        table_name = asset.stem
        if table_name not in pools:
            continue
        for guid in guids(asset.read_text(encoding="utf-8")):
            if guid in enemy_guid_to_name:
                pools[table_name].add(enemy_guid_to_name[guid])
    return pools


def load_boss_pool(enemy_guid_to_name: dict[str, str]) -> set[str]:
    bosses: set[str] = set()
    text = MAPGRAPH_PATH.read_text(encoding="utf-8")
    for block in re.findall(r"  Boss:\n((?:  - .*\n)*)", text):
        for guid in guids(block):
            if guid in enemy_guid_to_name:
                bosses.add(enemy_guid_to_name[guid])
    return bosses


def format_skill_line(skill: dict | None, refs: dict, label: str) -> str | None:
    if not skill:
        return None
    desc = describe_spawn_skill(skill, refs, "攻撃")
    line = f"- **{label}**: {desc}"
    if skill.get("log"):
        line += f"（ログ: {skill['log']}）"
    return line


def format_enemy_entry(enemy: dict) -> list[str]:
    lines = [f"##### {enemy['name']}", ""]
    lines.append(f"- **所属グループ**: {enemy['group']}")
    lines.append(f"- **種別**: {enemy['character_type']}")
    if enemy["is_boss"]:
        lines.append("- **ボス**: はい")
    lines.append(f"- **HP**: {enemy['hp']}")
    lines.append(f"- **攻撃倍率**: {enemy['attack_multiplier']}")
    lines.append(f"- **移動速度**: {enemy['move_speed']}")
    lines.append(f"- **攻撃性**: {enemy['aggression']}")
    lines.append(f"- **行動**: {enemy['behavior']}")

    traits = []
    if enemy["is_flying"]:
        traits.append("飛行")
    if enemy["can_through_walls"]:
        traits.append("壁抜け")
    if enemy["can_mimic"]:
        traits.append("擬態")
    if enemy["can_pick_up"]:
        traits.append("アイテム拾い")
    if enemy["can_use_item"]:
        traits.append("アイテム使用")
    if enemy["can_receive_gift"]:
        traits.append("プレイヤーからの贈り物")
    lines.append(f"- **特性**: {'／'.join(traits) if traits else 'なし'}")

    if enemy["flags"]:
        lines.append(f"- **常時フラグ**: {'／'.join(enemy['flags'])}")
    if enemy["element_attack"]:
        lines.append(f"- **属性攻撃倍率**: {', '.join(enemy['element_attack'])}")
    if enemy["element_damage_rate"]:
        lines.append(f"- **属性被ダメージ倍率**: {', '.join(enemy['element_damage_rate'])}")

    refs = enemy["refs"]
    for index, skill in enumerate(enemy["skills"], start=1):
        line = format_skill_line(skill, refs, f"スキル{index}")
        if line:
            lines.append(line)
    last_line = format_skill_line(enemy["last_skill"], refs, "死亡時スキル")
    if last_line:
        lines.append(last_line)

    if enemy["drop_rate"] > 0:
        drops = "、".join(enemy["drop_items"]) if enemy["drop_items"] else "（テーブル参照）"
        lines.append(f"- **ドロップ**: {pct(enemy['drop_rate'])} — {drops}")

    lines.append("")
    return lines


def category_sort_key(category: str) -> tuple:
    order = [
        "1~5F 洞窟",
        "6~10F 森",
        "11~15F 雪山",
        "16~20F ダンジョン",
        "21~25F 砂漠",
        "26~30F 火山",
        "ボス",
        "NPC",
        "その他",
    ]
    try:
        return (order.index(category), category)
    except ValueError:
        return (99, category)


def main():
    enemy_guid_to_name = build_guid_map("EnemyData")
    item_guid_to_name = build_guid_map("ItemData")

    enemies = [
        parse_enemy_asset(path, item_guid_to_name)
        for path in sorted(ENEMY_DIR.rglob("*.asset"))
    ]
    pools = load_enemy_tables(enemy_guid_to_name)
    boss_pool = load_boss_pool(enemy_guid_to_name)

    all_names = sorted({enemy["name"] for enemy in enemies})
    lines: list[str] = []
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 敵データ")
    lines.append("")
    lines.append(
        "データソース: `Assets/Database/EnemyData/`（敵定義）、"
        "`Assets/Database/DungeonBluePrintData/EnemyTable/`（出現テーブル）、"
        "`MapGraph.asset`（ボス配置）。"
    )
    lines.append("")

    lines.append("### 出現プール一覧（敵）")
    lines.append("")
    lines.append("各敵がどの敵テーブル／ボス配置に登録されているか。○ = 登録あり。")
    lines.append("")
    header = "| 敵 | " + " | ".join(TABLE_LABELS.values()) + " | ボス |"
    sep = "|---|" + "|".join([":---:"] * len(TABLE_LABELS)) + "|:---:|"
    lines.append(header)
    lines.append(sep)
    for name in all_names:
        cols = ["○" if name in pools[key] else "" for key in TABLE_LABELS]
        boss = "○" if name in boss_pool else ""
        lines.append(f"| {name} | {' | '.join(cols)} | {boss} |")
    lines.append("")

    lines.append("### 敵詳細")
    lines.append("")
    lines.append("各敵のステータス・行動・スキル。データは ScriptableObject 定義に基づく。")
    lines.append("")

    by_category: dict[str, list[dict]] = {}
    for enemy in enemies:
        by_category.setdefault(enemy["category"], []).append(enemy)

    for category in sorted(by_category.keys(), key=category_sort_key):
        items = sorted(by_category[category], key=lambda x: x["name"])
        lines.append(f"#### {category}")
        lines.append("")
        for enemy in items:
            lines.extend(format_enemy_entry(enemy))

    lines.append("---")
    lines.append("")

    out = "\n".join(lines)
    (root / "tools/wiki_enemy_data_section.md").write_text(out, encoding="utf-8")

    wiki_path = root / "WIKI.md"
    wiki_text = wiki_path.read_text(encoding="utf-8")

    toc_entry = "14. [敵データ](#敵データ)\n15. [未確定（追記待ち）](#未確定追記待ち)"
    wiki_text = wiki_text.replace(
        "14. [未確定（追記待ち）](#未確定追記待ち)",
        toc_entry,
    )
    wiki_text = wiki_text.replace(
        "   - [武器接頭辞（プレフィックス）](#武器接頭辞プレフィックス)\n14. [未確定（追記待ち）](#未確定追記待ち)",
        "   - [武器接頭辞（プレフィックス）](#武器接頭辞プレフィックス)\n14. [敵データ](#敵データ)\n"
        "   - [出現プール一覧（敵）](#出現プール一覧-1)\n"
        "   - [敵詳細](#敵詳細)\n"
        "15. [未確定（追記待ち）](#未確定追記待ち)",
    )

    insert_point = wiki_text.index("## 未確定（追記待ち）")
    if "## 敵データ" in wiki_text:
        insert_start = wiki_text.index("\n---\n\n## 敵データ")
        wiki_path.write_text(wiki_text[:insert_start] + out + "\n" + wiki_text[insert_point:], encoding="utf-8")
    else:
        insert_point = wiki_text.index("---\n## 未確定（追記待ち）")
        wiki_path.write_text(wiki_text[:insert_point] + out + wiki_text[insert_point:], encoding="utf-8")
    print(f"Wrote {len(lines)} lines to wiki enemy section and WIKI.md")


if __name__ == "__main__":
    main()
