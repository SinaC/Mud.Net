using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.POC.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Rat : RaceBase
{
    public override string Name => "rat";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms( "Edible", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Ear", "Eye", "Tail", "Claws");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge", "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
