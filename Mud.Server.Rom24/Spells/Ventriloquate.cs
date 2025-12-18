using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.None)]
[Syntax("cast [spell] <speaker> <message>")]
[Help(
@"This spell throws your voice, making it appear that some other object or
character in the room is saying your message.  Victims who make their saving
throw will know that someone is using ventriloquism, but not who.  Victims who
fail their saving throw will think that the object or character really did say
your message.")]
[OneLineHelp("allows the caster to put words in someone's mouth")]
public class Ventriloquate : SpellBase
{
    private const string SpellName = "Ventriloquate";

    private ICommandParser CommandParser { get; }

    public Ventriloquate(ILogger<Ventriloquate> logger, IRandomManager randomManager, ICommandParser commandParser)
        : base(logger, randomManager)
    {
        CommandParser = commandParser;
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
        Phrase = CommandParser.JoinParameters(spellActionInput.Parameters.Skip(1));
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
