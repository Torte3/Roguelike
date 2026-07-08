using System;
using UnityEngine;

namespace Domain.Model.Character.Affiliation
{
    public static class AggressionExtension
    {
        public static (float ally, float neutral, float enemy) GetAggression(this Aggression aggression)
        {
            return aggression switch
            {
                Aggression.AttackAnyone => (1, 1, 1),
                Aggression.AvoidAllies => (-1, 0, 1),
                Aggression.NeverHarmAllies => (-Mathf.Infinity, 0, 1),
                Aggression.AvoidNeutrals => (-Mathf.Infinity, -1, 1),
                Aggression.NeverHarmNeutrals => (-Mathf.Infinity, -Mathf.Infinity, 1),
                Aggression.AttackNone => (-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity),
                _ => throw new ArgumentOutOfRangeException(nameof(aggression), aggression, null)
            };
        }

        public static float GetAggression(this Aggression aggression, AffiliationType type)
        {
            return type switch
            {
                AffiliationType.Ally => aggression.GetAggression().ally,
                AffiliationType.Neutral => aggression.GetAggression().neutral,
                AffiliationType.Enemy => aggression.GetAggression().enemy,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}