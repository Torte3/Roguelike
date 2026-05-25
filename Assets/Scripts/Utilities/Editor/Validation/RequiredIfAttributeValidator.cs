#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Utilities.Validation;

[assembly: RegisterValidator(typeof(Utilities.Validation.Editor.RequiredIfAttributeValidator))]

namespace Utilities.Validation.Editor
{
    public sealed class RequiredIfAttributeValidator : AttributeValidator<RequiredIfAttribute, object>
    {
        private ValueResolver<bool> conditionResolver;

        protected override void Initialize()
        {
            this.conditionResolver = ValueResolver.Get<bool>(this.Property, this.Attribute.Condition);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.conditionResolver.HasError)
            {
                return;
            }

            if (!this.conditionResolver.GetValue())
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
