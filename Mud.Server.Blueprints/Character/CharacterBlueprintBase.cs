﻿using System.Runtime.Serialization;
using Mud.Domain;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Flags.Interfaces;

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
        public Sizes Size { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public long Wealth { get; set; }

        [DataMember]
        public int Alignment { get; set; }

        [DataMember]
        public string DamageNoun { get; set; }

        [DataMember]
        public SchoolTypes DamageType{ get; set; }

        [DataMember]
        public int DamageDiceCount { get; set; }

        [DataMember]
        public int DamageDiceValue { get; set; }

        [DataMember]
        public int DamageDiceBonus { get; set; }

        [DataMember]
        public int HitPointDiceCount { get; set; }

        [DataMember]
        public int HitPointDiceValue { get; set; }

        [DataMember]
        public int HitPointDiceBonus { get; set; }

        [DataMember]
        public int ManaDiceCount { get; set; }

        [DataMember]
        public int ManaDiceValue { get; set; }

        [DataMember]
        public int ManaDiceBonus { get; set; }

        [DataMember]
        public int HitRollBonus { get; set; }

        [DataMember]
        public int ArmorBash { get; set; }

        [DataMember]
        public int ArmorPierce { get; set; }

        [DataMember]
        public int ArmorSlash { get; set; }

        [DataMember]
        public int ArmorExotic { get; set; }

        [DataMember]
        public IActFlags ActFlags { get; set; }

        [DataMember]
        public IOffensiveFlags OffensiveFlags { get; set; }

        [DataMember]
        public IAssistFlags AssistFlags { get; set; }

        [DataMember]
        public ICharacterFlags CharacterFlags { get; set; }

        [DataMember]
        public IIRVFlags Immunities { get; set; }

        [DataMember]
        public IIRVFlags Resistances { get; set; }

        [DataMember]
        public IIRVFlags Vulnerabilities { get; set; }

        [DataMember]
        public string Race { get; set; }

        [DataMember]
        public string Class { get; set; }

        [DataMember]
        public IBodyForms BodyForms { get; set; }

        [DataMember]
        public IBodyParts BodyParts { get; set; }

        // TODO CharacterAttributes

        // TODO: affects, ...

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
