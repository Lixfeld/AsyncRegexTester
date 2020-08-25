using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AsyncRegexTester.Classes;

namespace AsyncRegexTester
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly ObservableCollection<DataGridColumn> dataGridGroupColumns;

        public ObservableCollection<RegexResult> RegexResultCollection { get; } = new ObservableCollection<RegexResult>();

        //No INotifyPropertyChanged Implementation (readonly) - Setter is needed for OneWayToSource-Binding
        public string InputText { get; set; }
        public string RegexPattern { get; set; }
        public bool IsSingleline { get; set; }
        public bool IsMultiline { get; set; }
        public bool IgnoreCase { get; set; }
        public bool ReadLineByLine { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            readLineByLineCheckBox.IsChecked = true;

            //Set DataGridColumns for groups (see xaml code)
            dataGridGroupColumns = new ObservableCollection<DataGridColumn>()
            {
                dataGrid.Columns[1],
                dataGrid.Columns[2],
                dataGrid.Columns[3],
                dataGrid.Columns[4],
                dataGrid.Columns[5]
            };
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = true;
            startButton.IsEnabled = false;
            cancellationTokenSource = new CancellationTokenSource();

            RegexResultCollection.Clear();
            List<string> lines = InputText.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //Set up ProgressBarStatus
            searchProgressBar.Maximum = lines.Count;
            IProgress<ProgressStatus> progress = new Progress<ProgressStatus>(status =>
            {
                searchProgressBar.Value = status.Value;
                progressTextBlock.Text = status.Text;
            });

            OnlyShowUsedColumns(new Regex(RegexPattern).GetGroupNumbers()?.Length ?? 0);
            await SearchLinesAsync(lines, progress, cancellationTokenSource.Token);
        }

        private async Task SearchLinesAsync(IList<string> lines, IProgress<ProgressStatus> progress, CancellationToken token)
        {
            try
            {
                //Reset progress
                progress.Report(new ProgressStatus(0, "Matches: 0"));

                int matchCount = 0;
                Match currentMatch;
                IEnumerable<string> groups;

                Regex regex = new Regex(RegexPattern, GetRegexOptions());
                if (ReadLineByLine)
                {
                    for (int i = 0; i < lines.Count(); i++)
                    {
                        //Cancellation
                        token.ThrowIfCancellationRequested();

                        currentMatch = regex.Match(lines[i]);
                        if (currentMatch.Success)
                        {
                            //Fake delay
                            await Task.Delay(TimeSpan.FromSeconds(0.5));

                            //Add matches/groups and line to collection
                            groups = currentMatch.Groups.OfType<Group>().Select(g => g.Value);
                            RegexResultCollection.Add(new RegexResult(++matchCount, lines[i], groups));
                        }
                        progress.Report(new ProgressStatus(i + 1, "Matches: " + matchCount));
                    }
                }
                else
                {
                    MatchCollection matches = regex.Matches(InputText);
                    for (int i = 0; i < matches.Count; i++)
                    {
                        //Add matches/groups to collection
                        groups = matches[i].Groups.OfType<Group>().Select(g => g.Value);
                        RegexResultCollection.Add(new RegexResult(++matchCount, string.Empty, groups));
                    }
                    progress.Report(new ProgressStatus(lines.Count, "Matches: " + matchCount));
                }
            }
            catch (Exception)
            {
                //Ignore cancellation
            }
            finally
            {
                cancelButton.IsEnabled = false;
                SetStartButtonStatus();
            }
        }

        private RegexOptions GetRegexOptions()
        {
            RegexOptions regexOptions = RegexOptions.None;
            if (IsSingleline)
                regexOptions |= RegexOptions.Singleline;
            if (IsMultiline)
                regexOptions |= RegexOptions.Multiline;
            if (IgnoreCase)
                regexOptions |= RegexOptions.IgnoreCase;
            return regexOptions;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            cancelButton.IsEnabled = false;
        }

        private void RegexPatternTextBox_TextChanged(object sender, TextChangedEventArgs e) => SetStartButtonStatus();

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) => SetStartButtonStatus();

        private void SetStartButtonStatus()
        {
            if (!string.IsNullOrWhiteSpace(InputText) && !string.IsNullOrWhiteSpace(RegexPattern))
            {
                startButton.IsEnabled = true;
            }
            else
            {
                startButton.IsEnabled = false;
            }
        }

        private void OnlyShowUsedColumns(int groupsCount)
        {
            const int MaxGroupIndex = 4;
            int groupIndex = groupsCount - 1;

            //Hide all (unused) columns larger than the index or zero
            for (int i = MaxGroupIndex; i > groupIndex && i > 0; i--)
            {
                dataGridGroupColumns[i].Visibility = Visibility.Collapsed;
            }

            //Show all other (used) columns
            for (int i = 0; i <= groupIndex; i++)
            {
                dataGridGroupColumns[i].Visibility = Visibility.Visible;
            }

            //Hide Line column if option read line-by-line is turned off
            lineTextColumn.Visibility = ReadLineByLine ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
