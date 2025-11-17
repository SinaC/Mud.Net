using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class Griffon : RaceBase
{
    public Griffon(IServiceProvider serviceProvider)
    : base(serviceProvider)
    {
    }

    public override string Name => "griffon";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "Infrared", "Flying", "Haste");
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Charm", "Bash", "Fire");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Pierce", "Cold");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Animal", "Mammal", "Bird");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Eye", "Wings", "Fangs");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
