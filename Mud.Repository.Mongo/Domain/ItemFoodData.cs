namespace Mud.Repository.Mongo.Domain
{
    public class ItemFoodData : ItemData
    {
        public int FullHours { get; set; }
        public int HungerHours { get; set; }
        public bool IsPoisoned { get; set; }
    }
}
