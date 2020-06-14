namespace Mud.Domain
{
    public class ItemCorpseData : ItemData
    {
        public ItemData[] Contains { get; set; }

        public bool IsPlayableCharacterCorpse { get; set; }

        public string CorpseName { get; set; }

        public bool HasBeenGeneratedByKillingCharacter { get; set; }
    }
}
