#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record AddEntity(EntityKey Key, string PrefabName, Vector2Int Position, Sprite? Icon, bool IsShiny, bool IsVisible) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Add(Key, PrefabName, Position, Icon, IsShiny, IsVisible);
        }
    }
}
