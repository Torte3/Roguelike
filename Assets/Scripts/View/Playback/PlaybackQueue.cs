#nullable enable
using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace View.Playback
{
    public sealed class PlaybackQueue : IStartable, IDisposable
    {
        private readonly PlaybackContext _context;
        private readonly PlaybackBacklog _backlog;
        private readonly PlaybackClock _clock;
        private IDisposable? _pumping;

        [Inject]
        public PlaybackQueue(PlaybackContext context, PlaybackBacklog backlog, PlaybackClock clock)
        {
            _context = context;
            _backlog = backlog;
            _clock = clock;
        }

        public void Start()
        {
            _pumping = Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(_ => Pump());
        }

        public void Dispose()
        {
            _pumping?.Dispose();
        }

        private bool IsIdle => _backlog.IsEmpty && !_clock.IsBlocked && !_clock.IsAnyEntityBusy;

        public void Enqueue(PlaybackStep step)
        {
            _backlog.Enqueue(step);
        }

        public UniTask WaitUntilIdle()
        {
            return IsIdle ? UniTask.CompletedTask : UniTask.WaitUntil(() => IsIdle);
        }

        private void Pump()
        {
            _clock.ForgetFinishedSubjects();
            while (!_backlog.IsEmpty && !_clock.IsBlocked)
            {
                var step = _backlog.Peek();
                if (step.IsWaitingForMovers && _clock.IsAnyEntityBusy)
                    return;
                if (step.Subject.HasValue && _clock.IsBusy(step.Subject.Value))
                    return;

                _backlog.Dequeue();
                Play(step);
            }
        }

        private void Play(PlaybackStep step)
        {
            var blockSeconds = 0f;
            foreach (var command in step.Commands)
                blockSeconds = Mathf.Max(blockSeconds, command.ApplyAndGetBlockSeconds(_context));

            if (blockSeconds > 0)
                _clock.Block(blockSeconds);
        }
    }
}
