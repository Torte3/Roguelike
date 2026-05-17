using StringSerializableEnum;

namespace Domain.Model.Character.Status
{
    [StringSerializable]
    public enum FlagStatType
    {
        CannotAct,
        CannotMove,
        Confused,
        Clairvoyant,
        Blind,
        OverDrive,
        AllConditionProof,
        Hard,
        ExplosionProof,
        Heavy,
        SecureHold,
        CurseProof,
        Negotiator,
        IsAffectedByTrap,
        AutoIdentify,
        RandomTeleport,
        RandomExplosion,
        BookMaster,
        WandMaster,
        PotionMaster,
        CurseIdentify,
        AdjacentAttackGuard,
        FullHpCritical,
        StealEmpower,
    }
}