#nullable enable
using UnityEngine;
using Utilities;
using View.Entities;

namespace View.Playback.Figures
{
    internal sealed class ChestFigure : InteractiveFigure
    {
        private StairsLock? _lock;

        public ChestFigure(EntityView view) : base(view)
        {
        }

        public override void SetLockCount(int lockCount)
        {
            if (lockCount > 0)
            {
                if (_lock == null)
                {
                    _lock = Object.Instantiate(ObjectLoader.LoadPrefab("Lock"), View.transform).GetComponent<StairsLock>();
                    Sprite.UpdateVisibility();
                }
                _lock.SetCount(lockCount);
            }
            else if (_lock != null)
            {
                _lock.UnLock();
                _lock = null;
            }
        }
    }
}
