using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation)]
[Syntax("cast [spell]")]
[Help(
@"This spell duplicates the built-in RECALL ability.  It is provided solely for
Merc-based muds which wish to eliminate the built-in ability while still
providing the spell.")]
public class WordOfRecall : SpellBase
{
    private const string SpellName = "Word of Recall";

    public WordOfRecall(ILogger<WordOfRecall> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected IPlayableCharacter Victim { get; set; } = default!;

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        ICharacter victim;
        if (spellActionInput.Parameters.Length < 1)
            victim = Caster;
        else
        {
            victim = FindHelpers.FindByName(Caster.Room.People, spellActionInput.Parameters[0])!;
            if (victim == null)
                return "They aren't here.";
        }
        if (victim is not IPlayableCharacter pcVictim)
            return "Spell failed.";
        Victim = pcVictim;
        if (Victim.CharacterFlags.IsSet("Curse")
            || Victim.Room.RoomFlags.IsSet("NoRecall"))
            return "Spell failed.";
        // victim found, is PC and is not affected by Curse or in NoRecall room
        return null;
    }

    protected override void Invoke()
    {
        var originalRoom = Victim.Room;

        var recallRoom = Victim.RecallRoom;
        if (recallRoom == null)
        {
            Logger.LogError("No recall room found for {name}", Victim.DebugName);
            Caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} completely lost.", Victim);
            return;
        }

        if (Victim.Fighting != null)
            Victim.StopFighting(true);

        Victim.UpdateMovePoints(-Victim.MovePoints / 2); // half move
        Victim.Act(ActOptions.ToRoom, "{0:N} disappears", Victim);
        Victim.ChangeRoom(recallRoom, false);
        Victim.Act(ActOptions.ToRoom, "{0:N} appears in the room.", Victim);
        StringBuilder sb = new ();
        Victim.Room.Append(sb, Victim);
        Victim.Send(sb);

        // Pets follows
        foreach (INonPlayableCharacter pet in Victim.Pets)
        {
            // no recursive call because Spell has been coded for IPlayableCharacter
            if (pet.CharacterFlags.IsSet("Curse"))
                continue; // pet failing doesn't impact return value
            if (pet.Fighting != null)
            {
                if (!RandomManager.Chance(80))
                    continue;// pet failing doesn't impact return value
                pet.StopFighting(true);
            }

            pet.Act(ActOptions.ToRoom, "{0:N} disappears", pet);
            pet.ChangeRoom(recallRoom, false);
            pet.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pet);
            StringBuilder sbPet = new ();
            pet.Room.Append(sbPet, pet);
            pet.Send(sbPet);
        }

        originalRoom?.Recompute();
        if (Victim.Room != originalRoom)
            Victim.Room.Recompute();
    }
}
