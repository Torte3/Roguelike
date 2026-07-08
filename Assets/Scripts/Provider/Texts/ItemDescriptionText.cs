#nullable enable
using System;
using Domain.Model.Item;
using Domain.Model.Item.Description;
using Provider.Texts;

namespace Provider.Texts
{
    public sealed class ItemDescriptionText
    {
        private readonly ItemDescription _description;

        public ItemDescriptionText(ItemDescription description)
        {
            _description = description;
        }

        public string Text => _description.Name.IsIdentified ? Identified(_description) : Unidentified(_description);

        private static string Unidentified(ItemDescription description)
        {
            var text = $"{StateText(description.State)}{new ItemNameText(description.Name).Text}\n";
            text += CurseText(description);
            if (description.CanUse)
                text += "それは使用可能である\n";
            if (description.CanThrow)
                text += "それは投擲可能である\n";
            return text;
        }

        private static string Identified(ItemDescription description)
        {
            var text = $"{StateText(description.State)}{new ItemNameText(description.Name).Text}";
            text += EquipText(description);
            text += "\n";
            if (description.UpgradeCount > 0)
                text += $"それは{ItemDescriptionRichText.RichMeta(description.UpgradeCount)}/{ItemDescriptionRichText.RichMeta(description.UpgradeLimit)}回強化されている\n";
            text += CurseText(description);
            text += description.Details;
            if (description.Abilities is { } abilities)
            {
                text += $"\n{AbilityTitle(abilities.Kind)} ({abilities.Names.Count}/{abilities.Limit})\n";
                foreach (var name in abilities.Names)
                    text += $"{name}\n";
            }

            return text;
        }

        private static string EquipText(ItemDescription description)
        {
            return description.EquipState switch
            {
                EquipState.Equipped => $" ({ItemDescriptionRichText.RichMeta("装備中")})",
                EquipState.Unequipped => $" ({ItemDescriptionRichText.RichMeta("未装備")})",
                EquipState.NotEquipment => description.MaxUses > 1
                    ? $" ({ItemDescriptionRichText.RichMeta(description.RemainingUses)}/{ItemDescriptionRichText.RichMeta(description.MaxUses)})"
                    : "",
                _ => throw new ArgumentOutOfRangeException(nameof(description.EquipState), description.EquipState, null),
            };
        }

        private static string AbilityTitle(ItemAbilityKind kind)
        {
            return kind switch
            {
                ItemAbilityKind.Features => "能力",
                ItemAbilityKind.PassiveSkills => "パッシブスキル",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            };
        }

        private static string StateText(ItemState state)
        {
            return state switch
            {
                ItemState.ShopItem => "[売品]",
                ItemState.UsedShopItem => "[売品(使用済み)]",
                ItemState.Stolen => "[盗品]",
                ItemState.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            };
        }

        private static string CurseText(ItemDescription description)
        {
            if (!description.IsCurseIdentified)
                return "それは呪われているかわからない\n";
            return description.IsCursed
                ? ItemDescriptionRichText.HarmfulLine("それは呪われている") + "\n"
                : "それは呪われていない\n";
        }
    }
}
