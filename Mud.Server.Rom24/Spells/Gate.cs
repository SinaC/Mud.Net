using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Transportation)]
[Syntax("cast [spell] <character>")]
[Help(
@"The gate spell is a powerful transportation magic that opens up a portal 
between your character and another person or creature somewhere else in the
world.  This portal will transport you and any pet you might have, but not
other members of your group.  Monsters recieve a save against gate, and
monster or players more than 3 levels higher than you can not be gated to at 
all.  God rooms, private rooms, and no recall rooms cannot be gated to, and
no recall rooms cannot be gated out of.  Finally, any god or hero is also 
immune to gate, as well as in player who has no summon set.  Clan members 
may not be gated to except by their fellow Clan members.")]
[OneLineHelp("transports the caster to the target")]
public class Gate : TransportationSpellBase
{
    private const string SpellName = "Gate";

    public Gate(ILogger<Gate> logger, IRandomManager randomManager, ICharacterManager characterManager)
        : base(logger, randomManager, characterManager)
    {
    }

    protected override void Invoke()
    {
        var originalRoom = Caster.Room;

        Caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", Caster);
        Caster.ChangeRoom(Victim.Room, false);
        Caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", Caster);
        AutoLook(Caster);

        // pets follows
        if (Caster is IPlayableCharacter pcCaster)
        {
            foreach (INonPlayableCharacter pet in pcCaster.Pets)
            {
                pet.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", pet);
                pet.ChangeRoom(Victim.Room, false);
                pet.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", pet);
                AutoLook(pet); // TODO: needed ?
            }
        }

        originalRoom?.Recompute();
        if (Caster.Room != originalRoom)
            Caster.Room.Recompute();
    }
}
