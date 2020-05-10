using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JoyMoe.Common.Validation
{
    /// <summary>
    /// Specifies at least one data field value is provided.
    /// </summary>
    public class OneOfAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext ctx)
        {
            var properties = ctx.ObjectType.GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(OneOfAttribute)))
                .Count(p => p.GetValue(ctx.ObjectInstance) is string stringValue && stringValue.Trim().Length != 0);

            return properties == 1
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}
