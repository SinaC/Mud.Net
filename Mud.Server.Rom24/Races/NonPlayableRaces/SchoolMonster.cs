using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races.NonPlayableRaces;

public class SchoolMonster : RaceBase
{
    public SchoolMonster(IServiceProvider serviceProvider)
    : base(serviceProvider)
    {
    }

    public override string Name => "school monster";
    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);
    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider, "Summon", "Charm");
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider);
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Magic");
    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Biped", "Animal", "Mammal");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Body", "Legs", "Heart", "Brains", "Guts", "Feet", "Ear", "Eye", "Tail", "Claws");
    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);
}
