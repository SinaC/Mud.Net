using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation)]
[Syntax("cast [spell]")]
[Help(
@"This spell takes you from your current location to a random location somewhere
in the world.")]
[OneLineHelp("sends the target to a random location")]
public class Teleport : TransportationSpellBase
{
    private const string SpellName = "Teleport";

    private IRoomManager RoomManager { get; }

    public Teleport(ILogger<Teleport> logger, IRandomManager randomManager, IRoomManager roomManager, ICharacterManager characterManager)
        : base(logger, randomManager, characterManager)
    {
        RoomManager = roomManager;
    }

    protected override void Invoke()
    {
        IRoom destination = RoomManager.GetRandomRoom(Caster);
        if (destination == null)
        {
            Logger.LogWarning("Teleport: no random room available for {name}", Victim.DebugName);
            Caster.Send("Spell failed.");
            return;
        }

        if (Victim != Caster)
            Victim.Send("You have been teleported!");

        Victim.Act(ActOptions.ToRoom, "{0:N} vanishes.", Victim);
        Victim.ChangeRoom(destination, true);
        Victim.Act(ActOptions.ToRoom, "{0:N} slowly fades into existence.", Victim);
        AutoLook(Victim);
    }

    protected override bool IsVictimValid()
    {
        if (Victim.Room == null
            || Victim.Room.RoomFlags.IsSet("NoRecall")
            || (Victim != Caster && Victim.Immunities.IsSet("Summon"))
            || (Victim is IPlayableCharacter pcVictim && pcVictim.Fighting != null)
            || (Victim != Caster && Victim.SavesSpell(Level - 5, SchoolTypes.Other)))
            return false;
        return true;
    }
}
