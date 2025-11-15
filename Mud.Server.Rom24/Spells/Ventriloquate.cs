using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.None)]
public class Ventriloquate : SpellBase
{
    private const string SpellName = "Ventriloquate";

    public Ventriloquate(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected ICharacter Victim { get; set; } = default!;
    protected string Phrase { get; set; } = default!;

    protected override void SaySpell()
    {
        // NOP
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.Parameters.Length < 2)
            return "Make who saying what?";

        Victim = FindHelpers.FindByName(Caster.Room.People, spellActionInput.Parameters[0])!;
        if (Victim == null)
            return "They aren't here.";
        if (Victim == Caster)
            return "Just say it.";
        Phrase = CommandHelpers.JoinParameters(spellActionInput.Parameters.Skip(1));
        return null;
    }

    protected override void Invoke()
    {
        var phraseSuccess = $"%g%{Victim.DisplayName} says '%x%{Phrase ?? ""}%g%'%x%.";
        var phraseFail = $"Someone makes %g%{Victim.DisplayName} say '%x%{Phrase ?? ""}%g%'%x%.";

        foreach (var character in Caster.Room.People.Where(x => x != Victim && x.Position > Positions.Sleeping))
        {
            if (character.SavesSpell(Level, SchoolTypes.Other))
                character.Send(phraseFail);
            else
                character.Send(phraseSuccess);
        }
    }
}
