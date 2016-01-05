using System.Collections.Concurrent;
using System.Text;
using Mud.Network;

namespace Mud.Server.Server
{
    internal class PlayingClient
    {
        public IClient Client { get; set; }
        public IPlayer Player { get; set; }

        private readonly ConcurrentQueue<string> _receiveQueue;
        //private readonly ConcurrentQueue<string> _sendQueue; // when using this, ProcessOutput must loop until DataToSend returns null
        private readonly StringBuilder _sendBuffer;

        private readonly Paging _paging;

        public Paging Paging
        {
            get { return _paging; }
        }

        public PlayingClient()
        {
            _receiveQueue = new ConcurrentQueue<string>();
            _sendBuffer = new StringBuilder();
            _paging = new Paging();
        }

        // Used in synchronous mode
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
            //_sendQueue.Enqueue(message);
            lock (_sendBuffer)
                _sendBuffer.Append(data);
        }

        public string DequeueDataToSend()
        {
            //string data;
            //bool taken = _sendQueue.TryDequeue(out data);
            //return taken ? data : null;
            lock (_sendBuffer)
            {
                string data = _sendBuffer.ToString();
                _sendBuffer.Clear();
                return data;
            }
        }
    }
}
