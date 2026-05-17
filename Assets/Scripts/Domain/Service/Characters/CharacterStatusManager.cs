#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Conditions;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Stats;

namespace Domain.Service.Characters
{
    public class CharacterStatusManager : IDisposable, ISerializable<CharacterStatusMemento>, IStatusManager
    {
        private readonly CharacterConditions _conditions;
        private readonly Subject<OnDamageReceivedMessage> _onDamageReceived = new();
        private readonly Subject<int> _onHealReceived = new();
        public IntResource Hp { get; init; }
        public Stat HpNaturalRecoveryAmount { get; init; }
        public Stat ViewRange { get; init; }
        public Resource WaitTime { get; init; }
        public Stat AttackMultiplier { get; init; }
        public Dictionary<Element, Stat> ElementAttackMultiplier { get; init; }
        public Dictionary<Element, Stat> ElementDamageRateMultiplier { get; init; }
        public Dictionary<string, Stat> ConditionResistance { get; init; }
        private readonly VisionRange _visionRange;
        private readonly Dictionary<FlagStatType, FlagStat> _flagStats = new();
        private readonly ICharacter _character;

        public CharacterStatusManager(CharacterStatusMemento data, ReadOnlyReactiveProperty<Vector2Int> position,
            ICharacter character, IMap map)
        {
            Hp = new IntResource(data.Stats.Hp);
            HpNaturalRecoveryAmount = new Stat(data.Stats.HpNaturalRecoveryAmount);
            AttackMultiplier = new Stat(data.Stats.AttackMultiplier);
            ElementAttackMultiplier =
                data.Stats.ElementAttackMultiplier.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
            ElementDamageRateMultiplier =
                data.Stats.ElementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
            ViewRange = new Stat(data.Stats.ViewRange);
            WaitTime = new Resource(data.Stats.WaitTime);

            ConditionResistance =
                data.Stats.ConditionResistance.ToDictionary(pair => pair.Key, pair => new Stat(pair.Value));
            _conditions = new CharacterConditions(character, data.Conditions, map);
            _flagStats = data.FlagStats.ToDictionary(x => x.Key, x => new FlagStat(x.Value));
            _visionRange = new VisionRange(position, ViewRange.Value, GetFlagStat(FlagStatType.Clairvoyant),
                GetFlagStat(FlagStatType.Blind), () => character.CanThroughWalls, map);
            _character = character;
        }

        public void Dispose()
        {
            Hp.Dispose();
            HpNaturalRecoveryAmount.Dispose();
            ViewRange.Dispose();
            WaitTime.Dispose();
            AttackMultiplier.Dispose();
            foreach (var element in ElementAttackMultiplier.Values)
            {
                element.Dispose();
            }

            foreach (var element in ElementDamageRateMultiplier.Values)
            {
                element.Dispose();
            }

            foreach (var condition in ConditionResistance.Values)
            {
                condition.Dispose();
            }
            _conditions.Dispose();
        }

        public CharacterStatusMemento Serialize()
        {
            return new CharacterStatusMemento
            (
                new CharacterStatsMemento
                (
                    Hp.GetData(),
                    HpNaturalRecoveryAmount.GetData(),
                    AttackMultiplier.GetData(),
                    ElementAttackMultiplier.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                    ElementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                    ConditionResistance.ToDictionary(pair => pair.Key, pair => pair.Value.GetData()),
                    ViewRange.GetData(),
                    WaitTime.GetData()
                ),
                _flagStats.ToDictionary(x => x.Key, x => x.Value.CurrentFlags),
                _conditions.ConditionsWithInflicter.Select(x => (x.actor, x.condition.Serialize())).ToList()
            );
        }

        public ReadOnlyReactiveProperty<int> MaxHp => Hp.Max.IntValue;
        public ReadOnlyReactiveProperty<int> HpValue => Hp.Value;
        public ReadOnlyReactiveProperty<float> WaitTimeValue => WaitTime.Value;

        public IVisionRange VisionRange => _visionRange;
        public IObservableCollection<ICondition> Conditions => _conditions.Conditions;

        public IStat GetStat(StatType type)
        {
            return type switch
            {
                StatType.MaxHp => Hp.Max,
                StatType.HpNaturalRecovery => HpNaturalRecoveryAmount,
                StatType.ViewRange => ViewRange,
                StatType.MaxWaitTime => WaitTime.Max,
                StatType.AttackMultiplier => AttackMultiplier,
                _ => throw new ArgumentException($"Invalid stat type: {type}")
            };
        }

