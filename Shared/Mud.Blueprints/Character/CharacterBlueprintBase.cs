using Mud.Blueprints.LootTable;
using Mud.Blueprints.MobProgram;
using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Character;

public abstract class CharacterBlueprintBase
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string ShortDescription { get; set; } = default!;

    public string LongDescription { get; set; } = default!;

    public string Description { get; set; } = default!;

    public Sex Sex { get; set; }

    public Sizes Size { get; set; }

    public int Level { get; set; }

    public long Wealth { get; set; }

    public int Alignment { get; set; }

    public string DamageNoun { get; set; } = default!;

    public SchoolTypes DamageType{ get; set; }

    public int DamageDiceCount { get; set; }

    public int DamageDiceValue { get; set; }

    public int DamageDiceBonus { get; set; }

    public int HitPointDiceCount { get; set; }

    public int HitPointDiceValue { get; set; }

    public int HitPointDiceBonus { get; set; }

    public int ManaDiceCount { get; set; }

    public int ManaDiceValue { get; set; }

    public int ManaDiceBonus { get; set; }

    public int HitRollBonus { get; set; }

    public int ArmorBash { get; set; }

    public int ArmorPierce { get; set; }

    public int ArmorSlash { get; set; }

    public int ArmorExotic { get; set; }

    public IActFlags ActFlags { get; set; } = default!;

    public IOffensiveFlags OffensiveFlags { get; set; } = default!;

    public IAssistFlags AssistFlags { get; set; } = default!;

    public ICharacterFlags CharacterFlags { get; set; } = default!;

    public IIRVFlags Immunities { get; set; } = default!;

    public IIRVFlags Resistances { get; set; } = default!;

    public IIRVFlags Vulnerabilities { get; set; } = default!;

    public IShieldFlags ShieldFlags { get; set; } = default!;

    public Positions StartPosition { get; set; } = default!;

    public Positions DefaultPosition { get; set; } = default!;

    public string Race { get; set; } = default!;

    public string Class { get; set; } = default!;

    public IBodyForms BodyForms { get; set; } = default!;

    public IBodyParts BodyParts { get; set; } = default!;

    // TODO CharacterAttributes

    // TODO: affects, ...

    public string SpecialBehavior { get; init; } = default!;

    public int Group { get; init; }

    public CharacterLootTable<int> LootTable { get; set; } = default!;

    public string ScriptTableName { get; set; } = default!;

    public List<MobProgramBase> MobPrograms { get; set; } = [];

    //
    public string[] Keywords => Name.Split([' '], StringSplitOptions.RemoveEmptyEntries);

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
