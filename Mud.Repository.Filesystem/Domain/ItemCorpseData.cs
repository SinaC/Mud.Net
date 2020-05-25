namespace Mud.Repository.Filesystem.Domain
{
    public class ItemCorpseData : ItemData
    {
        public bool IsPlayableCharacterCorpse { get; set; }

        public string CorpseName { get; set; }

        public ItemData[] Contains { get; set; }

        public bool HasBeenGeneratedByKillingCharacter { get; set; }
    }
}
