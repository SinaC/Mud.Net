using System.Collections.Generic;
using Mud.Network;

namespace Mud.Server.Tests
{
    public class ClientMock : IClient
    {
        public List<string> WrittenData { get; set; }

        #region IClient

        public event DataReceivedEventHandler DataReceived;
        public event DisconnectedEventHandler Disconnected;
        
        
        public void WriteData(string data)
        {
            WrittenData.Add(data);
        }

        public void Disconnect()
        {
            
        }

        #endregion

        public void Reset()
        {
            WrittenData.Clear();
        }
    }
}
