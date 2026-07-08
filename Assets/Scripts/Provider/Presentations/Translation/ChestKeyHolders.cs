#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Translation
{
    public sealed class ChestKeyHolders
    {
        private readonly Dictionary<Guid, IReadOnlyList<Guid>> _holders = new();

        internal void MapEntered()
        {
            _holders.Clear();
        }

        internal void ChestAppeared(Guid chest, IReadOnlyList<Guid> holders)
        {
            _holders[chest] = holders;
        }

        internal IReadOnlyList<ViewCommand> EntityDisappeared(Guid key)
        {
            if (!_holders.Remove(key, out var holders))
                return Array.Empty<ViewCommand>();

            return holders.Select(holder => (ViewCommand)new SetKeyHolder(holder, false)).ToList();
        }
    }
}
