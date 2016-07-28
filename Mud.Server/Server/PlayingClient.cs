using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mud.Network;

namespace Mud.Server.Server
{
    internal class PlayingClient
    {
        public IClient Client { get; set; }
        public IPlayer Player { get; set; }

        private readonly Queue<string> _receiveQueue;
        private readonly StringBuilder _sendBuffer;

        public Paging Paging { get; }

        public PlayingClient()
        {
            _receiveQueue = new Queue<string>();
            _sendBuffer = new StringBuilder();
            Paging = new Paging();
        }

        public void EnqueueReceivedData(string data)
        {
            _receiveQueue.Enqueue(data);
        }

        public string DequeueReceivedData()
        {
            //string data;
            //bool dequeued = _receiveQueue.TryDequeue(out data);
            //return dequeued ? data : null;
            return _receiveQueue.Count == 0 ? null : _receiveQueue.Dequeue();
        }

        public void EnqueueDataToSend(string data)
        {
            lock (_sendBuffer)
                _sendBuffer.Append(data);
        }

        public string DequeueDataToSend()
        {
            lock (_sendBuffer)
            {
                string data = _sendBuffer.ToString();
                _sendBuffer.Clear();
                return data;
            }
        }
    }
}
