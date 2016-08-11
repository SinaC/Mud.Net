using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Helpers
{
    /* should handle this kind of table
+-------------------------------------------------------+
|                     Mob1 (Sinac)                      |
+---------------------------+---------------------------+
| Strength  : [15008/15008] | Race  :             Troll |
| Agility   : [15015/15015] | Class :             Thief |
| Stamina   : [14991/14991] | Sex   :              Male |
| Intellect : [15000/15000] | Level :                28 |
| Spirit    : [14997/14997] |                           |
+---------------------------+--+------------------------+
| Hit    : [  750550/  750550] | Attack Power : [ 30066]|
| Mana   : [    2800/    2800] | Spell Power  : [ 15050]|
| Energy :           [100/100] | Attack Speed : [    20]|
| Rage   :           [120/120] | Armor:         [     0]|
| Runic  :           [130/130] |                        |
+------------------------------+------------------------+
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

            // + at the beginning and end or each row
            // whitespace before and after each cell
            // | between each column
            int maxWidth = _rows.Max(r => r.ColumnWidth.Sum(c => c + 4) - (r.ColumnWidth.Count - 1));

            return sb;
        }
    }
}
