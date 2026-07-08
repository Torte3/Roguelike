#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    public static class MapExtensions
    {
        public static bool IsKeyHolder(this IMap map, Id<IEntity> id)
        {
            return map.LockedEntities.Any(locked => locked.IsKeyHolder(id));
        }

        public static IEnumerable<ICharacter> GetVisibleCharacters(this IMap map, IHasBehavior character)
        {
            return map.Characters.Where(c => character.IsVisible(c.Entity.CurrentPosition)).Where(c => c != character);
        }

        public static IEnumerable<ICharacter> GetCharactersCanSeePosition(this IMap map, Vector2Int position)
        {
            return map.Characters.Where(character => character.IsVisible(position));
        }

        public static ICharacter? GetCharacter(this IMap map, Id<IEntity> id)
        {
            return map.Characters.FirstOrDefault(character => character.Affiliation.Id == id);
        }
    }
}