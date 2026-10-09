#nullable enable
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record AddCharacter(EntityKey Key, Vector2Int Position, bool IsShiny, bool IsVisible, CharacterSpec Spec) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.AddCharacter(Key, Position, IsShiny, IsVisible, Spec);
        }
    }
}
