namespace Domain.Model.Item
{
    public interface IItemKnowledge
    {
        public bool IsKnownItem(IReadOnlyItem item);
    }
}
