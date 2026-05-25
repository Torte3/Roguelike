using System.Collections.Generic;

namespace Utilities.Table
{
    public interface ITable<T>
    {
        T GetRandomItem();
        List<T> GetRandomItems(int count);
    }
}