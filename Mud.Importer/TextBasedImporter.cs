using System;
using System.IO;
using System.Text;
using Mud.Logger;

namespace Mud.Importer
{
    public abstract class TextBasedImporter
    {
        private int _currentIndex;
        private string _content;

        public string CurrentFilename { get; private set; }

        public void Load(string filename)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Reading " + filename);

            CurrentFilename = filename;
            using (TextReader tr = new StreamReader(filename))
            {
                _content = tr.ReadToEnd();
                _currentIndex = 0;
                tr.Close();
            }
        }

        public abstract void Parse();

        protected char GetChar()
        {
            return _content[_currentIndex++];
        }

        protected void UngetChar()
        {
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
            } while (Char.IsWhiteSpace(c));

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

            if (!Char.IsDigit(c))
            {
                Log.Default.WriteLine(LogLevels.Error, "ReadNumber: bad format");
                throw new ParseException("ReadNumber: bad format");
            }

            while (Char.IsDigit(c))
            {
                number = number * 10 + c - '0';
                c = GetChar();
            }

            if (sign)
                number = 0 - number;

            if (c == '|')
                number += ReadNumber();
            else if (c != ' ')
                UngetChar();

            return number;
        }

        protected string ReadWord() // fread_word in db.C
        {
            StringBuilder sb = new StringBuilder();
            char cEnd;
            do
            {
                cEnd = GetChar();
            } while (Char.IsWhiteSpace(cEnd));

            if (cEnd != '\'' && cEnd != '"')
            {
                sb.Append(cEnd);
                cEnd = ' ';
            }

            while (true)
            {
                char c = GetChar();
                if (cEnd == ' ' ? Char.IsWhiteSpace(c) : c == cEnd)
                {
                    if (cEnd == ' ')
                        UngetChar();
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
            } while (Char.IsWhiteSpace(c));

            if (c == '~')
                return String.Empty;
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
                    sb.Append(Environment.NewLine);
                else if (c == '\r')
                    ; // nop
                else if (c == '~') // smash tilde
                    return sb.ToString();
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
                {
                    Log.Default.WriteLine(LogLevels.Error, "ReadLine: EOF encountered on read");
                    throw new ParseException("ReadLine: EOF encountered on read");
                }
                c = GetChar();
            } while (Char.IsWhiteSpace(c));
            UngetChar();

            do
            {
                if (IsEof())
                {
                    Log.Default.WriteLine(LogLevels.Error, "ReadLine: EOF encountered on read");
                    throw new ParseException("ReadLine: EOF encountered on read");
                }
                c = GetChar();
                sb.Append(c);
            } while (c != '\n' && c != '\r');

            do
            {
                c = GetChar();
            } while (c == '\n' || c == '\r');

            UngetChar();
            return sb.ToString();
        }

        protected long ReadFlags() // fread_flag in db.C
        {
            char c;
            bool negative = false;

            do
            {
                c = GetChar();
            } while (Char.IsWhiteSpace(c));

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

            if (!Char.IsDigit(c))
            {
                while (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z'))
                {
                    number += flag_convert(c);
                    c = GetChar();
                }
            }

            while (Char.IsDigit(c))
            {
                number = number * 10 + c - '0';
                c = GetChar();
            }

            if (c == '|')
                number += ReadFlags();

            else if (c != ' ')
                UngetChar();

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
            } while (Char.IsWhiteSpace(c));

            return c;
        }

        protected static string UpperCaseFirst(string s)
        {
            // Check for empty string.
            if (String.IsNullOrWhiteSpace(s))
                return String.Empty;
            // Return char and concat substring.
            return Char.ToUpper(s[0]) + s.Substring(1);
        }

        protected static long flag_convert(char letter)
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
    }
}
