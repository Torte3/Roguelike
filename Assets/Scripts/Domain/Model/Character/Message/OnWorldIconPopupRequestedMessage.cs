#nullable enable
using UnityEngine;

namespace Domain.Model.Character.Message
{
    public record OnWorldIconPopupRequestedMessage(Sprite Icon, Vector2Int Position);
}
