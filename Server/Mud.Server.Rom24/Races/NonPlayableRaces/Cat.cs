using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Cat : RaceBase
{
    public override string Name => "cat";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("DarkVision");
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
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Ear", "Eye", "Tail", "Fangs", "Claws");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge", "Fast", "Bite");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
