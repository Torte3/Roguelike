#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Events;
using Domain.Service.Items;
using ObservableCollections;
using R3;
using Utilities;
using Utilities.Serialize.Option;

namespace Game
{
    public class EventEntityManager : ISerializable<EventEntitiesMemento>
    {
        public readonly List<MimicItemEntity> MimicItems = new();
        public readonly List<MimicMoney> MimicMoney = new();
        public readonly List<MimicStairs> MimicStairs = new();
        public readonly List<Stairs> Stairs = new();
        private readonly List<Chest> _chests = new();
        private readonly List<Trap> _traps = new();
        public readonly List<Statue> Statues = new();
        private readonly List<Money> _money = new();
        private Option<Bonfire> _bonfire = Option<Bonfire>.None;
        private Option<MagicPot> _magicPot = Option<MagicPot>.None;
        private Option<Workbench> _workbench = Option<Workbench>.None;
        private Option<Teleporter> _teleporter = Option<Teleporter>.None;
        private ObservableList<IEntityEventEntity> _standaloneEntityEventEntities = new();
        private ObservableList<ICharacterEventEntity> _standaloneCharacterEventEntities = new();
        private ObservableList<IPlayerEventEntity> _standalonePlayerEventEntities = new();
        private ObservableList<IScheduledEventEntity> _standaloneScheduledEventEntities = new();
        
        public IEnumerable<ILockedEntity> LockedEntities
        {
            get
            {
                foreach (var chest in _chests)
                {
                    if (chest.KeyCharacters.Count > 0)
                        yield return chest;
                }
            }
        }

