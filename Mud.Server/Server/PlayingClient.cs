using Mud.Container;
using Mud.Network.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Player;
using System.Collections.Concurrent;
using System.Text;

namespace Mud.Server.Server;

internal class PlayingClient
{
    public required IClient Client { get; set; }
    public required IPlayer Player { get; set; }

    private readonly ConcurrentQueue<string> _receiveQueue; // concurrent queue because network may write and server read at the same time
    private readonly StringBuilder _sendBuffer;

    protected ITimeManager TimeManager => DependencyContainer.Current.GetInstance<ITimeManager>();

    public Paging Paging { get; }

    public DateTime LastReceivedDataTimestamp { get; private set; }

    public PlayingClient()
    {
        _receiveQueue = new ConcurrentQueue<string>();
        _sendBuffer = new StringBuilder();
        Paging = new Paging();
        LastReceivedDataTimestamp = TimeManager.CurrentTime;
    }

    public void EnqueueReceivedData(string data)
    {
        _receiveQueue.Enqueue(data);
        LastReceivedDataTimestamp = TimeManager.CurrentTime;
    }

    public string? DequeueReceivedData()
    {
        var dequeued = _receiveQueue.TryDequeue(out var data);
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
