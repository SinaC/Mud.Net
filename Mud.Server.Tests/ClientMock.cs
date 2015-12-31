using System.Collections.Generic;
using Mud.Network;

namespace Mud.Server.Tests
{
    public class ClientMock : IClient
    {
        public List<string> WrittenData { get; set; }
        public List<string> ReceivedData { get; set; }

        public ClientMock()
        {
            WrittenData = new List<string>();
            ReceivedData = new List<string>();
        }

        public void OnDataReceived(string data)
        {
            ReceivedData.Add(data);
            if (DataReceived != null)
                DataReceived(this, data);
        }

        public void Reset()
        {
            WrittenData.Clear();
            ReceivedData.Clear();
        }

        #region IClient

        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;
        
        public bool ColorAccepted { get; set; }

        public void WriteData(string data)
        {
            WrittenData.Add(data);
        }

        public void Disconnect()
        {
        }

        #endregion
    }
}
