using Domain.Model.Entity;
using Domain.Model.Memento;
using R3;
using Utilities;

namespace Domain.Model.Character
{
    public interface IAffiliation : ISerializable<AffiliationMemento>
    {
        public Id<IEntity> Id { get; }
        public CharacterGroup Group { get; }
        public Observable<OnAffiliationChangedMessage> OnAffiliationChanged { get; }
        public float GetAffection(IAffiliation other);
        public void AddForceAffiliation(Id<IEntity> other, AffiliationType type);
        public void RemoveForceAffiliation(Id<IEntity> other, AffiliationType type);
        public AffiliationType GetAffiliationType(IAffiliation other);
        public bool IsAlly(IAffiliation other);
        public bool IsEnemy(IAffiliation other);
        public void ModifyAffection(Id<IEntity> targetId, float change);
        public void OnCharacterAttacked(IAffiliation attacker, IAffiliation target, float impact);
        public void OnCharacterHealed(IAffiliation healer, IAffiliation target, float impact);
    }
}