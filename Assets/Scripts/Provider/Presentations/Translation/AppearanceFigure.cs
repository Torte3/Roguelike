#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using View.Playback.Commands;
using View.Playback.Figures;

namespace Provider.Presentations.Translation
{
    internal sealed class AppearanceFigure
    {
        private readonly Appearance _appearance;

        public AppearanceFigure(Appearance appearance)
        {
            _appearance = appearance;
        }

        public AddEntity AddCommandFor(EntityRef entity) =>
            new(entity.Id.Value, Blueprint, entity.Position, _appearance.Icon, _appearance.IsShiny, entity.IsVisible);

        public FigureBlueprint Blueprint => _appearance.Kind switch
        {
            EntityKind.Character => new PlainFigureBlueprint("Character"),
            EntityKind.Item => new PlainFigureBlueprint("Item"),
            EntityKind.Money => new PlainFigureBlueprint("Money"),
            EntityKind.Trap => new PlainFigureBlueprint("Trap"),
            EntityKind.Fire => new PlainFigureBlueprint("Fire"),
            EntityKind.Chest => new ChestFigureBlueprint("Chest"),
            EntityKind.UpStairs => new InteractiveFigureBlueprint("UpStairs"),
            EntityKind.DownStairs => new InteractiveFigureBlueprint("DownStairs"),
            EntityKind.MagicCircle => new InteractiveFigureBlueprint("MagicCircle"),
            EntityKind.Bonfire => new BonfireFigureBlueprint("Bonfire"),
            EntityKind.Workbench or EntityKind.MagicPot or EntityKind.Statue or EntityKind.Teleporter
                => _appearance.Layer switch
                {
                    EntityLayer.Middle => new InteractiveFigureBlueprint("Entity"),
                    EntityLayer.Bottom or EntityLayer.Floor => new InteractiveFigureBlueprint("EntityBottom"),
                    _ => throw new NotImplementedException(),
                },
            _ => throw new NotImplementedException(),
        };
    }
}
