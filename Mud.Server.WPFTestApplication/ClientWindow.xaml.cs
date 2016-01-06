using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Mud.Network;

namespace Mud.Server.WPFTestApplication
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window, IClient, INetworkServer
    {
        public ClientWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            InputTextBox.Focus();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text.ToLower();
            if (DataReceived != null)
                DataReceived(this, input);
            InputTextBox.Focus();
            InputTextBox.SelectAll();
        }

        private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                SendButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); // http://stackoverflow.com/questions/728432/how-to-programmatically-click-a-button-in-wpf
        }

        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;
        public bool ColorAccepted { get; set; }

        public void EchoOff()
        {
            // NOP
        }

        public void EchoOn()
        {
            // NOP
        }

        public void WriteData(string data)
        {
            //Paragraph paragraph = new Paragraph();
            //paragraph.Inlines.Add(data);
            //OutputRichTextBox.Document.Blocks.Add(paragraph);

            Dispatcher.Invoke(() =>
            {
                Paragraph paragraph = new Paragraph();

                // Parse color code
                Brush currentColor = Brushes.White;
                string remaining = data;
                while (true)
                {
                    int startIndex = remaining.IndexOf("%", StringComparison.OrdinalIgnoreCase);
                    if (startIndex >= 0)
                    {
                        string preceding = remaining.Substring(0, startIndex);
                        remaining = remaining.Substring(startIndex + 1);
                        AddColoredTextToParagraph(paragraph, currentColor, preceding);
                        int endIndex = remaining.IndexOf("%", StringComparison.OrdinalIgnoreCase);
                        if (endIndex >= 0)
                        {
                            string colorCode = remaining.Substring(0, endIndex);
                            currentColor = GetColor(colorCode);
                            remaining = remaining.Substring(endIndex + 1);
                        }
                        else
                            AddColoredTextToParagraph(paragraph, currentColor, "%");
                    }
                    else
                    {
                        AddColoredTextToParagraph(paragraph, currentColor, remaining);
                        break;
                    }
                }
                OutputRichTextBox.Document.Blocks.Add(paragraph);
                OutputScrollViewer.ScrollToBottom();
            });
        }

        private void AddColoredTextToParagraph(Paragraph paragraph, Brush color, string text)
        {
            paragraph.Inlines.Add(new Run(text)
            {
                Foreground = color
            });
        }

        private Brush GetColor(string colorCode)
        {
            if (!ColorAccepted)
                return Brushes.White;
            if (colorCode == "x")
                return Brushes.White;
                // Normal
            else if (colorCode == "r")
                return Brushes.DarkRed;
            else if (colorCode == "g")
                return Brushes.DarkGreen;
            else if (colorCode == "y")
                return Brushes.Yellow;
            else if (colorCode == "b")
                return Brushes.DarkBlue;
            else if (colorCode == "m")
                return Brushes.DarkMagenta;
            else if (colorCode == "c")
                return Brushes.DarkCyan;
            else if (colorCode == "w")
                return Brushes.White;
                // Light
            else if (colorCode == "R")
                return Brushes.Red;
            else if (colorCode == "G")
                return Brushes.Green;
            else if (colorCode == "Y")
                return Brushes.LightYellow;
            else if (colorCode == "B")
                return Brushes.Blue;
            else if (colorCode == "M")
                return Brushes.Magenta;
            else if (colorCode == "C")
                return Brushes.Cyan;
            else if (colorCode == "D")
                return Brushes.Gray;
            return Brushes.White;
        }

        public void Disconnect()
        {
            // NOP
        }

        public event NewClientConnectedEventHandler NewClientConnected;

        public void Initialize()
        {
            // NOP
            if (NewClientConnected != null)
                NewClientConnected(this);
        }

        public void Start()
        {
            // NOP
        }

        public void Stop()
        {
            // NOP
        }
    }
}
