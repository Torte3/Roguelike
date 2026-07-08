#nullable enable
using System;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemThrownPresentation : Presentation<ItemThrown>
    {
        private readonly ProjectileSight _projectiles;

        public ItemThrownPresentation(ProjectileSight projectiles)
        {
            _projectiles = projectiles;
        }

        protected override Guid? SubjectOf(ItemThrown worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(ItemThrown worldEvent) => true;

        protected override IEnumerable<ViewCommand> CommandsOf(ItemThrown worldEvent)
        {
            foreach (var command in _projectiles.Flew(worldEvent.Flight))
                yield return command;
            yield return new PlayAttack(KeyOf(worldEvent.Entity));
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemThrown worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{new ItemNameText(worldEvent.ItemName).Text}を投げた");
        }
    }
}
