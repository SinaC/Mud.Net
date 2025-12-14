using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Snake : RaceBase
{
    public override string Name => "snake";
    public override Sizes Size => Sizes.Small;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Poison");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Cold");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms( "Edible", "Animal", "Reptile", "Snake", "ColdBlood");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Heart", "Brains", "Guts", "Eye", "Tail", "Claws", "Scales", "LongTongue");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Bite");
    public override IAssistFlags AssistFlags => new AssistFlags();
}
