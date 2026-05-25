using System.Linq;

namespace Domain.Model.Item
{
    public static class RarityWeightTableExtensions
    {
        public static RarityWeightTable<T> Cast<T, U>(this RarityWeightTable<U> table) where T : IHasRarity where U : T
        {
            return new RarityWeightTable<T>(table.Items.Cast<T>().ToList());
        }

        public static RarityWeightTable<T> Concat<T>(this RarityWeightTable<T> table, RarityWeightTable<T> other) where T : IHasRarity
        {
            return new RarityWeightTable<T>(table.Items.Concat(other.Items).ToList());
        }

        public static RarityWeightTable<T> Concat<T, T1, T2>(this RarityWeightTable<T1> table, RarityWeightTable<T2> other) where T : IHasRarity where T1 : T where T2 : T
        {
            return new RarityWeightTable<T>(table.Items.Cast<T>().Concat(other.Items.Cast<T>()).ToList());
        }
    }
}