using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRegexTester.Classes
{
    /// <summary>
    /// Container for progress bar with text block.
    /// Use for IProgress.Report(...)
    /// </summary>
    public class ProgressStatus
    {
        public int Value { get; }
        public string Text { get; }

        public ProgressStatus(int value, string text)
        {
            Value = value;
            Text = text;
        }
    }
}
