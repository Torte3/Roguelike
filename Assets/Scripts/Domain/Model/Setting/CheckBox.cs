#nullable enable
using R3;

namespace Domain.Model.Setting
{
    public record CheckBox : IOptionInput
    {
        public readonly string Name;
        public readonly ReadOnlyReactiveProperty<bool> IsEnabled;
        private readonly bool _defaultValue;

        public CheckBox(string name, bool defaultValue, ReactiveProperty<bool>? isEnabled = null)
        {
            Name = name;
            Value = new ReactiveProperty<bool>(defaultValue);
            IsEnabled = isEnabled ?? new ReactiveProperty<bool>(true);
            _defaultValue = defaultValue;
        }

        public void Reset()
        {
            Value.Value = _defaultValue;
        }

        public readonly ReactiveProperty<bool> Value;
        public bool CurrentValue => Value.CurrentValue;
    }
}