#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using UnityEngine;
using Domain.Service.Events;
using Domain.Service.Items;
using Utilities;

namespace Domain.Service.Effect
{
    internal enum BlowAwayCollisionKind
    {
        Soft,
        Hard,
        Statue,
        OtherRigid,
        Wall
    }

    internal readonly struct BlowAwayCollisionSide
    {
        public BlowAwayCollisionKind Kind { get; }
        public ICharacter? Character { get; }
        public Statue? Statue { get; }

        public bool IsSoft => Kind == BlowAwayCollisionKind.Soft;

        public BlowAwayCollisionSide(BlowAwayCollisionKind kind, ICharacter? character, Statue? statue)
        {
            Kind = kind;
            Character = character;
            Statue = statue;
        }

        public static BlowAwayCollisionSide FromCharacter(ICharacter character) =>
            character.Status.IsFlagStat(FlagStatType.Hard)
                ? new BlowAwayCollisionSide(BlowAwayCollisionKind.Hard, character, null)
                : new BlowAwayCollisionSide(BlowAwayCollisionKind.Soft, character, null);

        public static BlowAwayCollisionSide Wall() =>
            new BlowAwayCollisionSide(BlowAwayCollisionKind.Wall, null, null);

        public static BlowAwayCollisionSide OtherRigidObject() =>
            new BlowAwayCollisionSide(BlowAwayCollisionKind.OtherRigid, null, null);

        public static BlowAwayCollisionSide? FromEntity(IEntity? entity)
        {
            if (entity == null)
                return null;
            if (entity is ThrowAnimationEntity)
                return null;
            if (entity is ICharacter character)
                return FromCharacter(character);
            if (entity is Statue statue)
                return new BlowAwayCollisionSide(BlowAwayCollisionKind.Statue, null, statue);
            return OtherRigidObject();
        }
    }

    internal static class BlowAwayCollision
    {
        public static int CountStepsMoved(Vector2Int from, Vector2Int to, Direction8 direction)
        {
            var steps = 0;
            var pos = from;
            while (pos != to)
            {
                pos += direction.Vector();
                steps++;
            }

            return steps;
        }

        public static async UniTask Apply(
            BlowAwayCollisionSide mover,
            BlowAwayCollisionSide blocker,
            int remaining,
            ICharacter? attacker,
            IMap map)
        {
            if (remaining <= 0)
                return;

            var total = CommonSenseParameters.BlowAwayWallDamage(remaining);

            if (mover.IsSoft && blocker.IsSoft)
            {
                await ApplySoftSoft(mover.Character!, blocker.Character!, total, attacker, map);
                return;
            }

            if (mover.IsSoft)
            {
                await ApplyRigidSoft(blocker, mover, softIsMover: true, total, attacker, map);
                return;
            }

            if (blocker.IsSoft)
            {
                await ApplyRigidSoft(mover, blocker, softIsMover: false, total, attacker, map);
                return;
            }

            await ApplyRigidRigid(mover, blocker, attacker, map);
        }

        private static async UniTask ApplySoftSoft(
            ICharacter mover,
            ICharacter blocker,
            int total,
            ICharacter? attacker,
            IMap map)
        {
            var half = total / 2;
            var moverDamage = half;
            var blockerDamage = total - half;
            if (moverDamage > 0)
                await mover.LoseHp(moverDamage, $"は{blocker.GetName(map.Player)}に激しくぶつかった", attacker);
            if (blockerDamage > 0)
                await blocker.LoseHp(blockerDamage, $"は{mover.GetName(map.Player)}に激しくぶつかられた", attacker);
        }

        private static async UniTask ApplyRigidSoft(
            BlowAwayCollisionSide rigid,
            BlowAwayCollisionSide soft,
            bool softIsMover,
            int total,
            ICharacter? attacker,
            IMap map)
        {
            if (soft.Character != null)
                await soft.Character.LoseHp(total, BuildSoftLog(softIsMover, soft.Character, rigid, map.Player), attacker);

            await ApplyRigidSideEffect(rigid, soft.Character, rigidWasMover: !softIsMover, attacker, map);
        }

        private static async UniTask ApplyRigidRigid(
            BlowAwayCollisionSide mover,
            BlowAwayCollisionSide blocker,
            ICharacter? attacker,
            IMap map)
        {
            await ApplyRigidSideEffect(mover, blocker.Character, rigidWasMover: true, attacker, map);
            await ApplyRigidSideEffect(blocker, mover.Character, rigidWasMover: false, attacker, map);
        }

        private static async UniTask ApplyRigidSideEffect(
            BlowAwayCollisionSide rigid,
            ICharacter? otherCharacter,
            bool rigidWasMover,
            ICharacter? attacker,
            IMap map)
        {
            switch (rigid.Kind)
            {
                case BlowAwayCollisionKind.Hard when rigid.Character != null:
                    await ApplyHardDamage(rigid.Character, otherCharacter, rigidWasMover, attacker, map);
                    break;
                case BlowAwayCollisionKind.Statue when rigid.Statue != null:
                    rigid.Statue.Attacked();
                    break;
            }
        }

        private static async UniTask ApplyHardDamage(
            ICharacter hard,
            ICharacter? other,
            bool hardWasMover,
            ICharacter? attacker,
            IMap map)
        {
            var log = other != null
                ? hardWasMover
                    ? $"は{other.GetName(map.Player)}に激しくぶつかった"
                    : $"は{other.GetName(map.Player)}に激しくぶつかられた"
                : "は壁に激しくぶつかった";
            await hard.LoseHp(1, log, attacker);
        }

        private static string BuildSoftLog(bool softIsMover, ICharacter soft, BlowAwayCollisionSide rigid, IPlayer player)
        {
            if (rigid.Kind == BlowAwayCollisionKind.Wall)
                return "は壁に激しくぶつかった";
            if (rigid.Character != null)
            {
                return softIsMover
                    ? $"は{rigid.Character.GetName(player)}に激しくぶつかった"
                    : $"は{rigid.Character.GetName(player)}に激しくぶつかられた";
            }

            return "は激しくぶつかった";
        }
    }
}
