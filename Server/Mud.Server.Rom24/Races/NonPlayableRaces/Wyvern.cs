using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Wyvern : RaceBase
{
    public override string Name => "wyvern";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying", "DetectHidden", "DetectInvis");
    public override IIRVFlags Immunities => new IRVFlags("Poison");
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags("Light");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms( "Edible", "Poison", "Animal", "Dragon");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Ear", "Eye", "Tail", "Fangs", "Scales", "Wings");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Bash", "Dodge", "Fast", "Bite");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
