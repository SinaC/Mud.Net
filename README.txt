ROM2.4b6 update and frequency

PULSE_AREA = 120 * PULSE_PER_SECOND
	area_update
		reset area -> create mobiles/items
PULSE_MUSIC = 6 * PULSE_PER_SECOND
	song_update useless
PULSE_MOBILE = 4 * PULSE_PER_SECOND
	mobile_update
		foreach uncharmed mob in a room (also depends if area is empty and not update always)
			if spec_fun -> perform and continue on next mob
			if shop -> give gold
			if !standing -> next mob
			scavenge if ACT_SCAVENGE
			wander if ACT_SENTINEL
PULSE_VIOLENCE = 3 * PULSE_PER_SECOND
	violence_update
		foreach fighting character in a room
			if awake and same room -> multihit
			else -> stop fight
			if not fighting -> next mob
			check assist
PULSE_TICK = 60 * PULSE_PER_SECOND
	weather_update
		update time and weather
	char_update
		foreach character
			if position >= stunned
				if mob affected to a zone, not fighting and not charmed + some random -> delete mob and go to next mob
				regen if not maxed
			if position == stunned -> update position
			if non-immortal player
				update light
				if away for too long -> go to limbo
				gain condition
			decrease aura time left
			if affected by plague -> damage + spread
			else if affected by poison && !slowed -> damage
			else if position == incap + random -> 1 damage
			else if position == mortal -> 1 damage
		foreach player
			autosave if needed
			force quit if have to
	obj_update
		foreach item
			decrease aura time left
			if timer elapsed
				display msg
				give shop some money
				spill content
				delete item
aggr_update (every 250ms)
	foreach player
		foreach mob in room
			aggress on some random player


Rom2.4b6 fight system
multi_hit(dt)
	decrement mob wait/daze
	if stunned -> return
	if mob
		mob_hit
		return
	one_hit
	if not fighting anymore or dt = backstab -> return
	if second attack -> one hit
	if not fighting anymore -> return
	if third attack -> one hit

