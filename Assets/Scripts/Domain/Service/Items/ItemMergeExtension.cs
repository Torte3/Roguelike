#nullable enable
using Domain.Model.Item;

namespace Domain.Service.Items
{
    public static class ItemMergeExtension
    {
        public static bool CanSelectForMergedItem(IItem item, IItem mergeBaseItem)
        {
            if (item == mergeBaseItem)
                return false;
            if (item.IsDiscardBlocked)
                return false;
            return mergeBaseItem.CanAcceptMergeMaterial(item);
        }
    }
}