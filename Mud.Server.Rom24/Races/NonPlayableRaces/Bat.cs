using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Bat : RaceBase
{
    public Bat(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override string Name => "bat";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "Flying","DarkVision");
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider);
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Light");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Legs", "Eye", "Ear", "Heart", "Brains", "Feet", "Guts", "Wings");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Dodge", "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