one_hit(ch, victim, dt)
	if victim = ch || victim is null || ch is null -> return
	// figure out the type of damage message
	get ch wield
	if dt == TYPE_UNDEFINED (-1)
		dt = TYPE_HIT (arbitrary high value, higher than highest skill/spell gsn --> allow to differentiate skill/spell attack from backstab or auto attack)
		if wield != null && wield is weapon -> dt = TYPE_HIT + wield damage message (v3)
		else -> dt = TYPE_HIT + ch damage message (what is damage message for player?)
	if dt < TYPE_HIT
		if wield != null -> dam_type = attack_table[wield damage message].damage  (DAM_BASH/SLASH/PIERCE/POISON/HOLY...)
		else -> dam_type = attack_table[wield damage message].damage
	else -> dam_type = attack_table[dt - hit].damage
	if dam_type == -1 -> dam_type = DAM_BASH
	// get weapon skill
	sn = get_weapon_sn(ch)
	skill = 20 + get_weapon_skill(ch, sn)
	// calculate to-hit-armor-class-0 versus armor
	if ch is npc
		thac0_00 = 20
		thac0_32 = -4
		if act warrior -> thac0_32 = -10
		if act thief -> thac0_32 = -4
		if act cleric -> thac0_32 = 2
		if act mage -> thac0_32 = 6
	else
		thac0_00 = class.thac0_00
		thac0_32 = class.thac0_32
	thac0 = interp(level, thac0_00, thac0_32) // should be called extrapolate if ch level > 32 ;)    simple linear interpolation value_00 + level * (value_32 - value_00) / 32
	if (thac0 < 0) -> thac0 /= 2
	if (thac0 < 5) -> thac0 = -5 + (thac0 + 5)/2
	thac0 -= ch.gethitroll * skill / 100
	if (dt == backstab) -> thac0 -= 10 * (100 - ch.getskill(backstab))
	switch (dam_type)
		case PIERCE: victim_ac = victim.armor_pierce
		case BASH: victim_ac = victim.armo_bash
		case SLASH: victim_ac = victim.armor_slash
		default: victim_ac = victim.armor_exotic
	if (victim_ac < -15) -> victim_ac = (victim_ac + 15) / 5 - 15
	if (!can_see(ch, victim)) -> victim_ac -= 4
	if (victim.position < fighting) -> victim_ac += 4
	if (victim.position < resting) -> victim_ac += 6
	// moment of excitement
	diceroll = random.range(0,19)
	// miss ?
	if (diceroll == 0 
		|| (diceroll != 19 && diceroll < thac0 - victim_ac)
		damage(ch, victim, 0, dt, dam_type, true)
	// hit: calc damage
	if NPC -> dam = dice(mob.dice_number, mob.dice_value)  or  dam = random(mob.level/2, mob.level*3/2) + 50% if wield != null
	else
		if sn != -1 -> check skill improve(ch, true, 5) (weapon skill improve)
		if wield != null
			dam = dice(wield.dice_number, wield.dice_value)  or  dam = random(wield.value1 * skill / 100, wield.value2 * skill / 100)
			if not holding a shield -> dam = dam * 11 / 10
			if weapon is sharp and some randomness -> dam += some randomness
		else -> dam = random( 1 + 4 * skill / 100, 2 * ch.level / 3 * skill / 100) // skill should be hand 2 hand
	// bonuses
	if enhanced damage skill > 0, dam += some randomness
	if victim <= sleeping -> dam *= 2
	else if victim <= fighting -> dam = dam * 3/2
	if dt == backstab && wield != null -> dam += depends on level and dagger or not
	dam += ch.getdamroll * MIN(100, skill) / 100
	if dam <= 0 -> dam = 1
	result = damage(ch, victim, dam, dt, dam_type, true)
	// funky weapon ?
	if result && wield != null
		poison -> add poison
		vampiric -> damage + drain hit + change align
		flaming -> fire_effect + damage
		frost -> cold_effect + damage
		shocking -> shock_effect + damage

damage(ch, victim, dam, dt, dam_type, show)
	if victim == dead ->  return false
	if dam > 1200 && dt >= TYPE_HIT -> dam = 1200 + log and extract weapon (if not immortal)
	// dam reduction
	if dam > 35 -> dam = (dam - 35)/2 + 35
	if dam > 80 -> dam = (dam - 80)/2 + 80
	if victim != ch
		// certain attacks are forbidden, most other are returned
		if is_safe(ch, victim) -> return false
		check_killer
		if victim > stunned
			if victim.fighting == null -> set_fighting(victim, ch)
			if victim.timer <= 4 -> victim.position = fighting
		if victim > stunned
			if ch.fighting == null -> set_fighting(ch, victim)
		// more charm stuff
		if victim.master == ch
			stop_follower(victim)
	// inviso attack
	if ch is invisible -> remove invis + msg
	// damage modifier
	if dam > 1 && victim is player && victim drunk -> dam *= 9/10
	if dam > 1 && victim is affected by sanctuary -> dam /= 2
	if dam > 1 && victim is affected by protect good|evil and ch is evil|good -> dam -= dam/4
	immune = false
	// check for parry, dodge and shield block
	if dt >= TYPE_HIT && ch != victim
		if check_parry(ch, victim) -> return false
		if check_dodge(ch, victim) -> return false
		if check_shield_block(ch, victim) -> return false
	switch(check_immune(victim, dam_type))
		case immune: immune = true, dam = 0
		case resist: dam -= dam / 3
		case vuln: dam += dam*2
	if show -> dam_message(ch, victim, dam, dt, immune)
	if dam = 0 -> return false
	// hurt the victim
	victim.hit -= dam
	if victim.hit < 1 and victim is immortal -> victim.hit = 1
	update_pos(victim)
	display message depending on position and damage
	// sleep spells and extremely wounded folks
	if victim.position <= sleeping -> stop_fighting(victim, false)
	// payoff for killing
	if victim.position == dead
		 group_gain(ch, victim)
		 if victim is pc
			lose xp (2/3 back previous level)
		raw_kill(victim)
		if ch != victim && ch is pc and not same clan -> remove killer/thief flag
		autoloot/autosac
		return true
	if ch == victim -> return true
	// link dead
	if victim is pc and no descriptor and some randomness
		recall
		return true
	// wimp out
	if victim is npc and dam > 0 and victim.wait < PULSE_VIOLENCE / 2
		if victim act has wimpy && chance(25) && victim.hit < victim.max_hit/5
			or victim is charmed && victim.master != null && victim.master.room != victim.room
			victim.flee
	if victim is pc and victim.hit > 0 && victim.hit <= victim.wimpy && victim.wait < PULSE_VIOLENCE / 2
		victim.flee

A Player is in-game once he/she has Impersonate a Character
An Admin is in-game once he/she has Impersonate a Character or Incarnate an Entity (Character/Room/Item)

when char1 follows char2, char1.Leader = char2
when char2 groups char1, char2.GroupMembers.Add(char1) [can only group if char1.Leader == char2]
when char1 charms char2, char1.Slave = char2 + char2.Master = char1


Loot table: http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=9715
Its a bit more complex, but here is how one MMORPG that I am rather familiar with did it:
 You have two types of data:
 1. Treasure Tables
 2. Monster Treasure Lists
 A Treasure Table is a pre-defined set that contains a list of items, each with a given weight and maximum drop quantity. Weights are relative to the total weight of all items in the table. 
 TreasureTable_Spider
 - Spider Venom, weight=25, max=1
 - Spider Webbing, weight=65, max=1
 - Severed Spider Leg, weight=10, max=1
 This would give the Spider Venom a 25% chance of dropping if this table were called (25 / 100).
 Now, you also have Monster Treasure Lists. These are defined on the monster itself and refer to Treasure Tables using relative weights and max quantities. The monster also has a min/max of items it can drop (# of pulls from the tables).
 Spider
 - Min=1, Max=1
 - TreasureTable_Spider, weight=45, max=1
 - TreasureTable_RareLoot, weight=5, max=1
 - EmptyTreasureTable, weight=50, max=1
 So basically 50% of the time you would get no loot, 45% of the time you'd get something from TreasureTable_Spider and 5% of the time from TreasureTable_RareLoot.
 What this does is allows you to manage what mobs drop without having to alter each and every mob. You want to add another item to the rare loot table, next load it will be there and available to players.
 The downside to this system is it can be complex and it is harder to get an idea of the overall drop-rate of individual items. For example, if you were looking for Spider Webbing using the data above your effective % chance would be 29.25% (45% to call the Spider table, 65% to get Webbing inside that table).







 void display_resets( CHAR_DATA *ch )
{
    ROOM_INDEX_DATA	*pRoom;
    RESET_DATA		*pReset;
    MOB_INDEX_DATA	*pMob = NULL;
    char 		buf   [ MAX_STRING_LENGTH ];
    char 		final [ MAX_STRING_LENGTH ];
    int 		iReset = 0;

    EDIT_ROOM(ch, pRoom);
    final[0]  = '\0';
    
    send_to_char ( 
  " No.  Loads    Description       Location         Vnum    Max   Description"
  "\n\r"
  "==== ======== ============= =================== ======== [R  W] ==========="
  "\n\r", ch );

    for ( pReset = pRoom->reset_first; pReset; pReset = pReset->next )
    {
	OBJ_INDEX_DATA  *pObj;
	MOB_INDEX_DATA  *pMobIndex;
	OBJ_INDEX_DATA  *pObjIndex;
	OBJ_INDEX_DATA  *pObjToIndex;
	ROOM_INDEX_DATA *pRoomIndex;

	final[0] = '\0';
	sprintf( final, "[%2d] ", ++iReset );

	switch ( pReset->command )
	{
	default:
	    sprintf( buf, "Bad reset command: %c.", pReset->command );
	    strcat( final, buf );
	    break;

	case 'M':
	    if ( !( pMobIndex = get_mob_index( pReset->arg1 ) ) )
	    {
                sprintf( buf, "Load Mobile - Bad Mob %d\n\r", pReset->arg1 );
                strcat( final, buf );
                continue;
	    }

	    if ( !( pRoomIndex = get_room_index( pReset->arg3 ) ) )
	    {
                sprintf( buf, "Load Mobile - Bad Room %d\n\r", pReset->arg3 );
                strcat( final, buf );
                continue;
	    }

            pMob = pMobIndex;
            sprintf( buf, "M[%5d] %-13.13s in room             R[%5d] [%-2d%2d] %-15.14s\n\r",
                       pReset->arg1, pMob->short_descr, pReset->arg3,
                       pReset->arg4, pReset->arg2, pRoomIndex->name );
            strcat( final, buf );

	    /*
	     * Check for pet shop.
	     * -------------------
	     */
	    {
		ROOM_INDEX_DATA *pRoomIndexPrev;

		pRoomIndexPrev = get_room_index( pRoomIndex->vnum - 1 );
		if ( pRoomIndexPrev
		    && IS_SET( pRoomIndexPrev->room_flags, ROOM_PET_SHOP ) )
                    final[5] = 'P';
	    }

	    break;

	case 'O':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
                sprintf( buf, "Load Object - Bad Object %d\n\r",
		    pReset->arg1 );
                strcat( final, buf );
                continue;
	    }

            pObj       = pObjIndex;

	    if ( !( pRoomIndex = get_room_index( pReset->arg3 ) ) )
	    {
                sprintf( buf, "Load Object - Bad Room %d\n\r", pReset->arg3 );
                strcat( final, buf );
                continue;
	    }

            sprintf( buf, "O[%5d] %-13.13s in room             "
                          "R[%5d]       %-15.15s\n\r",
                          pReset->arg1, pObj->short_descr,
                          pReset->arg3, pRoomIndex->name );
            strcat( final, buf );

	    break;

	case 'P':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
                sprintf( buf, "Put Object - Bad Object %d\n\r",
                    pReset->arg1 );
                strcat( final, buf );
                continue;
	    }

            pObj       = pObjIndex;

	    if ( !( pObjToIndex = get_obj_index( pReset->arg3 ) ) )
	    {
                sprintf( buf, "Put Object - Bad To Object %d\n\r",
                    pReset->arg3 );
                strcat( final, buf );
                continue;
	    }

	    sprintf( buf,
		"O[%5d] %-13.13s inside              O[%5d]       %-15.15s\n\r",
		pReset->arg1,
		pObj->short_descr,
		pReset->arg3,
		pObjToIndex->short_descr );
            strcat( final, buf );

	    break;

	case 'G':
	case 'E':
	    if ( !( pObjIndex = get_obj_index( pReset->arg1 ) ) )
	    {
                sprintf( buf, "Give/Equip Object - Bad Object %d\n\r",
                    pReset->arg1 );
                strcat( final, buf );
                continue;
	    }

            pObj       = pObjIndex;

	    if ( !pMob )
	    {
                sprintf( buf, "Give/Equip Object - No Previous Mobile\n\r" );
                strcat( final, buf );
                break;
	    }

	    if ( pMob->pShop )
	    {
	    sprintf( buf,
		"O[%5d] %-13.13s in the inventory of S[%5d]       %-15.15s\n\r",
		pReset->arg1,
		pObj->short_descr,                           
		pMob->vnum,
		pMob->short_descr  );
	    }
	    else
	    sprintf( buf,
		"O[%5d] %-13.13s %-19.19s M[%5d]       %-15.15s\n\r",
		pReset->arg1,
		pObj->short_descr,
		(pReset->command == 'G') ?
		    flag_string( wear_loc_strings, WEAR_NONE )
		  : flag_string( wear_loc_strings, pReset->arg3 ),
		  pMob->vnum,
		  pMob->short_descr );
	    strcat( final, buf );

	    break;

	/*
	 * Doors are set in rs_flags don't need to be displayed.
	 * If you want to display them then uncomment the new_reset
	 * line in the case 'D' in load_resets in db.c and here.
	 */
	case 'D':
	    pRoomIndex = get_room_index( pReset->arg1 );
	    sprintf( buf, "R[%5d] %s door of %-19.19s reset to %s\n\r",
		pReset->arg1,
		capitalize( dir_name[ pReset->arg2 ] ),
		pRoomIndex->name,
		flag_string( door_resets, pReset->arg3 ) );
	    strcat( final, buf );

	    break;
	/*
	 * End Doors Comment.
	 */
	case 'R':
	    if ( !( pRoomIndex = get_room_index( pReset->arg1 ) ) )
	    {
		sprintf( buf, "Randomize Exits - Bad Room %d\n\r",
		    pReset->arg1 );
		strcat( final, buf );
		continue;
	    }

	    sprintf( buf, "R[%5d] Exits are randomized in %s\n\r",
		pReset->arg1, pRoomIndex->name );
	    strcat( final, buf );

	    break;
	}
	send_to_char( final, ch );
    }

    return;
}