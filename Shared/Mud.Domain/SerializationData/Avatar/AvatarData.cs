using Mud.Common.Attributes;

namespace Mud.Domain.SerializationData.Avatar;

[JsonBaseType(typeof(CharacterData))]
public class AvatarData : CharacterData
{
    public required int Version { get; set; }

    public required string AccountName { get; set; }

    public required DateTime CreationTime { get; set; }

    public required int RoomId { get; set; }

    public required long SilverCoins { get; set; }

    public required long GoldCoins { get; set; }

    public required int Wimpy { get; set; }

    public required long Experience { get; set; }

    public required int Alignment { get; set; }

    public required int Trains { get; set; }

    public required int Practices { get; set; }

    public required string AutoFlags { get; set; }

    public required Dictionary<Currencies, int> Currencies { get; set; }

    public required ActiveQuestData[] ActiveQuests { get; set; }

    public required LearnedAbilityData[] LearnedAbilities { get; set; }
    
    public required LearnedAbilityGroupData[] LearnedAbilityGroups { get; set; } = [];

    public required Dictionary<Conditions, int> Conditions { get; set; }

    public required Dictionary<string, string> Aliases { get; set; }

    public required Dictionary<string, int> Cooldowns { get; set; }

    public required PetData[] Pets { get; set; }

    // optional
    public Dictionary<AvatarStatisticTypes, long> Statistics { get; set; } = [];

    public string ImmortalMode { get; set; } = default!;

    public CompletedQuestData[] CompletedQuests { get; set; } = [];

    public int PulseLeftBeforeNextAutomaticQuest { get; set; }
}
