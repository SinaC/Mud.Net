using System;
using System.Linq;
using System.Text;

namespace Mud.POC
{
    public class TestPaging
    {
        private string[] _lines;
        private int _currentLine;

        public TestPaging()
        {
            _lines = null;
            _currentLine = 0;
        }

        public bool HasPaging
        {
            get { return _lines != null && _currentLine < _lines.Length; }
        }

        public void SetData(StringBuilder data)
        {
            _currentLine = 0;
            _lines = data.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        public string GetNextLines(int lineCount)
        {
            string lines = String.Join(Environment.NewLine, _lines.Skip(_currentLine).TakeWhile((n, i) => i < lineCount && i < _lines.Length));
            _currentLine = Math.Min(_currentLine + lineCount, _lines.Length);
            return lines;
        }
    }
}
