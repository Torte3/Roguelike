#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DamageSourceMemento
    {
        public enum Kind
        {
            Attack,
            CharacterCollision,
            ObstacleCollision,
            Plain,
        }

        [SerializeField] private Kind _kind;
        [SerializeField] private Option<OpponentMemento> _opponent;
        [SerializeField] private bool _isCritical;
        [SerializeField] private CollisionSide _side;
        [SerializeField] private CollisionObstacle _obstacle;
        [SerializeField] private DamageCause _cause;

        private DamageSourceMemento(Kind kind, Opponent? opponent)
        {
            _kind = kind;
            _opponent = opponent.ToOption().Map(value => new OpponentMemento(value));
        }

        public static DamageSourceMemento Attack(Opponent? opponent, bool isCritical) =>
            new(Kind.Attack, opponent) { _isCritical = isCritical };

        public static DamageSourceMemento CharacterCollision(Opponent opponent, CollisionSide side) =>
            new(Kind.CharacterCollision, opponent) { _side = side };

        public static DamageSourceMemento ObstacleCollision(CollisionObstacle obstacle) =>
            new(Kind.ObstacleCollision, null) { _obstacle = obstacle };

        public static DamageSourceMemento Plain(DamageCause cause, Opponent? opponent) =>
            new(Kind.Plain, opponent) { _cause = cause };

        public DamageSource Deserialize()
        {
            var opponent = _opponent.Map(value => value.Deserialize()).UnwrapOrNull();
            return _kind switch
            {
                Kind.Attack => new AttackDamage(opponent, _isCritical),
                Kind.CharacterCollision => new CharacterCollisionDamage(opponent!, _side),
                Kind.ObstacleCollision => new ObstacleCollisionDamage(_obstacle),
                Kind.Plain => new PlainDamage(_cause, opponent),
                _ => throw new ArgumentOutOfRangeException(nameof(_kind), _kind, null),
            };
        }
    }
}
