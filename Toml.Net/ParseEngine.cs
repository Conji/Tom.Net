using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Toml.Net
{
    public static class ParseEngine
    {
        public static object ParseObject(string value, ValueType type)
        {
            value = value.Split('=')[1];
            switch (type)
            {
                case ValueType.String:
                    return Regex.Escape(value).Replace("\"", "");
                case ValueType.LiteralString:
                    return value.Replace("'", "");
                case ValueType.Int:
                    return int.Parse(value.Replace("_", ""));
                case ValueType.Float:
                    return float.Parse(value.Replace("_", ""));
                case ValueType.Boolean:
                    return bool.Parse(value);
                case ValueType.DateTime:
                    return ToDateTime(value);
                case ValueType.Array:
                    throw new NotImplementedException();
                case ValueType.Various:
                    if (IsString(value)) return Regex.Escape(value).Replace("\"", "");
                    if (IsLiteralString(value)) return value.Replace("'", "");
                    if (IsInteger(value)) return int.Parse(value.Replace("_", ""));
                    if (IsFloat(value)) return float.Parse(value.Replace("_", ""));
                    if (IsBoolean(value)) return bool.Parse(value);
                    if (IsDateTime(value)) return ToDateTime(value);
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException($"Could not find value type of {value}");
            }
        }

        public static Dictionary<string, TomlKeyValuePair[]> Parse(string[] contents)
        {
            var d = new Dictionary<string, TomlKeyValuePair[]>();
            var n = "";
            var l = new List<TomlKeyValuePair>();
            for (var i = 0; i < contents.Length; i++)
            {
                var line = contents[i];
                if (IsTableName(line))
                {
                    if (n != "")
                    {
                        d.Add(n, l.ToArray());
                        l.Clear();
                    }
                    n = GetTableName(line);
                }
                else
                {
                    if (IsString(line))
                    {
                        if (!IsStringMultiLine(line)) l.Add(new TomlKeyValuePair(GetName(line), ParseObject(line, ValueType.String)));
                        else
                        {
                            var builder = new StringBuilder();
                            while (!IsStringEndOfMultiLine(contents[i++]))
                            {
                                builder.Append(contents[i]);
                            }
                            builder.Append(contents[i].Replace("\"\"\"", ""));
                            l.Add(new TomlKeyValuePair(GetName(line), builder.ToString()));
                        }
                    }
                    else if (IsLiteralString(line))
                    {
                        if (!IsLiteralStringMultiLine(line))
                            l.Add(new TomlKeyValuePair(GetName(line), ParseObject(line, ValueType.LiteralString)));
                        else
                        {
                            var builder = new StringBuilder();
                            while (!IsLiteralStringEndOfMultiLine(contents[i++]))
                            {
                                builder.Append(@contents[i]);
                            }
                            builder.Append(@contents[i].Replace("\"\"\"", ""));
                            l.Add(new TomlKeyValuePair(GetName(line), builder.ToString()));
                        }
                    }
                    else if (IsInteger(line))
                    {
                        l.Add(new TomlKeyValuePair(GetName(line), int.Parse(line.Split('=')[1])));
                    }
                    else if (IsFloat(line))
                    {
                        l.Add(new TomlKeyValuePair(GetName(line), float.Parse(line.Split('=')[1])));
                    }
                    else if (IsBoolean(line))
                    {
                        l.Add(new TomlKeyValuePair(GetName(line), bool.Parse(line.Split('=')[1])));
                    }
                    else if (IsDateTime(line))
                    {
                        l.Add(new TomlKeyValuePair(GetName(line), ToDateTime(line.Split('=')[1])));
                    }
                    else if (IsArray(line))
                    {
                        if (!IsArrayMultiLine(line)) l.Add(new TomlKeyValuePair(GetName(line), ParseObject(line, ValueType.Array)));
                        var c = new List<string>();
                        while (!IsArrayEndOfMultiLine(line))
                        {
                            c.Add(contents[i++]);
                        }
                        l.Add(new TomlKeyValuePair(GetName(line), Parse(c.ToArray())));
                    }
                    else if (IsInlineTable(line))
                    {
                        var inline = GetInlineTable(line);
                        d.Add(inline.Item1, inline.Item2);
                    }
                }
            }
            return d;
        }

        public static string GetName(string input)
        {
            return input.Split('=')[0].Trim();
        }

        public enum ValueType
        {
            String,
            LiteralString,
            Int,
            Float,
            Boolean,
            DateTime,
            Array,
            Table,
            InlineTables,
            ArrayOfTables,
            Various
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

        public static bool IsString(string line)
        {
            return line.Split('=')[1].StartsWith("\"");
        }

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

        public static DateTime ToDateTime(string input)
        {
            return XmlConvert.ToDateTime(input, XmlDateTimeSerializationMode.Local);
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
            return input.StartsWith("[") && input.EndsWith("]");
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
            input = input.Replace("{", "").Replace("}", "");
            var key = input.Split('=')[0].Trim();
            var values = input.Split('=')[1].Split(',');
            var l = (from value in values
                let vk = value.Split('=')[0].Trim()
                let vv = ParseObject(value.Split('=')[1], ValueType.Various)
                select new TomlKeyValuePair(vk, vv)).ToArray();
            return new Tuple<string, TomlKeyValuePair[]>(key, l);
        }

        #endregion

        #region Array Tables

        public static bool IsArrayTable(string input)
        {
            return input.Trim().StartsWith("[[") && input.Trim().EndsWith("]]");
        }

        public static bool IsValidArrayTableName(string input)
        {
            return input.StartsWith("[[") && input.EndsWith("]]");
        }

        public static string GetArrayTableName(string input)
        {
            return input.Substring(2, input.Length - 3);
        }

        #endregion
    }
}
