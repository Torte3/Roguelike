#nullable enable
using System.Collections.Generic;
using R3;

namespace Domain.Model.Setting
{
    public record LabeledSlider : IOptionInput
    {
        public readonly IReadOnlyList<(int Value, string Label)> Options;
        public readonly string Name;
        public readonly ReactiveProperty<bool> IsEnabled;
        private readonly int _defaultIndex;
        public LabeledSlider(
            string name,
            IReadOnlyList<(int Value, string Label)> options,
            int defaultIndex,
            ReactiveProperty<bool>? isEnabled = null)
        {
            Name = name;
            Options = options;
            Index = new ReactiveProperty<int>(defaultIndex);
            IsEnabled = isEnabled ?? new ReactiveProperty<bool>(true);
            _defaultIndex = defaultIndex;
        }

        public void Reset()
        {
            Index.Value = _defaultIndex;
        }

        public ReactiveProperty<int> Index { get; init; }
        public ReadOnlyReactiveProperty<int> Value => Index.Select(index => Options[index].Value).ToReadOnlyReactiveProperty();
        public int CurrentIndex => Index.CurrentValue;
        public int CurrentValue => Options[CurrentIndex].Value;
    }
}

