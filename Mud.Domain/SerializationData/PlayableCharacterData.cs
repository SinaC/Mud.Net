namespace Mud.Domain.SerializationData;

public class PlayableCharacterData : CharacterData
{
    public required DateTime CreationTime { get; set; }

    public required int RoomId { get; set; }

    public required long SilverCoins { get; set; }

    public required long GoldCoins { get; set; }

    public required long Experience { get; set; }

    public required int Alignment { get; set; }

    public required int Trains { get; set; }

    public required int Practices { get; set; }

    public required AutoFlags AutoFlags { get; set; }

    public required CurrentQuestData[] CurrentQuests { get; set; }

    public required LearnedAbilityData[] LearnedAbilities { get; set; }

    public required Dictionary<Conditions, int> Conditions { get; set; }

    public required Dictionary<string, string> Aliases { get; set; }

    public required Dictionary<string, int> Cooldowns { get; set; }

    public required PetData[] Pets { get; set; }
}
