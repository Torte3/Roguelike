#nullable enable
using View.Entities;

namespace View.Playback.Figures
{
    public abstract record FigureBlueprint(string PrefabName)
    {
        internal abstract EntityFigure Create(EntityView view);
    }
}
