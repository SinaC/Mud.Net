using System;
using System.Linq;
using System.Text;

namespace Mud.Server.Server
{
    internal class Paging
    {
        private string[] _lines;
        private int _currentLine;

        public Paging()
        {
            _lines = null;
            _currentLine = 0;
        }

        public bool HasPageLeft => _lines != null && _currentLine < _lines.Length;

        public void Clear()
        {
            _currentLine = 0;
            _lines = null;
        }

        public void SetData(StringBuilder data)
        {
            // if unread lines, they will be overridden by new ones
            _currentLine = 0;
            _lines = data.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string GetNextPage(int lineCount)
        {
            string lines = String.Join(Environment.NewLine, _lines.Skip(_currentLine).TakeWhile((n, i) => i < lineCount && i < _lines.Length)) + Environment.NewLine;
            _currentLine = Math.Min(_currentLine + lineCount, _lines.Length);
            return lines;
        }

        public string GetRemaining()
        {
            string lines = String.Join(Environment.NewLine, _lines) + Environment.NewLine;
            return lines;
        }
    }
}
