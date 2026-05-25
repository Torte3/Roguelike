#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using Utilities.Validation;

[assembly: RegisterValidator(typeof(Utilities.Validation.Editor.RequiredIfShownAttributeValidator))]

namespace Utilities.Validation.Editor
{
    public sealed class RequiredIfShownAttributeValidator : AttributeValidator<RequiredIfShownAttribute, object>
    {
        protected override void Validate(ValidationResult result)
        {
            if (!this.Property.State.Visible)
            {
                return;
            }

            if (ConditionalRequiredValidatorHelper.IsValueProvided(this.Value))
            {
                return;
            }

            ConditionalRequiredValidatorHelper.AddRequiredError(
                this.Property,
                this.Attribute.ErrorMessage,
                result);
        }
    }
}
#endif
