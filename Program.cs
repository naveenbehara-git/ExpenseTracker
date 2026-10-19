using System.Globalization;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ExpenseTrackerOptions>(
    builder.Configuration.GetSection(ExpenseTrackerOptions.SectionName));
builder.Services.AddSingleton<IExpenseStore, JsonExpenseStore>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute());
})
.AddMvcOptions(options =>
{
    options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();

/// <summary>
/// Accepts both the user's current-culture decimal separator and the invariant
/// dot separator commonly sent by browser number controls.
/// </summary>
public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(decimal)
            ? new BinderTypeModelBinder(typeof(FlexibleDecimalModelBinder))
            : null;
    }
}

public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);
        var rawValue = valueResult.FirstValue?.Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return Task.CompletedTask;
        }

        if (TryParseFlexible(rawValue, out var amount))
        {
            bindingContext.Result = ModelBindingResult.Success(amount);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "Enter a valid amount, using a decimal separator such as . or ,.");
        }

        return Task.CompletedTask;
    }

    private static bool TryParseFlexible(string rawValue, out decimal amount)
    {
        const NumberStyles styles = NumberStyles.AllowLeadingSign |
                                     NumberStyles.AllowDecimalPoint |
                                     NumberStyles.AllowThousands |
                                     NumberStyles.AllowLeadingWhite |
                                     NumberStyles.AllowTrailingWhite;
        var culture = CultureInfo.CurrentCulture;
        var cultureSeparator = culture.NumberFormat.NumberDecimalSeparator;
        var hasSingleDot = rawValue.Count(character => character == '.') == 1;
        var hasSingleComma = rawValue.Count(character => character == ',') == 1;
        var alternateSeparator = cultureSeparator == "." ? ',' : '.';
        if ((hasSingleDot || hasSingleComma) && rawValue.Contains(alternateSeparator) &&
            rawValue[(rawValue.LastIndexOf(alternateSeparator) + 1)..].Length is > 0 and <= 2)
        {
            var alternateNormalized = rawValue.Replace(alternateSeparator.ToString(), ".", StringComparison.Ordinal);
            if (decimal.TryParse(alternateNormalized, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out amount))
            {
                return true;
            }
        }

        if (decimal.TryParse(rawValue, styles, culture, out amount) ||
            decimal.TryParse(rawValue, styles, CultureInfo.InvariantCulture, out amount))
        {
            return true;
        }

        // Normalize a mixed-separator value by treating the final separator as
        // the decimal separator and all earlier separators as grouping marks.
        var lastDot = rawValue.LastIndexOf('.');
        var lastComma = rawValue.LastIndexOf(',');
        var decimalIndex = Math.Max(lastDot, lastComma);
        if (decimalIndex >= 0)
        {
            var whole = new string(rawValue[..decimalIndex].Where(c => c != '.' && c != ',').ToArray());
            var fraction = new string(rawValue[(decimalIndex + 1)..].Where(c => c != '.' && c != ',').ToArray());
            var normalized = $"{whole}.{fraction}";
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount))
            {
                return true;
            }
        }

        amount = default;
        return false;
    }
}
