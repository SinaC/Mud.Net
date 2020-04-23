namespace Mud.Repository.Mongo.Domain
{
    public class ItemCorpseData : ItemData
    {
        public bool IsPlayableCharacterCorpse { get; set; }

        public string CorpseName { get; set; }

        public ItemData[] Contains { get; set; }
    }
}
