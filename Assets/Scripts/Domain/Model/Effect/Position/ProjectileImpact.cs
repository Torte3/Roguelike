using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Model.Effect.Position
{
    public class ProjectileImpact : IActorlessEffectPosition
    {
        [Required] public IconSerializable Icon;
        public List<EntityLayer> CanHitLayer = new() { EntityLayer.Middle };
        public bool IsDirectional => true;
        public bool IsPiercing;

        public ProjectileImpact(IconSerializable icon, List<EntityLayer> canHitLayer, bool isPiercing)
        {
            Icon = icon;
            CanHitLayer = canHitLayer;
            IsPiercing = isPiercing;
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction,
            IMap map)
        {
            if (IsPiercing)
            {
                return map.GetThrowDestinationPiercing(position, direction, CommonSenseParameters.ThrowDistance, CanHitLayer.ToArray());
            }
            else
            {
                return new[] { map.GetThrowDestination(position, direction, CommonSenseParameters.ThrowDistance, CanHitLayer.ToArray()) };
            }
        }

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            return Get(position, direction, map);
        }

        public float EvaluateHitProbability()
        {
            return CommonSenseParameters.ProjectileImpactHitProbability;
        }

        public string Info()
        {
            return "着弾地点" + (IsPiercing ? "（貫通）" : "");
        }
    }
}