﻿using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Abilities;

namespace Mud.Server
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

        AutoFlags AutoFlags { get; }

        // Conditions: drunk, full, thirst, hunger
        int this[Conditions condition] { get; }
        void GainCondition(Conditions condition, int value);

        // Impersonation
        IPlayer ImpersonatedBy { get; }

        // Group
        IGroup Group { get; }
        void ChangeGroup(IGroup group);
        bool IsSameGroup(IPlayableCharacter character); // in group
        bool IsSameGroupOrPet(ICharacter character); // in group or pet


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
        bool CheckAbilityImprove(KnownAbility ability, bool abilityUsedSuccessfully, int multiplier);

        // Immortality
        void ChangeImmortalState(bool isImmortal);

        // Mapping
        PlayableCharacterData MapPlayableCharacterData();
    }
}
