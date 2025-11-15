using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class Fido : RaceBase
{
    public override string Name => "fido";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags("Magic");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms("Edible", "Poison", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Ear", "Eye", "Tail", "Fangs");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge");
    public override IAssistFlags AssistFlags => new AssistFlags("Race");
}
