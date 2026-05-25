#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Memento;

namespace Domain.Service.Effect
{
    public static class SkillExtension
    {
        public static TResult Match<TResult>(this ISkill skill, Func<SpawnEffectSkill, TResult> spawnEffectSkillFunc,
            Func<ItemTargetSkill, TResult> itemTargetSkillFunc, Func<InventoryTargetSkill, TResult> inventoryTargetSkillFunc,
            Func<EquipToggleSkill, TResult> equipToggleSkillFunc)
        {
            return skill switch
            {
                SpawnEffectSkill spawnEffectSkill => spawnEffectSkillFunc(spawnEffectSkill),
                ItemTargetSkill itemTargetSkill => itemTargetSkillFunc(itemTargetSkill),
                InventoryTargetSkill inventoryTargetSkill => inventoryTargetSkillFunc(inventoryTargetSkill),
                EquipToggleSkill equipToggleSkill => equipToggleSkillFunc(equipToggleSkill),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }

        public static async UniTask<TResult> Match<TResult>(this ISkill skill,
            Func<SpawnEffectSkill, UniTask<TResult>> spawnEffectSkillFunc,
            Func<ItemTargetSkill, UniTask<TResult>> itemTargetSkillFunc, Func<InventoryTargetSkill, UniTask<TResult>> inventoryTargetSkillFunc,
            Func<EquipToggleSkill, UniTask<TResult>> equipToggleSkillFunc)
        {
            return skill switch
            {
                SpawnEffectSkill spawnEffectSkill => await spawnEffectSkillFunc(spawnEffectSkill),
                ItemTargetSkill itemTargetSkill => await itemTargetSkillFunc(itemTargetSkill),
                InventoryTargetSkill inventoryTargetSkill => await inventoryTargetSkillFunc(inventoryTargetSkill),
                EquipToggleSkill equipToggleSkill => await equipToggleSkillFunc(equipToggleSkill),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }

        public static TResult Match<TResult>(this ISkillMemento memento,
            Func<SpawnEffectSkillMemento, TResult> spawnEffectSkillFunc,
            Func<ItemTargetSkillMemento, TResult> itemTargetSkillFunc, Func<InventoryTargetSkillMemento, TResult> inventoryTargetSkillFunc,
            Func<EquipToggleSkillMemento, TResult> equipToggleSkillMementoFunc)
        {
            return memento switch
            {
                SpawnEffectSkillMemento skillMemento => spawnEffectSkillFunc(skillMemento),
                ItemTargetSkillMemento itemTargetSkillMemento => itemTargetSkillFunc(itemTargetSkillMemento),
                InventoryTargetSkillMemento inventoryTargetSkillMemento => inventoryTargetSkillFunc(inventoryTargetSkillMemento),
                EquipToggleSkillMemento equipToggleSkillMemento => equipToggleSkillMementoFunc(equipToggleSkillMemento),
                _ => throw new ArgumentException("Invalid skill type")
            };
        }

        public static ISkillMemento Serialize(this ISkill skill)
        {
            return skill.Match<ISkillMemento>(
                spawnEffectSkill => spawnEffectSkill.Serialize(),
                itemTargetSkill => itemTargetSkill.Serialize(),
                inventoryTargetSkill => inventoryTargetSkill.Serialize(),
                equipToggleSkill => equipToggleSkill.Serialize());
        }

        public static ISkill Deserialize(this ISkillMemento memento)
        {
            return memento.Match<ISkill>(
                spawnEffectSkillMemento => new SpawnEffectSkill(spawnEffectSkillMemento),
                itemTargetSkillMemento => new ItemTargetSkill(itemTargetSkillMemento),
                inventoryTargetSkillMemento => new InventoryTargetSkill(inventoryTargetSkillMemento),
                equipToggleSkillMemento => new EquipToggleSkill(equipToggleSkillMemento));
        }
    }
}
