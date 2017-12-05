using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace Extensions
{
    public static class StringExtensions
    {
        public static SplitMode DEF_SPLIT_MODE = SplitMode.IfNecessary;

        private static readonly List<string> NEW_LINE_STRINGS = new List<string>() { Environment.NewLine, "\r", "\n" };
        private static readonly List<char> WORD_DELIMITERS = new List<char>() { ' ', ':', '!', '?', '/', ')', ']', '}', '\r', '\n' };

        public enum SplitMode
        {
            Never,
            IfNecessary,
            Always
        }

        public static string ReplaceWhitespace(this string value, string replacement)
        {
            return Regex.Replace(value, @"\s", replacement);
        }

        public static string Condense(this string value)
        {
            return Regex.Replace(value, @"\s+", " ").Trim();
        }

        public static string ReplaceControlChars(this string value, string replacement, bool replaceWhitespace)
        {
            string pattern;

            if (!replaceWhitespace)
            {
                pattern = @"(?=[^\s])(\p{Cc})";
            }
            else
            {
                pattern = @"\p{Cc}";
            }

            return Regex.Replace(value, pattern, replacement);
        }

        public static int CountLines(this string value)
        {
            return value.Split(NEW_LINE_STRINGS.ToArray(), StringSplitOptions.None).Length;
        }

        public static T Convert<T>(this string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter == null)
            {
                return default(T);
            }

            return (T)converter.ConvertFromString(value);
        }

        public static bool TryConvert<T>(this string value, out T result)
        {
            try
            {
                result = Convert<T>(value);
            }
            catch
            {
                result = default(T);
                return false;
            }

            return true;
        }

        public static Size MeasureSize(this string value, Typeface typeface, double fontSize)
        {
            FormattedText formattedString = new FormattedText(value, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, Brushes.Black);

            return new Size(formattedString.Width, formattedString.Height);
        }

        public static Size MeasureSize(this string value, Control control)
        {
            Typeface typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);

            return MeasureSize(value, typeface, control.FontSize);
        }

        public static string Insert(this string value, string text, int caretIndex, int selectionLength)
        {
            StringBuilder newValueStringBuilder = new StringBuilder();
            newValueStringBuilder.Append(value);

            if (selectionLength > 0)
            {
                newValueStringBuilder.Remove(caretIndex, selectionLength);
                newValueStringBuilder.Insert(caretIndex, text);
            }
            else
            {
                newValueStringBuilder.Insert(caretIndex, text);
            }

            return newValueStringBuilder.ToString();
        }

        public static string Wrap(this string value, int charCountPerLine)
        {
            return Wrap(value, charCountPerLine, DEF_SPLIT_MODE);
        }

        public static string Wrap(this string value, int charCountPerLine, SplitMode splitMode)
        {
            char[] chars = value.ToCharArray();
            char c;
            List<Word> words = new List<Word>();
            StringBuilder wordStringBuilder = new StringBuilder();

            for (int i = 0; i < chars.Length; i++)
            {
                c = chars[i];

                if (c == '\r' && i + 1 < chars.Length && chars[i + 1] == '\n')
                {
                    wordStringBuilder.AppendLine();
                    i++;
                }
                else
                {
                    wordStringBuilder.Append(c);
                }

                if (i + 1 >= chars.Length || (splitMode != SplitMode.Always && WORD_DELIMITERS.Contains(c)))
                {
                    words.Add(new Word(wordStringBuilder.ToString()));
                    wordStringBuilder.Clear();
                }
            }

            StringBuilder wrappedStringBuilder = new StringBuilder();

            int currentLineCharCount = 0;
            int fittingCharCount;
            bool wordProcessingFinished;

            foreach (Word w in words)
            {
                wordProcessingFinished = false;

                while (!wordProcessingFinished)
                {
                    if (currentLineCharCount > 0 && currentLineCharCount + w.Length > charCountPerLine)
                    {
                        wrappedStringBuilder.AppendLine();
                        currentLineCharCount = 0;
                    }

                    if (splitMode == SplitMode.Never)
                    {
                        wrappedStringBuilder.Append(w.Value);
                        currentLineCharCount += w.Length;
                        wordProcessingFinished = true;
                    }
                    else
                    {
                        if (currentLineCharCount + w.Length > charCountPerLine)
                        {
                            fittingCharCount = charCountPerLine - currentLineCharCount;
                            wrappedStringBuilder.Append(w.Value.Substring(0, fittingCharCount));
                            w.Value = w.Value.Remove(0, fittingCharCount);
                            currentLineCharCount += fittingCharCount;
                        }
                        else
                        {
                            wrappedStringBuilder.Append(w.Value);
                            currentLineCharCount += w.Length;
                            wordProcessingFinished = true;
                        }
                    }

                    if (wordProcessingFinished && w.EndsWithNewLine)
                    {
                        currentLineCharCount = 0;
                    }
                }
            }

            return wrappedStringBuilder.ToString();
        }

        public static WrapParallelResult WrapParallel(string[] master, string[] slave, int masterCharCountPerLine)
        {
            Word[] masterValues = master.Select(v => new Word(v)).ToArray();
            Word[] slaveValues = slave.Select(v => new Word(v)).ToArray();

            StringBuilder masterStringBuilder = new StringBuilder();
            StringBuilder slaveStringBuilder = new StringBuilder();
            Word m;
            Word s;

            int currentLineCharCount = 0;
            bool wordProcessingFinished;

            for (int i = 0; i < masterValues.Length; i++)
            {
                m = masterValues[i];
                s = slaveValues[i];

                wordProcessingFinished = false;

                while (!wordProcessingFinished)
                {
                    if (currentLineCharCount > 0 && currentLineCharCount + m.Length > masterCharCountPerLine)
                    {
                        masterStringBuilder.AppendLine();
                        slaveStringBuilder.AppendLine();
                        currentLineCharCount = 0;
                    }

                    masterStringBuilder.Append(m.Value);
                    slaveStringBuilder.Append(s.Value);
                    currentLineCharCount += m.Length;
                    wordProcessingFinished = true;

                    if (wordProcessingFinished && m.EndsWithNewLine)
                    {
                        currentLineCharCount = 0;
                    }
                }
            }

            return new WrapParallelResult(masterStringBuilder.ToString(), slaveStringBuilder.ToString());
        }

        public class WrapParallelResult
        {
            public WrapParallelResult(string masterValue, string slaveValue)
            {
                MasterValue = masterValue;
                SlaveValue = slaveValue;
            }

            public string MasterValue
            {
                get;
                private set;
            }

            public string SlaveValue
            {
                get;
                private set;
            }
        }

        private class Word
        {
            private string value;
            private bool endsWithNewLine;
            private int length;

            public Word(string value)
            {
                Value = value;
            }

            public string Value
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                    UpdateEndsWithNewLine();
                    UpdateLength();
                }
            }

            public bool EndsWithNewLine
            {
                get
                {
                    return endsWithNewLine;
                }
            }

            public int Length
            {
                get
                {
                    return length;
                }
            }

            private void UpdateEndsWithNewLine()
            {
                endsWithNewLine = false;

                foreach (string s in NEW_LINE_STRINGS)
                {
                    if (Value.EndsWith(s))
                    {
                        endsWithNewLine = true;
                        break;
                    }
                }
            }

            private void UpdateLength()
            {
                length = Value.Length;

                if (EndsWithNewLine)
                {
                    if (Value.EndsWith(Environment.NewLine))
                    {
                        length -= 2;
                    }
                    else
                    {
                        length -= 1;
                    }
                }
            }
        }
    }
}
