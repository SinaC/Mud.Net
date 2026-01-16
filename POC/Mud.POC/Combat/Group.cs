namespace Mud.POC.Combat;

public class Group : IGroup
{
    private IPlayableCharacter _leader = null!;
    private List<IPlayableCharacter> _members = [];

    public IPlayableCharacter Leader => _leader;
    public IEnumerable<IPlayableCharacter> Members => _members;

    public Group(IPlayableCharacter leader, IPlayableCharacter member)
    {
        _leader = leader;
        _members.Add(leader);
        _members.Add(member);
    }

    public void AddMember(IPlayableCharacter member)
    {
        if (_members.Contains(member))
            return;
        _members.Add(member);
    }

    public bool RemoveMember(IPlayableCharacter member)
    {
        // remove member
        _members.Remove(member);
        // if we removed the leader, elect a new one
        if (member == Leader)
        {
            var newLeader = _members.FirstOrDefault()!;
            if (newLeader == null) // should never happen
                return true;
            _leader = newLeader;
        }
        // if only leader is in the group, inform about group dissolve
        if (_members.Count == 1)
            return true;
        return false;
    }
}

public interface IGroup
{
    IPlayableCharacter Leader { get; }
    public IEnumerable<IPlayableCharacter> Members { get; } // including Leader

    void AddMember(IPlayableCharacter member);
    bool RemoveMember(IPlayableCharacter member); // return true if group only consists of Leader
}