using Microsoft.Extensions.Logging;
using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell summons a character from anywhere else in the world into your room.
Characters who are fighting may not be summoned.")]
[OneLineHelp("transports the target to the caster")]
public class Summon : TransportationSpellBase
{
    private const string SpellName = "Summon";

    public Summon(ILogger<Summon> logger, IRandomManager randomManager, ICharacterManager characterManager)
        : base(logger, randomManager, characterManager)
    {
    }

    protected override void Invoke()
    {
        Victim.Act(ActOptions.ToRoom, "{0:N} disappears suddenly.", Victim);
        Victim.ChangeRoom(Caster.Room, true);
        Victim.Act(ActOptions.ToRoom, "{0:N} arrives suddenly", Victim);
        Victim.Act(ActOptions.ToCharacter, "{0:N} has summoned you!", Caster);
        AutoLook(Victim);
    }

    protected override bool IsVictimValid() // TODO: try to refactor to use base.IsVictimValid
    {
        var npcVictim = Victim as INonPlayableCharacter;
        var pcVictim = Victim as IPlayableCharacter;
        if (Victim == Caster
            || Victim.Room == null
            || Caster.Room.RoomFlags.HasAny("Safe", "NoRecall")
            || Victim.Room.RoomFlags.HasAny("Safe", "Private", "Solitary", "NoRecall", "ImpOnly")
            || (npcVictim != null && npcVictim.ActFlags.IsSet("Aggressive"))
            || Victim.Level >= Level + 3
            || Victim.ImmortalMode.IsSet("AlwaysSafe")
            || Victim.Fighting != null
            || (npcVictim != null && npcVictim.Immunities.IsSet("Summon"))
            || (npcVictim != null && (npcVictim.Blueprint is CharacterShopBlueprintBase))
            //TODO: plr_nosummon || playableCharacterVictim
            || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
            return false;
        return true;
    }
}
