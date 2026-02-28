namespace Mud.POC.DeferredAction;

public class ScriptContext
{
    private readonly World _world;

    public ScriptContext(World world)
    {
        _world = world;
    }

    // Messaging
    public void Notify(string message)
    {
        // In a real MUD, send message to players in room or to mob's client
        Console.WriteLine(message);
    }
}
