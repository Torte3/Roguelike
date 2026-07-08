#nullable enable
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

namespace View.Playback
{
    public sealed class PlaybackQueue
    {
        private readonly Queue<PlaybackStep> _pending = new();
        private readonly Dictionary<EntityKey, float> _busyUntil = new();
        private readonly Dictionary<EntityKey, (float Seconds, int Remaining)> _walkPlans = new();
        private readonly PlaybackContext _context;
        private readonly DashState _dash;
        private float _anyBusyUntil;
        private float _blockedUntil;

        [Inject]
        public PlaybackQueue(PlaybackContext context, DashState dash)
        {
            _context = context;
            _dash = dash;
            context.Queue = this;
            Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate).Subscribe(_ => Pump());
        }

        private bool IsIdle => _pending.Count == 0 && Time.time >= _blockedUntil && !IsAnyEntityBusy();

        public void Enqueue(PlaybackStep step)
        {
            _pending.Enqueue(step);
        }

        public UniTask WaitUntilIdle()
        {
            return IsIdle ? UniTask.CompletedTask : UniTask.WaitUntil(() => IsIdle);
        }

        internal void ResetTimings()
        {
            _busyUntil.Clear();
            _anyBusyUntil = 0;
            _walkPlans.Clear();
            _blockedUntil = 0;
        }

        internal float TakeWalkSeconds(EntityKey subject)
        {
            if (_walkPlans.TryGetValue(subject, out var plan))
            {
                if (plan.Remaining <= 1)
                    _walkPlans.Remove(subject);
                else
                    _walkPlans[subject] = (plan.Seconds, plan.Remaining - 1);
                return plan.Seconds;
            }

            var queuedWalks = _pending
                .TakeWhile(step => !step.WaitsForMovers)
                .Count(step => step.Ops.Any(op => op.IsWalkOf(subject)));
            var seconds = WalkSeconds / (queuedWalks + 1);
            if (queuedWalks > 0)
                _walkPlans[subject] = (seconds, queuedWalks);
            return seconds;
        }

        private float WalkSeconds => (_dash.IsDashing()
            ? Settings.GlobalSettings.DashMilliseconds.CurrentValue
            : Settings.GlobalSettings.MoveMilliseconds.CurrentValue) / 1000f;

        private void Pump()
        {
            while (_pending.Count > 0 && Time.time >= _blockedUntil)
            {
                var step = _pending.Peek();
                if (step.WaitsForMovers && IsAnyEntityBusy())
                    return;
                if (step.Subject != null && IsBusy(step.Subject))
                    return;

                _pending.Dequeue();
                Play(step);
            }
        }

        private void Play(PlaybackStep step)
        {
            var blockSeconds = 0f;
            foreach (var op in step.Ops)
                blockSeconds = Mathf.Max(blockSeconds, op.Apply(_context));

            if (blockSeconds > 0)
                _blockedUntil = Time.time + blockSeconds;
        }

        internal void MarkBusy(EntityKey subject, float seconds)
        {
            _busyUntil[subject] = Time.time + seconds;
            _anyBusyUntil = Mathf.Max(_anyBusyUntil, Time.time + seconds);
        }

        private bool IsBusy(EntityKey subject)
        {
            if (!_busyUntil.TryGetValue(subject, out var until))
                return false;
            if (Time.time < until)
                return true;
            _busyUntil.Remove(subject);
            return false;
        }

        private bool IsAnyEntityBusy()
        {
            return Time.time < _anyBusyUntil;
        }
    }
}
