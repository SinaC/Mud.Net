using System.Collections.Generic;
using Mud.Domain;

namespace Mud.Server
{
    public interface IPlayableCharacter : ICharacter
    {
        long ExperienceToLevel { get; }

        // Attributes
        long Experience { get; }

        // Group/Follower
        IPlayableCharacter Leader { get; }
        IEnumerable<IPlayableCharacter> GroupMembers { get; } //!! leader is not be member of its own group and only leader stores GroupMembers
        bool IsSameGroup(IPlayableCharacter character); // check is 'this' and 'character' are in the same group

        // Impersonation
        IPlayer ImpersonatedBy { get; }

        // Quest
        IEnumerable<IQuest> Quests { get; }
        void AddQuest(IQuest quest);
        void RemoveQuest(IQuest quest);

        // Group/Follower
        bool ChangeLeader(IPlayableCharacter leader);
        bool AddGroupMember(IPlayableCharacter member, bool silent);
        bool RemoveGroupMember(IPlayableCharacter member, bool silent);
        bool AddFollower(IPlayableCharacter follower);
        bool StopFollower(IPlayableCharacter follower);

        // Impersonation
        bool ChangeImpersonation(IPlayer player);

        // Combat
        void GainExperience(long experience); // add/substract experience

        // CharacterData
        void FillCharacterData(CharacterData characterData);
    }
}
