using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Modron : RaceBase
{
    public Modron(IServiceProvider serviceProvider)
    : base(serviceProvider)
    {
    }

    public override string Name => "modron";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "Infrared");
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider, "Charm", "Negative", "Holy", "Mental", "Disease");
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Fire", "Cold", "Acid");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider);
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Light,
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Amulet, // 2 amulets
        Domain.EquipmentSlots.Amulet,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Cloak,
        Domain.EquipmentSlots.Waist,
        Domain.EquipmentSlots.Wrists, // 2 wrists
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Arms,
        Domain.EquipmentSlots.Hands,
        Domain.EquipmentSlots.Ring, // 2 rings
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Legs,
        Domain.EquipmentSlots.Feet,
        Domain.EquipmentSlots.MainHand, // 2 hands
        Domain.EquipmentSlots.OffHand,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Sentient");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Arms", "Legs", "Hands", "Feet", "Ear", "Eye", "Fingers");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Fast", "Bite");
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
