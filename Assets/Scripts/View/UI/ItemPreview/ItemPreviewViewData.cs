#nullable enable
using UnityEngine;

namespace View.UI.ItemPreview
{
    public record ItemPreviewViewData(
        string name,
        Sprite icon,
        int? count,
        bool showEquippedBadge,
        bool isCursed,
        bool isShiny,
        bool isCountIdentified,
        bool isCurseIdentified,
        string info
    );
}
