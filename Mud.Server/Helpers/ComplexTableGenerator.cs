using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Helpers
{
    //TODO

/* should handle this kind of table
+--------------------------------------------------------+
|                      Mob1 (Sinac)                      |
+---------------------------+----------------------------+
| Strength  : [14981/14981] | Race   :             Troll |
| Agility   : [14987/14987] | Class  :             Druid |
| Stamina   : [15003/15003] | Sex    :              Male |
| Intellect : [15011/15011] | Level  :                20 |
| Spirit    : [14995/14995] | NxtLvl :             16600 |
+---------------------------+--+-------------------------+
| Hit    : [  751150/  751150] | Attack Power : [ 30012] |
| Mana   : [    2000/    2000] | Spell Power  : [ 15061] |
| Energy :           [100/100] | Attack Speed : [    20] |
| Rage   :           [100/100] | Armor        : [     0] |
| Runic  :           [120/120] | Form         : [Normal] |
+------------------------------+-------------------------+
*/

    public class ComplexTableGenerator
    {
        // AddSeparator
        // SetColumns(columCount, width1, width2, ...) // set column count and width for further lines
        // AddRow(value1, value2, ...) // Add a row using last columns info

        private class Row
        {
            public bool IsSeparator { get; set; }
            public List<int> ColumnWidth { get; set; }
            public List<string> Values { get; set; }
        }

        private List<int> _currentColumnWidth;

        private readonly List<Row> _rows = new List<Row>();

        public void AddSeparator()
        {
            _rows.Add(new Row
            {
                IsSeparator = true
            });
        }

        public void SetColumns(params int[] widths)
        {
            _currentColumnWidth = widths.ToList();
        }

        public void AddRow(params string[] values)
        {
            _rows.Add(new Row
            {
                IsSeparator = false,
                ColumnWidth = _currentColumnWidth,
                Values = values.ToList()
            });
        }

        public StringBuilder Generate()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            // TODO

            // + at the beginning and end or each row
            // whitespace before and after each cell
            // | between each column
            int maxWidth = _rows.Max(r => r.ColumnWidth.Sum(c => c + 4) - (r.ColumnWidth.Count - 1));

            return sb;
        }
    }
}
