#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Memento;

namespace Domain.Model.Item
{
    public interface IItemEntity : IDisposable, ISerializable<ItemEntityMemento>, IIconEntity
    {
        public IItem Item { get; }
        public bool ShouldRevealMimic(IMap map);
    }
}