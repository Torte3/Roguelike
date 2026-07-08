#nullable enable
using System.Collections.Generic;
using Domain.Model.Character;
using Domain.Model.WorldEvents;
using Provider.Texts;
using Utilities;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal abstract class ConditionChangedPresentation<TEvent> : Presentation<TEvent> where TEvent : WorldEvent
    {
        protected static IEnumerable<ViewCommand> ParticlesOf(EntityRef entity, IReadOnlyList<ParticleType> particles)
        {
            yield return new SetParticles(KeyOf(entity), particles);
        }

        protected static IEnumerable<ViewCommand> LogOf(CharacterLabel label, string? conditionLog)
        {
            if (conditionLog != null)
                yield return AddLog.Line($"{new CharacterNameText(label).Text}{conditionLog}");
        }
    }
}
