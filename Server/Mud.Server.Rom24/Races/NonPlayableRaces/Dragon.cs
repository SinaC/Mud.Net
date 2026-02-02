using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Dragon : RaceBase
{
    public override string Name => "dragon";
    public override Sizes Size => Sizes.Huge;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Infrared", "Flying");
    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Charm", "Bash", "Fire");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Pierce", "Cold");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Light,
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Amulet, // 2 amulets
        Mud.Domain.EquipmentSlots.Amulet,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Cloak,
        Mud.Domain.EquipmentSlots.Ring, // 2 rings
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Legs,
        Mud.Domain.EquipmentSlots.Feet,
        Mud.Domain.EquipmentSlots.MainHand, // 2 hands
        Mud.Domain.EquipmentSlots.OffHand,
        Mud.Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms( "Edible", "Sentient", "Dragon");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Legs", "Hands", "Feet", "Fingers", "Ear", "Eye", "Wings", "Tail", "Fangs", "Scales", "Claws");
    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();
}
