﻿using Mud.Domain;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        private readonly int DefaultLevelIfAbilityNotKnown = 53;

        [Skill(5000, "Berserk", AbilityTargets.CharacterSelf)]
        public UseResults SkillBerserk(IAbility ability, ICharacter source)
        {
            int chance = source.LearnedAbility(ability);
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Berserk))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (pcSource.KnownAbilities.FirstOrDefault(x => x.Ability == ability)?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("You turn red in the face, but nothing happens.");
                return UseResults.NotKnown;
            }

            if (source.CurrentCharacterFlags.HasFlag(CharacterFlags.Berserk)
                || source.GetAura(ability) != null
                || source.GetAura("Frenzy") != null)
            {
                source.Send("You get a little madder.");
                return UseResults.Failed;
            }

            if (source.CurrentCharacterFlags.HasFlag(CharacterFlags.Calm))
            {
                source.Send("You're feeling to mellow to berserk.");
                return UseResults.Failed;
            }

            // TODO: mana cost 50 ??

            // modifiers
            if (source.Fighting != null)
                chance += 10;

            // Below 50%, hp helps, above hurts
            int hpPercent = (100 * source.HitPoints) / source.CurrentAttribute(CharacterAttributes.MaxHitPoints);
            chance += 25 - hpPercent / 2;

            //
            if (RandomManager.Chance(chance))
            {
                // TODO: set GCD to PulseViolence
                // TODO: mana -= 50
                // TODO: move /= 2
                // TODO: heal level*2
                source.Send("Your pulse races as you are consumed by rage!");
                source.Act(ActOptions.ToRoom, "{0:N} gets a wild look in {0:s} eyes.", source);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 2);

                /* add affects: berserk, +hit, +dam, +ac
                af.where	= TO_AFFECTS;
	            af.type		= gsn_berserk;
	            af.level	= ch->level;
	            af.duration	= number_fuzzy(ch->level / 8);
	            af.modifier	= UMAX(1,ch->level/5);
	            af.bitvector 	= AFF_BERSERK;

	            af.location	= APPLY_HITROLL;
	            affect_to_char(ch,&af);

	            af.location	= APPLY_DAMROLL;
	            affect_to_char(ch,&af);

	            af.modifier	= UMAX(10,10 * (ch->level/5));
	            af.location	= APPLY_AC;
	            affect_to_char(ch,&af);
                */
                return UseResults.Ok;
            }
            else
            {
                // TODO: set GCD to 3*PulseViolence
                // TODO: mana -= 25
                // TODO: move /= 2
                source.Send("Your pulse speeds up, but nothing happens.");
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 2);
                return UseResults.Failed;
            }
        }

        [Skill(5001, "Bash", AbilityTargets.CharacterOffensive)]
        public UseResults SkillBash(IAbility ability, ICharacter source, ICharacter victim)
        {
            int chance = source.LearnedAbility(ability);

            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Bash))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (pcSource.KnownAbilities.FirstOrDefault(x => x.Ability == ability)?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("Bashing? What's that?");
                return UseResults.NotKnown;
            }

            if (victim == source)
            {
                source.Send("You try to bash your brains out, but fail.");
                return UseResults.InvalidTarget;
            }

            if (victim.Position < Positions.Fighting)
            {
                source.Act(ActOptions.ToCharacter, "You'll have to let {0:m} get back up first.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: is safe check
            // TODO: check kill stealing
            // TODO: pet check

            // modifiers

            // size and weight
            // TODO: carry weight of source and victim
            // TODO: size source and victim
            // stats
            chance += source.CurrentAttribute(CharacterAttributes.Strength);
            chance -= (4 * victim.CurrentAttribute(CharacterAttributes.Dexterity)) / 3;
            chance -= victim.CurrentAttribute(CharacterAttributes.ArmorBash) / 25;
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 30;
            // level
            chance += source.Level - victim.Level;

            // dodge?
            int victimDodgeLearned = victim.LearnedAbility("Dodge");
            if (chance < victimDodgeLearned)
                chance -= 3 * (victimDodgeLearned - chance);

            // now the attack
            if (RandomManager.Chance(chance))
            {
                source.Act(ActOptions.ToCharacter, "You slam into {0}, and send {0:m} flying!", victim);
                source.Act(ActOptions.ToRoom, "{0:N} sends {1} sprawling with a powerful bash.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 1);
                // TODO: victim daze
                // TODO: set GCD
                // TODO: victim.Position = Positions.Resting
                //int damage = RandomManager.Range(2, 2+2* source.Size + chance/2)
                int damage = 2;
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, false);
                // TODO: check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, false); // starts a fight
                victim.Act(ActOptions.ToRoom, "{0:N} fall{0:v} flat on {0:s} face!", source);
                victim.Act(ActOptions.ToCharacter, "You evade {0:p} bash, causing {0:m} to fall flat on {0:s} face.", source);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 1);
                // TODO: victim.Position = Positions.Resting
                // TODO: set GCD
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }

        }

        [Skill(5002, "Dirt kicking", AbilityTargets.CharacterOffensive)]
        public UseResults SkillDirt(IAbility ability, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            int chance = source.LearnedAbility(ability);

            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.DirtKick))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (pcSource.KnownAbilities.FirstOrDefault(x => x.Ability == ability)?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("You get your feet dirty.");
                return UseResults.NotKnown;
            }

            if (victim == source)
            {
                source.Send("Very funny.");
                return UseResults.InvalidTarget;
            }

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Blind))
            {
                source.Act(ActOptions.ToCharacter, "{0:e}'s already been blinded.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: is safe check
            // TODO: check kill stealing
            // TODO: pet check

            // modifiers
            // dexterity
            chance += source.CurrentAttribute(CharacterAttributes.Dexterity);
            chance -= 2 * victim.CurrentAttribute(CharacterAttributes.Dexterity);
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 25;
            // level
            chance += (source.Level - victim.Level) * 2;
            // sloppy hack to prevent false zeroes
            if (chance % 5 == 0)
                chance += 1;
            // TODO: terrain
            //
            if (chance == 0)
            {
                source.Send("There isn't any dirt to kick.");
            }
            // now the attack
            if (RandomManager.Chance(chance))
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is blinded by the dirt in {0:s} eyes!", victim);
                victim.Act(ActOptions.ToCharacter, "{0:N} kicks dirt in your eyes!", source);
                victim.Send("You can't see a thing!");

                int damage = RandomManager.Range(2, 5);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.None, false);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 2);
                //WAIT_STATE(ch, skill_table[gsn_dirt].beats);

                // TODO
                //AFFECT_DATA af;
                //af.where = TO_AFFECTS;
                //af.type = gsn_dirt;
                //af.level = ch->level;
                //af.duration = 0;
                //af.location = APPLY_HITROLL;
                //af.modifier = -4;
                //af.bitvector = AFF_BLIND;
                //affect_to_char(victim, &af);
                // TODO: check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 2);
                //WAIT_STATE(ch, skill_table[gsn_dirt].beats);
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5003, "Trip", AbilityTargets.CharacterOffensive)]
        public UseResults SkillTrip(IAbility ability, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            int chance = source.LearnedAbility(ability);

            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Trip))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (pcSource.KnownAbilities.FirstOrDefault(x => x.Ability == ability)?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("Tripping?  What's that?");
                return UseResults.NotKnown;
            }

            if (victim == source)
            {
                source.Send("You fall flat on your face!");
                source.Act(ActOptions.ToRoom, "{0:N} trips over {0:s} own feet!", source);
                // TODO: set GCD
                return UseResults.InvalidTarget;
            }

            // TODO: is safe check
            // TODO: check kill stealing

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Flying))
            {
                source.Act(ActOptions.ToCharacter, "{0:s} feet aren't on the ground.", victim);
                return UseResults.InvalidTarget;
            }

            if (victim.Position < Positions.Fighting)
            {
                source.Act(ActOptions.ToCharacter, "{0:N} is already down..", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: pet check

            // modifiers
            // TODO: size
            // dexterity
            chance += source.CurrentAttribute(CharacterAttributes.Dexterity);
            chance -= (3 * victim.CurrentAttribute(CharacterAttributes.Dexterity)) / 2;
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 20;
            // level
            chance += (source.Level - victim.Level) * 2;

            // now the attack
            if (RandomManager.Chance(chance))
            {
                victim.Act(ActOptions.ToCharacter, "{0:N} trips you and you go down!", source);
                source.Act(ActOptions.ToCharacter, "You trip {0} and {0} goes down!", victim);
                source.ActToNotVictim(victim, "{0} trips {1}, sending {1:m} to the ground.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 1);
                //DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
                //WAIT_STATE(ch, skill_table[gsn_trip].beats);
                //victim->position = POS_RESTING;
                int damage = 2 + 2/* * victim.Size*/;
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                //WAIT_STATE(ch, skill_table[gsn_trip].beats * 2 / 3);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 1);
                // TODO check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5004, "Backstab", AbilityTargets.CharacterOffensive)]
        public UseResults SkillBackstab(IAbility ability, ICharacter source, ICharacter victim)
        {
            // TODO: should be done in caller
            //if (arg[0] == '\0')
            //{
            //    send_to_char("Backstab whom?\n\r",ch);
            //    return;
            //}

            if (source.Fighting != null)
            {
                source.Send("You are facing the wrong end.");
                return UseResults.MissingParameter;
            }

            if (victim == source)
            {
                source.Send("How can you sneak up on yourself?");
                return UseResults.InvalidTarget;
            }

            // TODO: is safe check
            // TODO: check kill stealing
            // TODO: check if wielding a weapon

            if (victim.HitPoints < victim.CurrentAttribute(CharacterAttributes.MaxHitPoints) / 3)
            {
                source.Act(ActOptions.ToCharacter, "{0} is hurt and suspicious ... you can't sneak up.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: check killer
            // TODO: GCD
            int learned = source.LearnedAbility(ability.Name);
            if (RandomManager.Chance(learned)
                || (learned > 1 && victim.Position <= Positions.Sleeping))
            {
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 1);
                victim.MultiHit(source); // TODO: pass ability, some nasty things are done if Backstab is passed as param
                return UseResults.Ok;
            }
            else
            {
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 1);
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true); // Starts fight without doing any damage
                return UseResults.Failed;
            }
        }

        [Skill(5005, "Kick", AbilityTargets.CharacterFighting)]
        public UseResults SkillKick(IAbility ability, ICharacter source)
        {
            KnownAbility knownAbility = source.KnownAbilities.FirstOrDefault(x => x.Ability == ability);
            if (source is IPlayableCharacter pcSource && pcSource.Level < (knownAbility?.Level ?? DefaultLevelIfAbilityNotKnown))
            {
                source.Send("You better leave the martial arts to fighters.");
                return UseResults.NotKnown;
            }

            if (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Kick))
                return UseResults.NotKnown;

            ICharacter victim = source.Fighting;
            if (victim == null)
            {
                source.Send("You aren't fighting anyone.");
                return UseResults.MustBeFighting;
            }

            // TODO: gcd
            if (RandomManager.Chance(knownAbility?.Learned ?? 0))
            {
                int damage = RandomManager.Range(1, source.Level);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                (victim as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 1);
                //check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                (victim as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 1);
                //check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5006, "Disarm", AbilityTargets.CharacterFighting)]
        public UseResults SkillDisarm(IAbility ability, ICharacter source)
        {
            KnownAbility knownAbility = source.KnownAbilities.FirstOrDefault(x => x.Ability == ability);
            int chance = knownAbility?.Learned ?? DefaultLevelIfAbilityNotKnown;
            if (chance == 0)
            {
                source.Send("You don't know how to disarm opponents.");
                return UseResults.NotKnown;
            }

            // TODO: check if wielding a weapon
            //        if (get_eq_char(ch, WEAR_WIELD) == NULL
            //&& ((hth = get_skill(ch, gsn_hand_to_hand)) == 0
            //|| (IS_NPC(ch) && !IS_SET(ch->off_flags, OFF_DISARM))))
            //        {
            //            send_to_char("You must wield a weapon to disarm.\n\r", ch);
            //            return;
            //        }

            ICharacter victim = source.Fighting;
            if (victim == null)
            {
                source.Send("You aren't fighting anyone.");
                return UseResults.MustBeFighting;
            }

            // TODO: get victim weapon
            //if ((obj = get_eq_char(victim, WEAR_WIELD)) == NULL)
            //{
            //    send_to_char("Your opponent is not wielding a weapon.\n\r", ch);
            //    return;
            //}

            // TODO: find weapon skills
            //ch_weapon = get_weapon_skill(ch, get_weapon_sn(ch));
            //vict_weapon = get_weapon_skill(victim, get_weapon_sn(victim));
            //ch_vict_weapon = get_weapon_skill(ch, get_weapon_sn(victim));

            // modifiers
            // skill
            //if (get_eq_char(ch, WEAR_WIELD) == NULL)
            //    chance = chance * hth / 150;
            //else
            //    chance = chance * ch_weapon / 100;
            //chance += (ch_vict_weapon / 2 - vict_weapon) / 2;
            // dex vs. strength
            chance += source.CurrentAttribute(CharacterAttributes.Dexterity);
            chance -= 2 * victim.CurrentAttribute(CharacterAttributes.Strength);
            // level
            chance += (source.Level - victim.Level) * 2;
            // and now the attack
            if (RandomManager.Chance(chance))
            {
                //TODO gcd WAIT_STATE(ch, skill_table[gsn_disarm].beats);
                //OBJ_DATA* obj;

                //if ((obj = get_eq_char(victim, WEAR_WIELD)) == NULL)
                //    return;

                //if (IS_OBJ_STAT(obj, ITEM_NOREMOVE))
                //{
                //    act("$S weapon won't budge!", ch, NULL, victim, TO_CHAR);
                //    act("$n tries to disarm you, but your weapon won't budge!",
                //        ch, NULL, victim, TO_VICT);
                //    act("$n tries to disarm $N, but fails.", ch, NULL, victim, TO_NOTVICT);
                //    return;
                //}

                victim.Act(ActOptions.ToCharacter, "{0:N} DISARMS you and sends your weapon flying!", source);
                victim.Act(ActOptions.ToRoom, "{0:N} disarm{0:v} {1}", source, victim);

                //obj_from_char(obj);
                //if (IS_OBJ_STAT(obj, ITEM_NODROP) || IS_OBJ_STAT(obj, ITEM_INVENTORY))
                //    obj_to_char(obj, victim);
                //else
                //{
                //    obj_to_room(obj, victim->in_room);
                //    if (IS_NPC(victim) && victim->wait == 0 && can_see_obj(victim, obj))
                //        get_obj(victim, obj, NULL);
                //}

                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, true, 1);
                // TODO  check_killer(ch, victim);
                return UseResults.Ok;
            }
            else
            {
                //TODO gcd WAIT_STATE(ch, skill_table[gsn_disarm].beats);
                source.Act(ActOptions.ToCharacter, "You fail to disarm {0}.", victim);
                source.Act(ActOptions.ToRoom, "{0:N} tries to disarm {1}, but fails.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(ability, false, 1);
                // TODO  check_killer(ch, victim);
                return UseResults.Failed;
            }
        }

    }
}