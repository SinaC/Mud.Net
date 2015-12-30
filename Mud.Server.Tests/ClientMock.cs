using System.Collections.Generic;
using Mud.Network;

namespace Mud.Server.Tests
{
    public class ClientMock : IClient
    {
        public List<string> WrittenData { get; set; }
        public List<string> ReceivedData { get; set; }
        private readonly Queue<string> _pendingData;

        public ClientMock()
        {
            WrittenData = new List<string>();
            ReceivedData = new List<string>();
            _pendingData = new Queue<string>();
        }

        public void OnDataReceived(string data)
        {
            ReceivedData.Add(data);
            _pendingData.Enqueue(data);
        }

        public void Reset()
        {
            WrittenData.Clear();
            ReceivedData.Clear();
            _pendingData.Clear();
        }

        #region IClient

        //public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;
        
        public bool ColorAccepted { get; set; }

        public string ReadData()
        {
            if (_pendingData.Count > 0)
                return _pendingData.Dequeue();
            return null;
        }

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
