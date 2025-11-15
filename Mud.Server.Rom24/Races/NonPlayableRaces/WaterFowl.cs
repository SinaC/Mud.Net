using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class WaterFowl : RaceBase
{
    public override string Name => "water fowl";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying", "Swim"); // TODO: walk on water and water breath
    public override IIRVFlags Immunities => new IRVFlags("Drowning");
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Legs,
        Domain.EquipmentSlots.Feet,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms("Edible", "Animal", "Bird");
    public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Eye", "Wings");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();
}
