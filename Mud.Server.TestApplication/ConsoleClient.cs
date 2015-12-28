using System;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    // Simple client dumping message to console
    internal class ConsoleClient : IClient
    {
        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;

        public string Name { get; set; }

        public ConsoleClient(string name)
        {
            Name = name;
        }

        public void WriteData(string data)
        {
            string remaining = data;

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Name+" <= ");
            Console.ResetColor();

            // Parse color code
            while (true)
            {
                int startIndex = remaining.IndexOf("%^", StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0)
                {
                    string preceding = remaining.Substring(0, startIndex);
                    remaining = remaining.Substring(startIndex + 2);
                    Console.Write(preceding);
                    int endIndex = remaining.IndexOf("%^", StringComparison.OrdinalIgnoreCase);
                    if (endIndex >= 0)
                    {
                        string colorCode = remaining.Substring(0, endIndex).ToLower();
                        SwitchColor(colorCode);
                        remaining = remaining.Substring(endIndex + 2);
                    }
                    else
                        remaining = remaining.Remove(startIndex, 2);
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

        private void SwitchColor(string colorCode)
        {
            if (colorCode == "reset") 
                Console.ResetColor();
            else if (colorCode == "default") 
                Console.ResetColor();
            else if (colorCode == "defaultback")
                Console.ResetColor();
            // Foreground
            else if (colorCode == "black")
                Console.ForegroundColor = ConsoleColor.Black;
            else if (colorCode == "red")
                Console.ForegroundColor = ConsoleColor.Red;
            else if (colorCode == "green")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (colorCode == "yellow")
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (colorCode == "blue")
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (colorCode == "magenta")
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (colorCode == "cyan")
                Console.ForegroundColor = ConsoleColor.Cyan;
            else if (colorCode == "white")
                Console.ForegroundColor = ConsoleColor.White;
            // Background
            else if (colorCode == "blackback")
                Console.BackgroundColor = ConsoleColor.Black;
            else if (colorCode == "redback")
                Console.BackgroundColor = ConsoleColor.Red;
            else if (colorCode == "greenback")
                Console.BackgroundColor = ConsoleColor.Green;
            else if (colorCode == "yellowback")
                Console.BackgroundColor = ConsoleColor.Yellow;
            else if (colorCode == "blueback")
                Console.BackgroundColor = ConsoleColor.Blue;
            else if (colorCode == "magentaback")
                Console.BackgroundColor = ConsoleColor.Magenta;
            else if (colorCode == "cyanback")
                Console.BackgroundColor = ConsoleColor.Cyan;
            else if (colorCode == "whiteback")
                Console.BackgroundColor = ConsoleColor.White;
        }
    }
}
