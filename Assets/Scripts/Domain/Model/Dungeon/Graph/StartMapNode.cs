using UnityEngine;
using XNode;

namespace Domain.Model.Dungeon.Graph
{
    [CreateNodeMenu("Start"), NodeWidth(160)]
    public class StartMapNode : Node
    {
        [Output(ShowBackingValue.Never, typeConstraint: TypeConstraint.Strict), SerializeField]
        private StairsLink _firstMap;
    }
}
