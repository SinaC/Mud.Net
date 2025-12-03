using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Specials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_guard")]
    public class Guard : ISpecialBehavior
    {
        // search a KILLER or a THIEF and attack it
        // or find someone evil (align < 300) fighting another npc in the same room
        public bool Execute(INonPlayableCharacter npc)
        {
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Domain.Positions.Sleeping || npc.Fighting != null)
                return false;

            //var killerOrThief = npc.Room.People.OfType<IPlayableCharacter>().FirstOrDefault(x => x.IsKiller || x.IsThief);
            //if (killerOrThief == null)
            // return false;
            //var crime = killerOrThief.IsKiller ? "KILLER" : "THIEF";
            // do_yell("%s is a %s!  PROTECT THE INNOCENT!  MORE BLOOOOD!!!");
            // npc.MultiHit(killerOrThief);
            // return true;

            // if evil
            // Act(ToRoom, "$n screams 'PROTECT THE INNOCENT!!  BANZAI!!",
            //npc.MultiHit(killerOrThief);
            // return true;

            // TODO
            return false;
        }
    }
    /*
bool spec_guard( CHAR_DATA *ch )
{
    char buf[MAX_STRING_LENGTH];
    CHAR_DATA *victim;
    CHAR_DATA *v_next;
    CHAR_DATA *ech;
    char *crime;
    int max_evil;

    if ( !IS_AWAKE(ch) || ch->fighting != NULL )
	return FALSE;

    max_evil = 300;
    ech      = NULL;
    crime    = "";

    for ( victim = ch->in_room->people; victim != NULL; victim = v_next )
    {
	v_next = victim->next_in_room;

	if ( !IS_NPC(victim) && IS_SET(victim->act, PLR_KILLER) 
	&&   can_see(ch,victim))
	    { crime = "KILLER"; break; }

	if ( !IS_NPC(victim) && IS_SET(victim->act, PLR_THIEF) 
	&&   can_see(ch,victim))
	    { crime = "THIEF"; break; }

	if ( victim->fighting != NULL
	&&   victim->fighting != ch
	&&   victim->alignment < max_evil )
	{
	    max_evil = victim->alignment;
	    ech      = victim;
	}
    }

    if ( victim != NULL )
    {
	sprintf( buf, "%s is a %s!  PROTECT THE INNOCENT!!  BANZAI!!",
	    victim->name, crime );
 	REMOVE_BIT(ch->comm,COMM_NOSHOUT);
	do_function(ch, &do_yell, buf );
	multi_hit( ch, victim, TYPE_UNDEFINED );
	return TRUE;
    }

    if ( ech != NULL )
    {
	act( "$n screams 'PROTECT THE INNOCENT!!  BANZAI!!",
	    ch, NULL, NULL, TO_ROOM );
	multi_hit( ch, ech, TYPE_UNDEFINED );
	return TRUE;
    }

    return FALSE;
}
    */
}
