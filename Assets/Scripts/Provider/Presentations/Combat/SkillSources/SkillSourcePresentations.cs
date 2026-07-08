#nullable enable
using System;
using Domain.Model.WorldEvents;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class SkillSourcePresentations
    {
        private readonly CharacterSkillSourcePresentation _character = new();
        private readonly ItemSkillSourcePresentation _item = new();
        private readonly DeviceSkillSourcePresentation _device = new();
        private readonly ThrownItemSkillSourcePresentation _thrownItem = new();

        public ISkillSourcePresentation Of(SkillSource source)
        {
            return source switch
            {
                CharacterSkillSource => _character,
                ItemSkillSource => _item,
                DeviceSkillSource => _device,
                ThrownItemSkillSource => _thrownItem,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source, null),
            };
        }
    }
}