        public EventEntityManager(EventEntitiesMemento eventEntities)
        {
            foreach (var trapMemento in eventEntities.Traps)
            {
                var trap = new Trap(trapMemento);
                _traps.Add(trap);
                Spawn(trap);
            }

            _teleporter = eventEntities.Teleporter.Map(teleporter => new Teleporter(teleporter));
            if (_teleporter.HasValue)
                Spawn(_teleporter.Value!);

            foreach (var mimicItemMemento in eventEntities.MimicItems)
            {
                var mimicItem = new MimicItemEntity(mimicItemMemento);
                MimicItems.Add(mimicItem);
                Spawn(mimicItem);
            }

            foreach (var mimicMoneyMemento in eventEntities.MimicMoney)
            {
                var mimicMoney = new MimicMoney(mimicMoneyMemento);
                MimicMoney.Add(mimicMoney);
                Spawn(mimicMoney);
            }

            foreach (var mimicStairsMemento in eventEntities.MimicStairs)
            {
                var mimicStairs = new MimicStairs(mimicStairsMemento);
                MimicStairs.Add(mimicStairs);
                Spawn(mimicStairs);
            }

            foreach (var moneyMemento in eventEntities.Money)
            {
                var money = new Money(moneyMemento);
                _money.Add(money);
                Spawn(money);
            }

            foreach (var stairsMemento in eventEntities.Stairs)
            {
                var stairs = new Stairs(stairsMemento);
                Stairs.Add(stairs);
                Spawn(stairs);
            }

            foreach (var chestMemento in eventEntities.Chests)
            {
                var chest = new Chest(chestMemento);
                _chests.Add(chest);
                Spawn(chest);
            }

            _bonfire = eventEntities.Bonfire.Map(bonfire => new Bonfire(bonfire));
            if (_bonfire.HasValue)
                Spawn(_bonfire.Value!);

            _magicPot = eventEntities.MagicPot.Map(magicPot => new MagicPot(magicPot));
            if (_magicPot.HasValue)
                Spawn(_magicPot.Value!);

            _workbench = eventEntities.Workbench.Map(workbench => new Workbench(workbench));
            if (_workbench.HasValue)
                Spawn(_workbench.Value!);

            foreach (var statueMemento in eventEntities.Statues)
            {
                var statue = new Statue(statueMemento);
                Statues.Add(statue);
                Spawn(statue);
            }

            _standaloneEntityEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
            _standaloneCharacterEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
            _standalonePlayerEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
            _standaloneScheduledEventEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, _) => Remove(entity)
            );
        }

        public EventEntitiesMemento Serialize()
        {
            return new EventEntitiesMemento
            (
                MimicItems.Select(mimicItem => mimicItem.Serialize()).ToList(),
                MimicMoney.Select(mimicMoney => mimicMoney.Serialize()).ToList(),
                MimicStairs.Select(mimicStairs => mimicStairs.Serialize()).ToList(),
                Stairs.Select(stairs => stairs.Serialize()).ToList(),
                _chests.Select(chest => chest.Serialize()).ToList(),
                _traps.Select(trap => trap.Serialize()).ToList(),
                Statues.Select(statue => statue.Serialize()).ToList(),
                _money.Select(money => money.Serialize()).ToList(),
                _bonfire.Map(bonfire => bonfire.Serialize()),
                _magicPot.Map(magicPot => magicPot.Serialize()),
                _workbench.Map(workbench => workbench.Serialize()),
                _teleporter.Map(teleporter => teleporter.Serialize())
            );
        }

        public static EventEntitiesMemento Build(
            IEnumerable<MimicItemMemento> mimicItems,
            IEnumerable<MimicMoneyMemento> mimicMoney,
            IEnumerable<MimicStairsMemento> mimicStairs,
            IEnumerable<StairsMemento> stairs,
            IEnumerable<ChestMemento> chests,
            IEnumerable<TrapMemento> traps,
            IEnumerable<StatueMemento> statues,
            IEnumerable<MoneyMemento> money,
            Option<BonfireMemento> bonfire,
            Option<MagicPotMemento> magicPot,
            Option<WorkbenchMemento> workbench,
            Option<EntityMemento> teleporter
        )
        {
            return new EventEntitiesMemento
            (
                mimicItems.ToList(),
                mimicMoney.ToList(),
                mimicStairs.ToList(),
                stairs.ToList(),
                chests.ToList(),
                traps.ToList(),
                statues.ToList(),
                money.ToList(),
                bonfire,
                magicPot,
                workbench,
                teleporter
            );
        }

        public IObservableCollection<IEntityEventEntity> StandaloneEntityEventEntities =>
            _standaloneEntityEventEntities;
        public IObservableCollection<ICharacterEventEntity> StandaloneCharacterEventEntities =>
            _standaloneCharacterEventEntities;
        public IObservableCollection<IPlayerEventEntity> StandalonePlayerEventEntities =>
            _standalonePlayerEventEntities;
        public IObservableCollection<IScheduledEventEntity> StandaloneScheduledEventEntities => _standaloneScheduledEventEntities;

        public void Spawn(IEntityEventEntity eventEntity)
        {
            _standaloneEntityEventEntities.Add(eventEntity);
        }

        public void AddTrap(Trap trap)
        {
            _traps.Add(trap);
            Spawn(trap);
        }

        public void Spawn(ICharacterEventEntity eventEntity)
        {
            _standaloneCharacterEventEntities.Add(eventEntity);
        }

        public void Spawn(IPlayerEventEntity eventEntity)
        {
            _standalonePlayerEventEntities.Add(eventEntity);
        }

        public void Spawn(IScheduledEventEntity eventEntity)
        {
            _standaloneScheduledEventEntities.Add(eventEntity);
        }

        public void Remove(IEntityEventEntity eventEntity)
        {
            _standaloneEntityEventEntities.Remove(eventEntity);
            if (eventEntity is Trap trap)
            {
                _traps.Remove(trap);
            }
            else if (eventEntity is Teleporter)
            {
                _teleporter = Option<Teleporter>.None;
            }
            else
            {
                throw new Exception($"Unknown {nameof(IEntityEventEntity)} implementation: {eventEntity.GetType()}");
            }
        }

        public void Remove(ICharacterEventEntity eventEntity)
        {
            _standaloneCharacterEventEntities.Remove(eventEntity);
            if (eventEntity is MimicItemEntity mimicItem)
            {
                MimicItems.Remove(mimicItem);
            }
            else if (eventEntity is MimicMoney mimicMoney)
            {
                MimicMoney.Remove(mimicMoney);
            }
            else if (eventEntity is MimicStairs mimicStairs)
            {
                MimicStairs.Remove(mimicStairs);
            }
            else if (eventEntity is Money money)
            {
                _money.Remove(money);
            }
            else
            {
                throw new Exception($"Unknown character event entity: {eventEntity.GetType()}");
            }
        }

        public void Remove(IPlayerEventEntity eventEntity)
        {
            _standalonePlayerEventEntities.Remove(eventEntity);
            if (eventEntity is Chest chest)
            {
                _chests.Remove(chest);
            }
            else if (eventEntity is Stairs stairs)
            {
                Stairs.Remove(stairs);
            }
            else if (eventEntity is Bonfire)
            {
                _bonfire = Option<Bonfire>.None;
            }
            else if (eventEntity is MagicPot)
            {
                _magicPot = Option<MagicPot>.None;
            }
            else if (eventEntity is Workbench)
            {
                _workbench = Option<Workbench>.None;
            }
            else
            {
                throw new Exception($"Unknown event entity: {eventEntity.GetType()}");
            }
        }

        public void Remove(IScheduledEventEntity eventEntity)
        {
            _standaloneScheduledEventEntities.Remove(eventEntity);
            if (eventEntity is Statue statue)
            {
                Statues.Remove(statue);
            }
            else
            {
                throw new Exception($"Unknown event entity: {eventEntity.GetType()}");
            }
        }
    }
}