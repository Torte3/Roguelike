#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;

namespace Domain.Service.Effect
{
    public class InventoryTargetSkill : ISerializable<InventoryTargetSkillMemento>, ISkill
    {
        private readonly IInventoryEffect _inventoryEffect;
        public bool IsDirectional => false;
        public bool IsUsable() => true;

        public InventoryTargetSkill(InventoryTargetSkillMemento memento)
        {
            _inventoryEffect = memento.InventoryEffect;
        }

        public InventoryTargetSkillMemento Serialize()
        {
            return new InventoryTargetSkillMemento
            (
                _inventoryEffect
            );
        }

        public static InventoryTargetSkillMemento Build(IInventoryEffect inventoryEffect)
        {
            return new InventoryTargetSkillMemento
            (
                inventoryEffect
            );
        }

        public UniTask<ISkillResult> Use(IStorage storage, IEntity itemHolder, IMap map)
        {
            _inventoryEffect.Apply(map.Player, storage, itemHolder, map.ItemPlaceholders);
            return UniTask.FromResult((ISkillResult)InventoryTargetSkillResult.Success);
        }

        public float Evaluate() => 0;

        public float EvaluatePrice()
        {
            return _inventoryEffect.EvaluatePrice();
        }

        public string Info(bool hasStorage = false)
        {
            var info = "";
            if (hasStorage)
            {
                info += $"中身を対象にして\n";
            }
            else
            {
                info += $"使用者のインベントリを対象にして\n";
            }
            info += _inventoryEffect.Info();
            return info;
        }
    }
}