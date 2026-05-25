#nullable enable
using UnityEngine;

namespace View.UI
{
    public record ItemViewData(
        string name,
        bool isUsable,
        Sprite icon,
        bool canSelect,
        int? count,
        bool showEquippedBadge,
        bool isCursed,
        bool isShiny,
        bool isCountIdentified,
        bool isCurseIdentified,
        string info
    );
}