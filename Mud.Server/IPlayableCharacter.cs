using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;

namespace Mud.Server
{
    public interface IPlayableCharacter : ICharacter
    {
        DateTime CreationTime { get; }

        long ExperienceToLevel { get; }

        // Attributes
        long Experience { get; }
        int Trains { get; }
        int Practices { get; }

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

        // Room
        IRoom RecallRoom { get; }

        // Group/Follower
        bool ChangeLeader(IPlayableCharacter leader);
        bool AddGroupMember(IPlayableCharacter member, bool silent);
        bool RemoveGroupMember(IPlayableCharacter member, bool silent);
        bool AddFollower(IPlayableCharacter follower);
        bool StopFollower(IPlayableCharacter follower);

        // Impersonation
        bool StopImpersonation();

        // Combat
        void GainExperience(long experience); // add/substract experience

        // Ability
        bool CheckAbilityImprove(KnownAbility ability, bool abilityUsedSuccessfully, int multiplier);

        // Mapping
        CharacterData MapCharacterData();
    }
}
