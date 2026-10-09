#nullable enable
using System;

namespace Domain.Model.WorldEvents
{
    public static class FixtureKindExtensions
    {
        public static string Name(this FixtureKind kind) =>
            kind switch
            {
                FixtureKind.Chest => "宝箱",
                FixtureKind.UpStairs or FixtureKind.DownStairs => "階段",
                FixtureKind.MagicCircle => "魔法陣",
                FixtureKind.Bonfire => "焚き火",
                FixtureKind.Workbench => "工作台",
                FixtureKind.MagicPot => "魔法の壺",
                FixtureKind.Teleporter => "テレポーター",
                FixtureKind.Fire => "炎",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

        public static EntityKind ToEntityKind(this FixtureKind kind) =>
            kind switch
            {
                FixtureKind.Chest => EntityKind.Chest,
                FixtureKind.UpStairs => EntityKind.UpStairs,
                FixtureKind.DownStairs => EntityKind.DownStairs,
                FixtureKind.MagicCircle => EntityKind.MagicCircle,
                FixtureKind.Bonfire => EntityKind.Bonfire,
                FixtureKind.Workbench => EntityKind.Workbench,
                FixtureKind.MagicPot => EntityKind.MagicPot,
                FixtureKind.Teleporter => EntityKind.Teleporter,
                FixtureKind.Fire => EntityKind.Fire,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
    }
}
