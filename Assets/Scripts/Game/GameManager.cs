#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities;
using VContainer;

namespace Game
{
    public class GameManager : IGameManager
    {
        private readonly World _world;
        private readonly TurnController _turnController;
        private readonly SaveDataManager _saveDataManager;
        private readonly ChoiceReceiver _choiceReceiver;
        private readonly CharacterSelectReceiver _characterSelectReceiver;
        private readonly TextInputReceiver _textInputReceiver;
        private readonly CharacterControlInputReceiver _receiver;
        private readonly TutorialReceiver _tutorialReceiver;
        public ReadOnlyReactiveProperty<int> Turn => _turnController.TurnInLevel;
        private GlobalStatistics _globalStatistics;
        public GlobalStatistics GlobalStatistics => _globalStatistics;
        private readonly ReactiveProperty<WorldStatistics?> _activeStatistics = new();
        public ReadOnlyReactiveProperty<WorldStatistics?> ActiveStatistics => _activeStatistics;
        private readonly ReactiveProperty<GameState> _state = new();
        public ReadOnlyReactiveProperty<GameState> State => _state;
        private readonly SerialDisposable _disposable = new();
        private HashSet<Guid> _eventExecutionIds = new();
        public bool IsEventExecuting => _eventExecutionIds.Count > 0;

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver,
            CharacterSelectReceiver characterSelectReceiver,
            TextInputReceiver textInputReceiver,
            CharacterControlInputReceiver receiver,
            TutorialReceiver tutorialReceiver)
        {
            _world = world;
            _turnController = new TurnController(input);
            _saveDataManager = new SaveDataManager(0);
            _choiceReceiver = choiceReceiver;
            _characterSelectReceiver = characterSelectReceiver;
            _textInputReceiver = textInputReceiver;
            _receiver = receiver;
            _tutorialReceiver = tutorialReceiver;

            _world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var player = mapChanged.Map.Player.Character;
                DeathRecord? death = null;
                _disposable.Disposable = new CompositeDisposable(
                    player.OnPerished.Subscribe(record => death = record),
                    player.Entity.OnDestroyed
                        .Where(_ => State.CurrentValue == GameState.Dungeon)
                        .Subscribe(async _ =>
                        {
                            if (death == null)
                                Log.Error("[Game]The player was removed without dying or being broken.");

                            await StopMap();
                            Save();
                            GameOver(death ?? new DeathRecord(player.Label, new DamageSource(DamageCause.Unknown)));
                        }));
            });

            var globalSaveData = _saveDataManager.LoadGlobal() ?? new GlobalSaveData(GlobalStatistics.Build(), new());
            _globalStatistics = new GlobalStatistics(globalSaveData.GlobalStatistics);
            Settings.GlobalSettings.SetValues(globalSaveData.GlobalSettings);

            var disposable = new SerialDisposable();
            _activeStatistics.SubscribeIncludingCurrentValueIgnoreNull(statistics =>
                disposable.Disposable = Settings.WorldSettings.EnableCheat.Value.Subscribe(value =>
                {
                    if (value)
                    {
                        statistics.IsCheating = true;
                    }
                })
            );
        }

        public UniTask<int> GetChoiceWithItemPreview(ChoiceMessage? message, IMap map, params IItem[] items) =>
            _choiceReceiver.GetChoiceWithItemPreview(message, map, items);

        public UniTask<int> GetChoiceWithItemPreview(
            ChoiceMessage? message,
            IMap map,
            int cancelChoiceIndex,
            params IItem[] items) =>
            _choiceReceiver.GetChoiceWithItemPreview(message, map, cancelChoiceIndex, items);

        public UniTask<int> GetChoice(ChoiceMessage? message, params string[] choices) =>
            _choiceReceiver.GetChoice(message, choices);

        public UniTask<int> GetChoice(ChoiceMessage? message, int cancelChoiceIndex, params string[] choices) =>
            _choiceReceiver.GetChoice(message, cancelChoiceIndex, choices);

        private async UniTask<PlayerData?> GetPlayerData()
        {
            var knownItemCount = _globalStatistics.KnownItemNames.Count;
            var cursedItemDiscoverCount = _globalStatistics.TotalCursedItemDiscoverCount;
            var stealCount = _globalStatistics.TotalStealCount;
            var monsterHouseEnterCount = _globalStatistics.TotalMonsterHouseEnterCount;
            var totalDamageDealt = _globalStatistics.TotalDamageDealt;
            var maxMapLevel = _globalStatistics.MaxMapLevel;

            var players = new List<(PlayerData data, UnlockCondition unlock)> {
                (ObjectLoader.Load<PlayerData>("Adventurer"),
                new UnlockCondition("最初から", 0, 0)),
                (ObjectLoader.Load<PlayerData>("knight"),
                new UnlockCondition("モンスターハウスに3回入る", monsterHouseEnterCount, 3)),
                (ObjectLoader.Load<PlayerData>("Priest"),
                new UnlockCondition("呪われたアイテムを10個発見", cursedItemDiscoverCount, 10)),
                (ObjectLoader.Load<PlayerData>("Thief"),
                new UnlockCondition("泥棒を3回行う", stealCount, 3)),
                (ObjectLoader.Load<PlayerData>("Witch"),
                new UnlockCondition("アイテム50種類発見", knownItemCount, 50)),
                (ObjectLoader.Load<PlayerData>("Doctor"),
                new UnlockCondition("アイテム70種類発見", knownItemCount, 70)),
                (ObjectLoader.Load<PlayerData>("Samurai"),
                new UnlockCondition("累計1万ダメージ", totalDamageDealt, 10000)),
                (ObjectLoader.Load<PlayerData>("Rabbit"),
                new UnlockCondition("10Fまで踏破", maxMapLevel, 10)),
                (ObjectLoader.Load<PlayerData>("Fairy"),
                new UnlockCondition("20Fまで踏破", maxMapLevel, 20)),
                (ObjectLoader.Load<PlayerData>("Demon Load"),
                new UnlockCondition("30Fまで踏破", maxMapLevel, 30)),
            };
            var index = await _characterSelectReceiver.GetCharacter(
                players.Select(player => (
                    player.data.name,
                    player.data.CharacterType.SubtypeName(),
                    player.unlock,
                    player.data.DescriptionWithoutName())).ToList());
            if (index == null)
                return null;
            return players[index.Value].data;
        }

        public UniTask<string?> GetTextInput(bool canCancel = false)
        {
            return _textInputReceiver.GetTextInput(canCancel);
        }

        public async UniTask Title()
        {
            _world.Events.AnnounceCleared();
            await StopGame();
            var saveData = _saveDataManager.Load();
            if (saveData != null)
            {
                var revivePlayer = false;
                LoadPreview(saveData);
                var firstWaitTime = saveData.TurnWaitTime;
                if (!saveData.World.IsPlayerDead)
                {
                    var choice = await GetChoice(null, "Continue", "New Game");
                    switch (choice)
                    {
                        case 0:
                            break;
                        case 1:
                            saveData = null;
                            firstWaitTime = 0;
                            break;
                    }
                }
                else if (Settings.WorldSettings.RetryOnDead.CurrentValue)
                {
                    var choice = await GetChoice(null, "Retry", "New Game");
                    switch (choice)
                    {
                        case 0:
                            revivePlayer = true;
                            break;
                        case 1:
                            saveData = null;
                            firstWaitTime = 0;
                            break;
                    }
                }
                else
                {
                    var _ = await GetChoice(null, "New Game");
                    saveData = null;
                    firstWaitTime = 0;
                }

                MapManager map;
                if (saveData == null)
                {
                    var playerData = await GetPlayerData();
                    if (playerData == null)
                    {
                        await Title();
                        return;
                    }
                    map = CreateSaveData(playerData);
                    await ChoiceDifficulty();
                }
                else if (revivePlayer)
                {
                    map = LoadSaveDataAndRevivePlayer(saveData);
                }
                else
                {
                    map = LoadSaveData(saveData);
                }

                StartGame(map, firstWaitTime);
            }
            else
            {
                var playerData = ObjectLoader.Load<PlayerData>("Adventurer");
                var map = CreateSaveData(playerData);
                var _ = await GetChoice(null, "New Game");
                await ChoiceDifficulty();
                await ShowTutorialIfNeeded(TutorialType.FirstGame);
                StartGame(map, 0);
            }

            _state.Value = GameState.Dungeon;
        }

        private MapManager LoadPreview(SaveData saveData)
        {
            return _world.LoadWorld(saveData.World, saveData.Maps, this, true);
        }

        private MapManager CreateSaveData(PlayerData playerData)
        {
            StartStatistics(WorldStatistics.Build());
            Settings.WorldSettings.Reset();

            _world.CreateNew();
            return _world.LoadStartMap(playerData, this);
        }

        private void StartStatistics(StatisticsMemento memento)
        {
            _activeStatistics.Value?.Dispose();
            _activeStatistics.Value = new WorldStatistics(memento, this, _world, _globalStatistics);
        }

        private async UniTask ChoiceDifficulty()
        {
            var choice = await _choiceReceiver.GetChoiceWithInfo(message: null, defaultIndex: 1, clearPreviousMenus: true,
                ("Easy", new ChoiceMessage("- Easy -", TextTone.Calm), "復活できます\nアイテムは自動で鑑定されます"),
                ("Normal", new ChoiceMessage("- Normal -", TextTone.Caution), "復活できません\nアイテムは自動で鑑定されます"),
                ("Hard", new ChoiceMessage("- Hard -", TextTone.Danger), "復活できません\nアイテムの詳細は鑑定するまで不明です")
            );
            switch (choice)
            {
                case 0:
                    Settings.WorldSettings.EnableCheat.Value.Value = true;
                    Settings.WorldSettings.RetryOnDead.Value.Value = true;
                    Settings.WorldSettings.AutoIdentify.Value.Value = true;
                    break;
                case 1:
                    Settings.WorldSettings.AutoIdentify.Value.Value = true;
                    break;
                case 2:
                    break;
            }
        }

        // 指定種類のチュートリアルを、未表示なら表示する。表示後に記録する
        //（永続化は通常のセーブ契機に任せる）。
        public async UniTask ShowTutorialIfNeeded(TutorialType type)
        {
            if (_globalStatistics.HasShownTutorial(type))
                return;
            await _tutorialReceiver.Show(type);
            _globalStatistics.RecordTutorialShown(type);
        }

        private MapManager LoadSaveData(SaveData saveData)
        {
            StartStatistics(saveData.Statistics);
            Settings.SetValues(saveData.Settings);
            if (saveData.IsRollbacked)
            {
                Log.Info($"[Game]rollback detected");
                _activeStatistics.Value.IsCheating = true;
            }

            return _world.LoadWorld(saveData.World, saveData.Maps, this, true);
        }

        private MapManager LoadSaveDataAndRevivePlayer(SaveData saveData)
        {
            var world = saveData.World.RevivePlayer();
            var map = LoadSaveData(saveData with { World = world });
            var randomPosition = map.GetAllBlankAndStandablePositionsOn().GetAtRandom().Position;
            map.Player.Character.Entity.Teleport(randomPosition);
            map.Player.Character.RestoreToFullHealth();
            map.Player.Character.Turn(Direction8.Down);
            return map;
        }

        private void StartGame(MapManager map, float firstWaitTime)
        {
            StartMap(map, firstWaitTime);
        }

        private void StartMap(MapManager map, float firstWaitTime)
        {
            Save();
            _receiver.Enable(true);
            _turnController.Run(this, map, firstWaitTime);
        }

        private async UniTask StopGame()
        {
            await StopMap();
        }

        private async UniTask StopMap()
        {
            _receiver.Enable(false);
            await _turnController.Stop();
        }

        public async void MoveMap(Id<IMap> mapId, Id<IEntity>? destination = null)
        {
            Log.Debug("[Game]Start LoadMap");
            await StopMap();
            var map = _world.LoadMap(mapId, destination, this);
            Save();
            // 初めて30Fに到達したときにチュートリアルを表示する（map.Depth が階層）。
            if (map.Depth >= 30)
                await ShowTutorialIfNeeded(TutorialType.Floor30);
            StartMap(map, 0);
            Log.Debug("[Game]End LoadMap");
        }

        public void SaveLight()
        {
            _saveDataManager.SaveLight(Turn.CurrentValue);
        }

        public void Save()
        {
            Log.Info("[Game]Save");
            var globalStatistics = _globalStatistics.Serialize();
            var globalSettings = Settings.GlobalSettings.GetValues();
            var globalSaveData = new GlobalSaveData(globalStatistics, globalSettings);

            var world = _world.Serialize();
            var maps = _world.SerializeUpdatedMaps().ToDictionary(map => map.Id, map => map);
            var statistics = _activeStatistics.Value.Serialize();
            var settings = Settings.WorldSettings.GetValues();
            var saveData = new SaveData(world, maps, statistics, settings, _turnController.GetWaitTime(), false);
            _saveDataManager.SaveFull(globalSaveData, saveData);
            Log.Info("[Game]End Save");
        }

        public void ReturnTitle()
        {
            _state.Value = GameState.Title;
        }

        private void GameOver(DeathRecord death)
        {
            _world.Events.Record(new GameOver(death, _activeStatistics.Value!.MaxMapLevel, GetScore()));
            _state.Value = GameState.Title;
        }

        public void Exit()
        {
            Application.Quit();
        }

        public Guid StartEvent()
        {
            var eventId = Guid.NewGuid();
            _eventExecutionIds.Add(eventId);
            return eventId;
        }

        public void EndEvent(Guid eventId)
        {
            if (!_eventExecutionIds.Contains(eventId))
                throw new Exception($"EventId {eventId} not found");
            _eventExecutionIds.Remove(eventId);
        }

        public float GetScore()
        {
            var score = 0f;

            score += Mathf.Pow(_globalStatistics.MaxMapLevel - 1, 2) * 100;

            var player = _world.CurrentMap.Player;
            score += player.Money;
            foreach (var item in player.Character.Inventory.AllItems)
            {
                score += item.GetPrice(_world.CurrentMap.MarketPriceTable);
            }
            return score;
        }
    }
}