#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class ItemSkillSourcePresentation : SkillSourcePresentation<ItemSkillSource>
    {
        protected override bool WaitsForMovers(SkillUsed used, ItemSkillSource source) =>
            used.IsVisible && source.Kind == ItemUseKind.Use;

        protected override IEnumerable<ViewOp> OpsOf(SkillUsed used, ItemSkillSource source)
        {
            if (source.Kind == ItemUseKind.Use)
                yield return new PlayAttack(used.Entity.Key());
            if (!used.IsVisible)
                yield break;

            var name = Names.Of(source.Label);
            var item = Names.Of(source.ItemName);
            yield return Logs.Line(source.Kind switch
            {
                ItemUseKind.Use => $"{name}は{item}を使った。",
                ItemUseKind.Equip => $"{name}は{item}を装備した。",
                ItemUseKind.Unequip => $"{name}は{item}を外した。",
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, null),
            });
            yield return new PlaySe(SoundOf(source));
        }

        private static SeKind SoundOf(ItemSkillSource source)
        {
            return source.Kind switch
            {
                ItemUseKind.Equip => SeKind.Equip,
                ItemUseKind.Unequip => SeKind.Unequip,
                _ => source.Category switch
                {
                    ItemCategory.Potions => SeKind.ItemUsePotion,
                    ItemCategory.Scrolls => SeKind.ItemUseScroll,
                    ItemCategory.Books => SeKind.ItemUseBook,
                    ItemCategory.Wands => SeKind.ItemUseWand,
                    ItemCategory.Weapons => SeKind.ItemUseWeapon,
                    ItemCategory.Others => SeKind.ItemUseOthers,
                    _ => throw new NotImplementedException($"Item category {source.Category} is not implemented"),
                },
            };
        }
    }
}
