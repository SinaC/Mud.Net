using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Character;

public interface IPlayableCharacter : ICharacter
{
    void Initialize(Guid guid, AvatarData data, IPlayer player, IRoom room);

    DateTime CreationTime { get; }

    IReadOnlyDictionary<string, string> Aliases { get; }
    void SetAlias(string alias, string command);
    void RemoveAlias(string alias);

    // Statistics
    long this[AvatarStatisticTypes avatarStatisticType] { get; }
    void IncrementStatistics(AvatarStatisticTypes avatarStatisticType, long increment = 1);

    // Currencies
    int this[Currencies currency] { get; }
    void UpdateCurrency(Currencies currency, int delta);

    // Attributes
    long ExperienceToLevel { get; }
    long Experience { get; }
    int Trains { get; }
    int Practices { get; }
    void UpdateTrainsAndPractices(int trainsAmount, int practicesAmount);

    // Wimpy
    int Wimpy { get; }
    void SetWimpy(int wimpy);

    //
    AutoFlags AutoFlags { get; }
    void AddAutoFlags(AutoFlags autoFlags);
    void RemoveAutoFlags(AutoFlags autoFlags);

    // Conditions: drunk, full, thirst, hunger
    int this[Conditions condition] { get; }
    void GainCondition(Conditions condition, int value);

    // Impersonation
    IPlayer? ImpersonatedBy { get; }

    // Group
    IGroup? Group { get; }
    void ChangeGroup(IGroup? group);
    bool IsSameGroup(IPlayableCharacter character); // in group

    // Pets
    IEnumerable<INonPlayableCharacter> Pets { get; }
    void AddPet(INonPlayableCharacter pet);
    void RemovePet(INonPlayableCharacter pet);

    // Quest
    int PulseLeftBeforeNextAutomaticQuest { get; }
    void SetTimeLeftBeforeNextAutomaticQuest(TimeSpan timeSpan);
    int DecreasePulseLeftBeforeNextAutomaticQuest(int pulseCount);
    IEnumerable<IQuest> ActiveQuests { get; }
    void AddQuest(IQuest quest);
    void RemoveQuest(IQuest quest);
    IEnumerable<ICompletedQuest> CompletedQuests { get; }
    void AddCompletedQuest(ICompletedQuest quest);

    // Room
    IRoom RecallRoom { get; }

    // Impersonation
    bool StopImpersonation();

    // Combat
    void KillingPayoff(ICharacter victim, int groupLevelSum);
    void GainExperience(long experience); // add/substract experience

    // Ability
    IEnumerable<IAbilityGroupLearned> LearnedAbilityGroups { get; }
    bool CheckAbilityImprove(string abilityName, bool abilityUsedSuccessfully, int multiplier);
    void AddLearnedAbilityGroup(IAbilityGroupUsage abilityGroupUsage);

    // Immortality
    void ChangeImmortalMode(ImmortalModeFlags mode);

    // Misc
    bool SacrificeItem(IItem item);
    bool SplitMoney(long amountSilver, long amountGold);

    // Mapping
    AvatarData MapAvatarData();
}
