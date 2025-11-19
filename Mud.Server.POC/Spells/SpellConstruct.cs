using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Animation)]
public class SpellConstruct : NoTargetSpellBase
{
    private const string SpellName = "Construct";

    private IServiceProvider ServiceProvider { get; }
    private ICharacterManager CharacterManager { get; }
    private IAuraManager AuraManager { get; }

    public SpellConstruct(ILogger<SpellConstruct> logger, IServiceProvider serviceProvider, IRandomManager randomManager, ICharacterManager characterManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        CharacterManager = characterManager;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Caster is IPlayableCharacter pcCaster)
        {
            var blueprint = CharacterManager.GetCharacterBlueprint(80000)!;
            INonPlayableCharacter construct = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, Caster.Room);
            pcCaster.AddPet(construct);
            AuraManager.AddAura(construct, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent | AuraFlags.NoDispel, true,
                new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Charm"), Operator = AffectOperators.Or });
        }
    }
}
