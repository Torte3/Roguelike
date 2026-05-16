namespace Domain.Model.Evaluation
{
    public static class CommonSenseParameters
    {
        public const int PlayerMaxHealth = 100;
        public const float PlayerNaturalRecoveryRate = 0.2f;
        public const int PlayerAttackPowerWhenUnarmed = 1;
        public const int MonsterMaxHealth = 15;
        public const int AttacksToDefeatMonster = 2;
        public const int AttacksToDefeatPlayer = 10;
        public const float AttacksPerTurn = 0.05f;
        public const float HpReductionPerTurn = 1 / AttacksToDefeatPlayer;
        public const float DamagePerAttack = MonsterMaxHealth / AttacksToDefeatPlayer;
        public const float OneTurnStunEquivalentHpReduction = OneTurnStunEquivalentDamage / MonsterMaxHealth;
        public const float OneTurnStunEquivalentDamage = 5;
        public const int DamageWhenExplosionProof = 1;
        public const float RandomTeleportProbability = 0.1f;
        public const float RandomExplosionProbability = 0.05f;
        public const int PlayerVisionRange = 8;
        public const float EvaluateCoefficient = 0.05f;
        public const int ThrowDistance = 10;
        public const float SkillOnUseProbabilityOfSuccess = 0.95f;
        public const float SkillOnThrowProbabilityOfSuccess = 0.9f;
        public const float SpawnEnemyProbabilityPerTurn = 1 / 64f;
        public const float SpawnGrassProbabilityPerTurn = 1 / 256f;
        public const float DestroyFireProbabilityPerTurn = 1 / 4f;
        public const float BookMasterUsageLossChance = 0.5f;
        public const float WandMasterUsageLossChance = 0.5f;
        public const float ProjectileImpactHitProbability = 2f;

        public const int BlowAwayWallDamagePerRemainingTile = 1;

        public static int BlowAwayWallDamage(int remainingDistance)
        {
            return System.Math.Max(1, remainingDistance * BlowAwayWallDamagePerRemainingTile);
        }

        public static float BlowAwayPrice(int distance)
        {
            return distance;
        }

        public static float BlowAwayEvaluate(int distance)
        {
            return 0.2f * distance;
        }

        public static float CircleAreaEvaluate(bool canIgnoreWalls, int radius)
        {
            if (canIgnoreWalls)
                return (radius + 1) * 3;
            return (radius + 1) * 2;
        }
    }
}