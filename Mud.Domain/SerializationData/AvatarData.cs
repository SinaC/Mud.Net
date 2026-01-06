using Mud.Common.Attributes;

namespace Mud.Domain.SerializationData;

[JsonBaseType(typeof(CharacterData))]
public class AvatarData : CharacterData
{
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

    public required AutoFlags AutoFlags { get; set; }

    public required Dictionary<Currencies, int> Currencies { get; set; }

    public int PulseLeftBeforeNextAutomaticQuest { get; set; } // not mandatory

    public required CurrentQuestData[] CurrentQuests { get; set; }

    public required LearnedAbilityData[] LearnedAbilities { get; set; }
    
    public required LearnedAbilityGroupData[] LearnedAbilityGroups { get; set; } = [];

    public required Dictionary<Conditions, int> Conditions { get; set; }

    public required Dictionary<string, string> Aliases { get; set; }

    public required Dictionary<string, int> Cooldowns { get; set; }

    public required PetData[] Pets { get; set; }
}
