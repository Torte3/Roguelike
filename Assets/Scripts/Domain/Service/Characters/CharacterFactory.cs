#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Domain.Service.Effect;
using Domain.Service.Items;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Service.Characters
{
    public sealed class CharacterFactory
    {
        public static PlayerMemento BuildPlayer(PlayerData data, Vector2Int spawnPosition)
        {
            var flags = data.Flags.ToHashSet();
            flags.Add(FlagStatType.IsAffectedByTrap);
            if (data.Name == "Thief")
                flags.Add(FlagStatType.StealEmpower);
            var defaultSkill = new SkillData(
                position: new AtFeet(),
                area: new LineArea(1, false, false),
                effects: new List<IEffect>
                {
                    new AttackEffect(
                        new List<ElementPower>
                        {
                            new ElementPower(Element.Physical, 1)
                        },
                        0
                    )
                },
                repeats: 1,
                probabilityOfSuccess: CommonSenseParameters.SkillOnUseProbabilityOfSuccess,
                cost: 0,
                rushDistance: 0,
                backStepDistance: 0,
                chargeTurn: 0,
                coolTime: 0,
                log: "は殴りかかった。"
            );

            var character = new CharacterMemento
            (
                name: data.name,
                characterType: data.CharacterType,
                behavior: PlayerBehavior.Build(),
                status: CharacterStatusManager.Build(
                    maxHp: data.Hp,
                    hpNaturalRecoveryAmount: CommonSenseParameters.PlayerNaturalRecoveryRate,
                    attackMultiplier: data.AttackMultiplier,
                    elementAttackMultiplier: data.ElementAttackMultiplier,
                    elementDamageRateMultiplier: data.ElementDamageRateMultiplier,
                    conditionResistance: data.ConditionResistance,
                    viewRange: CommonSenseParameters.PlayerVisionRange,
                    flags: flags,
                    waitTime: data.MoveSpeed.ToWaitTime(),
                    isSlept: false,
                    doActImmediately: false
                ),
                entity: EntityBase.Build(spawnPosition, EntityLayer.Middle),
                direction: Direction8.Down,
                skills: new List<CharacterSkillWithRuleMemento>
                {
                    new CharacterSkillWithRuleMemento(
                        SkillWithCost.Build(defaultSkill),
                        0
                    )
                },
                lastSkill: Option<SpawnEffectSkillMemento>.None,
                inventory: Storage.Build(data.InventoryCapacity, new(), true, true),
                knownItemNames: new List<string>(),
                affiliation: CharacterAffiliationManager.Build(CharacterGroup.Human),
                aggression: Aggression.AttackAnyone,
                isLeader: true,
                isShiny: false,
                isBoss: data.IsBoss,
                isFlying: data.IsFlying,
                canThroughWalls: data.CanThroughWalls,
                canPickUp: true,
                canUseItem: true,
                canReceivePlayerGift: false
            );
            return new PlayerMemento(character, 0);
        }

        public static CharacterMemento BuildCharacter(EnemyData data, Vector2Int spawnPosition,
            IItemMemento? additionalDropItem = null,
            Direction8 direction = Direction8.Down, bool isSlept = false, bool isShiny = false,
            IAffiliation? affiliation = null, Location? homeLocation = null, bool doActImmediately = false)
        {
            var items = new List<IItemMemento>();
            if (RandUtils.IsLessThanProbability(data.DropItemRate) && data.DropItemTable.Count > 0)
            {
                var dropItem = data.DropItemTable.GetRandomItem();
                items.Add(Item.Build(dropItem));
            }
            if (additionalDropItem != null)
            {
                items.Add(additionalDropItem);
            }
            var inventory = Storage.Build(20, items, true, true);

            var attackMultiplier = data.AttackMultiplier;
            if (isShiny)
                attackMultiplier += 1f;

            return new CharacterMemento
            (
                name: isShiny ? "☆" + data.Name : data.Name,
                characterType: data.CharacterType,
                behavior: EnemyBehavior.Build(
                    data.Behavior,
                    homeLocation.ToOption()
                ),
                status: CharacterStatusManager.Build(
                    maxHp: isShiny ? data.Hp * 5 : data.Hp,
                    hpNaturalRecoveryAmount: 0.1f,
                    attackMultiplier: attackMultiplier,
                    elementAttackMultiplier: data.ElementAttackMultiplier,
                    elementDamageRateMultiplier: data.ElementDamageRateMultiplier,
                    conditionResistance: data.ConditionResistance,
                    viewRange: 8,
                    flags: data.Flags.ToHashSet(),
                    waitTime: data.MoveSpeed.ToWaitTime(),
                    isSlept: isSlept,
                    doActImmediately: doActImmediately
                ),
                entity: EntityBase.Build(spawnPosition, EntityLayer.Middle),
                direction: direction,
                skills: data.Skills.Select(x => CharacterSkillWithRule.Build(x)).ToList(),
                lastSkill: (data.HasLastSkill ? SpawnEffectSkill.Build(data.LastSkill) : null).ToOption(),
                inventory: inventory,
                knownItemNames: new List<string>(),
                affiliation: CharacterAffiliationManager.Build(data.Group, affiliation),
                aggression: data.Aggression,
                isLeader: data.IsBoss,
                isShiny: isShiny,
                isBoss: data.IsBoss,
                isFlying: data.IsFlying,
                canThroughWalls: data.CanThroughWalls,
                canPickUp: data.CanPickUp,
                canUseItem: data.CanUseItem,
                canReceivePlayerGift: data.CanReceivePlayerGift
            );
        }

        public static IPlayer CreatePlayer(PlayerMemento playerData, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            return new Player(playerData, receiver, gameManager, map);
        }

        public static ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior, IMap map)
        {
            return new Character(data, behavior, map, false);
        }

    }
}