namespace Utilities.Table
{
    public interface ICorrectionTable<T>
    {
        T GetRandomItem(float progress);
    }
}