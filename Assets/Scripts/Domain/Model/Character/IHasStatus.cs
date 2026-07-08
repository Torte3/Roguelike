using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
using Domain.Model.Entity;

namespace Domain.Model.Character
{
    public interface IHasStatus
    {
        public IReadOnlyStatus Status { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }

        /// <summary>
        /// Takes damage
        /// </summary>
        /// <param name="value">The amount of damage to take</param>
        /// <param name="attacker">The character that caused the damage, or null if not applicable (e.g. trap, poison).</param>
        /// <returns>The actual amount of HP reduced</returns>
        public UniTask<int> LoseHp(int value, DamageSource source, ICharacter? attacker);

        /// <summary>
        /// Recovers HP
        /// </summary>
        /// <param name="value">The amount of HP to recover</param>
        public void GainHp(int value, HealCause cause);
        public void RestoreToFullHealth();
    }
}