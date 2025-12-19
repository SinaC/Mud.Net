using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Ability.Spell;

public abstract class TransportationSpellBase : SpellBase
{
    private ICharacterManager CharacterManager { get; }

    protected TransportationSpellBase(ILogger<TransportationSpellBase> logger, IRandomManager randomManager, ICharacterManager characterManager)
        : base(logger, randomManager)
    {
        CharacterManager = characterManager;
    }

    protected ICharacter Victim { get; set; } = default!;

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
        {
            Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter ?? Caster;
            return null;
        }

        if (spellActionInput.Parameters.Length < 1)
            Victim = Caster;
        else
        {
            Victim = FindHelpers.FindChararacterInWorld(CharacterManager, Caster, spellActionInput.Parameters[0])!;
            if (Victim == null)
                return StringHelpers.CharacterNotFound;
        }
        if (!IsVictimValid())
            return "You failed.";
        return null;
    }

    protected virtual bool IsVictimValid()
    {
        var npcVictim = Victim as INonPlayableCharacter;
        if (Victim == Caster
            || Victim.Room == null
            || !Caster.CanSee(Victim.Room)
            || Caster.Room.RoomFlags.HasAny("Safe", "NoRecall")
            || Victim.Room.RoomFlags.HasAny("Safe", "Private", "Solitary", "NoRecall", "ImpOnly")
            || Victim.Level >= Level + 3
            // TODO: clan check
            // TODO: hero level check 
            || (npcVictim != null && npcVictim.Immunities.IsSet("Summon"))
            || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
            return false;
        return true;
    }

    protected virtual void AutoLook(ICharacter victim)
    {
        StringBuilder sb = new ();
        victim.Room.Append(sb, victim);
        victim.Send(sb);
    }
}
