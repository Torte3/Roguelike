#nullable enable
using System;
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
                name: data.Name,
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

        public static ICharacter CreateCharacter(CharacterMemento data, ICharacterBehavior behavior, IGameManager gameManager, IMap map)
        {
            return new Character(data, behavior, gameManager, map, false);
        }

        public static float EvaluateDamageRate(EnemyData enemyData)
        {
            var sum = 0f;
            foreach (var element in Enum.GetValues(typeof(Element)))
            {
                if (enemyData.ElementDamageRateMultiplier.TryGetValue((Element)element, out var multiplier))
                {
                    sum += multiplier;
                }
                else
                {
                    sum += 1;
                }
            }
            return sum / Enum.GetValues(typeof(Element)).Length;
        }
        public static float EvaluateSkills(IEnumerable<SkillData> skills)
        {
            var virtualSkills = skills.Select(x => new VirtualSkill(x)).ToList();
            var sum = 0f;
            var turn = 0;
            while (turn < 100)
            {
                foreach (var skill in virtualSkills)
                {
                    skill.Update(1);
                }
                var selectedSkill = virtualSkills.Where(x => x.IsReady()).MaxByOrDefault(x => x.Value, null);
                if (selectedSkill == null)
                {
                    turn++;
                    continue;
                }
                if (selectedSkill.ChargeTurn > 0)
                {
                    foreach (var skill in virtualSkills)
                    {
                        skill.Update(selectedSkill.ChargeTurn);
                    }

                    sum += selectedSkill.Value * selectedSkill.ChargeTurn;
                    turn += selectedSkill.ChargeTurn;
                }
                selectedSkill.Use();
                sum += selectedSkill.Value;
                turn++;
            }
            return sum / turn;
        }
        private class VirtualSkill
        {
            public float Value;
            public int ChargeTurn;
            public int CoolTime;
            private int _remainingCoolTime;
            public VirtualSkill(SkillData skill)
            {
                Value = new SpawnEffectSkill(SpawnEffectSkill.Build(skill)).EvaluatePrice();
                ChargeTurn = skill.ChargeTurn;
                CoolTime = skill.CoolTime;
                _remainingCoolTime = 0;
            }
            public void Update(int turnCount)
            {
                _remainingCoolTime = Mathf.Max(0, _remainingCoolTime - turnCount);
            }
            public void Use()
            {
                _remainingCoolTime = 1 + CoolTime;
            }
            public bool IsReady()
            {
                return _remainingCoolTime == 0;
            }
        }
    }
}