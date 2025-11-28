using Mud.Common;
using Mud.DataStructures.HeapPriorityQueue;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("path", "Information", MustBeImpersonated = true)]
[Syntax("[cmd] <location>")]
public class Path : AdminGameAction
{
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }

    public Path(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager)
    {
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
    }

    protected IRoom Where { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;


        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Where = FindHelpers.FindLocation(RoomManager, CharacterManager, ItemManager, Impersonating, actionInput.Parameters[0])!;
        if (Where == null)
            return "No such location.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        string path = BuildPath(Impersonating.Room, Where);
        Actor.Send("Following path will lead to {0}:", Where.DisplayName);
        Actor.Send("%c%" + path + "%x%");
    }

    private static string BuildPath(IRoom origin, IRoom destination)
    {
        if (origin == destination)
            return destination.DisplayName + " is here.";

        Dictionary<IRoom, int> distance = new(500);
        Dictionary<IRoom, Tuple<IRoom, ExitDirections>> previousRoom = new(500);
        HeapPriorityQueue<IRoom> pQueue = new(500);

        // Search path
        distance[origin] = 0;
        pQueue.Enqueue(origin, 0);

        // Dijkstra
        while (!pQueue.IsEmpty())
        {
            var nearest = pQueue.Dequeue();
            if (nearest == destination)
                break;
            if (nearest == null)
                break; // no path found
            foreach (ExitDirections direction in EnumHelpers.GetValues<ExitDirections>())
            {
                var neighbour = nearest.GetRoom(direction);
                if (neighbour != null && !distance.ContainsKey(neighbour))
                {
                    int neighbourDist = distance[nearest] + 1;
                    if (!distance.TryGetValue(neighbour, out int bestNeighbourDist))
                        bestNeighbourDist = int.MaxValue;
                    if (neighbourDist < bestNeighbourDist)
                    {
                        distance[neighbour] = neighbourDist;
                        pQueue.Enqueue(neighbour, neighbourDist);
                        previousRoom[neighbour] = new Tuple<IRoom, ExitDirections>(nearest, direction);
                    }
                }
            }
        }

        // Build path
        if (previousRoom.TryGetValue(destination, out var previous))
        {
            StringBuilder sb = new(500);
            while (true)
            {
                sb.Insert(0, previous.Item2.ShortExitDirections());
                if (previous.Item1 == origin)
                    break;
                if (!previousRoom.TryGetValue(previous.Item1, out previous))
                {
                    sb.Insert(0, "???");
                    break;
                }
            }
            // compress path:  ssswwwwnn -> 3s4w2n
            return Compress(sb.ToString());
        }
        return "No path found.";
    }

    private static string Compress(string str) //http://codereview.stackexchange.com/questions/64929/string-compression-implementation-in-c
    {
        StringBuilder builder = new();
        for (int i = 1, cnt = 1; i <= str.Length; i++, cnt++)
        {
            if (i == str.Length || str[i] != str[i - 1])
            {
                if (cnt == 1)
                    builder.Append(str[i - 1]);
                else
                    builder.Append(cnt).Append(str[i - 1]);
                cnt = 0;
            }
        }
        return builder.ToString();
    }
}
