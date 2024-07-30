using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFractor.Attributes;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for extracting names and values from enums
    /// </summary>
    public static class EnumHelper
    {
        public static TEnum ToEnum<TEnum>(this string value) where TEnum : System.Enum
        {
            TryParse<TEnum>(value, out var result);

            return result;
        }

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : System.Enum
        {
            result = default(TEnum);
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));

            foreach (var enumValue in enumValues)
            {
                var stringValue = enumValue.ToString();
                if (string.Compare(stringValue, value, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    result = enumValue;
                    return true;
                }
            }

            return false;
        }


        public static Tuple<string, int> GetDisplayValue(Enum value)
        {
            return new Tuple<string, int>(GetEnumDescription(value), Convert.ToInt32(value));
        }

        public static List<Tuple<string, int>> GetDisplayValues<TEnum>() where TEnum : System.Enum
        {
            var values = Enum.GetValues(typeof(TEnum));

            var results = new List<Tuple<string, int>>();
            foreach (var v in values)
            {
                var name = GetEnumDescription(v as Enum);

                results.Add(new Tuple<string, int>(name, (int)v));
            }

            return results.Where(v => string.IsNullOrEmpty(v.Item1) == false).ToList();
        }

        public static string GetEnumDescription(Enum value)
        {
            var fi = value.GetType().GetRuntimeField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
            {
                return attributes[0].Description;
            }

            return string.Empty;
        }

        public static List<string> GetEnumValueDescriptions(IEnumerable<Enum> values)
        {
            var result = new List<string>();
            foreach (var v in values)
            {
                result.Add(GetEnumDescription(v));
            }

            return result;
        }

        public static List<string> GetEnumValueDescriptions<TEnum>() where TEnum : System.Enum
        {
            var result = new List<string>();

            var values = Enum.GetValues(typeof(TEnum));

            foreach (var v in values)
            {
                result.Add(GetEnumDescription(v as Enum));
            }

            return result;
        }

        public static Tuple<double, int> GetScaleValue(Enum value)
        {
            return new Tuple<double, int>(GetEnumScale(value), Convert.ToInt32(value));
        }

        public static List<Tuple<double, int>> GetScaleValues<TEnum>() where TEnum : System.Enum
        {
            var values = Enum.GetValues(typeof(TEnum));

            var results = new List<Tuple<double, int>>();
            foreach (var v in values)
            {
                var name = GetEnumScale(v as Enum);
                results.Add(new Tuple<double, int>(name, (int)v));
            }

            return results.Where(v => !double.IsNaN(v.Item1)).ToList();
        }

        public static double GetEnumScale(Enum value)
        {
            var fi = value.GetType().GetRuntimeField(value.ToString());

            var attributes =
                (ScaleFactorAttribute[])fi.GetCustomAttributes(
                    typeof(ScaleFactorAttribute),
                    false);

            if (attributes != null &&
                attributes.Length > 0)
            {
                return attributes[0].Scale;
            }

            return double.NaN;
        }

        public static List<double> GetEnumScaleValues(IEnumerable<Enum> values)
        {
            var result = new List<double>();
            foreach (var v in values)
            {
                result.Add(GetEnumScale(v));
            }

            return result;
        }


    }
}