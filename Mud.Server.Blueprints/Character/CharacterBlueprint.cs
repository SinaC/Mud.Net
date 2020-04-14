using System;
using System.Runtime.Serialization;
using Mud.Domain;
using Mud.Server.Blueprints.LootTable;

namespace Mud.Server.Blueprints.Character
{
    [DataContract]
    public class CharacterBlueprint : IEquatable<CharacterBlueprint>
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

        [DataMember]
        public Sex Sex { get; set; }

        [DataMember]
        public int Level { get; set; }

        // TODO: race, class, flags, armor, damage, ...

        public CharacterLootTable<int> LootTable { get; set; }

        public string ScriptTableName { get; set; }

        #region IEquatable

        public bool Equals(CharacterBlueprint other)
        {
            return other != null && Id == other.Id;
        }
        
        #endregion
    }
}