        public IStat GetAttackMultiplierStat() => AttackMultiplier;

        public IStat GetElementAttackMultiplierStat(Element element)
        {
            if (!ElementAttackMultiplier.ContainsKey(element))
            {
                ElementAttackMultiplier[element] = new Stat(1);
            }

            return ElementAttackMultiplier[element];
        }

        public IStat GetElementDamageRateMultiplierStat(Element element)
        {
            if (!ElementDamageRateMultiplier.ContainsKey(element))
            {
                ElementDamageRateMultiplier[element] = new Stat(1);
            }

            return ElementDamageRateMultiplier[element];
        }

        public IStat GetConditionResistanceStat(ConditionTemplate condition)
        {
            if (IsFlagStat(FlagStatType.AllConditionProof))
            {
                return new Stat(1);
            }

            if (!ConditionResistance.ContainsKey(condition.name))
            {
                ConditionResistance[condition.name] = new Stat(0);
            }

            return ConditionResistance[condition.name];
        }

        public float GetStatValue(StatType type)
        {
            return GetStat(type).CurrentValue;
        }

        public float GetAttackMultiplier() => AttackMultiplier.CurrentValue;

        public float GetElementAttackMultiplier(Element element)
        {
            return GetElementAttackMultiplierStat(element).CurrentValue;
        }

        public float GetCombinedElementAttackMultiplier(Element element)
        {
            return GetAttackMultiplier() + GetElementAttackMultiplier(element) - 1f;
        }

        public float GetElementDamageRateMultiplier(Element element)
        {
            return GetElementDamageRateMultiplierStat(element).CurrentValue;
        }

        public float GetConditionResistance(ConditionTemplate condition)
        {
            return GetConditionResistanceStat(condition).CurrentValue;
        }

        public IFlagStat GetFlagStat(FlagStatType type)
        {
            if (!_flagStats.TryGetValue(type, out var flagStat))
            {
                flagStat = new FlagStat(0);
                _flagStats[type] = flagStat;
            }

            return flagStat;
        }

        public bool IsFlagStat(FlagStatType type)
        {
            return GetFlagStat(type).CurrentValue;
        }

        public ReadOnlyReactiveProperty<bool> GetFlagProperty(FlagStatType type)
        {
            return GetFlagStat(type).Value;
        }

        public bool IsDead => Hp.Value.CurrentValue <= 0;
        public Observable<OnDamageReceivedMessage> OnDamageReceived => _onDamageReceived;
        public Observable<int> OnHealReceived => _onHealReceived;

        public int GainHp(float value, bool notifyOnlyActualGain = false)
        {
            var gainValue = Hp.Gain(value);
            if (notifyOnlyActualGain)
            {
                if (gainValue > 0)
                {
                    _onHealReceived.OnNext(gainValue);
                }
            }
            else
            {
                _onHealReceived.OnNext(Mathf.RoundToInt(value));
            }

            return gainValue;
        }

        public async UniTask<int> LoseHp(float value, string causeOfDeathLog, ICharacter? attacker, bool notifyOnlyActualLoss = false)
        {
            var loseValue = Hp.Lose(value);
            if (notifyOnlyActualLoss)
            {
                if (loseValue > 0)
                {
                    _onDamageReceived.OnNext(new OnDamageReceivedMessage(loseValue, causeOfDeathLog, attacker));
                }
            }
            else
            {
                _onDamageReceived.OnNext(new OnDamageReceivedMessage(Mathf.RoundToInt(value), causeOfDeathLog, attacker));
            }

            if (loseValue == 0)
                return 0;

            if (IsDead)
            {
                await _character.UseItemOnDeath();
            }

            if (IsDead)
            {
                await _character.UseLastSkill();
                _character.Die(causeOfDeathLog);
            }

            return loseValue;
        }

        public void RestoreToFullHealth()
        {
            Hp.Set(Hp.Max.CurrentIntValue);
            _conditions.Clear();
        }

        public async UniTask UpdateTurn(bool characterVisible)
        {
            if (HpNaturalRecoveryAmount.CurrentValue > 0)
                GainHp(HpNaturalRecoveryAmount.CurrentValue, true);
            else
                await LoseHp(-HpNaturalRecoveryAmount.CurrentValue, "は毒で死んだ", null, true);
            _conditions.UpdateTurn(characterVisible);
        }

        public void WasAttacked()
        {
            _conditions.WasAttacked();
        }

