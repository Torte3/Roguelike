using System;

namespace Utilities.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RequiredIfShownAttribute : Attribute
    {
        public string ErrorMessage { get; }

        public RequiredIfShownAttribute()
        {
        }

        public RequiredIfShownAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
