using System;

namespace SoftCircuits.SimpleLogFile
{
    /// <summary>
    /// Argument for <see cref="LogFile.LineLogged"/> event.
    /// </summary>
    public class LineLoggedEventArgs
        : EventArgs
    {
        public string Text { get; set; }

        public LineLoggedEventArgs(string text)
        {
            Text = text;
        }
    }
}
