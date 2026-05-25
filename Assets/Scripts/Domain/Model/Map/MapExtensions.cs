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
        public static IEnumerable<ICharacter> GetVisibleCharacters(this IMap map, IHasBehavior character)
        {
            return map.Characters.Where(c => character.VisionRange.IsVisible(c.Entity.CurrentPosition)).Where(c => c != character);
        }

        public static IEnumerable<ICharacter> GetCharactersCanSeePosition(this IMap map, Vector2Int position)
        {
            return map.Characters.Where(character => character.VisionRange.IsVisible(position));
        }

        public static ICharacter? GetCharacter(this IMap map, Id<IEntity> id)
        {
            return map.Characters.FirstOrDefault(character => character.Affiliation.Id == id);
        }
    }
}