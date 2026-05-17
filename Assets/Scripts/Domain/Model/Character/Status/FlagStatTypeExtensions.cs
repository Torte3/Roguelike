using System;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Utilities;

namespace Domain.Model.Character.Status
{
    public static class FlagStatTypeExtensions
    {
        public static string GetName(this FlagStatType type) => type switch
        {
            FlagStatType.CannotAct => "行動不能",
            FlagStatType.CannotMove => "移動不能",
            FlagStatType.Confused => "混乱",
            FlagStatType.Clairvoyant => "透視",
            FlagStatType.Blind => "盲目",
            FlagStatType.OverDrive => "オーバードライブ",
            FlagStatType.AllConditionProof => "全状態異常耐性",
            FlagStatType.Hard => "硬質",
            FlagStatType.ExplosionProof => "爆発耐性",
            FlagStatType.Heavy => "スーパーアーマー",
            FlagStatType.SecureHold => "手放さず",
            FlagStatType.CurseProof => "呪い耐性",
            FlagStatType.Negotiator => "値切り",
            FlagStatType.IsAffectedByTrap => "帯電",
            FlagStatType.AutoIdentify => "自動識別",
            FlagStatType.RandomTeleport => "気まぐれワープ",
            FlagStatType.RandomExplosion => "気まぐれ爆発",
            FlagStatType.BookMaster => "魔法書マスター",
            FlagStatType.WandMaster => "杖マスター",
            FlagStatType.PotionMaster => "ポーションマスター",
            FlagStatType.CurseIdentify => "呪い識別",
            FlagStatType.AdjacentAttackGuard => "隣接ダメージ半減",
            FlagStatType.FullHpCritical => "満タンクリティカル",
            FlagStatType.StealEmpower => "盗むたびダメージ強化",
            _ => throw new ArgumentException($"Invalid flag stat type: {type}")
        };

        public static ParticleType GetParticleType(this FlagStatType type) => type switch
        {
            FlagStatType.CannotAct => ParticleType.Paralysis,
            FlagStatType.CannotMove => ParticleType.Paralysis,
            FlagStatType.Confused => ParticleType.Confusion,
            FlagStatType.Clairvoyant => ParticleType.None,
            FlagStatType.Blind => ParticleType.Blind,
            FlagStatType.OverDrive => ParticleType.None,
            FlagStatType.AllConditionProof => ParticleType.None,
            FlagStatType.Hard => ParticleType.None,
            FlagStatType.ExplosionProof => ParticleType.None,
            FlagStatType.Heavy => ParticleType.None,
            FlagStatType.SecureHold => ParticleType.None,
            FlagStatType.CurseProof => ParticleType.None,
            FlagStatType.Negotiator => ParticleType.None,
            FlagStatType.IsAffectedByTrap => ParticleType.Electric,
            FlagStatType.AutoIdentify => ParticleType.None,
            FlagStatType.RandomTeleport => ParticleType.None,
            FlagStatType.RandomExplosion => ParticleType.None,
            FlagStatType.BookMaster => ParticleType.None,
            FlagStatType.WandMaster => ParticleType.None,
            FlagStatType.PotionMaster => ParticleType.None,
            FlagStatType.CurseIdentify => ParticleType.None,
            FlagStatType.AdjacentAttackGuard => ParticleType.None,
            FlagStatType.FullHpCritical => ParticleType.None,
            FlagStatType.StealEmpower => ParticleType.None,
            _ => throw new ArgumentException($"Invalid flag stat type: {type}")
        };

