using System;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    // Simple client dumping message to console
    internal class ConsoleClient : IClient
    {
        public bool DisplayPlayerName { get; set; }
        public bool IsEchoOff { get; private set; }
        
        public string Name { get; set; }

        public ConsoleClient(string name)
        {
            Name = name;
            ColorAccepted = true;
            DisplayPlayerName = true;
        }

        public void OnDataReceived(string data)
        {
            if (DataReceived != null)
                DataReceived(this, data);
        }

        #region IClient

        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;

        public bool ColorAccepted { get; set; }

        public void EchoOff()
        {
            //Console.ForegroundColor = Console.BackgroundColor;
            IsEchoOff = true;
        }

        public void EchoOn()
        {
            //Console.ResetColor();
            IsEchoOff = false;
        }

        public void WriteData(string data)
        {
            string remaining = data;

            if (DisplayPlayerName)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(Name + " <= ");
                Console.ResetColor();
            }

            // Parse color code
            while (true)
            {
                int startIndex = remaining.IndexOf("%", StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    string preceding = remaining.Substring(0, startIndex);
                    remaining = remaining.Substring(startIndex + 1);
                    Console.Write(preceding);
                    int endIndex = remaining.IndexOf("%", StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        string colorCode = remaining.Substring(0, endIndex);
                        SwitchColor(colorCode);
                        remaining = remaining.Substring(endIndex + 1);
                    }
                    else
                        Console.Write("%");
                }
                else
                {
                    Console.Write(remaining);
                    break;
                }
            }
            //Log.Default.WriteLine(LogLevels.Info, "[{0}] <= [[[" + data + "]]]", Name);
        }

        public void Disconnect()
        {
            // NOP
        }

        #endregion

        private void SwitchColor(string colorCode)
        {
            if (!ColorAccepted)
                return;
            if (colorCode == "x") 
                Console.ResetColor();
            // Normal
            else if (colorCode == "r")
                Console.ForegroundColor = ConsoleColor.DarkRed;
            else if (colorCode == "g")
                Console.ForegroundColor = ConsoleColor.DarkGreen;
            else if (colorCode == "y")
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else if (colorCode == "b")
                Console.ForegroundColor = ConsoleColor.DarkBlue;
            else if (colorCode == "m")
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
            else if (colorCode == "c")
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            else if (colorCode == "w")
                Console.ForegroundColor = ConsoleColor.White;
            // Light
            else if (colorCode == "R")
                Console.ForegroundColor = ConsoleColor.Red;
            else if (colorCode == "G")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (colorCode == "Y")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (colorCode == "B")
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (colorCode == "M")
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (colorCode == "C")
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (colorCode == "D")
                Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
