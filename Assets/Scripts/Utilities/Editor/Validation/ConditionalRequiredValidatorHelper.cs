#if UNITY_EDITOR
using System;
using System.Collections;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities.Validation.Editor
{
    internal static class ConditionalRequiredValidatorHelper
    {
        public static void AddRequiredError(InspectorProperty property, string errorMessage, ValidationResult result)
        {
            var message = string.IsNullOrEmpty(errorMessage)
                ? $"<b>{property.NiceName}</b> is required."
                : errorMessage;
            result.AddError(message);
        }

        public static bool IsValueProvided(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is Object unityObject)
            {
                return unityObject != null;
            }

            if (value is string text)
            {
                return !string.IsNullOrEmpty(text);
            }

            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            if (value is IEnumerable enumerable and not string)
            {
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    return enumerator.MoveNext();
                }
                finally
                {
                    (enumerator as IDisposable)?.Dispose();
                }
            }

            return true;
        }
    }
}
#endif