        public void AddWaitTime(float value)
        {
            WaitTime.Gain(value);
        }

        public void ResetWaitTime()
        {
            WaitTime.Set(0);
        }

        public bool IsWaitTimeFull()
        {
            return WaitTime.IsFull();
        }

        public static CharacterStatusMemento Build(int maxHp, float hpNaturalRecoveryAmount, float attackMultiplier,
            Dictionary<Element, float> elementAttackMultiplier, Dictionary<Element, float> elementDamageRateMultiplier,
            Dictionary<ConditionTemplate, float> conditionResistance, float viewRange, HashSet<FlagStatType> flags, float waitTime, bool isSlept, bool doActImmediately)
        {
            var conditions = new List<(Id<IEntity> actor, ConditionMemento condition)>();
            if (isSlept)
            {
                conditions.Add(
                    (
                        Id<IEntity>.Empty,
                        Condition.Build(ObjectLoader.Load<ConditionTemplate>("まどろみ"))
                    )
                );
            }

            var flagStats = new Dictionary<FlagStatType, int>();
            if (isSlept)
            {
                flagStats[FlagStatType.CannotAct] = 1;
                flagStats[FlagStatType.Blind] = 1;
            }

            foreach (var flag in flags)
            {
                flagStats[flag] = 1;
            }

            return new CharacterStatusMemento
            (
                new CharacterStatsMemento
                (
                    hp: new ResourceData(new StatData(maxHp, minValue: 0f), maxHp),
                    hpNaturalRecovery: new StatData(hpNaturalRecoveryAmount),
                    attackMultiplier: new StatData(attackMultiplier, minValue: 0f),
                    elementAttackMultiplier: elementAttackMultiplier.ToDictionary(pair => pair.Key, pair => new StatData(pair.Value, minValue: 0f)),
                    elementDamageRateMultiplier: elementDamageRateMultiplier.ToDictionary(pair => pair.Key, pair => new StatData(pair.Value, minValue: 0f)),
                    conditionResistance: conditionResistance.ToDictionary(pair => pair.Key.name, pair => new StatData(pair.Value, minValue: 0f, maxValue: 1f)),
                    viewRange: new StatData(viewRange, minValue: 0f),
                    waitTime: new ResourceData(new StatData(waitTime, minValue: 0f), doActImmediately? waitTime : 0)
                ),
                flagStats,
                conditions
            );
        }

        public void AddCondition(Id<IEntity> actor, ConditionTemplate condition)
        {
            _conditions.Add(actor, condition);
        }

        public void RemoveConditionType(Type conditionType)
        {
            _conditions.RemoveType(conditionType);
        }

        public void ClearCondition()
        {
            _conditions.Clear();
        }

        public string Info()
        {
            var info = "";
            info += $"Hp:{Hp.Value}/{Hp.Max.CurrentValue}\n";
            info += $"Hp自然回復量:{HpNaturalRecoveryAmount.CurrentValue}\n";
            info += $"視界範囲:{ViewRange.CurrentValue}\n";
            info += $"待機時間:{WaitTime.Max.CurrentValue}\n";
            if (AttackMultiplier.CurrentValue != 1)
                info += $"攻撃倍率:{AttackMultiplier.CurrentValue:P0}\n";
            foreach (var element in ElementAttackMultiplier.Keys)
            {
                if (ElementAttackMultiplier[element].CurrentValue == 1)
                    continue;
                info += $"攻撃倍率:{element.Name()}:{ElementAttackMultiplier[element].CurrentValue:P0}\n";
            }
            foreach (var element in ElementDamageRateMultiplier.Keys)
            {
                if (ElementDamageRateMultiplier[element].CurrentValue == 1)
                    continue;
                info += $"被ダメージ倍率:{element.Name()}:{ElementDamageRateMultiplier[element].CurrentValue:P0}\n";
            }
            foreach (var condition in ConditionResistance.Keys)
            {
                if (ConditionResistance[condition].CurrentValue == 0)
                    continue;
                info += $"状態異常耐性:{condition}:{ConditionResistance[condition].CurrentValue:P0}\n";
            }
            info += "フラグ:\n";
            foreach (var flag in _flagStats)
            {
                if (!flag.Value.CurrentValue)
                    continue;
                info += $"{flag.Key.GetName()}\n";
            }
            info += "状態異常:\n";
            foreach (var condition in _conditions.Conditions)
            {
                info += $"{condition.Info()}\n";
            }
            return info;
        }
    }
}