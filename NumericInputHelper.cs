using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using Sandtrap.Extensions;
using Sandtrap.Web.Extensions;
using System.Linq;

namespace Sandtrap.Web.Html
{

    // TODO: Add parameter for rounding
    // No decimals if type = int

    /// <summary>
    /// Renders the html for a custom numeric control.
    /// </summary>
    public static class NumericInputHelper
    {

        #region .Declarations 

        // Error messages
        private const string _NotNumeric = "The property {0} is type {1} which is not numeric";

        #endregion

        #region .Methods 

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="helper">
        /// The HtmlHelper instance that this method extends.
        /// </param>
        /// <param name="expression">
        /// An expression that identifies the property to display.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the property is type not a numeric type.
        /// </exception>
        public static MvcHtmlString NumericInputFor<TModel, TValue> (this HtmlHelper<TModel> helper, 
            Expression<Func<TModel, TValue>> expression)
        {
            // Get the model metadata
            ModelMetadata metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            // Get the fully qualified name of the property
            string fieldName = ExpressionHelper.GetExpressionText(expression);
            // Get the validation attributes
            IDictionary<string, object> attributes = helper.GetUnobtrusiveValidationAttributes(fieldName, metaData);
            // Return the html
            return MvcHtmlString.Create(NumericInput(metaData, fieldName, attributes, true));
        }

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        /// <param name="attributes">
        /// The validation attributes to rendered in the input.
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        /// <returns></returns>
        public static string NumericInputForMetadata(ModelMetadata metaData,
            string fieldName, IDictionary<string, object> attributes, bool includeID = false)
        {
            // Check its numeric
            if (!metaData.ModelType.IsNumeric())
            {
                throw new ArgumentException(string
                    .Format(_NotNumeric, fieldName, metaData.ModelType.Name));
            }
            return NumericInput(metaData, fieldName, attributes, includeID);
        }

        #endregion

        #region .Helper methods 

        /// <summary>
        /// Returns the html for a numeric input that formats the display of values.
        /// </summary>
        /// <param name="metaData">
        /// The metadata of the property.
        /// </param>
        /// <param name="fieldName">
        /// The fully qualified name of the property.
        /// </param>
        /// <param name="attributes">
        /// The validation attributes to rendered in the input.
        /// </param>
        /// <param name="includeID">
        /// A value indicating the the 'id' attribute should be rendered for the input.
        /// </param>
        private static string NumericInput(ModelMetadata metaData, string fieldName,
            IDictionary<string, object> attributes, bool includeID)
        {
            string defaultFormat = "{0:N}";
            StringBuilder html = new StringBuilder();
            // Build formatted display text
            TagBuilder display = new TagBuilder("div");
            // Add essential styles
            display.MergeAttribute("style", "position:absolute;");
            // Add class names
            display.AddCssClass("numeric-text");
            if (Convert.ToDouble(metaData.Model) < 0)
            {
                display.AddCssClass("negative");
            }
            if (metaData.IsCurrency())
            {
                defaultFormat = "{0:C}";
                display.AddCssClass("currency");
            }
            else if (metaData.IsPercent())
            {
                defaultFormat = "{0:P}";
                display.AddCssClass("percent");
            }
            else
            {
                display.AddCssClass("number");
            }
            if (Type.GetTypeCode(metaData.ModelType) == TypeCode.Int32)
            {
                defaultFormat = "{0:N0}";
            }
            // Format text
            defaultFormat = metaData.DisplayFormatString ?? defaultFormat;
            display.InnerHtml = string.Format(defaultFormat, metaData.Model ?? metaData.DefaultValue());

            // TODO: What about nullable types
            //metaData.NullDisplayText;

            html.Append(display.ToString());
            // Build input
            TagBuilder input = new TagBuilder("input");
            input.AddCssClass("numeric-input");
            input.MergeAttribute("autocomplete", "off");
            input.MergeAttribute("type", "text");
            if (includeID)
            {
                input.MergeAttribute("id", HtmlHelper.GenerateIdFromName(fieldName));
            }
            input.MergeAttribute("name", fieldName);
            input.MergeAttribute("value", string.Format("{0}", metaData.Model ?? metaData.DefaultValue()));
            // Remove the data-val-number attribute (the client script ensures its a number and jquery 
            // validation generates an error message if the first character is a decimal point which
            // disappears as soon as the next digit is entered - PITA when entering percentage values)
            // TODO: Still happens if a Range attribute is added!
            if (attributes != null && attributes.ContainsKey("data-val-number"))
            {
                attributes.Remove("data-val-number");
            }
            input.MergeAttributes(attributes);
            html.Append(input.ToString());
            // Build container
            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("numeric-container");
            // Add data attributes for use by client script
            if (metaData.IsCurrency())
            {
                //container.MergeAttributes(GetCurrencyDataAttributes(metaData.DisplayFormatString));
                container.MergeAttributes(GetCurrencyDataAttributes(defaultFormat));
            }
            else if (metaData.IsPercent())
            {
                //container.MergeAttributes(GetPercentDataAttributes(metaData.DisplayFormatString));
                container.MergeAttributes(GetPercentDataAttributes(defaultFormat));
            }
            else
            {
                //container.MergeAttributes(GetNumberDataAttributes(metaData.DisplayFormatString));
                container.MergeAttributes(GetNumberDataAttributes(defaultFormat));
            }
            // Add essential styles
            container.MergeAttribute("style", "position:relative;margin:0;padding:0;");
            container.InnerHtml = html.ToString();
            // Return the html
            return container.ToString();
        }

