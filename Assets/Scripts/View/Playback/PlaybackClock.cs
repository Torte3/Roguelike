#nullable enable
using System;
using System.Collections.Generic;
using Configuration;
using UnityEngine;

namespace View.Playback
{
    public sealed class PlaybackClock
    {
        private readonly Dictionary<Guid, float> _busyUntil = new();
        private readonly Dictionary<Guid, (float Seconds, int Remaining)> _walkPlans = new();
        private readonly List<Guid> _finishedSubjects = new();
        private readonly PlaybackBacklog _backlog;
        private readonly IDashInput _dash;
        private readonly GlobalSettings _settings;
        private float _anyBusyUntil;
        private float _blockedUntil;

        public PlaybackClock(PlaybackBacklog backlog, IDashInput dash, GlobalSettings settings)
        {
            _backlog = backlog;
            _dash = dash;
            _settings = settings;
        }

        internal bool IsBlocked => Time.time < _blockedUntil;

        internal bool IsAnyEntityBusy => Time.time < _anyBusyUntil;

        private float WalkSeconds => (_dash.IsDashPressed
            ? _settings.DashMilliseconds.CurrentValue
            : _settings.MoveMilliseconds.CurrentValue) / 1000f;

        internal void Reset()
        {
            _busyUntil.Clear();
            _anyBusyUntil = 0;
            _walkPlans.Clear();
            _blockedUntil = 0;
        }

        internal void Block(float seconds)
        {
            _blockedUntil = Time.time + seconds;
        }

        internal float TakeWalkSeconds(Guid subject)
        {
            if (_walkPlans.TryGetValue(subject, out var plan))
            {
                if (plan.Remaining <= 1)
                    _walkPlans.Remove(subject);
                else
                    _walkPlans[subject] = (plan.Seconds, plan.Remaining - 1);
                return plan.Seconds;
            }

            var queuedWalks = _backlog.CountWalksBeforeMoverWait(subject);
            var seconds = WalkSeconds / (queuedWalks + 1);
            if (queuedWalks > 0)
                _walkPlans[subject] = (seconds, queuedWalks);
            return seconds;
        }

        internal void MarkBusy(Guid subject, float seconds)
        {
            _busyUntil[subject] = Time.time + seconds;
            _anyBusyUntil = Mathf.Max(_anyBusyUntil, Time.time + seconds);
        }

        internal float RemainingBusySeconds(Guid subject)
        {
            return _busyUntil.TryGetValue(subject, out var until) ? Mathf.Max(0, until - Time.time) : 0;
        }

        internal bool IsBusy(Guid subject)
        {
            return _busyUntil.TryGetValue(subject, out var until) && Time.time < until;
        }

        internal void ForgetFinishedSubjects()
        {
            foreach (var pair in _busyUntil)
            {
                if (Time.time >= pair.Value)
                    _finishedSubjects.Add(pair.Key);
            }

            foreach (var subject in _finishedSubjects)
                _busyUntil.Remove(subject);
            _finishedSubjects.Clear();
        }
    }
}
