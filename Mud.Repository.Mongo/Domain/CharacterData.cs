namespace Mud.Repository.Mongo.Domain
{
    public class CharacterData
    {
        public string Name { get; set; }

        public int RoomId { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public int Sex { get; set; }

        public long Experience { get; set; }

        // TODO: aura, equipments, inventory, cooldown, quests, ...
    }
}
