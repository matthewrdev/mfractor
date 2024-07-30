using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;
using MFractor.Maui.Thickness;

namespace MFractor.Maui.Utilities
{
    public static class ThicknessHelper
    {
        public static string ToFormattedValueString(double left, double right, double top, double bottom)
        {
            return $"{left},{top},{right},{bottom}";
        }

        public static string ToFormattedValueString(IReadOnlyList<double> thickness)
        {
            if (!ProcessThickness(thickness, out var left, out var right, out var top, out var bottom))
            { 
                return string.Empty;
            }

            return ToFormattedValueString(left, right, top, bottom);
        }

        public static bool ProcessThickness(XmlNode thicknessNode, out IReadOnlyList<double> thickness)
        {
            thickness = null;

            if (ProcessThickness(thicknessNode, out var left, out var right, out var top, out var bottom))
            {
                thickness = new List<double>()
                {
                    left, right, top, bottom
                };

                return true;
            }

            return false;
        }

        public static bool ProcessThickness(XmlNode thicknessNode, out double left, out double right, out double top, out double bottom)
        {
            left = right = top = bottom = 0.0;

            if (thicknessNode == null)
            {
                return false;
            }

            if (thicknessNode.HasValue)
            {
                return ProcessThickness(thicknessNode.Value, out left, out right, out top, out bottom);
            }

            var success = ProcessThicknessDimension(thicknessNode, "Left", out left)
                | ProcessThicknessDimension(thicknessNode, "Right", out right)
                | ProcessThicknessDimension(thicknessNode, "Top", out top)
                | ProcessThicknessDimension(thicknessNode, "Bottom", out bottom);

            return success;
        }

        public static bool ProcessThicknessDimension(XmlNode thicknessNode, string dimensionName, out double dimension)
        {
            var attribute = thicknessNode.GetAttributeByName(dimensionName);
            var node = thicknessNode.GetChildNode(thicknessNode.Name + "." + dimensionName);

            return double.TryParse(attribute?.Value?.Value?.Trim(), out dimension)
                 || double.TryParse(node?.Value?.Trim(), out dimension);
        }

        public static bool ProcessThickness(IReadOnlyList<double> thickness, out double left, out double right, out double top, out double bottom)
        {
            left = right = top = bottom = 0.0;

            if (thickness == null)
            {
                return false;
            }

            if (thickness.Count == 1) // EG: Padding="1" -> Left, right, top, bottom dimensions at 1
            {
                left = right = top = bottom = thickness[0];
            }
            else if (thickness.Count == 2)// EG: Padding="1,0" -> Left and right dimensions at 1, top and bottom at 0
            {
                left = right = thickness[0];
                top = bottom = thickness[1];
            }
            else if (thickness.Count == 4) // EG: Padding="1,0,2,3" -> Left at 1, top at 0, right at 2, bottom at 3
            {
                left = thickness[0];
                right = thickness[2];
                top = thickness[1];
                bottom = thickness[3];
            }
            else
            {
                // Unknown thickness format
                return false;
            }

            return true;
        }

        public static bool ProcessThickness(string value, TextSpan valueSpan, out IReadOnlyList<ThicknessValue> values)
        {
            values = null;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Single value thickness such as "10" == all dimensions.
            if (!value.Contains(","))
            {
                if (!double.TryParse(value, out var thicknessSize))
                {
                    return false;
                }

                values = new List<ThicknessValue>()
                {
                    new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.All,
                        Value = thicknessSize,
                        Span = valueSpan,
                    }
                };

                return true;
            }
            else
            {
                var result = new List<ThicknessValue>();
                var thicknessValues = value.Split(',');

                // Horizontal/Vertical split
                if (thicknessValues.Length == 2
                    && thicknessValues.All(tv => double.TryParse(tv.Trim(), out _)))
                {
                    // Horizontal
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Horizontal,
                        Span = new TextSpan(valueSpan.Start, thicknessValues[0].Length),
                        Value = double.Parse(thicknessValues[0].Trim())
                    });

                    // Vertical
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Vertical,
                        Span = new TextSpan(valueSpan.Start + thicknessValues[0].Length + 1, thicknessValues[1].Length),
                        Value = double.Parse(thicknessValues[1].Trim())
                    });

                    values = result;
                    return true;
                }
                // Left, Top, Right, Bottom
                else if (thicknessValues.Length == 4
                         && thicknessValues.All(tv => double.TryParse(tv.Trim(), out _)))
                {
                    // Left
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Left,
                        Span = new TextSpan(valueSpan.Start, thicknessValues[0].Length),
                        Value = double.Parse(thicknessValues[0].Trim())
                    });

                    // Top
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Top,
                        Span = new TextSpan(result.Last().Span.End + 1, thicknessValues[1].Length),
                        Value = double.Parse(thicknessValues[1].Trim())
                    });

                    // Right
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Right,
                        Span = new TextSpan(result.Last().Span.End + 1, thicknessValues[2].Length),
                        Value = double.Parse(thicknessValues[2].Trim())
                    });

                    // Bottom
                    result.Add(new ThicknessValue()
                    {
                        Dimension = ThicknessDimension.Bottom,
                        Span = new TextSpan(result.Last().Span.End + 1, thicknessValues[3].Length),
                        Value = double.Parse(thicknessValues[3].Trim())
                    });

                    values = result;
                    return true;
                }
            }

            return false;
        }


        public static bool ProcessThickness(string value, out double left, out double right, out double top, out double bottom)
        {
            left = right = top = bottom = 0.0;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var values = value.Split(',')
                              .Select(v => v.Trim())
                              .Where(v => double.TryParse(v, out var __))
                              .Select(v => double.Parse(v))
                              .ToList();

            return ProcessThickness(values, out left, out right, out top, out bottom);
        }

        public static bool ProcessThickness(string value, out IReadOnlyList<double> thickness)
        {
            thickness = new List<double>();

            if (!ProcessThickness(value, out var left, out var right, out var top, out var bottom))
            {
                return false;
            }

            thickness = new List<double>()
            {
                left, top, right, bottom
            };

            return true;
        }

    }
}
