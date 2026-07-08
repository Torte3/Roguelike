#nullable enable
using View.Entities;

namespace View.Playback.Figures
{
    public sealed record InteractiveFigureBlueprint(string PrefabName) : FigureBlueprint(PrefabName)
    {
        internal override EntityFigure Create(EntityView view) => new InteractiveFigure(view);
    }
}
