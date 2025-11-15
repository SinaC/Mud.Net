namespace Mud.Domain;

public class ItemCorpseData : ItemData
{
    public required ItemData[] Contains { get; set; }

    public required bool IsPlayableCharacterCorpse { get; set; }

    public required string CorpseName { get; set; }

    public required bool HasBeenGeneratedByKillingCharacter { get; set; }
}
