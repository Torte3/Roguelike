using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace View.Effects
{
    public class FlushController : MonoBehaviour
    {
        [SerializeField] private Image _img;
        private SerialDisposable _disposable = new();

        public void Flush(int duration)
        {
            _img.color = new Color(0.5f, 0, 0, 0.5f);
            _disposable.Disposable =
                Observable
                    .Interval(TimeSpan.FromMilliseconds(1000 / 60f))
                    .Take(duration * 60 / 1000)
                    .Index()
                    .Subscribe(x => { _img.color = Color.Lerp(_img.color, Color.clear, x * 1000 / (duration * 60f)); }
                    );
        }
    }
}