using System;
using System.Globalization;
using System.Linq;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        private readonly int DefaultLevelIfAbilityNotKnown = 53;

        [Skill(5000, "Berserk", AbilityTargets.None, PulseWaitTime = 24)]
        public UseResults SkillBerserk(IAbility ability, ICharacter source)
        {
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Berserk))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (knownAbility?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("You turn red in the face, but nothing happens.");
                return UseResults.NotKnown;
            }

            if (source.CharacterFlags.HasFlag(CharacterFlags.Berserk)
                || source.GetAura(ability) != null
                || source.GetAura("Frenzy") != null)
            {
                source.Send("You get a little madder.");
                return UseResults.Failed;
            }

            if (source.CharacterFlags.HasFlag(CharacterFlags.Calm))
            {
                source.Send("You're feeling to mellow to berserk.");
                return UseResults.Failed;
            }

            if (source[ResourceKinds.Mana] < 50)
            {
                source.Send("You can't get up enough energy.");
                return UseResults.NotEnoughResource;
            }

            // modifiers
            if (source.Fighting != null)
                chance += 10;

            // Below 50%, hp helps, above hurts
            int hpPercent = (100 * source.HitPoints) / source[CharacterAttributes.MaxHitPoints];
            chance += 25 - hpPercent / 2;

            //
            if (RandomManager.Chance(chance))
            {
                source.UpdateResource(ResourceKinds.Mana, 50);
                source.UpdateMovePoints(source.MovePoints / 2);
                source.Heal(source, ability, source.Level * 2, false);

                source.Send("Your pulse races as you are consumed by rage!");
                source.Act(ActOptions.ToRoom, "{0:N} gets a wild look in {0:s} eyes.", source);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 2);

                int duration = RandomManager.Fuzzy(source.Level / 8);
                int modifier = Math.Max(1, source.Level / 5);
                int acModifier = Math.Max(10, 10 * (source.Level / 5));
                World.AddAura(source, ability, source, source.Level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Berserk, Operator = AffectOperators.Or },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = acModifier, Operator = AffectOperators.Add });
                return UseResults.Ok;
            }
            else
            {
                source.UpdateResource(ResourceKinds.Mana, 25);
                source.UpdateMovePoints(source.MovePoints / 2);

                source.Send("Your pulse speeds up, but nothing happens.");

                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 2);
                return UseResults.Failed;
            }
        }

        [Skill(5001, "Bash", AbilityTargets.CharacterOffensive, PulseWaitTime = 20)]
        public UseResults SkillBash(IAbility ability, ICharacter source, ICharacter victim)
        {
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Bash))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (knownAbility?.Level ?? DefaultLevelIfAbilityNotKnown)))
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
            chance += source[CharacterAttributes.Strength];
            chance -= (4 * victim[CharacterAttributes.Dexterity]) / 3;
            chance -= victim[CharacterAttributes.ArmorBash] / 25;
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 30;
            // level
            chance += source.Level - victim.Level;

            // dodge?
            int victimDodgeLearned = victim.GetKnownAbility("Dodge")?.Learned ?? 0;
            if (chance < victimDodgeLearned)
                chance -= 3 * (victimDodgeLearned - chance);

            // now the attack
            if (RandomManager.Chance(chance))
            {
                source.Act(ActOptions.ToCharacter, "You slam into {0}, and send {0:m} flying!", victim);
                source.Act(ActOptions.ToRoom, "{0:N} sends {1} sprawling with a powerful bash.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 1);
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
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 1);
                // TODO: victim.Position = Positions.Resting
                // TODO: set GCD
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }

        }

        [Skill(5002, "Dirt kicking", AbilityTargets.CharacterOffensive, PulseWaitTime = 24)]
        public UseResults SkillDirt(IAbility ability, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.DirtKick))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (knownAbility?.Level ?? DefaultLevelIfAbilityNotKnown)))
            {
                source.Send("You get your feet dirty.");
                return UseResults.NotKnown;
            }

            if (victim == source)
            {
                source.Send("Very funny.");
                return UseResults.InvalidTarget;
            }

            if (victim.CharacterFlags.HasFlag(CharacterFlags.Blind))
            {
                source.Act(ActOptions.ToCharacter, "{0:e}'s already been blinded.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: is safe check
            // TODO: check kill stealing
            // TODO: pet check

            // modifiers
            // dexterity
            chance += source[CharacterAttributes.Dexterity];
            chance -= 2 * victim[CharacterAttributes.Dexterity];
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
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
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 2);
                //WAIT_STATE(ch, skill_table[gsn_dirt].beats);
                // TODO check killer

                World.AddAura(victim, ability, source, source.Level, TimeSpan.FromMinutes(0)/*TODO  0 ???*/, AuraFlags.NoDispel, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Or });
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 2);
                //WAIT_STATE(ch, skill_table[gsn_dirt].beats);
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5003, "Trip", AbilityTargets.CharacterOffensive, PulseWaitTime = 24)]
        public UseResults SkillTrip(IAbility ability, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Trip))
                || (source is IPlayableCharacter pcSource && pcSource.Level < (knownAbility?.Level ?? DefaultLevelIfAbilityNotKnown)))
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

            if (victim.CharacterFlags.HasFlag(CharacterFlags.Flying))
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
            chance += source[CharacterAttributes.Dexterity];
            chance -= (3 * victim[CharacterAttributes.Dexterity]) / 2;
            // speed
            if ((source as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 20;
            // level
            chance += (source.Level - victim.Level) * 2;

            // now the attack
            if (RandomManager.Chance(chance))
            {
                victim.Act(ActOptions.ToCharacter, "{0:N} trips you and you go down!", source);
                source.Act(ActOptions.ToCharacter, "You trip {0} and {0} goes down!", victim);
                source.ActToNotVictim(victim, "{0} trips {1}, sending {1:m} to the ground.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 1);
                //DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
                //victim->position = POS_RESTING;
                // TODO: check_killer(ch, victim)
                int damage = 2 + 2/* * victim.Size*/;
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 1);
                // TODO check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5004, "Backstab", AbilityTargets.CharacterOffensive, PulseWaitTime = 24)]
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

            if (victim.HitPoints < victim[CharacterAttributes.MaxHitPoints] / 3)
            {
                source.Act(ActOptions.ToCharacter, "{0} is hurt and suspicious ... you can't sneak up.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: check killer
            KnownAbility knownAbility = source[ability];
            int learned = knownAbility?.Learned ?? 0;
            if (RandomManager.Chance(learned)
                || (learned > 1 && victim.Position <= Positions.Sleeping))
            {
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 1);
                victim.MultiHit(source); // TODO: pass ability, some nasty things are done if Backstab is passed as param
                return UseResults.Ok;
            }
            else
            {
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 1);
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true); // Starts fight without doing any damage
                return UseResults.Failed;
            }
        }

        [Skill(5005, "Kick", AbilityTargets.CharacterFighting, PulseWaitTime = 12)]
        public UseResults SkillKick(IAbility ability, ICharacter source)
        {
            KnownAbility knownAbility = source[ability];
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

            int chance = knownAbility?.Learned ?? 0;
            if (RandomManager.Chance(chance))
            {
                int damage = RandomManager.Range(1, source.Level);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                (victim as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 1);
                //check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                (victim as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 1);
                //check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5006, "Disarm", AbilityTargets.CharacterFighting, PulseWaitTime = 24)]
        public UseResults SkillDisarm(IAbility ability, ICharacter source)
        {
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
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
            chance += source[CharacterAttributes.Dexterity];
            chance -= 2 * victim[CharacterAttributes.Strength];
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

                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 1);
                // TODO  check_killer(ch, victim);
                return UseResults.Ok;
            }
            else
            {
                //TODO gcd WAIT_STATE(ch, skill_table[gsn_disarm].beats);
                source.Act(ActOptions.ToCharacter, "You fail to disarm {0}.", victim);
                source.Act(ActOptions.ToRoom, "{0:N} tries to disarm {1}, but fails.", source, victim);
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 1);
                // TODO  check_killer(ch, victim);
                return UseResults.Failed;
            }
        }

        [Skill(5007, "Sneak", AbilityTargets.None, PulseWaitTime = 12)]
        public UseResults SkillSneak(IAbility ability, ICharacter source)
        {
            source.Send("You attempt to move silently.");
            source.RemoveAuras(x => x.Ability == ability, true);

            if (source.CharacterFlags.HasFlag(CharacterFlags.Sneak))
                return UseResults.Failed;

            bool success = false;
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (RandomManager.Chance(chance))
            {
                World.AddAura(source, ability, source, source.Level, TimeSpan.FromMinutes(source.Level), AuraFlags.None, true,
                    new CharacterFlagsAffect {Modifier = CharacterFlags.Sneak, Operator = AffectOperators.Or});
                success = true;
            }

            (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, success, 3);
            source.Recompute();

            return success
                ? UseResults.Ok
                : UseResults.Failed;
        }

        [Skill(5008, "Hide", AbilityTargets.None, PulseWaitTime = 12)]
        public UseResults SkillHide(IAbility ability, ICharacter source)
        {
            source.Send("You attempt to hide.");

            if (source.CharacterFlags.HasFlag(CharacterFlags.Hide))
                source.RemoveBaseCharacterFlags(CharacterFlags.Hide);

            bool success = false;
            KnownAbility knownAbility = source[ability];
            int chance = knownAbility?.Learned ?? 0;
            if (RandomManager.Chance(chance))
            {
                source.AddBaseCharacterFlags(CharacterFlags.Hide);
                success = true;
            }

            (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, success, 3);
            source.Recompute();

            return success
                ? UseResults.Ok
                : UseResults.Failed;
        }

        [Skill(5009, "Recall", AbilityTargets.None, PulseWaitTime = 12)]
        public UseResults SkillRecall(IAbility ability, ICharacter source)
        {
            IPlayableCharacter pcSource = source as IPlayableCharacter;
            if (pcSource == null)
            {
                source.Send("Only players can recall.");
                return UseResults.InvalidTarget;
            }

            pcSource.Act(ActOptions.ToRoom, "{0} prays for transportation!", source);

            IRoom recallRoom = pcSource.RecallRoom;
            if (recallRoom == null)
            {
                pcSource.Send("You are completely lost.");
                Log.Default.WriteLine(LogLevels.Error, "No recall room found for {0}", pcSource.ImpersonatedBy.DisplayName);
                return UseResults.TargetNotFound;
            }

            if (pcSource.CharacterFlags.HasFlag(CharacterFlags.Curse)
                || pcSource.Room.RoomFlags.HasFlag(RoomFlags.NoRecall))
            {
                pcSource.Send("Spell failed."); // TODO: message related to deity
                return UseResults.Failed;
            }

            //if (recallRoom == pcSource.Room)
            //    return UseResults.Failed;

            ICharacter victim = pcSource.Fighting;
            if (victim != null)
            {
                KnownAbility knownAbility = pcSource[ability];
                int chance = (80*knownAbility.Learned)/100;
                if (!RandomManager.Chance(chance))
                {
                    pcSource.CheckAbilityImprove(knownAbility, false, 6);
                    pcSource.Send("You failed.");
                    return UseResults.Failed;
                }

                int lose = 50;
                // TODO: gain negative experience 50
                pcSource.CheckAbilityImprove(knownAbility, true, 5);
                pcSource.Send("You recall from combat! You lose {0} exps.", lose);
                pcSource.StopFighting(true);
            }

            pcSource.UpdateMovePoints(-pcSource.MovePoints / 2); // half move
            pcSource.Act(ActOptions.ToRoom, "{0:N} disappears", pcSource);
            pcSource.ChangeRoom(recallRoom);
            pcSource.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcSource);
            pcSource.AutoLook();

            // Pet follows
            ICharacter slave = pcSource.Slave;
            if (slave != null) // no recursive call because DoRecall has been coded for IPlayableCharacter
            {
                if (slave.CharacterFlags.HasFlag(CharacterFlags.Curse))
                    return UseResults.Ok; // slave failing doesn't impact return value
                if (slave.Fighting != null)
                {
                    if (!RandomManager.Chance(80))
                        return UseResults.Ok;// slave failing doesn't impact return value
                    slave.StopFighting(true);
                }

                slave.Act(ActOptions.ToRoom, "{0:N} disappears", pcSource);
                slave.ChangeRoom(recallRoom);
                slave.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcSource);
                slave.AutoLook();
            }

            return UseResults.Ok;
        }

        [Skill(5010, "Pick lock", AbilityTargets.Custom, PulseWaitTime = 12)]
        public UseResults SkillPickLock(IAbility ability, ICharacter source, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                source.Send("Pick what?");
                return UseResults.TargetNotFound;
            }
            // Look for guards
            INonPlayableCharacter guard = source.Room.NonPlayableCharacters.FirstOrDefault(x => x.Position > Positions.Sleeping && x.Level > source.Level + 5);
            if (guard != null)
            {
                source.Act(ActOptions.ToCharacter, "{0:N} is standing too close to the lock.", guard);
                return UseResults.InvalidTarget;
            }
            // Search for item to pick lock
            KnownAbility knownAbility = source[ability];
            IItem item = FindHelpers.FindItemHere(source, parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable closeable)
                {
                    return InnerPick(closeable, source, knownAbility);
                }
                else
                {
                    source.Send("You can't do that.");
                    return UseResults.InvalidTarget;
                }
            }
            // Search for exit/direction
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(parameters[0].Value, out direction))
            {
                IExit exit = source.Room.Exit(direction);
                if (exit == null)
                {
                    source.Send("Nothing special there.");
                    return UseResults.InvalidTarget;
                }
                return InnerPick(exit, source, knownAbility);
            }
            return UseResults.InvalidTarget;
        }

        [Skill(5011, "Envenom", AbilityTargets.ItemInventory, PulseWaitTime = 36)]
        public UseResults SkillEnvenom(IAbility ability, ICharacter source, IItem item)
        {
            KnownAbility knownAbility = source[ability];
            int learned = knownAbility?.Learned ?? 0;
            if (learned < 1)
            {
                source.Send("Are you crazy? You'd poison yourself!");
                return UseResults.Failed;
            }

            // food/drink container
            if (item is IItemPoisonable poisonable)
            {
                if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless) || poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof))
                {
                    source.Act(ActOptions.ToCharacter, "You fail to poison {0}.", poisonable);
                    return UseResults.Failed;
                }
                bool success = false;
                if (RandomManager.Chance(learned))
                {
                    source.Act(ActOptions.ToAll, "{0:N} treats {1} with deadly poison.", source, poisonable);
                    poisonable.Poison();
                    poisonable.Recompute();
                    success = true;
                }
                else
                    source.Act(ActOptions.ToCharacter, "You fail to poison {0}.", poisonable);
                if (source is IPlayableCharacter playableCharacter)
                    playableCharacter.CheckAbilityImprove(knownAbility, success, 4);
                return success
                    ? UseResults.Ok
                    : UseResults.Failed;
            }
            // weapon
            if (item is IItemWeapon weapon)
            {
                // check on sharpness
                if (weapon.DamageType == SchoolTypes.Bash)
                {
                    source.Send("You can only envenom edged weapons.");
                    return UseResults.InvalidTarget;
                }
                if (weapon.WeaponFlags == WeaponFlags.Poison)
                {
                    source.Act(ActOptions.ToCharacter, "{0} is already envenomed.", weapon);
                    return UseResults.InvalidTarget;
                }
                if (weapon.WeaponFlags != WeaponFlags.None
                    || weapon.ItemFlags.HasFlag(ItemFlags.Bless)
                    || weapon.ItemFlags.HasFlag(ItemFlags.BurnProof))
                {
                    source.Act(ActOptions.ToCharacter, "You can't seem to envenom {0}.", weapon);
                    return UseResults.InvalidTarget;
                }
                bool success = false;
                int percent = RandomManager.Range(1, 100);
                if (percent < learned)
                {
                    int level = (source.Level * percent) / 100;
                    int duration = (source.Level * percent) / (2 * 100);
                    World.AddAura(weapon, ability, source, level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                        new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or });
                    source.Act(ActOptions.ToAll, "{0:N} coat{0:v} {1} with deadly venom.", source, weapon);
                    success = true;
                }
                else
                    source.Act(ActOptions.ToCharacter, "You fail to envenom {0}.", weapon);
                if (source is IPlayableCharacter playableCharacter)
                    playableCharacter.CheckAbilityImprove(knownAbility, success, 4);
                return success
                    ? UseResults.Ok
                    : UseResults.Failed;
            }
            source.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
            return UseResults.InvalidTarget;
        }

        //*******************************
        private UseResults InnerPick(ICloseable closeable, ICharacter source, KnownAbility knownAbility)
        {
            if (!closeable.IsCloseable)
            {
                source.Send("You can't do that.");
                return UseResults.InvalidTarget;
            }
            if (!closeable.IsClosed)
            {
                source.Send("It's not closed.");
                return UseResults.InvalidTarget;
            }
            if (!closeable.IsLockable)
            {
                source.Send("It can't be unlocked.");
                return UseResults.InvalidTarget;
            }
            if (closeable.IsPickProof)
            {
                source.Send("You failed.");
                return UseResults.Failed;
            }
            int chance = knownAbility?.Learned ?? 0;
            if (closeable.IsEasy)
                chance *= 2;
            if (closeable.IsHard)
                chance /= 2;
            if (!RandomManager.Chance(chance))
            {
                source.Send("You failed.");
                (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, false, 2);
                return UseResults.Failed;
            }
            closeable.Unlock();
            source.Act(ActOptions.ToAll, "{0:N} picks the lock on {1}.", source, closeable);
            (source as IPlayableCharacter)?.CheckAbilityImprove(knownAbility, true, 2);
            return UseResults.Ok;
        }
    }
}
