using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class Horse : RaceBase
{
    public override string Name => "horse";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms("Edible", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Eye");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Bash", "Kick");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
