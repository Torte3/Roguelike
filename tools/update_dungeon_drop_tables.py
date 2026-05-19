"""Update Dungeon.asset MasterItemDataBase drop pools safely.

- Rebuilds only the MasterItemDataBase section (SpawnItem / Traps / etc. are preserved verbatim).
- ShopItems block is copied unchanged from the file before editing.
- Never uses items:[] (breaks Odin RequiredListLength and Unity parsing).
- Floor Artifacts list is left as-is (SpawnItem.Artifacts=0 so rings do not drop on floor).
"""
import re
from pathlib import Path

root = Path(__file__).resolve().parents[1]
item_root = root / "Assets/Database/ItemData"
dungeon_path = root / "Assets/Database/DungeonBluePrintData/Dungeon.asset"

SCRIPT_ITEM = "42c9d6e8f38cdc547ad116f2420ef50e"
SCRIPT_DIRECT = "840a56d02e5d5f6418762374474908e9"
SCRIPT_RANGED = "834d363c1c9b2414b93fe96509c32721"
SCRIPT_ARTIFACT = "8d05bbe6caa05a74e8a0d30dc3973d2c"

EXCLUDE_NAMES = {"極光の魔法書"}
FUKKATSUSO_GUID = "3686947615f6f3a4092e428ed051b93f"


def asset_guid(asset_path: Path) -> str | None:
    meta = asset_path.with_suffix(asset_path.suffix + ".meta")
    if not meta.exists():
        return None
    m = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", meta.read_text(encoding="utf-8"), re.M)
    return m.group(1) if m else None


def script_guid(asset_path: Path) -> str | None:
    text = asset_path.read_text(encoding="utf-8")
    m = re.search(r"m_Script: \{fileID: 11500000, guid: ([0-9a-f]{32}), type: 3\}", text)
    return m.group(1) if m else None


def classify(asset_path: Path, sg: str) -> str | None:
    rel = asset_path.relative_to(item_root)
    if "指輪" in rel.parts:
        return "Artifacts"
    name = asset_path.stem
    if name in EXCLUDE_NAMES:
        return None
    if sg == SCRIPT_DIRECT:
        return "DirectWeapons"
    if sg == SCRIPT_RANGED:
        return "RangedWeapons"
    if sg == SCRIPT_ARTIFACT:
        return "Artifacts"
    if sg != SCRIPT_ITEM:
        return None
    if "ポーション" in rel.parts:
        return "Potions"
    if "巻物" in rel.parts:
        return "Scrolls"
    if "本" in rel.parts or "ユニーク" in rel.parts:
        return "Books"
    if "杖" in rel.parts:
        return "Wands"
    return "Others"


def extract_pool_block(text: str, pool_name: str) -> list[str]:
    """Extract YAML lines for one pool (header + items), excluding following pools."""
    pattern = rf"(    {re.escape(pool_name)}:\n(?:      .*\n?)*)"
    m = re.search(pattern, text)
    if not m:
        raise RuntimeError(f"Pool block not found: {pool_name}")
    return m.group(1).rstrip("\n").split("\n")


def build_pool_lines(pool_name: str, guids: list[str]) -> list[str]:
    seen: set[str] = set()
    unique: list[str] = []
    for g in guids:
        if g in seen:
            continue
        seen.add(g)
        unique.append(g)
    lines = [f"    {pool_name}:", "      items:"]
    for g in unique:
        lines.append(f"      - {{fileID: 11400000, guid: {g}, type: 2}}")
    return lines


def collect_pools() -> dict[str, list[str]]:
    pools: dict[str, list[str]] = {
        "Potions": [],
        "Scrolls": [],
        "Books": [],
        "Wands": [],
        "DirectWeapons": [],
        "RangedWeapons": [],
        "Artifacts": [],
        "Others": [],
    }
    for asset in sorted(item_root.rglob("*.asset")):
        sg = script_guid(asset)
        if not sg:
            continue
        category = classify(asset, sg)
        if not category:
            continue
        g = asset_guid(asset)
        if not g:
            print(f"SKIP (no meta): {asset}")
            continue
        pools[category].append(g)
    for key in pools:
        pools[key] = sorted(set(pools[key]))
        print(f"{key}: {len(pools[key])}")
    return pools


def main() -> None:
    text = dungeon_path.read_text(encoding="utf-8")

    master_start = text.index("  MasterItemDataBase:\n")
    spawn_start = text.index("\n  SpawnItem:", master_start)
    before = text[:master_start]
    after = text[spawn_start + 1 :]  # keep leading newline on SpawnItem via after starting with "  SpawnItem"

    shop_start = text.index("    ShopItems:\n", master_start)
    if shop_start >= spawn_start:
        raise RuntimeError("ShopItems block not found before SpawnItem")
    shop_block_lines = text[shop_start:spawn_start].rstrip("\n").split("\n")

    artifacts_floor_lines = extract_pool_block(text, "Artifacts")

    pools = collect_pools()

    sections: list[str] = ["  MasterItemDataBase:"]
    for name in ("Potions", "Scrolls", "Books", "Wands", "DirectWeapons", "RangedWeapons"):
        sections.extend(build_pool_lines(name, pools[name]))
        sections.append("")
    sections.extend(artifacts_floor_lines)
    sections.append("")
    sections.extend(build_pool_lines("Others", pools["Others"]))
    sections.append("")
    sections.extend(build_pool_lines("ChestItems", [FUKKATSUSO_GUID]))
    sections.append("")
    sections.extend(build_pool_lines("ChestDirectWeapons", pools["DirectWeapons"]))
    sections.append("")
    sections.extend(build_pool_lines("ChestRangedWeapons", pools["RangedWeapons"]))
    sections.append("")
    sections.extend(build_pool_lines("ChestArtifacts", pools["Artifacts"]))
    sections.append("")
    sections.extend(shop_block_lines)

    new_text = before + "\n".join(sections) + "\n" + after
    dungeon_path.write_text(new_text, encoding="utf-8")
    print(f"Updated {dungeon_path}")
    print("Preserved: ShopItems, SpawnItem, Traps, Statues, WeaponPrefixes, Npcs")


if __name__ == "__main__":
    main()
