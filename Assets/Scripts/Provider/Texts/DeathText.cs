#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Entity;

namespace Provider.Texts
{
    internal static class DeathText
    {
        public static string Of(CharacterLabel victim, DamageSource source)
        {
            return CharacterNameText.Of(victim, true) + Predicate(source);
        }

        private static string Predicate(DamageSource source)
        {
            return source.Cause switch
            {
                DamageCause.Attack or DamageCause.CriticalAttack => $"は{Opponent(source)}の攻撃で殺された",
                DamageCause.Explosion => "は爆発に巻き込まれた",
                DamageCause.Fire => "は火に焼かれた",
                DamageCause.Poison => "は毒で死んだ",
                DamageCause.ItemCost => "はアイテムに命を吸われた",
                DamageCause.Break => "は破壊された",
                DamageCause.Unknown => "は死んだ。",
                DamageCause.Collision => source.Collision switch
                {
                    CollisionRole.Struck => $"は{Opponent(source)}に激しくぶつかった",
                    CollisionRole.StruckBy => $"は{Opponent(source)}に激しくぶつかられた",
                    CollisionRole.Wall => "は壁に激しくぶつかった",
                    CollisionRole.Object => "は激しくぶつかった",
                    _ => throw new ArgumentOutOfRangeException(nameof(source), source.Collision, null),
                },
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Cause, null),
            };
        }

        private static string Opponent(DamageSource source)
        {
            return source.Opponent == null
                ? CharacterNameText.Unknown
                : CharacterNameText.Of(source.Opponent.Label, source.Opponent.IsVisible);
        }
    }
}
