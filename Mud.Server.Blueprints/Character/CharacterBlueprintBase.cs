using System.Runtime.Serialization;
using Mud.Domain;
using Mud.Server.Blueprints.LootTable;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Blueprints.Character;

[DataContract]
[KnownType(typeof(CharacterNormalBlueprint))]
[KnownType(typeof(CharacterQuestorBlueprint))]
public abstract class CharacterBlueprintBase
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string Name { get; set; } = default!;

    [DataMember]
    public string ShortDescription { get; set; } = default!;

    [DataMember]
    public string LongDescription { get; set; } = default!;

    [DataMember]
    public string Description { get; set; } = default!;

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
    public string DamageNoun { get; set; } = default!;

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
    public IActFlags ActFlags { get; set; } = default!;

    [DataMember]
    public IOffensiveFlags OffensiveFlags { get; set; } = default!;

    [DataMember]
    public IAssistFlags AssistFlags { get; set; } = default!;

    [DataMember]
    public ICharacterFlags CharacterFlags { get; set; } = default!;

    [DataMember]
    public IIRVFlags Immunities { get; set; } = default!;

    [DataMember]
    public IIRVFlags Resistances { get; set; } = default!;

    [DataMember]
    public IIRVFlags Vulnerabilities { get; set; } = default!;

    [DataMember]
    public IShieldFlags ShieldFlags { get; set; } = default!;

    [DataMember]
    public string Race { get; set; } = default!;

    [DataMember]
    public string Class { get; set; } = default!;

    [DataMember]
    public IBodyForms BodyForms { get; set; } = default!;

    [DataMember]
    public IBodyParts BodyParts { get; set; } = default!;

    // TODO CharacterAttributes

    // TODO: affects, ...

    [DataMember]
    public string SpecialBehavior { get; init; } = default!;

    [DataMember]
    public int Group { get; init; }

    [DataMember]
    public CharacterLootTable<int> LootTable { get; set; } = default!;

    [DataMember]
    public string ScriptTableName { get; set; } = default!;

    public bool Equals(CharacterBlueprintBase? other)
    {
        return other != null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as CharacterBlueprintBase);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