        /// <summary>
        /// Gets the data attributes for a currency value
        /// </summary>
        internal static Dictionary<string, string> GetCurrencyDataAttributes(string formatString)
        {
            // Initialise attributes
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            // Get the number format
            NumberFormatInfo format = Thread.CurrentThread.CurrentCulture.NumberFormat;
            // Precision
            attributes["data-precision"] = format.CurrencyDecimalDigits.ToString();
            if (formatString != null)
            {
                // Get the number of decimal places
                Regex regex = new Regex(@"{\d{1}:C(\d?)}", RegexOptions.IgnoreCase);
                Match match = regex.Match(formatString);
                if (match.Success && match.Groups[1].Value.Length > 0)
                {
                    attributes["data-precision"] = match.Groups[1].Value;
                }
            }
            // Decimal seperator (hex)
            attributes["data-decimalSeperator"] = ((int)format.CurrencyDecimalSeparator[0]).ToString("X");
            // Negative sign (hex)
            attributes["data-negativeSign"] = ((int)format.NegativeSign[0]).ToString("X");
            // Group separator and size
            if (format.CurrencyGroupSizes[0] != 0)
            {
                attributes["data-groupSeperator"] = ((int)format.CurrencyGroupSeparator[0]).ToString("X");
                attributes["data-groupSizes"] = string.Join(",", format.CurrencyGroupSizes
                    .Select(i => i.ToString()).ToArray());
            }
            // Currency pattern
            string symbol = format.CurrencySymbol;
            string negative = format.NegativeSign;
            switch (format.CurrencyPositivePattern)
            {
                case 0: // $n
                    attributes["data-positivePattern"] = string.Format("{0}n", symbol);
                    break;
                case 1: // n$
                    attributes["data-positivePattern"] = string.Format("n{0}", symbol); ;
                    break;
                case 2: // $ n
                    attributes["data-positivePattern"] = string.Format("{0} n", symbol);
                    break;
                case 3: // n $
                    attributes["data-positivePattern"] = string.Format("n {0}", symbol);
                    break;
            }
            switch (format.CurrencyNegativePattern)
            {
                case 0: // ($n)
                    attributes["data-negativePattern"] = string.Format("({0}n)", symbol);
                    break;
                case 1: // -$n
                    attributes["data-negativePattern"] = string.Format("{0}{1}n", negative, symbol);
                    break;
                case 2: // $-n
                    attributes["data-negativePattern"] = string.Format("{0}{1}n", symbol, negative);
                    break;
                case 3: // $n-
                    attributes["data-negativePattern"] = string.Format("{0}n{1}", symbol, negative);
                    break;
                case 4: // (n$)
                    attributes["data-negativePattern"] = string.Format("(n{0})", symbol);
                    break;
                case 5: // -n$
                    attributes["data-negativePattern"] = string.Format("{0}n{1}", negative, symbol);
                    break;
                case 6: // n-$
                    attributes["data-negative"] = string.Format("n{0}{1}", negative, symbol);
                    break;
                case 7: // n$-
                    attributes["data-negativePattern"] = string.Format("n{0}{1}", symbol, negative);
                    break;
                case 8: // -n $
                    attributes["data-negativePattern"] = string.Format("{0}n {1}", negative, symbol);
                    break;
                case 9: // -$ n
                    attributes["data-negativePattern"] = string.Format("{0}{1} n", negative, symbol);
                    break;
                case 10: // n $-
                    attributes["data-negativePattern"] = string.Format("n {0}{1}", symbol, negative);
                    break;
                case 11: // $ n-
                    attributes["data-negativePattern"] = string.Format("{0} n{1}", symbol, negative);
                    break;
                case 12: // $ -n
                    attributes["data-negativePattern"] = string.Format("{0} {1}n", symbol, negative);
                    break;
                case 13: // n- $
                    attributes["data-negativePattern"] = string.Format("n{0} {1}", negative, symbol);
                    break;
                case 14: // ($ n)
                    attributes["data-negativePattern"] = string.Format("({0} n)", symbol);
                    break;
                case 15: // (n $)
                    attributes["data-negativePattern"] = string.Format("(n {0})", symbol);
                    break;
            }
            // Return the attributes
            return attributes;
        }

