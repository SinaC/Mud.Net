using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class SongBird : RaceBase
{
    public override string Name => "song bird";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying");
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms("Edible", "Animal", "Bird");
    public override IBodyParts BodyParts => new BodyParts("Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Eye", "Wings");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Dodge", "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
