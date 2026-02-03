using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_executioner")]
public class Executioner : ISpecialBehavior
{
    // search a KILLER or a THIEF and attack it
    public bool Execute(INonPlayableCharacter npc)
    {
        if (!npc.IsValid || npc.Room == null
            || npc.Position <= Positions.Sleeping || npc.Fighting != null)
            return false;

        //var killerOrThief = npc.Room.People.OfType<IPlayableCharacter>().FirstOrDefault(x => x.IsKiller || x.IsThief);
        //if (killerOrThief == null)
        // return false;
        //var crime = killerOrThief.IsKiller ? "KILLER" : "THIEF";
        // do_yell("%s is a %s!  PROTECT THE INNOCENT!  MORE BLOOOOD!!!");
        // npc.MultiHit(killerOrThief);
        // return true;

        // TODO
        return false;
    }
}
/*
bool spec_executioner( CHAR_DATA *ch )
{
char buf[MAX_STRING_LENGTH];
CHAR_DATA *victim;
CHAR_DATA *v_next;
char *crime;

if ( !IS_AWAKE(ch) || ch->fighting != NULL )
	return FALSE;

crime = "";
for ( victim = ch->in_room->people; victim != NULL; victim = v_next )
{
	v_next = victim->next_in_room;

	if ( !IS_NPC(victim) && IS_SET(victim->act, PLR_KILLER) 
	&&   can_see(ch,victim))
	    { crime = "KILLER"; break; }

	if ( !IS_NPC(victim) && IS_SET(victim->act, PLR_THIEF) 
	&&   can_see(ch,victim))
	    { crime = "THIEF"; break; }
}

if ( victim == NULL )
	return FALSE;

sprintf( buf, "%s is a %s!  PROTECT THE INNOCENT!  MORE BLOOOOD!!!",
	victim->name, crime );
REMOVE_BIT(ch->comm,COMM_NOSHOUT);
do_function(ch, &do_yell, buf );
multi_hit( ch, victim, TYPE_UNDEFINED );
return TRUE;
}*/
