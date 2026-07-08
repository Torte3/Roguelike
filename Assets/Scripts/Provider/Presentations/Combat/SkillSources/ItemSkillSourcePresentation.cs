#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class ItemSkillSourcePresentation : SkillSourcePresentation<ItemSkillSource>
    {
        protected override bool IsWaitingForMovers(SkillUsed used, ItemSkillSource source) =>
            used.IsVisible && source.EquipState == EquipState.NotEquipment;

        protected override IEnumerable<ViewCommand> CommandsOf(SkillUsed used, ItemSkillSource source)
        {
            if (source.EquipState == EquipState.NotEquipment)
                yield return new PlayAttack(used.Entity.Id.Value);
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used, ItemSkillSource source)
        {
            var name = new CharacterNameText(source.Label).Text;
            var itemName = new ItemNameText(source.ItemName).Text;
            yield return AddLog.Line(source.EquipState switch
            {
                EquipState.NotEquipment => $"{name}は{itemName}を使った。",
                EquipState.Unequipped => $"{name}は{itemName}を装備した。",
                EquipState.Equipped => $"{name}は{itemName}を外した。",
                _ => throw new ArgumentOutOfRangeException(nameof(source.EquipState), source.EquipState, null),
            });
            yield return new PlaySE(SoundOf(source));
        }

        private static SEKind SoundOf(ItemSkillSource source)
        {
            return source.EquipState switch
            {
                EquipState.Unequipped => SEKind.Equip,
                EquipState.Equipped => SEKind.Unequip,
                _ => source.Category switch
                {
                    ItemCategory.Potions => SEKind.ItemUsePotion,
                    ItemCategory.Scrolls => SEKind.ItemUseScroll,
                    ItemCategory.Books => SEKind.ItemUseBook,
                    ItemCategory.Wands => SEKind.ItemUseWand,
                    ItemCategory.Weapons => SEKind.ItemUseWeapon,
                    ItemCategory.Others => SEKind.ItemUseOthers,
                    _ => throw new NotImplementedException($"Item category {source.Category} is not implemented"),
                },
            };
        }

        public override ViewCommand ResultLine(string text) => AddLog.Appended(text);
    }
}
