using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Character
{
    public interface IPlayableCharacter : ICharacter
    {
        DateTime CreationTime { get; }

        IReadOnlyDictionary<string, string> Aliases { get; }

        long ExperienceToLevel { get; }
        bool IsImmortal { get; }

        // Attributes
        long Experience { get; }
        int Trains { get; }
        int Practices { get; }
        void UpdateTrainsAndPractices(int trainsAmount, int practicesAmount);

        AutoFlags AutoFlags { get; }
        void AddAutoFlags(AutoFlags autoFlags);
        void RemoveAutoFlags(AutoFlags autoFlags);

        // Conditions: drunk, full, thirst, hunger
        int this[Conditions condition] { get; }
        void GainCondition(Conditions condition, int value);

        // Impersonation
        IPlayer ImpersonatedBy { get; }

        // Group
        IGroup Group { get; }
        void ChangeGroup(IGroup group);
        bool IsSameGroup(IPlayableCharacter character); // in group

        // Pets
        IEnumerable<INonPlayableCharacter> Pets { get; }
        void AddPet(INonPlayableCharacter pet);
        void RemovePet(INonPlayableCharacter pet);

        // Quest
        IEnumerable<IQuest> Quests { get; }
        void AddQuest(IQuest quest);
        void RemoveQuest(IQuest quest);

        // Room
        IRoom RecallRoom { get; }

        // Impersonation
        bool StopImpersonation();

        // Combat
        void GainExperience(long experience); // add/substract experience

        // Ability
        bool CheckAbilityImprove(string abilityName, bool abilityUsedSuccessfully, int multiplier);

        // Immortality
        void ChangeImmortalState(bool isImmortal);

        // Mapping
        PlayableCharacterData MapPlayableCharacterData();
    }
}
