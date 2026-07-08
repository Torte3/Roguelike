#nullable enable
using View.Entities;

namespace View.Playback.Figures
{
    public sealed record ChestFigureBlueprint(string PrefabName) : FigureBlueprint(PrefabName)
    {
        internal override EntityFigure Create(EntityView view) => new ChestFigure(view);
    }
}