        /// <summary>
        /// Gets the data attributes for a percentage value
        /// </summary>
        internal static Dictionary<string, string> GetPercentDataAttributes(string formatString)
        {

            // Initialise attributes
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            // Get the number format
            NumberFormatInfo format = Thread.CurrentThread.CurrentCulture.NumberFormat;
            // Precision
            attributes["data-precision"] = format.PercentDecimalDigits.ToString();
            if (formatString != null)
            {
                // Get the number of decimal places
                Regex regex = new Regex(@"{\d{1}:P(\d?)}", RegexOptions.IgnoreCase);
                Match match = regex.Match(formatString);
                if (match.Success && match.Groups[1].Value.Length > 0)
                {
                    attributes["data-precision"] = match.Groups[1].Value;
                }
            }
            // Decimal seperator (hex)
            attributes["data-decimalSeperator"] = ((int)format.PercentDecimalSeparator[0]).ToString("X");
            // Negative sign (hex)
            attributes["data-negativeSign"] = ((int)format.NegativeSign[0]).ToString("X");
            // Group separator and size
            if (format.PercentGroupSizes[0] != 0)
            {
                attributes["data-groupSeperator"] = ((int)format.PercentGroupSeparator[0]).ToString("X");
                attributes["data-groupSizes"] = string.Join(",", format.PercentGroupSizes
                    .Select(i => i.ToString()).ToArray());
            }
            // Currency pattern
            string symbol = format.PercentSymbol;
            string negative = format.NegativeSign;

            switch (format.PercentPositivePattern)
            {
                case 0: // n %
                    attributes["data-positivePattern"] = string.Format("n {0}", symbol);
                    break;
                case 1: // n%
                    attributes["data-positivePattern"] = string.Format("n{0}", symbol); ;
                    break;
                case 2: // %n
                    attributes["data-positivePattern"] = string.Format("{0}n", symbol);
                    break;
                case 3: // % n
                    attributes["data-positivePattern"] = string.Format("{0} n", symbol);
                    break;
            }
            switch (format.PercentNegativePattern)
            {
                case 0: // -n %
                    attributes["data-negativePattern"] = string.Format("{0}n {1}", negative, symbol);
                    break;
                case 1: // -n%
                    attributes["data-negativePattern"] = string.Format("{0}n{1}", negative, symbol);
                    break;
                case 2: // -%n
                    attributes["data-negativePattern"] = string.Format("{0}{1}n", negative, symbol);
                    break;
                case 3: // %-n
                    attributes["data-negativePattern"] = string.Format("{0}{1}n", symbol, negative);
                    break;
                case 4: // %n-
                    attributes["data-negativePattern"] = string.Format("{0}n{1}", symbol, negative);
                    break;
                case 5: // n-%
                    attributes["data-negativePattern"] = string.Format("n{0}{1}", negative, symbol);
                    break;
                case 6: // n%-
                    attributes["data-negativePattern"] = string.Format("n{0}{1}", symbol, negative);
                    break;
                case 7: // -% n
                    attributes["data-negativePattern"] = string.Format("{0}{1} n", negative, symbol);
                    break;
                case 8: // n %-
                    attributes["data-negativePattern"] = string.Format("n {0}{1}", symbol, negative);
                    break;
                case 9: // % n-
                    attributes["data-negativePattern"] = string.Format("n {0}{1}", symbol, negative);
                    break;
                case 10: // % -n
                    attributes["data-negativePattern"] = string.Format("{0} {1}n", symbol, negative);
                    break;
                case 11: // n- %
                    attributes["data-negativePattern"] = string.Format("n{0} {1}", negative, symbol);
                    break;
            }
            // Return the attributes
            return attributes;

        }

