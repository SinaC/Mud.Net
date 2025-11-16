using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class Rabbit : RaceBase
{
    public Rabbit(IServiceProvider serviceProvider)
    : base(serviceProvider)
    {
    }

    public override string Name => "rabbit";
    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider);
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider);
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Ear", "Eye");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Dodge", "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
