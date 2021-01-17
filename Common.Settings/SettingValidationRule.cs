using System.Globalization;
using System.Windows.Controls;

namespace Common
{
    public class SettingValidationRule : ValidationRule
    {

        public ISetting Setting { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) =>
            new ValidationResult(Setting.Validate(value), string.Empty);

    }

}
