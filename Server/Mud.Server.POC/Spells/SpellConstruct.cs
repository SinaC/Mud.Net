using Microsoft.Extensions.Logging;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Creation | AbilityEffects.Animation)]
public class SpellConstruct : NoTargetSpellBase
{
    private const string SpellName = "Construct";

    private ICharacterManager CharacterManager { get; }
    private IAuraManager AuraManager { get; }

    public SpellConstruct(ILogger<SpellConstruct> logger, IRandomManager randomManager, ICharacterManager characterManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        CharacterManager = characterManager;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Caster is IPlayableCharacter pcCaster)
        {
            var blueprint = CharacterManager.GetCharacterBlueprint(80000)!;
            var construct = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, Caster.Room);
            if (construct == null)
            {
                Logger.LogError("SpellConstruct: cannot create mob {blueprintId} for {caster}", blueprint.Id, Caster.DebugName);
                Caster.Send(StringHelpers.SomethingGoesWrong);
                return;
            }
            pcCaster.AddPet(construct);
            AuraManager.AddAura(construct, SpellName, Caster, Level, Pulse.Infinite, new AuraFlags("Permanent", "NoDispel"), true,
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Charm"), Operator = AffectOperators.Or });
        }
    }
}
