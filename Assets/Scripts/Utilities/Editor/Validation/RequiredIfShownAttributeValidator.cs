#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using Utilities.Validation;

[assembly: RegisterValidator(typeof(Utilities.Editor.Validation.RequiredIfShownAttributeValidator))]

namespace Utilities.Editor.Validation
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
