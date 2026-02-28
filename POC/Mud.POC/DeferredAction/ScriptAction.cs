namespace Mud.POC.DeferredAction;

public class ScriptAction : IGameAction
{
    private readonly Action<ScriptContext> _script;

    public ScriptAction(Action<ScriptContext> script)
    {
        _script = script;
    }

    public void Execute(World world)
    {
        var context = new ScriptContext(world);
        _script(context);
    }
}
