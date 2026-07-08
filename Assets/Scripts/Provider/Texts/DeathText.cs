#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Entity;

namespace Provider.Texts
{
    internal sealed class DeathText
    {
        private readonly CharacterLabel _victim;
        private readonly DamageSource _source;

        public DeathText(CharacterLabel victim, DamageSource source)
        {
            _victim = victim;
            _source = source;
        }

        public string Text => new CharacterNameText(_victim).Text + Predicate;

        private string Predicate => _source switch
        {
            AttackDamage attack => $"は{NameOf(attack.Opponent)}の攻撃で殺された",
            CharacterCollisionDamage collision => collision.Side switch
            {
                CollisionSide.Struck => $"は{NameOf(collision.Opponent)}に激しくぶつかった",
                CollisionSide.StruckBy => $"は{NameOf(collision.Opponent)}に激しくぶつかられた",
                _ => throw new ArgumentOutOfRangeException(nameof(collision.Side), collision.Side, null),
            },
            ObstacleCollisionDamage collision => collision.Obstacle switch
            {
                CollisionObstacle.Wall => "は壁に激しくぶつかった",
                CollisionObstacle.Object => "は激しくぶつかった",
                _ => throw new ArgumentOutOfRangeException(nameof(collision.Obstacle), collision.Obstacle, null),
            },
            PlainDamage plain => plain.Cause switch
            {
                DamageCause.Explosion => "は爆発に巻き込まれた",
                DamageCause.Fire => "は火に焼かれた",
                DamageCause.Poison => "は毒で死んだ",
                DamageCause.ItemCost => "はアイテムに命を吸われた",
                DamageCause.Break => "は破壊された",
                _ => throw new ArgumentOutOfRangeException(nameof(plain.Cause), plain.Cause, null),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(_source), _source, null),
        };

        private static string NameOf(Opponent? opponent) => opponent == null || !opponent.IsVisible
            ? CharacterNameText.Unknown
            : new CharacterNameText(opponent.Label).Text;
    }
}
