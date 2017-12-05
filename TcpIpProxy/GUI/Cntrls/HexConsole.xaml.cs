using Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TcpIpProxy.Threading;
using System.Linq;

namespace TcpIpProxy.GUI.Cntrls
{
    /// <summary>
    /// Interaktionslogik für ConsoleDataGrid.xaml
    /// </summary>
    public partial class HexConsole : UserControl
    {
        public static readonly DependencyProperty TranslateHexEnabledProperty =
            DependencyProperty.Register("TranslateHexEnabled", typeof(bool), typeof(HexConsole), new FrameworkPropertyMetadata(false, OnTranslateHexEnabledChanged));
        public static readonly DependencyProperty AddTimestampEnabledProperty =
            DependencyProperty.Register("AddTimestampEnabled", typeof(bool), typeof(HexConsole), new FrameworkPropertyMetadata(false, OnAddTimestampEnabledChanged));

        private readonly List<State> consoleEntries = new List<State>();
        private bool consoleUpdateRunning = false;
        private int displayedEntriesCount = 0;
        private double charWidth;
        private double lastTextBoxMessageWidth = 0;

        private DispatcherTimer resizeTimer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500)
        };

        public HexConsole()
        {
            InitializeComponent();

            charWidth = "A".MeasureSize(textBoxState).Width;

            resizeTimer.Tick += ResizeTimer_Tick;
        }

        private void TextBoxMessage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            double currentTextBoxMessageWidth = textBoxMessage.ActualWidth;

            if (lastTextBoxMessageWidth != currentTextBoxMessageWidth)
            {
                lastTextBoxMessageWidth = currentTextBoxMessageWidth;

                resizeTimer.Stop();
                UpdateConsoleAsync(true);
            }
        }

        private void CopyConsole_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder clipboardStringBuilder = new StringBuilder();
            State s;

            for (int i = 0; i < consoleEntries.Count; i++)
            {
                s = consoleEntries[i];

                if (i > 0)
                {
                    clipboardStringBuilder.AppendLine();
                }

                if (AddTimestampEnabled)
                {
                    clipboardStringBuilder.Append(s.Timestamp + ":");
                }

                clipboardStringBuilder.Append(s.DescriptionLong.Condense());

                if (TranslateHexEnabled && s.HexData != null)
                {
                    clipboardStringBuilder.Append(" " + string.Join("", s.HexData));
                }

                if (!string.IsNullOrEmpty(s.DataWithoutWhitespace))
                {
                    clipboardStringBuilder.Append(" " + s.DataWithoutWhitespace);
                }
            }

            Clipboard.SetText(clipboardStringBuilder.ToString());
        }

        private void ClearConsole_Click(object sender, RoutedEventArgs e)
        {
            consoleEntries.Clear();
            UpdateConsoleAsync(true);
        }

        public bool TranslateHexEnabled
        {
            get
            {
                return (bool)GetValue(TranslateHexEnabledProperty);
            }
            set
            {
                SetValue(TranslateHexEnabledProperty, value);
            }
        }

        public bool AddTimestampEnabled
        {
            get
            {
                return (bool)GetValue(AddTimestampEnabledProperty);
            }
            set
            {
                SetValue(AddTimestampEnabledProperty, value);
            }
        }

        private static void OnTranslateHexEnabledChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            HexConsole sourceControl = source as HexConsole;

            if (((e.NewValue) as bool?).GetValueOrDefault())
            {
                sourceControl.textBoxHex.Visibility = sourceControl.gridSplitterHex.Visibility = Visibility.Visible;
                sourceControl.scrollViewerGlobal.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                sourceControl.ColumnTextBoxHex.Width = new GridLength(3.7, GridUnitType.Star);
            }
            else
            {
                sourceControl.textBoxHex.Visibility = sourceControl.gridSplitterHex.Visibility = Visibility.Collapsed;
                sourceControl.scrollViewerGlobal.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                sourceControl.ColumnTextBoxHex.Width = new GridLength(0);
            }

            sourceControl.UpdateConsoleAsync(true);
        }

        private static void OnAddTimestampEnabledChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            (source as HexConsole).UpdateConsoleAsync(true);
        }

        public void AppendLine(State state)
        {
            consoleEntries.Add(state);
            UpdateConsoleAsync(false);
        }

        private async void UpdateConsoleAsync(bool rebuildAll)
        {
            if (consoleUpdateRunning)
            {
                return;
            }
            else
            {
                consoleUpdateRunning = true;
            }

            int stateCharsPerLine = Math.Max((int)(textBoxState.ActualWidth / charWidth) - 2, 1);
            int messageCharsPerLine;

            if (TranslateHexEnabled)
            {
                messageCharsPerLine = (Math.Max(((int)(textBoxHex.ActualWidth / charWidth) - 1), 4) / 4);
            }
            else
            {
                messageCharsPerLine = (Math.Max(((int)(textBoxMessage.ActualWidth / charWidth)) - 2, 1));
            }

            UpdateConsoleResult result = await UpdateConsoleAsyncInternal(rebuildAll, TranslateHexEnabled, AddTimestampEnabled, stateCharsPerLine, messageCharsPerLine, consoleEntries.Count);

            if (rebuildAll)
            {
                textBoxState.Clear();
                textBoxMessage.Clear();
                textBoxHex.Clear();
            }

            textBoxState.AppendText(result.newStatusText);
            textBoxMessage.AppendText(result.newMessageText);
            textBoxHex.AppendText(result.newHexText);
            consoleUpdateRunning = false;

            if (displayedEntriesCount != consoleEntries.Count)
            {
                UpdateConsoleAsync(rebuildAll);
            }
        }

        private Task<UpdateConsoleResult> UpdateConsoleAsyncInternal(bool rebuildAll, bool translateHexEnabled, bool addTimestampEnabled, int statusCharsPerLine, int messageCharsPerLine, int lastEntriesCount)
        {
            return Task.Run(() =>
            {
                StringBuilder newStatusStringBuilder = new StringBuilder();
                StringBuilder newMessageStringBuilder = new StringBuilder();
                StringBuilder newHexStringBuilder = new StringBuilder();

                int firstEntryToRebuildIndex;

                if (rebuildAll)
                {
                    firstEntryToRebuildIndex = 0;
                }
                else
                {
                    firstEntryToRebuildIndex = displayedEntriesCount;
                }

                StringBuilder descriptionStringBuilder = new StringBuilder();

                State entry;
                int newStatusTextLineCount;
                int newMessageTextLineCount;

                for (int i = firstEntryToRebuildIndex; i < lastEntriesCount; i++)
                {
                    descriptionStringBuilder.Clear();
                    entry = consoleEntries[i];

                    if (addTimestampEnabled)
                    {
                        descriptionStringBuilder.Append(entry.Timestamp + ":");
                    }

                    descriptionStringBuilder.Append(entry.DescriptionLong);
                    newStatusStringBuilder.Append(descriptionStringBuilder.ToString().Wrap(statusCharsPerLine));

                    if (entry.DataWithoutWhitespace != string.Empty)
                    {
                        if (translateHexEnabled)
                        {
                            StringExtensions.WrapParallelResult result = 
                                StringExtensions.WrapParallel(
                                FormatHexData(entry.HexData).ToArray(), 
                                entry.DataWithoutWhitespace.ToCharArray().Select(c => c.ToString()).ToArray(), 
                                messageCharsPerLine * 4);
                            newHexStringBuilder.Append(result.MasterValue);
                            newMessageStringBuilder.Append(result.SlaveValue);
                        }
                        else
                        {
                            newMessageStringBuilder.Append(entry.Data);
                        }
                    }

                    newStatusStringBuilder.AppendLine();
                    newMessageStringBuilder.AppendLine();
                    newHexStringBuilder.AppendLine();

                    newStatusTextLineCount = newStatusStringBuilder.ToString().CountLines();
                    newMessageTextLineCount = newMessageStringBuilder.ToString().CountLines();

                    for (int j = 0; j < Math.Abs(newStatusTextLineCount - newMessageTextLineCount); j++)
                    {
                        if (newStatusTextLineCount >= newMessageTextLineCount)
                        {
                            newHexStringBuilder.AppendLine();
                            newMessageStringBuilder.AppendLine();
                        }

                        if (newStatusTextLineCount <= newMessageTextLineCount)
                        {
                            newStatusStringBuilder.AppendLine();
                        }
                    }
                }

                displayedEntriesCount = lastEntriesCount;

                return new UpdateConsoleResult(newStatusStringBuilder, newMessageStringBuilder, newHexStringBuilder);
            });
        }

        private List<string> FormatHexData(List<string> hexStrings)
        {
            int shortestLength = hexStrings.OrderBy(s => s.Length).First().Length;
            int diffToShortest;
            StringBuilder hexStringBuilder = new StringBuilder();
            return hexStrings.Select(s =>
            {
                if (s.Length % shortestLength == 0)
                {
                    return s;
                }

                diffToShortest = s.Length % shortestLength;
                hexStringBuilder.Clear();
                hexStringBuilder.Append(' ', diffToShortest / 2);
                hexStringBuilder.Append(s);
                hexStringBuilder.Append(' ', diffToShortest / 2);
                return hexStringBuilder.ToString();
            }).ToList();
        }

        private class UpdateConsoleResult
        {
            public string newStatusText
            {
                get;
                private set;
            }

            public string newMessageText
            {
                get;
                private set;
            }

            public string newHexText
            {
                get;
                private set;
            }

            public UpdateConsoleResult(StringBuilder newStatusText, StringBuilder newMessageText, StringBuilder newHexText)
            {
                this.newStatusText = newStatusText.ToString();
                this.newMessageText = newMessageText.ToString();
                this.newHexText = newHexText.ToString();
            }
        }

    }
}
