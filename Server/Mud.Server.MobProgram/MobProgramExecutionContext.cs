using Mud.Random;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

public class MobProgramExecutionContext : IMobProgramExecutionContext
{
    public required INonPlayableCharacter Self { get; set; } // $i
    public ICharacter? Triggerer { get; set; } // $n
    public ICharacter? Secondary { get; set; } // $t
    public ICharacter? Target => Self.MobProgramTarget; // $q

    public IItem? PrimaryObject { get; set; } // $o
    public IItem? SecondaryObject { get; set; } // $p

    public Dictionary<string, object> Variables { get; set; } = new(); // other variables such as hour

    public required int MobProgramId { get; set; }
    public required IRandomManager RandomManager { get; set; } 
    public required ICharacterManager CharacterManager { get; set; }
    public required ITimeManager TimeManager { get; set; }
}
