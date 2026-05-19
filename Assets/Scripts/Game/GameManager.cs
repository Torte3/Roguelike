#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Message;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Setting;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Domain.Service.Logs;
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
        public Observable<Unit> OnTurnChanged => _turnController.OnTurnChanged;
        public ReadOnlyReactiveProperty<int> Turn => _turnController.TurnInLevel;
        private GlobalStatistics _globalStatistics;
        public GlobalStatistics GlobalStatistics => _globalStatistics;
        private readonly ReactiveProperty<WorldStatistics?> _activeStatistics = new();
        public ReadOnlyReactiveProperty<WorldStatistics?> ActiveStatistics => _activeStatistics;
        private readonly Subject<BGM> _onPlayBGM = new();
        public Observable<BGM> OnPlayBGM => _onPlayBGM;
        private BGM? _currentBgm;
        private readonly Subject<SE> _onPlaySE = new();
        public Observable<SE> OnPlaySE => _onPlaySE;
        private readonly Subject<ItemCategory> _onPlayItemUseSE = new();
        public Observable<ItemCategory> OnPlayItemUseSE => _onPlayItemUseSE;
        private readonly Subject<OnWorldIconPopupRequestedMessage> _onWorldIconPopupRequested = new();
        public Observable<OnWorldIconPopupRequestedMessage> OnWorldIconPopupRequested => _onWorldIconPopupRequested;
        private readonly ReactiveProperty<GameState> _state = new();
        public ReadOnlyReactiveProperty<GameState> State => _state;
        private readonly SerialDisposable _disposable = new();
        private HashSet<Guid> _eventExecutionIds = new();
        public bool IsEventExecuting => _eventExecutionIds.Count > 0;

        [Inject]
        public GameManager(World world, GameInput input, ChoiceReceiver choiceReceiver,
            CharacterSelectReceiver characterSelectReceiver,
            TextInputReceiver textInputReceiver,
            CharacterControlInputReceiver receiver)
        {
            _world = world;
            _turnController = new TurnController(input);
            _saveDataManager = new SaveDataManager(0);
            _choiceReceiver = choiceReceiver;
            _characterSelectReceiver = characterSelectReceiver;
            _textInputReceiver = textInputReceiver;
            _receiver = receiver;

            _world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                _disposable.Disposable = mapChanged.Map.Player.Character.Entity.OnDestroyed
                    .Where(_ => State.CurrentValue == GameState.Dungeon)
                    .Subscribe(async _ =>
                {
                    await StopMap();
                    Save();
                    GameOver();
                });
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

        public UniTask<int> GetChoiceWithInfo(string? text, params (string choice, string infoTitle, string info)[] choices) =>
            _choiceReceiver.GetChoiceWithInfo(text, choices);

        public UniTask<int> GetChoiceWithInfo(
            string? text,
            int cancelChoiceIndex,
            params (string choice, string infoTitle, string info)[] choices) =>
            _choiceReceiver.GetChoiceWithInfo(text, cancelChoiceIndex, choices);

        public UniTask<int> GetChoice(string? text, params string[] choices) =>
            _choiceReceiver.GetChoice(text, choices);

        public UniTask<int> GetChoice(string? text, int cancelChoiceIndex, params string[] choices) =>
            _choiceReceiver.GetChoice(text, cancelChoiceIndex, choices);

        private async UniTask<PlayerData?> GetPlayerData()
        {
            var knownItemCount = _globalStatistics.KnownItemNames.Count;
            var cursedItemDiscoverCount = _globalStatistics.TotalCursedItemDiscoverCount;
            var stealCount = _globalStatistics.TotalStealCount;
            var monsterHouseEnterCount = _globalStatistics.TotalMonsterHouseEnterCount;
            var totalDamageDealt = _globalStatistics.TotalDamageDealt;
            var maxMapLevel = _globalStatistics.MaxMapLevel;

            var players = new List<(PlayerData data, string unlockCondition, bool usable)> {
                (ObjectLoader.Load<PlayerData>("Adventurer"),
                FormatUnlockCondition("最初から", true), true),
                (ObjectLoader.Load<PlayerData>("knight"),
                FormatUnlockCondition("モンスターハウスに3回入る", monsterHouseEnterCount >= 3, monsterHouseEnterCount, 3), monsterHouseEnterCount >= 3),
                (ObjectLoader.Load<PlayerData>("Priest"),
                FormatUnlockCondition("呪われたアイテムを10個発見", cursedItemDiscoverCount >= 10, cursedItemDiscoverCount, 10), cursedItemDiscoverCount >= 10),
                (ObjectLoader.Load<PlayerData>("Thief"),
                FormatUnlockCondition("泥棒を3回行う", stealCount >= 3, stealCount, 3),
                stealCount >= 3),
                (ObjectLoader.Load<PlayerData>("Witch"),
                FormatUnlockCondition("アイテム50種類発見", knownItemCount >= 50, knownItemCount, 50),
                knownItemCount >= 50),
                (ObjectLoader.Load<PlayerData>("Doctor"),
                FormatUnlockCondition("アイテム70種類発見", knownItemCount >= 70, knownItemCount, 70),
                knownItemCount >= 70),
                (ObjectLoader.Load<PlayerData>("Samurai"),
                FormatUnlockCondition("累計1万ダメージ", totalDamageDealt >= 10000, totalDamageDealt, 10000),
                totalDamageDealt >= 10000),
                (ObjectLoader.Load<PlayerData>("Rabbit"),
                FormatUnlockCondition("10Fまで踏破", maxMapLevel >= 10, maxMapLevel, 10),
                maxMapLevel >= 10),
                (ObjectLoader.Load<PlayerData>("Fairy"),
                FormatUnlockCondition("20Fまで踏破", maxMapLevel >= 20, maxMapLevel, 20),
                maxMapLevel >= 20),
                (ObjectLoader.Load<PlayerData>("Demon Load"),
                FormatUnlockCondition("30Fまで踏破", maxMapLevel >= 30, maxMapLevel, 30),
                maxMapLevel >= 30),
            };
            var index = await _characterSelectReceiver.GetCharacter(
                players.Select(player => (
                    player.data.Name,
                    player.data.CharacterType.SubtypeName(),
                    $"解放条件\n{player.unlockCondition}\n\n{player.data.InfoWithoutName()}",
                    player.usable)).ToList());
            if (index == null)
                return null;
            return players[index.Value].data;
        }

        private static string FormatUnlockCondition(string description, bool usable, int current = 0, int required = 0)
        {
            if (usable || required <= 0)
                return description;

            return $"{description}\n進捗: {Math.Min(current, required)}/{required}";
        }

        public UniTask<string?> GetTextInput(bool canCancel = false)
        {
            return _textInputReceiver.GetTextInput(canCancel);
        }

        public async UniTask Title()
        {
            GameLog.Clear();
            await StopGame();
            var saveData = _saveDataManager.Load();
            if (saveData != null)
            {
                PlayBGM(saveData.Bgm);
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
                PlayBGM(BGM.Normal);
                var playerData = ObjectLoader.Load<PlayerData>("Adventurer");
                var map = CreateSaveData(playerData);
                var _ = await GetChoice(null, "New Game");
                await ChoiceDifficulty();
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
            PlayBGM(BGM.Normal);
            _activeStatistics.Value = new WorldStatistics(WorldStatistics.Build(), this, _world, _globalStatistics);
            Settings.WorldSettings.Reset();

            _world.CreateNew();
            return _world.LoadStartMap(playerData, this);
        }

        private async UniTask ChoiceDifficulty()
        {
            var choice = await GetChoiceWithInfo(null,
                ("Easy", "<color=#00BFFF>- Easy -</color>", "復活できます\nアイテムは自動で鑑定されます\n敵の強さはNormalと同じです"),
                ("Normal", "<color=#FFFF00>- Normal -</color>", "復活できません\nアイテムの詳細は鑑定するまで不明です")
            );
            switch (choice)
            {
                case 0:
                    Settings.WorldSettings.EnableCheat.Value.Value = true;
                    Settings.WorldSettings.RetryOnDead.Value.Value = true;
                    Settings.WorldSettings.AutoIdentify.Value.Value = true;
                    break;
                case 1:
                    break;
            }
        }

        private MapManager LoadSaveData(SaveData saveData)
        {
            PlayBGM(saveData.Bgm);
            _activeStatistics.Value = new WorldStatistics(saveData.Statistics, this, _world, _globalStatistics);
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
            PlayBGM(BGM.Normal);
            var map = _world.LoadMap(mapId, destination, this);
            Save();
            StartMap(map, 0);
            Log.Debug("[Game]End LoadMap");
        }

        public void PlayBGM(BGM bgm)
        {
            if (_currentBgm == bgm)
                return;
            _currentBgm = bgm;
            _onPlayBGM.OnNext(bgm);
        }

        public void PlaySE(SE se)
        {
            _onPlaySE.OnNext(se);
        }

        public void PlayItemUseSE(ItemCategory category)
        {
            _onPlayItemUseSE.OnNext(category);
        }

        public void RequestWorldIconPopup(Sprite icon, Vector2Int position)
        {
            _onWorldIconPopupRequested.OnNext(new OnWorldIconPopupRequestedMessage(icon, position));
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
            var saveData = new SaveData(world, maps, statistics, settings, _turnController.GetWaitTime(), false, _currentBgm ?? BGM.Normal);
            _saveDataManager.SaveFull(globalSaveData, saveData);
            Log.Info("[Game]End Save");
        }

        public void ReturnTitle()
        {
            _state.Value = GameState.Title;
        }

        public void GameOver()
        {
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
            score += player.Money.CurrentValue;
            foreach (var item in player.Character.Inventory.AllItems)
            {
                score += item.GetPrice(_world.CurrentMap.MarketPriceTable);
            }
            return score;
        }
    }
}