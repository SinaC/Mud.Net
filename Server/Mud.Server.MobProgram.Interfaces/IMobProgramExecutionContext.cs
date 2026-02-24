using Mud.Random;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.MobProgram.Interfaces;

public interface IMobProgramExecutionContext
{
    public INonPlayableCharacter Self { get; } // $i
    public ICharacter? Triggerer { get; } // $n
    public ICharacter? Secondary { get; } // $t
    public ICharacter? Target => Self.MobProgramTarget; // $q

    public IItem? PrimaryObject { get; } // $o
    public IItem? SecondaryObject { get; } // $p

    public Dictionary<string, object> Variables { get; } // other variables such as hour

    public int MobProgramId { get; }
    public IRandomManager RandomManager { get; }
    public ICharacterManager CharacterManager { get; }
    public ITimeManager TimeManager { get; }
}
