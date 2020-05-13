using System;
using System.Collections.Concurrent;
using System.Text;
using Mud.Container;
using Mud.Network;

namespace Mud.Server.Server
{
    internal class PlayingClient
    {
        public IClient Client { get; set; }
        public IPlayer Player { get; set; }

        private readonly ConcurrentQueue<string> _receiveQueue; // concurrent queue because network may write and server read at the same time
        private readonly StringBuilder _sendBuffer;

        protected ITimeManager TimeHandler => DependencyContainer.Current.GetInstance<ITimeManager>();

        public Paging Paging { get; }

        public DateTime LastReceivedDataTimestamp { get; private set; }

        public PlayingClient()
        {
            _receiveQueue = new ConcurrentQueue<string>();
            _sendBuffer = new StringBuilder();
            Paging = new Paging();
            LastReceivedDataTimestamp = TimeHandler.CurrentTime;
        }

        public void EnqueueReceivedData(string data)
        {
            _receiveQueue.Enqueue(data);
            LastReceivedDataTimestamp = TimeHandler.CurrentTime;
        }

        public string DequeueReceivedData()
        {
            string data;
            bool dequeued = _receiveQueue.TryDequeue(out data);
            return dequeued ? data : null;
        }

        public void EnqueueDataToSend(string data)
        {
            lock (_sendBuffer) // TODO: is this really needed ???
                _sendBuffer.Append(data);
        }

        public string DequeueDataToSend()
        {
            lock (_sendBuffer) // TODO: is this really needed ???   DequeueDataToSend is processed in Server.ProcessOutput and EnqueueDataToSend is processed in Server.ProcessInput+Server.HandleXXX
            {
                string data = _sendBuffer.ToString();
                _sendBuffer.Clear();
                return data;
            }
        }
    }
}
