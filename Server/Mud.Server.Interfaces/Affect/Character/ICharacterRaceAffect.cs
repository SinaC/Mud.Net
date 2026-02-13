using Mud.Server.Race.Interfaces;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterRaceAffect : ICharacterAffect
{
    IRace Race { get; }
}