        public static Impact GetImpact(this FlagStatType type) => type switch
        {
            FlagStatType.CannotAct => Impact.Harmful,
            FlagStatType.CannotMove => Impact.Harmful,
            FlagStatType.Confused => Impact.Harmful,
            FlagStatType.Clairvoyant => Impact.Beneficial,
            FlagStatType.Blind => Impact.Harmful,
            FlagStatType.OverDrive => Impact.Beneficial,
            FlagStatType.AllConditionProof => Impact.Beneficial,
            FlagStatType.Hard => Impact.Beneficial,
            FlagStatType.ExplosionProof => Impact.Beneficial,
            FlagStatType.Heavy => Impact.Beneficial,
            FlagStatType.SecureHold => Impact.Beneficial,
            FlagStatType.CurseProof => Impact.Beneficial,
            FlagStatType.Negotiator => Impact.Beneficial,
            FlagStatType.IsAffectedByTrap => Impact.Harmful,
            FlagStatType.AutoIdentify => Impact.Beneficial,
            FlagStatType.RandomTeleport => Impact.Harmful,
            FlagStatType.RandomExplosion => Impact.Harmful,
            FlagStatType.BookMaster => Impact.Beneficial,
            FlagStatType.WandMaster => Impact.Beneficial,
            FlagStatType.PotionMaster => Impact.Beneficial,
            FlagStatType.CurseIdentify => Impact.Beneficial,
            FlagStatType.AdjacentAttackGuard => Impact.Beneficial,
            FlagStatType.FullHpCritical => Impact.Beneficial,
            FlagStatType.StealEmpower => Impact.Beneficial,
            _ => throw new ArgumentException($"Invalid flag stat type: {type}")
        };

        public static float Evaluate(this FlagStatType type, ITargetOfEffect target)
        {
            if (target.Status.IsFlagStat(type))
                return 0;
            return type switch
            {
                FlagStatType.CannotAct => CommonSenseParameters.OneTurnStunEquivalentHpReduction,
                FlagStatType.CannotMove => CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2,
                FlagStatType.Confused => CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2,
                FlagStatType.Clairvoyant => 0.05f,
                FlagStatType.Blind => CommonSenseParameters.OneTurnStunEquivalentHpReduction / 2,
                FlagStatType.OverDrive => 0.1f,
                FlagStatType.AllConditionProof => 0.5f,
                FlagStatType.Hard => CommonSenseParameters.DamagePerAttack / CommonSenseParameters.MonsterMaxHealth,
                FlagStatType.ExplosionProof => 0.1f,
                FlagStatType.Heavy => 0.1f,
                FlagStatType.SecureHold => 0.1f,
                FlagStatType.CurseProof => 0.1f,
                FlagStatType.Negotiator => 0.1f,
                FlagStatType.IsAffectedByTrap => 0.1f,
                FlagStatType.AutoIdentify => 1f,
                FlagStatType.RandomTeleport => 0.1f,
                FlagStatType.RandomExplosion => 0.1f,
                FlagStatType.BookMaster => 0.2f,
                FlagStatType.WandMaster => 0.2f,
                FlagStatType.PotionMaster => 0.2f,
                FlagStatType.CurseIdentify => 0.3f,
                FlagStatType.AdjacentAttackGuard => 0.2f,
                FlagStatType.FullHpCritical => 0.2f,
                FlagStatType.StealEmpower => 0.2f,
                _ => throw new ArgumentException($"Invalid flag stat type: {type}")
            };
        }

        public static float EvaluatePrice(this FlagStatType type) => type switch
        {
            FlagStatType.CannotAct => CommonSenseParameters.OneTurnStunEquivalentDamage,
            FlagStatType.CannotMove => CommonSenseParameters.OneTurnStunEquivalentDamage / 2,
            FlagStatType.Confused => CommonSenseParameters.OneTurnStunEquivalentDamage / 2,
            FlagStatType.Clairvoyant => 2f,
            FlagStatType.Blind => CommonSenseParameters.OneTurnStunEquivalentDamage / 2,
            FlagStatType.OverDrive => 5f,
            FlagStatType.AllConditionProof => 3f,
            FlagStatType.Hard => 2f,
            FlagStatType.ExplosionProof => 1f,
            FlagStatType.Heavy => 1f,
            FlagStatType.SecureHold => 1f,
            FlagStatType.CurseProof => 1f,
            FlagStatType.Negotiator => 0.2f,
            FlagStatType.IsAffectedByTrap => 0.5f,
            FlagStatType.AutoIdentify => 3f,
            FlagStatType.RandomTeleport => 1f,
            FlagStatType.RandomExplosion => 1f,
            FlagStatType.BookMaster => 0.4f,
            FlagStatType.WandMaster => 0.4f,
            FlagStatType.PotionMaster => 0.4f,
            FlagStatType.CurseIdentify => 0.5f,
            FlagStatType.AdjacentAttackGuard => 0.5f,
            FlagStatType.FullHpCritical => 0.5f,
            FlagStatType.StealEmpower => 0.5f,
            _ => 0f
        };
    }
}