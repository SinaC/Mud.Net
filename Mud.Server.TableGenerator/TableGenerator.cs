using Mud.Server.Common.Extensions;
using System.Text;

namespace Mud.Server.TableGenerator;

// TODO: multi table
// TODO: typed column:
//   AddBooleanColumn: automatically convert to Yes/No
//   AddIntColumn: automatically convert to string
//   AddDelayColumn: automatically call StringHelpers.FormatDelay
public class TableGenerator<T>
{
    public static readonly Func<T, string>? AlignLeftFunc = T => default!;

    public class ColumnOptions
    {
        public bool MergeIdenticalValue { get; set; } = false;
        public bool AlignLeft { get; set; }
        public bool SeparatorAfterIdenticalValue { get; set; } = false; // TODO: will add a horizontal separator after 2 or more identical consecutive values
        public Func<T, int> GetMergeLengthFunc { get; set; } = t => 0;
        public Func<T, string> GetTrailingSpaceFunc { get; set; } = t => " ";
    }

    private class Column
    {
        public required string Header { get; init; }
        public required int Width { get; init; }
        public required Func<T,string?> GetValueFunc { get; init; }

        public required ColumnOptions Options { get; init; }
    }

    private readonly List<Column> _columns;

    public TableGenerator()
    {
        _columns = [];
    }

    public void AddColumn(string header, int width, Func<T, string?> getValueFunc)
    {
        AddColumn(header, width, getValueFunc, new ColumnOptions());
    }

    public void AddColumn(string header, int width, Func<T, string?> getValueFunc, ColumnOptions options)
    {
        Column column = new() { Header = header, Width = width, GetValueFunc = getValueFunc, Options = options};
        _columns.Add(column);
    }

    public int Width => 1 + _columns.Sum(x => x.Width) + _columns.Count;

    public StringBuilder Generate(IEnumerable<string> titles, IEnumerable<T> items)
    {
        StringBuilder sb = BuildTable(titles, 1, items);
        return sb;
    }

    public StringBuilder Generate(string title, IEnumerable<T> items)
    {
        StringBuilder sb = BuildTable([title], 1, items);
        return sb;
    }

    public StringBuilder Generate(string title, int columnRepetionCount, IEnumerable<T> items)
    {
        StringBuilder sb = BuildTable([title], columnRepetionCount, items);
        return sb;
    }

    private void AddPreHeaderLine(StringBuilder sb, int columnRepetionCount, string content)
    {
        int width = Width * columnRepetionCount;

        sb.Append("| ");
        sb.Append(content);
        int contentLengthNoColor = content.LengthNoColor();
        int repetitionCount = width - (3 + contentLengthNoColor);
        if (repetitionCount > 0)
            sb.Append(' ', repetitionCount);
        sb.AppendLine("|");
    }

    private void AddSeparatorLine(StringBuilder sb, int columnRepetionCount)
    {
        int width = Width * columnRepetionCount;

        sb.Append('+');
        int repetionCount = width - 2;
        if (repetionCount > 0)
            sb.Append('-', repetionCount);
        sb.AppendLine("+");
    }

    private void AddSeparatorWithColumnsLine(StringBuilder sb, int columnRepetionCount)
    {
        for (int i = 0; i < columnRepetionCount; i++)
        {
            foreach (Column column in _columns)
            {
                sb.Append('+');
                sb.Append('-', column.Width);
            }

            sb.Append('+');
        }
        sb.AppendLine();
    }

    private void AddEmptyLineNoCrLf(StringBuilder sb)
    {
        foreach (var column in _columns)
        {
            sb.Append('|');
            sb.Append(' ', column.Width);
        }

        sb.Append('|');
    }

    private StringBuilder BuildTable(IEnumerable<string> titles, int columnRepetionCount, IEnumerable<T> items)
    {
        StringBuilder sb = new ();

        //-->
        // line before titles
        AddSeparatorLine(sb, columnRepetionCount);

        // titles
        foreach (string title in titles ?? [])
            AddPreHeaderLine(sb, columnRepetionCount, title);

        // line after titles
        AddSeparatorWithColumnsLine(sb, columnRepetionCount);

        // column headers
        for (int i = 0; i < columnRepetionCount; i++)
        {
            foreach (Column column in _columns)
            {
                sb.Append("| ");
                sb.Append(column.Header);
                int columnHeaderLengthNoColor = column.Header.LengthNoColor();
                int repetionCount = column.Width - (1 + columnHeaderLengthNoColor);
                if (repetionCount > 0)
                    sb.Append(' ', repetionCount);
            }
            sb.Append('|');
        }
        sb.AppendLine();

        // line after column header
        AddSeparatorWithColumnsLine(sb, columnRepetionCount);
        //<-- can be precomputed

        // Values
        string[] previousValues = new string[_columns.Count];
        foreach (IEnumerable<T> itemByChunk in items.Chunk(columnRepetionCount))
        {
            int repetitionCount = 0;
            foreach (T item in itemByChunk)
            {
                // TODO: if hasToMergeIdenticalValue was true for any column and is false now and SeparatorAfterIdenticalValue is true, add separator
                // TODO: hand merge identical value with columnRepetition > 1
                for (int index = 0; index < _columns.Count; index++)
                {
                    var column = _columns[index];

                    string value = column.GetValueFunc(item) ?? "";
                    bool hasToMergeIdenticalValue = column.Options.MergeIdenticalValue && columnRepetionCount == 1 && value == previousValues[index];
                    var trailingSpace = column.Options.AlignLeft
                        ? AlignLeftFunc?.Invoke(item)
                        : column.Options.GetTrailingSpaceFunc(item);
                    int columnWidth = column.Width;
                    int mergeLength = column.Options.GetMergeLengthFunc(item);
                    if (mergeLength > 0)
                    {
                        // if cell is merged, increase artificially column width
                        for (int i = index + 1; i < index + 1 + mergeLength; i++)
                            columnWidth += 1 + _columns[i].Width;
                        //for (int i = 0; i < mergeLength; i++) // reset previous values
                        //    previousValues[index + i] = null;
                        index += mergeLength;
                    }

                    sb.Append('|');
                    if (trailingSpace == null) // if not trailing space, align on left
                    {
                        if (hasToMergeIdenticalValue)
                        {
                            sb.Append(' ', columnWidth);
                        }
                        else
                        {
                            sb.Append(' ');
                            sb.Append(value);
                            for (int i = value.LengthNoColor() - 1; i < columnWidth - 2; i++)
                                sb.Append(' ');
                        }
                    }
                    else // if trailing space, align on right
                    {
                        if (hasToMergeIdenticalValue)
                        {
                            sb.Append(' ', columnWidth);
                        }
                        else
                        {
                            for (int i = 1 + value.LengthNoColor() + trailingSpace.LengthNoColor() - 1; i < columnWidth; i++)
                                sb.Append(' ');
                            sb.Append(value);
                            sb.Append(trailingSpace);
                        }
                    }

                    // store previous value
                    previousValues[index] = value;
                }

                sb.Append('|');
                repetitionCount++;
            }

            if (repetitionCount != columnRepetionCount)
            {
                for(int i = repetitionCount; i < columnRepetionCount; i++)
                    AddEmptyLineNoCrLf(sb);
            }

            sb.AppendLine();
        }

        // line after values
        AddSeparatorWithColumnsLine(sb, columnRepetionCount);

        // ONLY FOR TEST PURPOSE
        //Console.Write(sb.ToString());
        return sb;
    }
}
