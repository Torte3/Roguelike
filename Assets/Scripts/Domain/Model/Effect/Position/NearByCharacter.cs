using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class NearByCharacter : IEffectPosition
    {
        [MinValue(1)] public int NumberOfTarget = 1;
        public bool TargetAlly;
        public bool TargetEnemy;
        public bool TargetNeutral;
        public bool TargetSelf;
        public bool IsDirectional => false;

        public NearByCharacter(int numberOfTarget, bool targetAlly, bool targetEnemy, bool targetNeutral, bool targetSelf)
        {
            NumberOfTarget = numberOfTarget;
            TargetAlly = targetAlly;
            TargetEnemy = targetEnemy;
            TargetNeutral = targetNeutral;
            TargetSelf = targetSelf;
        }

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            var positions = new List<Vector2Int>();
            if (TargetSelf)
                positions.Add(actor.Entity.CurrentPosition);
            if (TargetAlly)
                positions.AddRange(map.Characters.In(actor.VisibleArea).ByAffiliation(actor, AffiliationType.Ally)
                    .Positions());
            if (TargetNeutral)
                positions.AddRange(map.Characters.In(actor.VisibleArea).ByAffiliation(actor, AffiliationType.Neutral)
                    .Positions());
            if (TargetEnemy)
                positions.AddRange(map.Characters.In(actor.VisibleArea).ByAffiliation(actor, AffiliationType.Enemy)
                    .Positions());
            return positions
                .OrderBy(p => Vector2Int.Distance(p, position))
                .Take(NumberOfTarget);
        }

        public float EvaluateHitProbability()
        {
            return 4 + 2 * NumberOfTarget;
        }

        public string Description()
        {
            var description = "近くの";
            if (TargetAlly && TargetNeutral && TargetEnemy)
            {
                description += "キャラクター";
            }
            else
            {
                var targets = new List<string>();
                if (TargetAlly) targets.Add("味方");
                if (TargetNeutral) targets.Add("中立");
                if (TargetEnemy) targets.Add("敵");

                description += string.Join("、", targets);
            }

            if (TargetSelf) description += "（自分含む）";
            description += $"{ItemDescriptionRichText.RichSpatial(NumberOfTarget)}体";

            return description;
        }
    }
}