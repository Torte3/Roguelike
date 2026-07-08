#nullable enable
using System;
using Domain.Model;

namespace Provider.Texts
{
    internal sealed class UnlockText
    {
        private readonly UnlockCondition _unlock;
        private readonly string _description;

        public UnlockText(UnlockCondition unlock, string description)
        {
            _unlock = unlock;
            _description = description;
        }

        public string Text => $"解放条件\n{Condition}\n\n{_description}";

        private string Condition => _unlock.IsUnlocked
            ? _unlock.Description
            : $"{_unlock.Description}\n進捗: {Math.Min(_unlock.Current, _unlock.Required)}/{_unlock.Required}";
    }
}
