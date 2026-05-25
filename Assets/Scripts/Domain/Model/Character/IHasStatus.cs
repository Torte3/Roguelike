using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;

namespace Domain.Model.Character
{
    public interface IHasStatus
    {
        public IStatusManager Status { get; }
        public int CurrentMaxHp { get; }
        public int CurrentHp { get; }

        /// <summary>
        /// Takes damage
        /// </summary>
        /// <param name="value">The amount of damage to take</param>
        /// <param name="causeOfDamageLog">Log message for cause of damage</param>
        /// <param name="attacker">The character that caused the damage, or null if not applicable (e.g. trap, poison).</param>
        /// <returns>The actual amount of HP reduced</returns>
        public UniTask<int> LoseHp(int value, string causeOfDamageLog, ICharacter? attacker);

        /// <summary>
        /// Recovers HP
        /// </summary>
        /// <param name="value">The amount of HP to recover</param>
        /// <returns>The actual amount of HP recovered</returns>
        public int GainHp(int value);
        public void RestoreToFullHealth();
    }
}