#nullable enable
using Domain.Model;

namespace Domain.Model.Entity
{
    /// <summary><see cref="IEntity"/> を引数に取る <see cref="IEntityEvent"/> を持つスタンドアロン実体（キャラ専用の <see cref="ICharacterEventEntity"/> より広い段）。</summary>
    public interface IEntityEventEntity : IEntity
    {
        public IEntityEvent Event { get; }
    }
}
