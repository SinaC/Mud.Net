using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces;

public interface IGroup
{
    bool IsValid { get; }

    IPlayableCharacter Leader { get; }
    IEnumerable<IPlayableCharacter> Members { get; }

    bool AddMember(IPlayableCharacter member);
    bool RemoveMember(IPlayableCharacter member);
    bool ChangeLeader(IPlayableCharacter member);
    void Disband();
}
