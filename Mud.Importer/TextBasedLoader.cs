using System;
using System.IO;
using System.Text;
using Mud.Logger;

namespace Mud.Importer
{
    // TODO: Importer.Smaug, Importer.Rom, Importer.Merc, Importer.Classic, Importer.Circle Importer.DSA, Importer.OldMerc (see db.C)
    public abstract class TextBasedLoader
    {
        private int _currentLine;
        private int _currentIndex;
        private string _content;

        public string CurrentFilename { get; private set; }

        public void Load(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("Cannot load file", filename);
            Log.Default.WriteLine(LogLevels.Debug, "Reading " + filename);

            CurrentFilename = filename;
            using (TextReader tr = new StreamReader(filename))
            {
                _currentLine = 1;
                _content = tr.ReadToEnd();
                _currentIndex = 0;
            }
        }

        public abstract void Parse();

        protected void Warn(string format, params object[] parameters)
        {
            string message = CurrentFilename + ":" + _currentLine + "->" + string.Format(format, parameters);
            Log.Default.WriteLine(LogLevels.Warning, message);
        }

        protected void RaiseParseException(string format, params object[] parameters)
        {
            string message = CurrentFilename + ":" + _currentLine + "->" + string.Format(format, parameters);
            Log.Default.WriteLine(LogLevels.Error, message);
            throw new ParseException(message);
        }

        protected char GetChar()
        {
            char currentChar = _content[_currentIndex++];
            if (currentChar == '\n')
                _currentLine++;
            return currentChar;
        }

        protected void UngetChar(char c)
        {
            if (c == '\n')
                _currentLine--;
            _currentIndex--;
        }

        protected bool IsEof()
        {
            return _currentIndex == _content.Length;
        }

        protected void ReadDice(int[] values)
        {
            values[0] = (int)ReadNumber();
            ReadLetter(); // d
            values[1] = (int)ReadNumber();
            ReadLetter(); // +
            values[2] = (int)ReadNumber();
        }

        protected long ReadNumber() // fread_number in db.C
        {
            char c;

            do
            {
                c = GetChar();
            } while (char.IsWhiteSpace(c));

            long number = 0;

            bool sign = false;
            if (c == '+')
            {
                c = GetChar();
            }
            else if (c == '-')
            {
                sign = true;
                c = GetChar();
            }

            if (!char.IsDigit(c))
                RaiseParseException("ReadNumber: bad format");

            while (char.IsDigit(c))
            {
                number = number * 10 + c - '0';
                c = GetChar();
            }

            if (sign)
                number = 0 - number;

            if (c == '|')
                number += ReadNumber();
            else if (c != ' ')
                UngetChar(c);

            return number;
        }

        protected string ReadWord() // fread_word in db.C
        {
            StringBuilder sb = new StringBuilder();
            char cEnd;
            do
            {
                cEnd = GetChar();
            } while (char.IsWhiteSpace(cEnd));

            if (cEnd != '\'' && cEnd != '"')
            {
                sb.Append(cEnd);
                cEnd = ' ';
            }

            while (true)
            {
                char c = GetChar();
                if (cEnd == ' ' ? char.IsWhiteSpace(c) : c == cEnd)
                {
                    if (cEnd == ' ')
                        UngetChar(c);
                    ReplaceColorCode(sb);
                    return sb.ToString();
                }
                else
                    sb.Append(c);
            }
        }

        protected string ReadString() // fread_string in db.C (fread_string_noalloc)
        {
            StringBuilder sb = new StringBuilder();

            char c;
            do
            {
                c = GetChar();
            } while (char.IsWhiteSpace(c));

            if (c == '~')
                return string.Empty;
            sb.Append(c);


            for (; ; )
            {
                c = GetChar();
                if (IsEof())
                {
                    Log.Default.WriteLine(LogLevels.Error, "ReadString: EOF");
                    return null;
                }
                else if (c == '\n')
                {
                    sb.AppendLine();
                }
                else if (c == '\r')
#pragma warning disable 642
                    ; // nop
#pragma warning restore 642
                else if (c == '~') // smash tilde
                {
                    ReplaceColorCode(sb);
                    return sb.ToString();
                }
                else
                    sb.Append(c);
            }
        }

        protected string ReadLine() // fread_line in db.C
        {
            StringBuilder sb = new StringBuilder();

            char c;
            do
            {
                if (IsEof())
                    RaiseParseException("ReadLine: EOF encountered on read");
                c = GetChar();
            } while (char.IsWhiteSpace(c));
            UngetChar(c);

            do
            {
                if (IsEof())
                    RaiseParseException("ReadLine: EOF encountered on read");
                c = GetChar();
                sb.Append(c);
            } while (c != '\n' && c != '\r');

            do
            {
                c = GetChar();
            } while (c == '\n' || c == '\r');

            UngetChar(c);
            ReplaceColorCode(sb);
            return sb.ToString();
        }

        protected long ReadFlags() // fread_flag in db.C
        {
            char c;
            bool negative = false;

            do
            {
                c = GetChar();
            } while (char.IsWhiteSpace(c));

            if (c == '\'')
            {
                c = GetChar();
                if (c != '\'')
                    Log.Default.WriteLine(LogLevels.Error, "Wrong flag format. (Double quote expected)");
                return 0;
            }

            if (c == '-')
            {
                negative = true;
                c = GetChar();
            }

            long number = 0;

            if (!char.IsDigit(c))
            {
                while (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                {
                    number += FlagConvert(c);
                    c = GetChar();
                }
            }

            while (char.IsDigit(c))
            {
                number = number * 10 + c - '0';
                c = GetChar();
            }

            if (c == '|')
                number += ReadFlags();

            else if (c != ' ')
                UngetChar(c);

            if (negative)
                return -1 * number;

            return number;
        }

        protected char ReadLetter() // fread_letter in db.C
        {
            char c;
            do
            {
                c = GetChar();
            } while (char.IsWhiteSpace(c));

            return c;
        }

        protected void ReadToEol()
        {
            char c;
            do
            {
                c = GetChar();
            } while (c != '\n' && c != '\r');

            do
            {
                c = GetChar();
            } while (c == '\n' || c == '\r');

            UngetChar(c);
        }

        protected void ReadToNextSection()
        {
            string line;
            do
            {
                line = ReadLine().Replace("\r", string.Empty).Replace("\n", string.Empty);
            } while(!IsEof() && line != "0 $~");
        }

        protected static string UpperCaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        protected static long FlagConvert(char letter)
        {
            long bitsum = 0;
            char i;

            if ('A' <= letter && letter <= 'Z')
            {
                bitsum = 1;
                for (i = letter; i > 'A'; i--)
                    bitsum *= 2;
            }
            else if ('a' <= letter && letter <= 'z')
            {
                bitsum = 67108864; /* 2^26 */
                for (i = letter; i > 'a'; i--)
                    bitsum *= 2;
            }

            if (bitsum < 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "Negative bit!!!!");
                return 0;
            }
            return bitsum;
        }

        protected static void ReplaceColorCode(StringBuilder sb)
        {
            sb.Replace("{{", "{")
                .Replace("{x", "%x%")
                .Replace("{D", "%w%")
                .Replace("{r", "%r%").Replace("{b", "%b%").Replace("{g", "%g%").Replace("{y", "%y%").Replace("{m", "%m%").Replace("{c", "%c%").Replace("{w", "%w%")
                .Replace("{R", "%R%").Replace("{B", "%B%").Replace("{G", "%G%").Replace("{Y", "%Y%").Replace("{M", "%M%").Replace("{C", "%C%").Replace("{W", "%W%");
        }
    }
}