        /// <summary>
        /// Gets the data attributes for a numeric value
        /// </summary>
        internal static Dictionary<string, string> GetNumberDataAttributes(string formatString)
        {
            // Initialise attributes
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            // Get the number format
            NumberFormatInfo format = Thread.CurrentThread.CurrentCulture.NumberFormat;
            // Precision
            attributes["data-precision"] = format.NumberDecimalDigits.ToString();
            if (formatString != null)
            {
                // TODO: What about 'D, E, F and G' format specifiers
                // Get the number of decimal places
                Regex regex = new Regex(@"{\d{1}:N(\d?)}", RegexOptions.IgnoreCase);
                Match match = regex.Match(formatString);
                if (match.Success && match.Groups[1].Value.Length > 0)
                {
                    attributes["data-precision"] = match.Groups[1].Value;
                }
            }
            // Decimal seperator (hex)
            attributes["data-decimalSeperator"] = ((int)format.NumberDecimalSeparator[0]).ToString("X");
            // Negative sign (hex)
            attributes["data-negativeSign"] = ((int)format.NegativeSign[0]).ToString("X");
            // Group separator and size
            if (format.CurrencyGroupSizes[0] != 0)
            {
                attributes["data-groupSeperator"] = ((int)format.NumberGroupSeparator[0]).ToString("X");
                attributes["data-groupSizes"] = string.Join(",", format.NumberGroupSizes
                    .Select(i => i.ToString()).ToArray());
            }
            // Number pattern
            attributes["data-positivePattern"] = "n";
            string negative = format.NegativeSign;
            switch (format.NumberNegativePattern)
            {
                case 0: // (n)
                    attributes["data-negativePattern"] = "(n)";
                    break;
                case 1: // -n
                    attributes["data-negativePattern"] = string.Format("{0}n", negative);
                    break;
                case 2: // - n
                    attributes["data-negativePattern"] = string.Format("{0} n", negative);
                    break;
                case 3: // n-
                    attributes["data-negativePattern"] = string.Format("n{0}", negative);
                    break;
                case 4: // n -
                    attributes["data-negativePattern"] = string.Format("n {0}", negative);
                    break;
            }
            // Return the attributes
            return attributes;
        }

        #endregion

    }

}
