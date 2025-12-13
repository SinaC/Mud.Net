using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You are no longer invisible.")]
[AbilityItemWearOffMessage("{0} fades into view.")]
[AbilityDispellable("{0:N} fades into existence.")]
[Syntax(
    "cast [spell] <character>",
    "cast [spell] <object>")]
[Help(
@"The INVIS spell makes the target character invisible.  Invisible characters
will become visible when they attack. It may also be cast on an object
to render the object invisible.")]
[OneLineHelp("turns the target invisible")]
public class Invisibility : ItemOrDefensiveSpellBase
{
    private const string SpellName = "Invisibility";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }
    private IFlagFactory<IItemFlags, IItemFlagValues> ItemFlagFactory { get; }
    private IAuraManager AuraManager { get; }

    public Invisibility(ILogger<Invisibility> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        CharacterFlagFactory = characterFlagFactory;
        ItemFlagFactory = itemFlagFactory;
        AuraManager = auraManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        if (victim.CharacterFlags.IsSet("Invisible"))
            return;

        victim.Act(ActOptions.ToAll, "{0:N} fade{0:v} out of existence.", victim);
        AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 12), AuraFlags.None, true,
            new CharacterFlagsAffect(CharacterFlagFactory) { Modifier = CharacterFlagFactory.CreateInstance("Invisible"), Operator = AffectOperators.Or });
    }

    protected override void Invoke(IItem item)
    {
        if (item.ItemFlags.IsSet("Invis"))
        {
            Caster.Act(ActOptions.ToCharacter, "{0} is already invisible.", item);
            return;
        }

        Caster.Act(ActOptions.ToAll, "{0} fades out of sight.", item);
        AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(Level + 12), AuraFlags.None, true,
            new ItemFlagsAffect(ItemFlagFactory) { Modifier = ItemFlagFactory.CreateInstance("Invis"), Operator = AffectOperators.Or });
    }
}
