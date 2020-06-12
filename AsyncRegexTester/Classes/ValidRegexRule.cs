using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace AsyncRegexTester.Classes
{
    public class ValidRegexRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string regexPattern)
            {
                if (!string.IsNullOrWhiteSpace(regexPattern))
                {
                    //Check if regex pattern is valid (ArgumentException if not)
                    try
                    {
                        new Regex(regexPattern);
                        return ValidationResult.ValidResult;
                    }
                    catch
                    {
                        return new ValidationResult(false, "The regex pattern is not valid.");
                    }
                }
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Value is not of type string.");
        }
    }
}
