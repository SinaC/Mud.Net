using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.POC.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Bee : RaceBase
{
    public override string Name => "bee";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying", "Infrared", "Haste");
    public override IIRVFlags Immunities => new IRVFlags("Poison");
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms("Poison", "Animal", "Insect");
    public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Wings", "Guts");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge", "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags("Race");
}
