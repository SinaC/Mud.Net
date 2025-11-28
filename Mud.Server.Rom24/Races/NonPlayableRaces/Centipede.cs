using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

[Export(typeof(IRace)), Shared]
public class Centipede : RaceBase
{
    public Centipede(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override string Name => "centipede";
    public override Sizes Size => Sizes.Small;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "DarkVision");
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Pierce", "Cold");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Bash");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Poison", "Animal", "Insect");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Legs", "Eye");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
