#nullable enable
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using Utilities;

namespace Provider.Presentations.Translation
{
    public sealed class PlayerEntity
    {
        private Id<IEntity> _id = Id<IEntity>.Empty;

        internal void MapEntered(Id<IEntity> player)
        {
            _id = player;
        }

        internal bool Is(EntityRef entity) => entity.Id == _id;
    }
}
