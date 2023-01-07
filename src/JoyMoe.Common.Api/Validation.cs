using FluentValidation;

namespace JoyMoe.Common.Api;

public static class Validation
{
    public static async IAsyncEnumerable<KeyValuePair<string, string>> ValidateAsync<TRequest>(
        IValidator<TRequest>? validator,
        TRequest?             request) {
        if (validator == null) yield break;
        if (request == null) {
            throw new ArgumentNullException(nameof(request));
        }

        var results = await validator.ValidateAsync(request);
        if (results.IsValid || !results.Errors.Any()) yield break;

        yield return new KeyValuePair<string, string>("errors", results.Errors.Count.ToString());

        foreach (var error in results.Errors) {
            var field  = string.Join('.', error.PropertyName.Split('.').Select(p => p.ToSnakeCase()));
            var code   = error.ErrorCode[..^9].ToSnakeCase();
            var values = error.FormattedMessagePlaceholderValues;
            if (values.TryGetValue("ComparisonValue", out var c)) {
                code += $",{c}";
            } else if (values.ContainsKey("From")) {
                code += $",{values["From"]},{values["To"]}";
            } else if (values.ContainsKey("ExpectedPrecision")) {
                code += $",{values["ExpectedPrecision"]},{values["ExpectedScale"]}";
            } else {
                if (values.TryGetValue("MinLength", out var l)) {
                    code += $",{l}";
                }

                if (values.TryGetValue("MaxLength", out var u) && !u.Equals(l)) {
                    code += $",{u}";
                }
            }

            yield return new KeyValuePair<string, string>("error", $"{field}={code}");
        }
    }
}
