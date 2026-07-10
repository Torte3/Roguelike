#nullable enable
using Domain.Model;

namespace Domain.Model.Entity
{
    /// <summary>起動時に <see cref="ICharacterEvent"/>（<see cref="ICharacter"/> 引数）で処理するスタンドアロン実体。<see cref="IEntityEventEntity"/> より段が細かい。</summary>
    public interface ICharacterEventEntity : IEntity
    {
        public ICharacterEvent Event { get; }
    }
}
