#!/usr/bin/env python3
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ITEM_ROOT = ROOT / "Assets/Database/ItemData"

SHOPS = {
    "yorozu": (35, [
        "ポーション/回復のポーション", "ポーション/再生のポーション", "ポーション/癒しのポーション",
        "ポーション/防護のポーション", "ポーション/加速のポーション",
        "巻物/吹き飛ばしの巻物", "巻物/拘束の巻物", "巻物/混乱の巻物", "巻物/鈍足の巻物",
        "本/識別の魔法書", "杖/回復の杖", "杖/テレポートの杖", "杖/吹き飛ばしの杖",
    ]),
    "kusuri": (20, [
        "ポーション/回復のポーション", "ポーション/再生のポーション", "ポーション/毒のポーション",
        "ポーション/麻痺のポーション", "ポーション/睡眠のポーション", "ポーション/癒しのポーション",
        "ポーション/防護のポーション", "ポーション/加速のポーション", "ポーション/千里眼のポーション",
        "ポーション/忘却のポーション",
    ]),
    "makimono": (15, [
        "巻物/吹き飛ばしの巻物", "巻物/拘束の巻物", "巻物/混乱の巻物", "巻物/盲目の巻物",
        "巻物/鈍足の巻物", "巻物/散り散りの巻物", "巻物/壁破壊の巻物", "巻物/トラップ破壊の巻物",
        "巻物/修理の巻物", "巻物/強化の巻物", "巻物/解呪の巻物", "巻物/隕石の巻物",
    ]),
    "mahou": (10, [
        "本/回復の魔法書", "本/暗黒の魔法書", "本/氷結の魔法書", "本/火炎の魔法書",
        "本/閃光の魔法書", "本/障壁の魔法書", "本/電撃の魔法書", "本/識別の魔法書", "本/解呪の魔法書",
    ]),
    "tsue": (10, [
        "杖/テレポートの杖", "杖/トラップ破壊の杖", "杖/トンネルの杖", "杖/吹き飛ばしの杖",
        "杖/回復の杖", "杖/飛びつきの杖", "杖/加速の杖", "杖/鈍足の杖", "杖/ふしぎな杖",
        "杖/支配の杖", "杖/盾の杖",
    ]),
    "koubou": (10, [
        "巻物/修理の巻物", "巻物/強化の巻物", "巻物/解呪の巻物", "巻物/トラップ破壊の巻物",
        "巻物/壁破壊の巻物", "杖/トラップ破壊の杖", "杖/トンネルの杖",
        "武器/DirectWeapon/魔法のほうき", "武器/DirectWeapon/シャベル", "武器/DirectWeapon/こんぼう",
    ]),
}


def guid_for(rel: str) -> str:
    meta = ITEM_ROOT / f"{rel}.asset.meta"
    text = meta.read_text(encoding="utf-8")
    m = re.search(r"^guid: (.+)$", text, re.M)
    if not m:
        raise SystemExit(f"no guid: {meta}")
    return m.group(1)


def shop_yaml(weight: int, paths: list[str]) -> str:
    lines = ["      - Item:", "          Items:", "            items:"]
    for p in paths:
        g = guid_for(p)
        lines.append(f"            - {{fileID: 11400000, guid: {g}, type: 2}}")
    lines.append(f"        Weight: {weight}")
    return "\n".join(lines)


if __name__ == "__main__":
    out = ["    ShopItems:", "      items:"]
    for name, (weight, paths) in SHOPS.items():
        out.append(shop_yaml(weight, paths))
    print("\n".join(out))
