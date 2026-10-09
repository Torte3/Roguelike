#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;

namespace Provider.Presentations
{
    internal static class Prefabs
    {
        public static string NameOf(Appearance appearance)
        {
            return appearance.Kind switch
            {
                EntityKind.Character => "Character",
                EntityKind.Item => "Item",
                EntityKind.Money => "Money",
                EntityKind.Trap => "Trap",
                EntityKind.Chest => "Chest",
                EntityKind.UpStairs => "UpStairs",
                EntityKind.DownStairs => "DownStairs",
                EntityKind.MagicCircle => "MagicCircle",
                EntityKind.Bonfire => "Bonfire",
                EntityKind.Fire => "Fire",
                EntityKind.Workbench or EntityKind.MagicPot or EntityKind.Statue or EntityKind.Teleporter
                    => appearance.Layer switch
                    {
                        EntityLayer.Middle => "Entity",
                        EntityLayer.Bottom or EntityLayer.Floor => "EntityBottom",
                        _ => throw new NotImplementedException(),
                    },
                _ => throw new NotImplementedException(),
            };
        }
    }
}
