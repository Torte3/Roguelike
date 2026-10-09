#nullable enable
using System;
using Domain.Model.Item;
using Provider.Texts;

namespace Provider
{
    public static class ItemDescriptionText
    {
        public static string Of(ItemDescription description)
        {
            return description.Name.IsIdentified ? Identified(description) : Unidentified(description);
        }

        private static string Unidentified(ItemDescription description)
        {
            var text = $"{StateText(description.State)}{ItemNameText.Unidentified(description.Name)}\n";
            text += CurseText(description.Curse);
            if (description.CanUse)
                text += "それは使用可能である\n";
            if (description.CanThrow)
                text += "それは投擲可能である\n";
            return text;
        }

        private static string Identified(ItemDescription description)
        {
            var text = $"{StateText(description.State)}{ItemNameText.Of(description.Name)}";
            if (description.IsEquipped is { } isEquipped)
            {
                text += isEquipped
                    ? $" ({ItemDescriptionRichText.RichMeta("装備中")})"
                    : $" ({ItemDescriptionRichText.RichMeta("未装備")})";
            }
            else if (description.MaxUses > 1)
            {
                text += $" ({ItemDescriptionRichText.RichMeta(description.RemainingUses)}/{ItemDescriptionRichText.RichMeta(description.MaxUses)})";
            }

            text += "\n";
            if (description.UpgradeCount > 0)
                text += $"それは{ItemDescriptionRichText.RichMeta(description.UpgradeCount)}/{ItemDescriptionRichText.RichMeta(description.UpgradeLimit)}回強化されている\n";
            text += CurseText(description.Curse);
            text += description.Details;
            if (description.Abilities is { } abilities)
            {
                text += $"\n{AbilityTitle(abilities.Kind)} ({abilities.Count}/{abilities.Limit})\n";
                foreach (var name in abilities.Names)
                    text += $"{name}\n";
            }

            return text;
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

        private static string CurseText(CurseKnowledge curse)
        {
            return curse switch
            {
                CurseKnowledge.Cursed => ItemDescriptionRichText.HarmfulLine("それは呪われている") + "\n",
                CurseKnowledge.NotCursed => "それは呪われていない\n",
                CurseKnowledge.Unknown => "それは呪われているかわからない\n",
                _ => throw new ArgumentOutOfRangeException(nameof(curse), curse, null),
            };
        }
    }
}
