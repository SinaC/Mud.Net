using Mud.Blueprints.Room;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.Room;
using System.Diagnostics;

namespace Mud.Server.Room;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Exit : IExit
{
    public Exit(ExitBlueprint blueprint, IRoom destination, IFlagsManager flagsManager)
    {
        Name = blueprint.Keyword;
        Keywords = Name.Split([' '], StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
        Description = blueprint.Description;
        Destination = destination;
        Blueprint = blueprint;
        ExitFlags = Blueprint.Flags;
        flagsManager.CheckFlags(ExitFlags);
    }

    #region IExit

    #region ICloseable

    public int KeyId => Blueprint.Key;
    public bool IsCloseable => ExitFlags.IsSet("Door");
    public bool IsLockable => KeyId >= 0;
    public bool IsClosed => ExitFlags.IsSet("Closed");
    public bool IsLocked => ExitFlags.IsSet("Locked");
    public bool IsPickProof => ExitFlags.IsSet("PickProof");
    public bool IsEasy => ExitFlags.IsSet("Easy");
    public bool IsHard => ExitFlags.IsSet("Hard");

    public void Open()
    {
        ExitFlags.Unset("Closed");
    }

    public void Close()
    {
        if (IsCloseable)
            ExitFlags.Set("Closed");
    }

    public void Unlock()
    {
        ExitFlags.Unset("Locked");
    }

    public void Lock()
    {
        if (IsLockable && IsClosed)
            ExitFlags.Set("Locked");
    }

    #endregion

    public ExitBlueprint Blueprint { get; private set; }

    public string Name { get; }
    public IEnumerable<string> Keywords { get; }
    public string Description { get; }
    public IRoom Destination { get; private set; }
    public IExitFlags ExitFlags { get; private set; }

    public bool IsDoor => ExitFlags.IsSet("Door");
    public bool IsHidden => ExitFlags.IsSet("Hidden");

    public void OnRemoved()
    {
        Destination = null!;
        Blueprint = null!;
    }

    #endregion

    //
    private string DebuggerDisplay => $"Exit {Name} Dir:{Blueprint?.Direction} Dest:{Destination?.Blueprint?.Id} Flags:{ExitFlags}";
}
