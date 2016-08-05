using System.Collections.Concurrent;
using System.Text;
using Mud.Network;

namespace Mud.Server.Server
{
    internal class PlayingClient
    {
        private readonly ConcurrentQueue<string> _receiveQueue; // concurrent queue is needed because client and server may modify receive queue at the same time
        private readonly StringBuilder _sendBuffer; // lock is not mandatory because only main loop modify sendBuffer

        public IClient Client { get; set; }
        public IPlayer Player { get; set; }

        public Paging Paging { get; }

        public PlayingClient()
        {
            _receiveQueue = new ConcurrentQueue<string>();
            _sendBuffer = new StringBuilder();
            Paging = new Paging();
        }

        public void EnqueueReceivedData(string data)
        {
            _receiveQueue.Enqueue(data);
        }

        public string DequeueReceivedData()
        {
            string data;
            bool dequeued = _receiveQueue.TryDequeue(out data);
            return dequeued ? data : null;
        }

        public void EnqueueDataToSend(string data)
        {
            //lock (_sendBuffer)
                _sendBuffer.Append(data);
        }

        public string DequeueDataToSend()
        {
            //lock (_sendBuffer)
            {
                string data = _sendBuffer.ToString();
                _sendBuffer.Clear();
                return data;
            }
        }
    }
}
