using System.Globalization;
using System.Windows.Controls;

namespace Common
{

    /// <summary>Calls Setting.Validate() and returns a <see cref="ValidationResult"/> with the value.</summary>
    public class SettingValidationRule : ValidationRule
    {

        public ISetting Setting { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
            new ValidationResult(Setting.Validate(value), string.Empty);

    }

}
