using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Aura;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

// ReSharper disable UnusedMember.Global

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        [Skill(5000, "Berserk", AbilityTargets.None, PulseWaitTime = 24, LearnDifficultyMultiplier = 2, CharacterWearOffMessage = "You feel your pulse slow down.")]
        public UseResults SkillBerserk(IAbility ability, int learned, ICharacter source)
        {
            int chance = learned;
            if (chance == 0
                || (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Berserk)))
            {
                source.Send("You turn red in the face, but nothing happens.");
                return UseResults.NotKnown;
            }

            if (source.CharacterFlags.HasFlag(CharacterFlags.Berserk)
                || source.GetAura(ability) != null
                || source.GetAura("Frenzy") != null)
            {
                source.Send("You get a little madder.");
                return UseResults.InvalidTarget;
            }

            if (source.CharacterFlags.HasFlag(CharacterFlags.Calm))
            {
                source.Send("You're feeling to mellow to berserk.");
                return UseResults.InvalidTarget;
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
            int hpPercent = (100 * source.HitPoints) / source.MaxHitPoints;
            chance += 25 - hpPercent / 2;

            //
            if (RandomManager.Chance(chance))
            {
                source.UpdateResource(ResourceKinds.Mana, 50);
                source.UpdateMovePoints(source.MovePoints / 2);
                source.UpdateHitPoints(source.Level * 2);

                source.Send("Your pulse races as you are consumed by rage!");
                source.Act(ActOptions.ToRoom, "{0:N} gets a wild look in {0:s} eyes.", source);

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

                return UseResults.Failed;
            }
        }

        [Skill(5001, "Bash", AbilityTargets.CharacterOffensive, PulseWaitTime = 20)]
        public UseResults SkillBash(IAbility ability, int learned, ICharacter source, ICharacter victim)
        {
            INonPlayableCharacter npcSource = source as INonPlayableCharacter;
            int chance = learned;
            if (chance == 0
                || (npcSource != null && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Bash)))
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

            if (victim.IsSafe(source))
                return UseResults.InvalidTarget;

            // TODO: check kill stealing

            if (source.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcSource?.ControlledBy == victim)
            {
                source.Act(ActOptions.ToCharacter, "But {0:N} is your friend!", victim);
                return UseResults.InvalidTarget;
            }

            // modifiers

            // size and weight
            chance += source.CarryWeight / 250;
            chance -= victim.CarryWeight / 250;
            if (source.Size < victim.Size)
                chance -= (victim.Size - source.Size) * 15; // big drawback to bash someone bigger
            else
                chance += (source.Size - victim.Size) * 10; // big advantage to bash someone smaller
            // stats
            chance += source[CharacterAttributes.Strength];
            chance -= (4 * victim[CharacterAttributes.Dexterity]) / 3;
            chance -= victim[CharacterAttributes.ArmorBash] / 25;
            // speed
            if (npcSource?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 30;
            // level
            chance += source.Level - victim.Level;

            // dodge?
            var victimDodgeInfo = victim.GetLearnInfo("Dodge");
            if (chance < victimDodgeInfo.learned)
                chance -= 3 * (victimDodgeInfo.learned - chance);

            // now the attack
            if (RandomManager.Chance(chance))
            {
                source.Act(ActOptions.ToCharacter, "You slam into {0}, and send {0:m} flying!", victim);
                source.Act(ActOptions.ToRoom, "{0:N} sends {1} sprawling with a powerful bash.", source, victim);
                // TODO: victim daze
                victim.ChangePosition(Positions.Resting);
                int damage = RandomManager.Range(2, 2 + 2 * (int)source.Size + chance / 20);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, false);
                // TODO: check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, false); // starts a fight
                victim.Act(ActOptions.ToRoom, "{0:N} fall{0:v} flat on {0:s} face!", source);
                victim.Act(ActOptions.ToCharacter, "You evade {0:p} bash, causing {0:m} to fall flat on {0:s} face.", source);
                victim.ChangePosition(Positions.Resting);
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }

        }

        [Skill(5002, "Dirt kicking", AbilityTargets.CharacterOffensive, PulseWaitTime = 24, LearnDifficultyMultiplier = 2, CharacterWearOffMessage = "You rub the dirt out of your eyes.", DamageNoun = "kicked dirt")]
        public UseResults SkillDirt(IAbility ability, int learned, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            INonPlayableCharacter npcSource = source as INonPlayableCharacter;
            int chance = learned;
            if (chance == 0
                || (npcSource != null && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.DirtKick)))
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

            if (victim.IsSafe(source))
                return UseResults.InvalidTarget;

            // TODO: check kill stealing

            if (source.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcSource?.ControlledBy == victim)
            {
                source.Act(ActOptions.ToCharacter, "But {0:N} is your friend!", victim);
                return UseResults.InvalidTarget;
            }

            // modifiers
            // dexterity
            chance += source[CharacterAttributes.Dexterity];
            chance -= 2 * victim[CharacterAttributes.Dexterity];
            // speed
            if (npcSource?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || source.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance += 10;
            if ((victim as INonPlayableCharacter)?.OffensiveFlags.HasFlag(OffensiveFlags.Fast) == true || victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
                chance -= 25;
            // level
            chance += (source.Level - victim.Level) * 2;
            // sloppy hack to prevent false zeroes
            if (chance % 5 == 0)
                chance += 1;
            // terrain
            switch (source.Room.SectorType)
            {
                case SectorTypes.Inside: chance -= 20; break;
                case SectorTypes.City: chance -= 10; break;
                case SectorTypes.Field: chance += 5; break;
                case SectorTypes.Forest: break;
                case SectorTypes.Hills: break;
                case SectorTypes.Mountain: chance -= 10; break;
                case SectorTypes.WaterSwim: chance = 0; break;
                case SectorTypes.WaterNoSwim: chance = 0; break;
                case SectorTypes.Burning: chance = 0; break;
                case SectorTypes.Air: chance = 0; break;
                case SectorTypes.Desert: chance += 10; break;
                case SectorTypes.Underwater: chance = 0; break;
                default: chance = 0; break;
            }
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
                // TODO check killer

                World.AddAura(victim, ability, source, source.Level, TimeSpan.FromSeconds(1)/*originally 0*/, AuraFlags.NoDispel, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Or });
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true);
                // TODO: check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5003, "Trip", AbilityTargets.CharacterOffensive, PulseWaitTime = 24)]
        public UseResults SkillTrip(IAbility ability, int learned, ICharacter source, ICharacter victim) // almost copy/paste from bash
        {
            INonPlayableCharacter npcSource = source as INonPlayableCharacter;
            int chance = learned;
            if (chance == 0
                || (npcSource != null && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Trip)))
            {
                source.Send("Tripping?  What's that?");
                return UseResults.NotKnown;
            }

            if (victim == source)
            {
                source.Send("You fall flat on your face!");
                source.Act(ActOptions.ToRoom, "{0:N} trips over {0:s} own feet!", source);
                return UseResults.InvalidTarget;
            }

            if (victim.IsSafe(source))
                return UseResults.InvalidTarget;

            // TODO: check kill stealing

            if (source.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcSource?.ControlledBy == victim)
            {
                source.Act(ActOptions.ToCharacter, "But {0:N} is your friend!", victim);
                return UseResults.InvalidTarget;
            }

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

            // modifiers
            if (source.Size < victim.Size)
                chance -= (victim.Size - source.Size) * 10; // bigger = harder to trip
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
                //DAZE_STATE(victim, 2 * PULSE_VIOLENCE);
                victim.ChangePosition(Positions.Resting);
                // TODO: check_killer(ch, victim)
                int damage = RandomManager.Range(2, 2 + 2 * (int)source.Size);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                // TODO check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5004, "Backstab", AbilityTargets.CharacterOffensive, PulseWaitTime = 24)]
        public UseResults SkillBackstab(IAbility ability, int learned, ICharacter source, ICharacter victim)
        {
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

            if (victim.IsSafe(source))
                return UseResults.InvalidTarget;

            // TODO: check kill stealing

            if (!(source.GetEquipment(EquipmentSlots.MainHand) is IItemWeapon))
            {
                source.Send("You need to wield a weapon to backstab.");
                return UseResults.CantUseRequiredResource;
            }

            if (victim.HitPoints < victim.MaxHitPoints / 3)
            {
                source.Act(ActOptions.ToCharacter, "{0} is hurt and suspicious ... you can't sneak up.", victim);
                return UseResults.InvalidTarget;
            }

            // TODO: check killer
            if (RandomManager.Chance(learned)
                || (learned > 1 && victim.Position <= Positions.Sleeping))
            {
                BackstabMultitHitModifier modifier = new BackstabMultitHitModifier(ability, learned);

                source.MultiHit(victim, modifier);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.None, true); // Starts fight without doing any damage
                return UseResults.Failed;
            }
        }

        [Skill(5005, "Kick", AbilityTargets.CharacterFighting)]
        public UseResults SkillKick(IAbility ability, int learned, ICharacter source, ICharacter victim)
        {
            if (learned == 0)
            {
                source.Send("You better leave the martial arts to fighters.");
                return UseResults.NotKnown;
            }

            if (source is INonPlayableCharacter npcSource && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Kick))
                return UseResults.NotKnown;

            if (RandomManager.Chance(learned))
            {
                int damage = RandomManager.Range(1, source.Level);
                victim.AbilityDamage(source, ability, damage, SchoolTypes.Bash, true);
                //check_killer(ch,victim);
                return UseResults.Ok;
            }
            else
            {
                victim.AbilityDamage(source, ability, 0, SchoolTypes.Bash, true);
                //check_killer(ch,victim);
                return UseResults.Failed;
            }
        }

        [Skill(5006, "Disarm", AbilityTargets.CharacterFighting, PulseWaitTime = 24)]
        public UseResults SkillDisarm(IAbility ability, int learned, ICharacter source, ICharacter victim)
        {
            int chance = learned;
            if (chance == 0)
            {
                source.Send("You don't know how to disarm opponents.");
                return UseResults.NotKnown;
            }

            INonPlayableCharacter npcSource = source as INonPlayableCharacter;

            // source has weapon or hand to hand
            IItemWeapon sourceWield = source.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
            var hand2HandLearned = source.GetLearnInfo("Hand to hand");
            if (sourceWield == null
                && (hand2HandLearned.learned == 0 
                    || (npcSource != null && !npcSource.OffensiveFlags.HasFlag(OffensiveFlags.Disarm))))
            {
                source.Send("You must wield a weapon to disarm.");
                return UseResults.CantUseRequiredResource;
            }

            // victim wield
            IItemWeapon victimWield = victim.GetEquipment(EquipmentSlots.MainHand) as IItemWeapon;
            if (victimWield == null)
            {
                source.Send("Your opponent is not wielding a weapon.");
                return UseResults.InvalidTarget;
            }

            // find weapon learned
            int sourceLearned = source.GetWeaponLearnInfo(sourceWield).learned;
            int victimLearned = victim.GetWeaponLearnInfo(sourceWield).learned;
            int sourceOnVictimWeaponLearned = source.GetWeaponLearnInfo(victimWield).learned;

            // modifiers
            // skill
            if (sourceWield == null)
                chance = (chance * hand2HandLearned.learned) / 150;
            else
                chance = chance * sourceLearned / 100;
            chance += (sourceOnVictimWeaponLearned / 2 - victimLearned) / 2;
            // dex vs. strength
            chance += source[CharacterAttributes.Dexterity];
            chance -= 2 * victim[CharacterAttributes.Strength];
            // level
            chance += (source.Level - victim.Level) * 2;
            // and now the attack
            if (RandomManager.Chance(chance))
            {
                if (victimWield.ItemFlags.HasFlag(ItemFlags.NoRemove))
                {
                    source.Act(ActOptions.ToCharacter, "{0:S} weapon won't budge!", victim);
                    victim.Act(ActOptions.ToCharacter, "{0:N} tries to disarm you, but your weapon won't budge!", source);
                    victim.Act(ActOptions.ToRoom, "{0} tries to disarm {1}, but fails.", source, victim); // equivalent of NO_NOTVICT
                }

                victim.Act(ActOptions.ToCharacter, "{0:N} DISARMS you and sends your weapon flying!", source);
                victim.Act(ActOptions.ToRoom, "{0:N} disarm{0:v} {1}", source, victim);

                victimWield.ChangeEquippedBy(null, true);
                if (!victimWield.ItemFlags.HasFlag(ItemFlags.NoDrop) && !victimWield.ItemFlags.HasFlag(ItemFlags.Inventory))
                {
                    victimWield.ChangeContainer(victim.Room);
                    // TODO: NPC tries to get its weapon back
                    //if (victim is INonPlayableCharacter && victim.CanSee(victimWield)) // && .Wait == 0 ???
                    //    victim.GetItem(victimWield);
                }

                // TODO  check_killer(ch, victim);
                return UseResults.Ok;
            }
            else
            {
                source.Act(ActOptions.ToCharacter, "You fail to disarm {0}.", victim);
                source.Act(ActOptions.ToRoom, "{0:N} tries to disarm {1}, but fails.", source, victim);
                // TODO  check_killer(ch, victim);
                return UseResults.Failed;
            }
        }

        [Skill(5007, "Sneak", AbilityTargets.None, LearnDifficultyMultiplier = 3)]
        public UseResults SkillSneak(IAbility ability, int learned, ICharacter source)
        {
            source.Send("You attempt to move silently.");
            source.RemoveAuras(x => x.Ability == ability, true);

            if (source.CharacterFlags.HasFlag(CharacterFlags.Sneak))
                return UseResults.InvalidTarget;

            if (RandomManager.Chance(learned))
            {
                World.AddAura(source, ability, source, source.Level, TimeSpan.FromMinutes(source.Level), AuraFlags.None, true,
                    new CharacterFlagsAffect {Modifier = CharacterFlags.Sneak, Operator = AffectOperators.Or});
                return UseResults.Ok;
            }

            return UseResults.Failed;
        }

        [Skill(5008, "Hide", AbilityTargets.None, LearnDifficultyMultiplier = 3)]
        public UseResults SkillHide(IAbility ability, int learned, ICharacter source)
        {
            source.Send("You attempt to hide.");

            if (source.CharacterFlags.HasFlag(CharacterFlags.Hide))
                source.RemoveBaseCharacterFlags(CharacterFlags.Hide);

            if (RandomManager.Chance(learned))
            {
                source.AddBaseCharacterFlags(CharacterFlags.Hide);
                source.Recompute();
                return UseResults.Ok;
            }

            return UseResults.Failed;
        }

        [Skill(5009, "Recall", AbilityTargets.None, LearnDifficultyMultiplier = 6)]
        public UseResults SkillRecall(IAbility ability, int learned, ICharacter source)
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
                Wiznet.Wiznet($"No recall room found for {pcSource.ImpersonatedBy.DisplayName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return UseResults.TargetNotFound;
            }

            if (pcSource.CharacterFlags.HasFlag(CharacterFlags.Curse)
                || pcSource.Room.RoomFlags.HasFlag(RoomFlags.NoRecall))
            {
                pcSource.Send("Mota has forsaken you."); // TODO: message related to deity
                return UseResults.InvalidTarget;
            }

            //if (recallRoom == pcSource.Room)
            //    return UseResults.InvalidTarget;

            ICharacter victim = pcSource.Fighting;
            if (victim != null)
            {
                int chance = (80*learned)/100;
                if (!RandomManager.Chance(chance))
                {
                    pcSource.Send("You failed.");
                    return UseResults.Failed;
                }

                int lose = 50;
                pcSource.GainExperience(-lose);
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

        [Skill(5010, "Pick lock", AbilityTargets.Custom, LearnDifficultyMultiplier = 2)]
        public UseResults SkillPickLock(IAbility ability, int learned, ICharacter source, string rawParameters, params CommandParameter[] parameters)
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
            IItem item = FindHelpers.FindItemHere(source, parameters[0]);
            if (item != null)
            {
                if (item is IItemCloseable closeable)
                    return InnerPick(closeable, source, learned);
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
                IExit exit = source.Room[direction];
                if (exit != null)
                    return InnerPick(exit, source, learned);
                else
                {
                    source.Send("Nothing special there.");
                    return UseResults.InvalidTarget;
                }
            }
            return UseResults.InvalidTarget;
        }

        [Skill(5011, "Envenom", AbilityTargets.ItemInventory, PulseWaitTime = 36, LearnDifficultyMultiplier = 4, ItemWearOffMessage = "The poison on {0} dries up.")]
        public UseResults SkillEnvenom(IAbility ability, int learned, ICharacter source, IItem item)
        {
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
                if (RandomManager.Chance(learned))
                {
                    source.Act(ActOptions.ToAll, "{0:N} treats {1} with deadly poison.", source, poisonable);
                    poisonable.Poison();
                    poisonable.Recompute();
                    return UseResults.Ok;
                }
                source.Act(ActOptions.ToCharacter, "You fail to poison {0}.", poisonable);
                return UseResults.Failed;
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
                int percent = RandomManager.Range(1, 100);
                if (RandomManager.Chance(percent))
                {
                    int level = (source.Level * percent) / 100;
                    int duration = (source.Level * percent) / (2 * 100);
                    World.AddAura(weapon, ability, source, level, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                        new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or });
                    source.Act(ActOptions.ToAll, "{0:N} coat{0:v} {1} with deadly venom.", source, weapon);
                    return UseResults.Ok;
                }
                source.Act(ActOptions.ToCharacter, "You fail to envenom {0}.", weapon);
                return UseResults.Failed;
            }
            source.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
            return UseResults.InvalidTarget;
        }

        [Skill(5012, "Scrolls", AbilityTargets.Custom, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
        public UseResults SkillScrolls(IAbility ability, int learned, ICharacter source, string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                source.Send("Recite what?");
                return UseResults.MissingParameter;
            }

            IItem item = FindHelpers.FindByName(source.Inventory.Where(source.CanSee), parameters[0]);
            if (item == null)
            {
                source.Send("You do not have that scroll.");
                return UseResults.TargetNotFound;
            }

            IItemScroll scroll = item as IItemScroll;
            if (scroll == null)
            {
                source.Send("You can recite only scrolls.");
                return UseResults.InvalidTarget;
            }

            if (source.Level < scroll.Level)
            {
                source.Send("This scroll is too complex for you to comprehend.");
                return UseResults.InvalidTarget;
            }

            IEntity target;
            if (parameters.Length == 1)
                target = source;
            else
                target = FindHelpers.FindByName(source.Room.People, parameters[1]) as IEntity
                    ?? FindHelpers.FindItemHere(source, parameters[1]) as IEntity;
            if (target == null)
            {
                source.Send("You can't find it.");
                return UseResults.TargetNotFound;
            }

            // let's go
            source.Act(ActOptions.ToAll, "{0:N} recite{0:v} {1}.", source, scroll);

            int chance = 20 + (4 * learned) / 5;
            if (!RandomManager.Chance(chance))
            {
                source.Send("You mispronounce a syllable.");
                World.RemoveItem(scroll);
                return UseResults.Failed;
            }

            CastFromItem(scroll.FirstSpell, scroll.Level, source, target, rawParameters, parameters);
            CastFromItem(scroll.SecondSpell, scroll.Level, source, target, rawParameters, parameters);
            CastFromItem(scroll.ThirdSpell, scroll.Level, source, target, rawParameters, parameters);
            CastFromItem(scroll.FourthSpell, scroll.Level, source, target, rawParameters, parameters);
            World.RemoveItem(scroll);
            return UseResults.Ok;
        }

        [Skill(5013, "Wands", AbilityTargets.Custom, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
        public UseResults SkillWands(IAbility ability, int learned, ICharacter source, string rawParameters, params CommandParameter[] parameters)
        {
            IEntity target;
            if (parameters.Length == 0)
                target = source.Fighting;
            else
                target = FindHelpers.FindByName(source.Room.People, parameters[0]) as IEntity 
                         ?? FindHelpers.FindItemHere(source, parameters[0]) as IEntity;

            if (target == null)
            {
                source.Send("Zap whom or what?");
                return UseResults.TargetNotFound;
            }

            IItemWand wand = source.GetEquipment<IItemWand>(EquipmentSlots.OffHand);
            if (wand == null)
            {
                source.Send("You can zap only with a wand.");
                return UseResults.InvalidTarget;
            }

            bool? success = null;
            if (wand.CurrentChargeCount > 0)
            {
                source.Act(ActOptions.ToAll, "{0:N} zap{0:v} {1} with {2}.", source, target, wand);
                int chance = 20 + (4 * learned) / 5;
                if (source.Level < wand.Level
                    || !RandomManager.Chance(chance))
                {
                    source.Act(ActOptions.ToAll, "{0:P} efforts with {1} produce only smoke and sparks.", source, wand);
                    success = false;
                }
                else
                {
                    CastFromItem(wand.Spell, wand.SpellLevel, source, target, rawParameters, parameters);
                    success = true;
                }
                wand.Use();
            }

            if (wand.CurrentChargeCount == 0)
            {
                source.Act(ActOptions.ToAll, "{0:P} {1} explodes into fragments.", source, wand);
                World.RemoveItem(wand);
            }

            return success.HasValue
                ? (success.Value
                    ? UseResults.Ok
                    : UseResults.Failed)
                : UseResults.InvalidParameter;
        }

        [Skill(5014, "Staves", AbilityTargets.Custom, PulseWaitTime = 24, LearnDifficultyMultiplier = 2)]
        public UseResults SkillStaves(IAbility ability, int learned, ICharacter source, string rawParameters, params CommandParameter[] parameters)
        {
            IItemStaff staff = source.GetEquipment<IItemStaff>(EquipmentSlots.OffHand);
            if (staff == null)
            {
                source.Send("You can brandish only with a staff.");
                return UseResults.InvalidTarget;
            }

            bool? success = null;
            if (staff.CurrentChargeCount > 0)
            {
                source.Act(ActOptions.ToAll, "{0:N} brandish{0:v} {1}.", source, staff);
                int chance = 20 + (4 * learned) / 5;
                if (source.Level < staff.Level
                    || !RandomManager.Chance(chance))
                {
                    source.Act(ActOptions.ToCharacter, "You fail to invoke {0}.", staff);
                    source.Act(ActOptions.ToRoom, "...and nothing happens.");
                    success = false;
                }
                else if (staff.Spell != null)
                {
                    INonPlayableCharacter npcSource = source as INonPlayableCharacter;
                    IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(source.Room.People.ToList()); // clone because victim can die and be removed from room
                    foreach (ICharacter victim in clone)
                    {
                        INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
                        bool cast = true;
                        switch (staff.Spell.Target)
                        {
                            case AbilityTargets.None:
                                if (victim == source)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterOffensive:
                                if (npcSource != null ? npcVictim != null : npcVictim == null)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterDefensive:
                                if (npcSource != null ? npcVictim == null : npcVictim != null)
                                    cast = false;
                                break;
                            case AbilityTargets.CharacterSelf:
                                if (victim == source)
                                    cast = false;
                                break;
                            default:
                                Wiznet.Wiznet($"SkillStaves: spell {staff.Spell} has invalid target in staff {staff.DebugName}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                                return UseResults.Error;
                        }
                        if (cast)
                            CastFromItem(staff.Spell, staff.SpellLevel, source, victim, rawParameters, parameters);
                    }
                    success = true;
                }
                else
                    Wiznet.Wiznet($"SkillStaves: no spell found in staff {staff.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);

                staff.Use();
            }

            if (staff.CurrentChargeCount == 0)
            {
                source.Act(ActOptions.ToAll, "{0:P} {1} blazes bright and is gone.", source, staff);
                World.RemoveItem(staff);
            }
            return success.HasValue
                ? (success.Value
                    ? UseResults.Ok
                    : UseResults.Failed)
                : UseResults.InvalidParameter;
        }

        //*******************************
        private UseResults InnerPick(ICloseable closeable, ICharacter source, int learned)
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
                return UseResults.InvalidTarget;
            }
            int chance = learned;
            if (closeable.IsEasy)
                chance *= 2;
            if (closeable.IsHard)
                chance /= 2;
            if (!RandomManager.Chance(chance))
            {
                source.Send("You failed.");
                return UseResults.Failed;
            }
            closeable.Unlock();
            source.Act(ActOptions.ToAll, "{0:N} picks the lock on {1}.", source, closeable);
            return UseResults.Ok;
        }

        public class BackstabMultitHitModifier : IMultiHitModifier
        {
            public BackstabMultitHitModifier(IAbility ability, int learned)
            {
                Ability = ability;
                Learned = learned;
            }

            #region IMultiHitModifier

            #region IHitModifier

            public int MaxAttackCount => 2;

            #endregion

            public IAbility Ability { get; }

            public int Learned { get; }

            public int DamageModifier(IItemWeapon weapon, int level, int baseDamage)
            {
                if (weapon != null)
                {
                    if (weapon?.Type != WeaponTypes.Dagger)
                        return baseDamage * (2 + level / 10);
                    else
                        return baseDamage * (2 + level / 8);
                }
                return baseDamage;
            }

            public int Thac0Modifier(int baseThac0)
            {
                return baseThac0 - 10 * (100 - Learned);
            }

            #endregion
        }
    }
}
