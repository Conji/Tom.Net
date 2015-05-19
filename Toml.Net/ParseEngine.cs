using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Toml.Net
{
    public static class ParseEngine
    {
        public enum ValueType
        {
            String,
            Int,
            Float,
            Boolean,
            DateTime,
            Array,
            Table,
            InlineTables,
            ArrayOfTables
        }

        public static string[] StripComments(string[] input)
        {
            var list = new List<string>();
            var readingInline = false;
            foreach (var t in input)
            {
                if (readingInline)
                {
                    if (t.Contains("*/"))
                    {
                        readingInline = false;
                        list.Add(t.Split("*/".ToCharArray())[1]);
                    }
                    continue;
                }
                if (input.Contains("/*"))
                {
                    readingInline = true;
                    list.Add(t.Split("/*".ToCharArray())[0]);
                }
                else
                {
                    list.Add(t.Split('#')[0]);
                }
            }
            return list.ToArray();
        }

        #region Strings

        public static bool IsStringMultiLine(string line)
        {
            return line.TrimEnd().EndsWith("\"\"\"");
        }

        public static bool IsStringEndOfMultiLine(string line)
        {
            return line.Trim() == "\"\"\"";
        }

        public static string ChangeNixNl(string input)
        {
            return Regex.Replace(input, "(?<!\r)\n", "\r\n");
        }

        public static bool IsLiteralString(string input)
        {
            return input.StartsWith("'");
        }

        public static bool IsLiteralStringMultiLine(string line)
        {
            return line.TrimEnd().EndsWith("'''");
        }

        public static bool IsLiteralStringEndOfMultiLine(string line)
        {
            return line.Trim() == "'''";
        }

        #endregion

        #region Integers

        public static bool IsInteger(string input)
        {
            long i;
            return long.TryParse(input.Replace("_", ""), out i);
        }

        public static bool IsFloat(string input)
        {
            float f;
            return float.TryParse(input.Replace("_", ""), out f);
        }

        public enum FloatType
        {
            Fractional,
            Exponential,
            Both
        }

        public static FloatType GetFloatType(string input)
        {
            input = input.ToLower();
            if (input.Contains(".") && !input.Contains("e")) return FloatType.Fractional;
            if (!input.Contains(".") && input.Contains("e")) return FloatType.Exponential;
            if (input.Contains(".") && input.Contains("e")) return FloatType.Both;
            throw new InvalidCastException();
        }

        #endregion

        #region Boolean

        public static bool IsBoolean(string input)
        {
            return input == "false" || input == "true";
        }

        #endregion

        #region DateTime

        public static bool IsDateTime(string input)
        {
            try
            {
                XmlConvert.ToDateTime(input, XmlDateTimeSerializationMode.Local);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Arrays

        public static bool IsArray(string input)
        {
            return input.Split('=')[1].Trim().StartsWith("[");
        }

        public static bool IsArrayMultiLine(string input)
        {
            return input.Split('=')[1].Trim() == "[";
        }

        public static bool IsArrayEndOfMultiLine(string input)
        {
            return input.Trim() == "]";
        }

        public static int GetArrayDimensions(string input)
        {
            var count = 0;
            foreach (var c in input)
            {
                if (c == '[') count++;
                if (c == ']') break;
            }
            return count;
        }

        public static Array GetArray(params string[] input)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Tables

        public static bool IsTableName(string input)
        {
            return input.Trim().StartsWith("[") && input.Trim().EndsWith("]");
        }

        public static bool IsValidTableName(string input)
        {
            return Regex.IsMatch(input, "[(A-Za-z0-9_-)]", RegexOptions.IgnorePatternWhitespace);
        }

        public static string GetTableName(string input)
        {
            return input.Substring(1, input.Length - 2);
        }

        #endregion

        #region Inline Tables

        public static bool IsInlineTable(string input)
        {
            return input.Split('=')[1].StartsWith("{") && input.Split('=')[1].EndsWith("}");
        }

        public static Tuple<string, TomlKeyValuePair[]> GetInlineTable(string input)
        {
            
        }

        #endregion

        #region Array Tables

        public static bool IsArrayTable(string input)
        {
            return input.Trim().StartsWith("[[") && input.Trim().EndsWith("]]");
        }

        public static bool IsValidArrayTableName(string input)
        {
            return Regex.IsMatch(input, "[[(A-Za-z0-9_-)]]", RegexOptions.IgnorePatternWhitespace);
        }

        public static string GetArrayTableName(string input)
        {
            return input.Substring(2, input.Length - 3);
        }

        #endregion
    }
}
