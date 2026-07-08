#nullable enable
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using Utilities;
using View.Playback;

namespace Provider.Presentations
{
    internal static class EntityKeys
    {
        public static EntityKey Key(this EntityRef entity) => entity.Id.Key();

        public static EntityKey Key(this Id<IEntity> id) => new(id.Value);
    }
}
