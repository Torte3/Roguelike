using System;
using System.ComponentModel.DataAnnotations;
using Sirenix.OdinInspector;
using Utilities;

namespace Domain.Model.Condition
{
    [Serializable]
    public class RemovalConditionData
    {
        public bool RemoveByElapsedTurn;
        [ShowIf("@" + nameof(RemoveByElapsedTurn))] public int Duration;
        public bool RemoveByDamage;

        [ShowIf("@" + nameof(RemoveByDamage))]
        [Range(0, 1)]
        public float Probability;

        public bool RemoveByCharacterNearby;

        [ShowIf("@" + nameof(RemoveByCharacterNearby))]
        [Range(0, 1)]
        public float CharacterNearbyProbability;

        public RemovalConditionData(int duration = -1, float damageProbability = -1,
            float characterNearbyProbability = -1)
        {
            if (duration > 0)
            {
                RemoveByElapsedTurn = true;
                Duration = duration;
            }

            if (damageProbability > 0)
            {
                RemoveByDamage = true;
                Probability = damageProbability;
            }

            if (characterNearbyProbability > 0)
            {
                RemoveByCharacterNearby = true;
                CharacterNearbyProbability = characterNearbyProbability;
            }
        }

        public bool IsFinished(int elapsedTurns, bool characterVisible)
        {
            return (RemoveByElapsedTurn && elapsedTurns >= Duration) ||
                   (RemoveByCharacterNearby && characterVisible && RandUtils.IsLessThanProbability(CharacterNearbyProbability));
        }

        public bool IsFinishedByDamage()
        {
            return RemoveByDamage && RandUtils.IsLessThanProbability(Probability);
        }

        public float EvaluateTurn()
        {
            var estimatedTurns = float.MaxValue;

            if (RemoveByElapsedTurn)
            {
                estimatedTurns = Duration;
            }

            if (RemoveByDamage)
            {
                var damageTurns = 1 / Probability;
                if (damageTurns < estimatedTurns)
                {
                    estimatedTurns = damageTurns;
                }
            }

            if (RemoveByCharacterNearby)
            {
                var enemyNearbyTurns = 1 / CharacterNearbyProbability;
                if (enemyNearbyTurns < estimatedTurns)
                {
                    estimatedTurns = enemyNearbyTurns;
                }
            }

            return estimatedTurns;
        }

        public string Info(int elapsedTurn)
        {
            var info = "解除条件:\n";
            if (RemoveByElapsedTurn)
            {
                info += $"ターン経過:あと{Duration - elapsedTurn}ターン\n";
            }
            if (RemoveByDamage)
            {
                info += $"ダメージを受けたとき:{Probability:P0}\n";
            }
            if (RemoveByCharacterNearby)
            {
                info += $"近くにキャラクターがいるとき:{CharacterNearbyProbability:P0}\n";
            }
            return info;
        }
    }
}