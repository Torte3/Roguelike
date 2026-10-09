#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public static class GameEnumerableExtension
    {
        public static IEnumerable<IMapPosition> In(this IEnumerable<IMapPosition> ie, IEnumerable<Vector2Int> area)
        {
            return ie.Where(position => area.Contains(position.Position));
        }

        public static IEnumerable<Vector2Int> Values(this IEnumerable<IMapPosition> ie)
        {
            return ie.Select(position => position.Position);
        }

        public static IEnumerable<T> In<T>(this IEnumerable<T> ie, IEnumerable<Vector2Int> area) where T : IEntity
        {
            return ie.Where(item => area.Contains(item.Entity.CurrentPosition));
        }

        public static IEnumerable<T> On<T>(this IEnumerable<T> ie, params EntityLayer[] layers) where T : IEntity
        {
            return ie.Where(item => layers.Contains(item.Entity.Layer));
        }

        public static IEnumerable<T> At<T>(this IEnumerable<T> ie, Vector2Int position) where T : IEntity
        {
            return ie.Where(item => item.Entity.CurrentPosition == position);
        }

        public static IEnumerable<T> ByAffiliation<T>(this IEnumerable<T> ie, IHasAffiliation viewer,
            AffiliationType type) where T : ICharacter
        {
            return ie.Where(item => viewer.Affiliation.GetAffiliationType(item.Affiliation) == type);
        }

        public static T? ById<T>(this IEnumerable<T> ie, Id<IEntity> id) where T : IEntity
        {
            return ie.FirstOrDefault(item => item.Entity.Id == id);
        }

        public static IItemEntity? ById(this IEnumerable<IItemEntity> ie, Id<IItem> id)
        {
            return ie.FirstOrDefault(item => item.Item.Id == id);
        }

        public static IItem? ById(this IEnumerable<IItem> ie, Id<IItem> id)
        {
            return ie.FirstOrDefault(item => item.Id == id);
        }

        public static IEnumerable<T> IsVisible<T>(this IEnumerable<T> ie, Vector2Int position) where T : ICharacter
        {
            return ie.Where(item => item.IsVisible(position));
        }

        public static IEnumerable<Vector2Int> Positions<T>(this IEnumerable<T> ie) where T : IEntity
        {
            return ie.Select(item => item.Entity.CurrentPosition);
        }
    }
}