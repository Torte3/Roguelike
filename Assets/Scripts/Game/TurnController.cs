using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Configuration;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Map;
using Domain.Model.WorldEvents;
using R3;
using Unity.Logging;
using UnityEngine;
using Utilities.Stats;

namespace Game
{
    internal sealed class TurnController
    {
        private readonly GameInput _input;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private UniTaskCompletionSource _runCompletionSource;
        private ReactiveProperty<int> _turnInLevel = new(0);
        private Resource _turnWaitTime { get; init; }
        public ReadOnlyReactiveProperty<int> TurnInLevel => _turnInLevel;

        public TurnController(GameInput input)
        {
            _input = input;
            _turnWaitTime = new Resource(1);
        }

        public async void Run(IGameManager gameManager, IMap map, float firstWaitTime)
        {
            _turnWaitTime.Set(firstWaitTime);
            _turnInLevel.Value = 0;
            if (_isRunning)
                throw new Exception("Turn is already running");
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _runCompletionSource = new UniTaskCompletionSource();

            while (!_cancellationTokenSource.Token.IsCancellationRequested && map.Characters.Any())
            {
                var characters = map.Characters
                    .OrderByDescending(character => character.IsPlayer)
                    .ToList();
                var timeStopped = characters.Any(character => character.Status.IsFlagStat(FlagStatType.OverDrive));
                if (timeStopped)
                {
                    characters.RemoveAll(character => !character.Status.IsFlagStat(FlagStatType.OverDrive));
                }

                var minWaitTime = characters.Min(character =>
                    character.Status.GetStatValue(StatType.MaxWaitTime) - character.Status.WaitTimeValue.CurrentValue);
                minWaitTime = Mathf.Min(minWaitTime,
                    _turnWaitTime.Max.CurrentValue - _turnWaitTime.Value.CurrentValue);

                foreach (var character in characters)
                {
                    if (timeStopped && !character.Status.IsFlagStat(FlagStatType.OverDrive))
                        continue;

                    character.Status.AddWaitTime(minWaitTime);
                    if (character.Status.IsWaitTimeFull())
                    {
                        character.UpdateCharacterTurn();
                        switch (character.State)
                        {
                            case CharacterState.Wait:
                                await DoCharacterAction(gameManager, map, character);
                                break;
                            case CharacterState.Act:
                            case CharacterState.Finish:
                                Log.Debug($"[Turn]{character.Name} already did action.");
                                break;
                            case CharacterState.Think:
                                throw new InvalidOperationException(
                                    $"[Turn] {character.Name} is unexpectedly in thinking state before their turn to action decision");
                        }

                        await WaitWhileIfNeeded(() => gameManager.IsEventExecuting);
                    }

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        _isRunning = false;
                        _runCompletionSource.TrySetResult();
                        Log.Debug("[Turn]Loop canceled.");
                        return;
                    }
                }

                await Update(gameManager, map, characters, timeStopped, minWaitTime);

                foreach (var character in characters.Where(character => character.Status.IsWaitTimeFull()))
                {
                    character.Status.ResetWaitTime();
                    character.SetWaitState();
                }

                if (Settings.GlobalSettings.AutoSave.CurrentValue)
                {
                    gameManager.Save();
                }
                else
                {
                    gameManager.SaveLight();
                }
            }

            _isRunning = false;
            _runCompletionSource.TrySetResult();
        }

        private static UniTask WaitWhileIfNeeded(Func<bool> predicate)
        {
            return predicate() ? UniTask.WaitWhile(predicate) : UniTask.CompletedTask;
        }

        private async UniTask Update(IGameManager gameManager, IMap map, List<ICharacter> characters, bool timeStopped, float minWaitTime)
        {
            _turnWaitTime.Gain(minWaitTime);
            if (!_turnWaitTime.IsFull())
                return;

            foreach (var character in characters)
            {
                if (timeStopped && !character.Status.IsFlagStat(FlagStatType.OverDrive))
                    continue;

                if (character.IsDead)
                    continue;

                await character.UpdateTurn();
            }

            _turnInLevel.Value++;
            map.Events.Record(new TurnPassed(_turnInLevel.Value));
            Log.Debug($"[Turn]Start turn in level:{_turnInLevel.Value})\nCharacters:{map.Characters.Count}");
            if (!timeStopped)
                await map.UpdateTurn(_turnInLevel.Value);

            _turnWaitTime.Set(0);
        }

        private async UniTask DoCharacterAction(IGameManager gameManager, IMap map, ICharacter character)
        {
            if (character.Status.IsFlagStat(FlagStatType.CannotAct) || character.IsDead)
            {
                character.ResetChargeAction();
                Log.Debug($"[Turn]{character.Name} cannot act.");
                map.Events.Record(new CharacterTurnSkipped(character.Entity.Ref, character.Label));
            }
            else
            {
                Log.Debug($"[Turn]{character.Name} think...");
                try
                {
                    await character.DoNextAction(gameManager, map, _input)
                        .AttachExternalCancellation(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException e)
                {
                    Log.Debug($"OperationCanceledException: {e}");
                    return;
                }
            }
        }

        public async UniTask Stop()
        {
            if (!_isRunning) return;
            _cancellationTokenSource.Cancel();
            await _runCompletionSource.Task;
            Log.Debug("[Turn]Stop");
        }

        public float GetWaitTime()
        {
            return _turnWaitTime.Value.CurrentValue;
        }
    }
}