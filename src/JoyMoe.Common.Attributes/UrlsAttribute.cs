using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace JoyMoe.Common.Attributes
{
    /// <summary>
    /// Provides URL validation for an array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class UrlsAttribute : DataTypeAttribute
    {
        private readonly UrlAttribute _url;

        public UrlsAttribute() : base(DataType.Url)
        {
            ErrorMessage = "The {0} field is not a valid fully-qualified http, https, or ftp URL.";

            _url = new UrlAttribute();
        }

        public override bool IsValid(object value)
        {
            if (!(value is string[] valueAsStringArray))
            {
                return true;
            }

            return valueAsStringArray.All(valueAsString => _url.IsValid(valueAsString));
        }
    }
}
