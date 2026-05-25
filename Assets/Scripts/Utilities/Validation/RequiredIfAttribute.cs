using System;

namespace Utilities.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RequiredIfAttribute : Attribute
    {
        public string Condition { get; }
        public string ErrorMessage { get; }

        public RequiredIfAttribute(string condition)
        {
            Condition = condition;
        }

        public RequiredIfAttribute(string condition, string errorMessage)
        {
            Condition = condition;
            ErrorMessage = errorMessage;
        }
    }
}
