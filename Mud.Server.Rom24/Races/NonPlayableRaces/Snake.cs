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
    public Snake(IServiceProvider serviceProvider)
    : base(serviceProvider)
    {
    }

    public override string Name => "snake";
    public override Sizes Size => Sizes.Small;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Poison");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Cold");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Animal", "Reptile", "Snake", "ColdBlood");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Heart", "Brains", "Guts", "Eye", "Tail", "Claws", "Scales", "LongTongue");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Bite");
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
