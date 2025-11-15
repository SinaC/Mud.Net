using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Group;

public class Group : IGroup
{
    public const int MaxGroupSize = 5;

    private readonly List<IPlayableCharacter> _members;

    public Group(IPlayableCharacter leader)
    {
        _members = [];
        IsValid = true;

        AddMember(leader);
    }

    #region IGroup

    public bool IsValid { get; protected set; }

    public IPlayableCharacter Leader { get; protected set; } = default!;

    public IEnumerable<IPlayableCharacter> Members
        => _members.OrderBy(x => x == Leader).ThenByDescending(x => x.Level); // leader first, then descending by level

    public bool AddMember(IPlayableCharacter member)
    {
        if (!IsValid)
            return false;
        if (member.Group != null) // already in a group
            return false;
        if (_members.Contains(member)) // already in this group (this is useless because previous test will handle this case)
            return false;
        if (_members.Count >= MaxGroupSize)
            return false;
        _members.Add(member);
        member.ChangeGroup(this);
        if (_members.Count > 1)
            Leader.Act(ActOptions.ToGroup, "{0:N} {0:h} joined the group.", member);
        if (_members.Count == 1)
        {
            Leader = member;
            Leader.Send("You are now leader of the group.", member);
        }
        return true;
    }

    public bool RemoveMember(IPlayableCharacter member)
    {
        if (!IsValid)
            return false;
        bool wasLeader = member == Leader;
        bool removed = _members.Remove(member);
        if (!removed)
            return false;
        member.ChangeGroup(null!);
        member.Send("You have left the group.");
        if (_members.Count == 0) // was last member of the group -> delete group
        {
            Leader = null!;
            _members.Clear(); // should be empty
            IsValid = false;
            return true;
        }
        if (wasLeader)
        {
            Leader = _members.First();
            Leader.Act(ActOptions.ToGroup, "{0:N} {0:b} now leader of the group.", Leader);
        }
        Leader.Act(ActOptions.ToGroup, "{0:N} has left the group.", member);
        return true;
    }

    public bool ChangeLeader(IPlayableCharacter member)
    {
        if (!IsValid)
            return false;
        if (Leader == member)
            return false;
        if (!_members.Contains(member))
            return false;
        Leader = member;
        Leader.Act(ActOptions.ToGroup, "{0:N} {0:b} now leader of the group.", Leader);
        return true;
    }

    public void Disband()
    {
        if (!IsValid)
            return;
        foreach (IPlayableCharacter member in _members)
        {
            member.Send("You have left the group.");
            member.ChangeGroup(null);
        }
        Leader = null!;
        _members.Clear(); // should be empty
        IsValid = false;
    }

    #endregion
}
