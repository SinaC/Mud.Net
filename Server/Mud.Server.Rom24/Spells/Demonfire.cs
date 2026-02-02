using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <target>")]
[Help(
@"Demonfire is a spell of blackest evil, and as such can only be used by those
who follow the paths of darkness.  It conjures forth demonic spirits to 
inflict terrible wounds on the enemies of the caster.")]
[OneLineHelp("a powerful, but very evil, spell")]
public class Demonfire : DamageSpellBase
{
    private const string SpellName = "Demonfire";

    public Demonfire(ILogger<Demonfire> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Negative;
    protected override string DamageNoun => "torments";
    protected override int DamageValue => RandomManager.Dice(Level, 10);

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        var baseSetTargets = base.SetTargets(spellActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        // Check alignment
        if (Caster is IPlayableCharacter && !Caster.IsEvil)
        {
            Victim = Caster;
            Caster.Send("The demons turn upon you!");
        }
        return null;
    }

    protected override void Invoke()
    {
        if (Victim != Caster)
        {
            Caster.Act(ActOptions.ToRoom, "{0} calls forth the demons of Hell upon {1}!", Caster, Victim);
            Victim.Act(ActOptions.ToCharacter, "{0} has assailed you with the demons of Hell!", Caster);
            Caster.Send("You conjure forth the demons of hell!");
        }

        base.Invoke();

        Caster.UpdateAlignment(-50);
    }
}
