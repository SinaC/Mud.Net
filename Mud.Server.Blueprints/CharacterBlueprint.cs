using System.Runtime.Serialization;

namespace Mud.Server.Blueprints
{
    [DataContract]
    public class CharacterBlueprint
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string LongDescription { get; set; }

        [DataMember]
        public string Description { get; set; }

        // TODO: flags, level, armor, damage, ...
    }
}
