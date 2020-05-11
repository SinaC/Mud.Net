using System.Runtime.Serialization;
using Mud.Domain;
using Mud.Server.Blueprints.LootTable;

namespace Mud.Server.Blueprints.Character
{
    [DataContract]
    [KnownType(typeof(CharacterNormalBlueprint))]
    [KnownType(typeof(CharacterQuestorBlueprint))]
    public abstract class CharacterBlueprintBase
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

        [DataMember]
        public ActFlags ActFlags { get; set; }

        [DataMember]
        public OffensiveFlags OffensiveFlags { get; set; }

        [DataMember]
        public CharacterFlags CharacterFlags { get; set; }

        [DataMember]
        public IRVFlags Immunities { get; set; }

        [DataMember]
        public IRVFlags Resistances { get; set; }

        [DataMember]
        public IRVFlags Vulnerabilities { get; set; }

        // TODO CharacterAttributes

        [DataMember]
        public int Alignment { get; set; }

        // TODO: race, class, flags, armor, damage, ...

        [DataMember]
        public CharacterLootTable<int> LootTable { get; set; }

        [DataMember]
        public string ScriptTableName { get; set; }

        public bool Equals(CharacterBlueprintBase other)
        {
            return other != null && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CharacterBlueprintBase);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
