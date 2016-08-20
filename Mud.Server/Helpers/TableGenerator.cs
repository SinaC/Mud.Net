using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Server.Common;

namespace Mud.Server.Helpers
{
    public class TableGenerator<T>
    {
        public static readonly Func<T, string> AlignLeftFunc = T => null;

        private class Column
        {
            public string Header { get; }
            public int Width { get; }

            public Func<T, string> GetValueFunc { get; }
            public Func<T, int> GetMergeLengthFunc { get; } = t => 0;
            public Func<T, string> GetTrailingSpaceFunc { get; } = t => " ";

            public Column(string header, int width, Func<T, string> getValueFunc)
            {
                Header = header;
                Width = width;
                GetValueFunc = getValueFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc)
                : this(header, width, getValueFunc)
            {
                GetMergeLengthFunc = getMergeLengthFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, string> getTrailingSpaceFunc)
                : this(header, width, getValueFunc)
            {
                GetTrailingSpaceFunc = getTrailingSpaceFunc;
            }

            public Column(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc, Func<T, string> getTrailingSpaceFunc)
                : this(header, width, getValueFunc)
            {
                GetMergeLengthFunc = getMergeLengthFunc;
                GetTrailingSpaceFunc = getTrailingSpaceFunc;
            }
        }

        private readonly string _title;
        private readonly List<Column> _columns;

        public TableGenerator(string title)
        {
            _title = title;
            _columns = new List<Column>();
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc)
        {
            Column column = new Column(header, width, getValueFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc)
        {
            Column column = new Column(header, width, getValueFunc, getMergeLengthFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, string> getTrailingSpaceFunc)
        {
            Column column = new Column(header, width, getValueFunc, getTrailingSpaceFunc);
            _columns.Add(column);
        }

        public void AddColumn(string header, int width, Func<T, string> getValueFunc, Func<T, int> getMergeLengthFunc, Func<T, string> getTrailingSpaceFunc)
        {
            Column column = new Column(header, width, getValueFunc, getMergeLengthFunc, getTrailingSpaceFunc);
            _columns.Add(column);
        }

        public StringBuilder Generate(IEnumerable<T> items)
        {
            StringBuilder sb = new StringBuilder();

            int width = 1 + _columns.Sum(x => x.Width) + _columns.Count;

            // Header

            //-->
            // line before title
            sb.Append('+');
            for (int i = 1; i < width - 1; i++)
                sb.Append('-');
            sb.AppendLine("+");

            // title
            sb.Append("| ");
            sb.Append(_title);
            for (int i = 3 + _title.LengthNoColor(); i < width; i++)
                sb.Append(' ');
            sb.AppendLine("|");

            // line after title
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");

            // column headers
            foreach (Column column in _columns)
            {
                sb.Append("| ");
                sb.Append(column.Header);
                for (int i = 1 + column.Header.LengthNoColor(); i < column.Width; i++)
                    sb.Append(' ');
            }
            sb.AppendLine("|");

            // line after column header
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");
            //<-- can be precomputed

            // Values
            foreach (T item in items)
            {
                for (int index = 0; index < _columns.Count; index++)
                {
                    Column column = _columns[index];

                    string value = column.GetValueFunc(item);
                    string trailingSpace = column.GetTrailingSpaceFunc(item);
                    int columnWidth = column.Width;
                    int mergeLength = column.GetMergeLengthFunc(item);
                    if (mergeLength > 0)
                    {
                        // if cell is merged, increase articially column width
                        for (int i = index + 1; i < index + 1 + mergeLength; i++)
                            columnWidth += 1 + _columns[i].Width;
                        index += mergeLength;
                    }

                    sb.Append('|');
                    if (trailingSpace == null) // if not trailing space, align on left
                    {
                        sb.Append(' ');
                        sb.Append(value);
                        for (int i = value.LengthNoColor() - 1; i < columnWidth-2; i++)
                            sb.Append(' ');
                    }
                    else // if trailing space, align on right
                    {
                        for (int i = 1 + value.LengthNoColor() + trailingSpace.LengthNoColor() - 1; i < columnWidth; i++)
                            sb.Append(' ');
                        sb.Append(value);
                        sb.Append(trailingSpace);
                    }
                }
                sb.AppendLine("|");
            }

            // line after values
            foreach (Column column in _columns)
            {
                sb.Append('+');
                for (int i = 0; i < column.Width; i++)
                    sb.Append('-');
            }
            sb.AppendLine("+");

            // ONLY FOR TEST PURPOSE
            //Console.Write(sb.ToString());

            return sb;
        }
    }
}
