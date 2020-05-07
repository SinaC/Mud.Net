﻿using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Item;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Server.Aura;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        [Spell(1, "Acid Blast", AbilityTargets.CharacterOffensive)]
        public void SpellAcidBlast(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(level, 12);
            if (victim.SavesSpell(level, SchoolTypes.Acid))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Acid, true);
        }

        [Spell(2, "Armor", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You feel less armored.")]
        public void SpellArmor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Armor") != null)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} is already armored.");
                return;
            }
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add });
            victim.Send("You feel someone protecting you.");
            if (victim != caster)
                caster.Act(ActOptions.ToCharacter, "{0} is protected by your magic.", victim);
        }

        [Spell(3, "Bless", AbilityTargets.ItemInventoryOrCharacterDefensive, CharacterDispelMessage = "You feel less righteous.", ItemDispelMessage = "{0}'s holy aura fades.")]
        public void SpellBless(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                {
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already blessed.");
                    return;
                }
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Evil))
                {
                    IAura evilAura = item.GetAura("Curse");
                    if (!SavesDispel(level, evilAura?.Level ?? item.Level, 0))
                    {
                        if (evilAura != null)
                            item.RemoveAura(evilAura, false);
                        caster.Act(ActOptions.ToAll, "{0} glows a pale blue.", item);
                        item.RemoveBaseItemFlags(ItemFlags.Evil);
                        return;
                    }
                    caster.Act(ActOptions.ToCharacter, "The evil of {0} is too powerful for you to overcome.", item);
                    return;
                }
                World.AddAura(item, ability, caster, level, TimeSpan.FromMinutes(6 + level), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add},
                    new ItemFlagsAffect { Modifier = ItemFlags.Bless, Operator = AffectOperators.Or });
                caster.Act(ActOptions.ToAll, "{0} glows with a holy aura.", item);
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                IAura blessAura = victim.GetAura("Bless");
                if (victim.Position == Positions.Fighting || blessAura != null)
                {
                    if (caster == victim)
                        caster.Send("You are already blessed.");
                    else
                        caster.Act(ActOptions.ToCharacter, "{0:N} already has divine favor.", victim);
                    return;
                }
                victim.Send("You feel righteous.");
                if (victim != caster)
                    caster.Act(ActOptions.ToCharacter, "You grant {0} the favor of your god.", victim);
                int duration = 6 + level;
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = level / 8, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -level / 8, Operator = AffectOperators.Add });
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "SpellBless: invalid target type {0}", target.GetType());
        }

        [Spell(4, "Blindness", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "You can see again.")]
        public void SpellBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Blind) || victim.SavesSpell(level, SchoolTypes.None))
                return;

            int duration = 1 + level;
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = -4, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Blind, Operator = AffectOperators.Add });
            victim.Send("You are blinded!");
            victim.Act(ActOptions.ToRoom, "{0:N} appears to be blinded.", victim);
        }

        private static readonly int[] BurningsHandsDamageTable =
        {
            0,
            0,  0,  0,  0, 14, 17, 20, 23, 26, 29,
            29, 29, 30, 30, 31, 31, 32, 32, 33, 33,
            34, 34, 35, 35, 36, 36, 37, 37, 38, 38,
            39, 39, 40, 40, 41, 41, 42, 42, 43, 43,
            44, 44, 45, 45, 46, 46, 47, 47, 48, 48
        };
        [Spell(5, "Burning Hands", AbilityTargets.CharacterOffensive)]
        public void SpellBurningHands(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Fire, BurningsHandsDamageTable);
        }

        [Spell(6, "Call Lightning", AbilityTargets.None)]
        public void SpellCallLightning(IAbility ability, int level, ICharacter caster)
        {
            //TODO: no weather implemented
            caster.Send(StringHelpers.NotYetImplemented);
        }

        [Spell(7, "Calm", AbilityTargets.None, CharacterDispelMessage = "You have lost your peace of mind.")]
        public void SpellCalm(IAbility ability, int level, ICharacter caster)
        {
            // Stops all fighting in the room

            // Sum/Max/Count of fighting people in room
            int count = 0;
            int maxLevel = 0;
            int sumLevel = 0;
            foreach (ICharacter character in caster.Room.People.Where(x => x.Fighting != null))
            {
                count++;
                if (character is INonPlayableCharacter)
                    sumLevel += character.Level;
                else
                    sumLevel += character.Level / 2;
                maxLevel = Math.Max(maxLevel, character.Level);
            }

            // Compute chance of stopping combat
            int chance = 4 * level - maxLevel + 2 * count;
            // Always works if admin
            if (caster is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is Admin.Admin)
                sumLevel = 0;
            // Harder to stop large fights
            if (RandomManager.Range(0, chance) < sumLevel)
                return;
            //
            foreach (ICharacter victim in caster.Room.People)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;

                // IsNpc, immune magic or undead
                if (npcVictim != null && (npcVictim.CurrentImmunities.HasFlag(IRVFlags.Magic) || npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
                    continue;

                // Is affected by berserk, calm or frenzy
                if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Frenzy") != null)
                    continue;

                victim.Send("A wave of calm passes over you.");

                if (victim.Fighting != null && victim.Position == Positions.Fighting)
                    victim.StopFighting(false);

                int modifier = npcVictim != null
                    ? -5
                    : -2;
                int duration = level / 4;
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add, },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Calm, Operator = AffectOperators.Or });
            }
        }

        [Spell(8, "Cancellation", AbilityTargets.CharacterDefensive)]
        public void SpellCancellation(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if ((caster is IPlayableCharacter && victim is INonPlayableCharacter && !caster.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm) && caster.ControlledBy == victim)
                || (caster is INonPlayableCharacter && victim is IPlayableCharacter))
            {
                caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = TryDispels(level+2, victim);

            if (found)
                caster.Send("Ok.");
            else
                caster.Send("Spell failed.");
        }

        [Spell(9, "Cause Light", AbilityTargets.CharacterOffensive)]
        public void SpellCauseLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(1, 8) + level / 3;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(10, "Cause Critical", AbilityTargets.CharacterOffensive)]
        public void SpellCauseCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(3, 8) + level - 6;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(11, "Cause Serious", AbilityTargets.CharacterOffensive)]
        public void SpellCauseSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(2, 8) + level / 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(12, "Chain Lightning", AbilityTargets.CharacterOffensive)]
        public void SpellChainLightning(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.Act(ActOptions.ToRoom, "A lightning bolt leaps from {0}'s hand and arcs to {1}.", caster, victim);
            caster.Act(ActOptions.ToCharacter, "A lightning bolt leaps from your hand and arcs to {0}.", victim);
            victim.Act(ActOptions.ToCharacter, "A lightning bolt leaps from {0}'s hand and hits you!", caster);

            int damage = RandomManager.Dice(level, 6);
            if (victim.SavesSpell(level, SchoolTypes.Lightning))
                damage /= 3;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);

            // Hops from one victim to another
            ICharacter lastVictim = victim;
            level -= 4; // decrement damage
            while (level > 0)
            {
                // search a new victim
                ICharacter target = caster.Room.People.FirstOrDefault(x => x != lastVictim && victim.IsSafeSpell(caster, true));
                if (target != null) // target found
                {
                    target.Act(ActOptions.ToRoom, "The bolt arcs to {0}!", target);
                    target.Send("The bolt hits you!");
                    damage = RandomManager.Dice(level, 6);
                    if (victim.SavesSpell(level, SchoolTypes.Lightning))
                        damage /= 3;
                    victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                    level -= 4; // decrement damage
                    lastVictim = target;
                }
                else // no target found, hits caster
                {
                    //if (caster == null)
                    //    return;
                    if (lastVictim == caster) // no double hits
                    {
                        caster.Act(ActOptions.ToRoom, "The bolt seems to have fizzled out.");
                        caster.Send("The bolt grounds out through your body.");
                        return;
                    }
                    caster.Act(ActOptions.ToRoom, "The bolt arcs to {0}...whoops!", caster);
                    caster.Send("You are struck by your own lightning!");
                    damage = RandomManager.Dice(level, 6);
                    if (victim.SavesSpell(level, SchoolTypes.Lightning))
                        damage /= 3;
                    victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                    level -= 4; // decrement damage
                    lastVictim = caster;
                }
            }
        }

        [Spell(13, "Change Sex", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "Your body feels familiar again.")]
        public void SpellChangeSex(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Change Sex") != null)
            {
                if (victim == caster)
                    caster.Send("You've already been changed.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} has already had {0:s}(?) sex changed.", victim);
                return;
            }

            if (victim.SavesSpell(level, SchoolTypes.Other))
                return;

            Sex newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != victim.CurrentSex));
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(2 * level), AuraFlags.None, true,
                new CharacterSexAffect { Value = newSex });
            victim.Send("You feel different.");
            victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like $mself anymore...", victim);
        }

        [Spell(14, "Charm Person", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "You feel more self-confident.")]
        public void SpellCharmPerson(IAbility ability, int level, ICharacter caster, INonPlayableCharacter victim)
        {
            if (victim.IsSafe(caster))
                return;

            if (caster == victim)
            {
                caster.Send("You like yourself even better!");
                return;
            }

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm)
                || caster.CurrentCharacterFlags.HasFlag(CharacterFlags.Charm)
                || level < victim.Level
                || victim.CurrentImmunities.HasFlag(IRVFlags.Charm)
                || victim.SavesSpell(level, SchoolTypes.Charm))
                return;

            if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Law))
            {
                caster.Send("The mayor does not allow charming in the city limits.");
                return;
            }

            victim.ChangeController(caster);
            caster.ChangeSlave(victim);

            int duration = RandomManager.Fuzzy(level / 4);
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });

            victim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", caster);
            if (caster != victim)
                caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", victim);
        }

        private static readonly int[] ChillTouchDamageTable =
        {
            0,
            0,  0,  6,  7,  8,  9, 12, 13, 13, 13,
            14, 14, 14, 15, 15, 15, 16, 16, 16, 17,
            17, 17, 18, 18, 18, 19, 19, 19, 20, 20,
            20, 21, 21, 21, 22, 22, 22, 23, 23, 23,
            24, 24, 24, 25, 25, 25, 26, 26, 26, 27
        };
        [Spell(15, "Chill Touch", AbilityTargets.CharacterOffensive)]
        public void SpellChillTouch(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool savesSpell = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Cold, ChillTouchDamageTable);
            if (!savesSpell)
            {
                victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", victim);
                IAura existingAura = victim.GetAura(ability);
                if (existingAura != null)
                    existingAura.AddOrUpdateAffect( // TODO: update duration
                        x => x.Location == CharacterAttributeAffectLocations.Strength,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        x => x.Modifier -= 1);
                else
                    World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(6), AuraFlags.None, true,
                        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
            }
        }

        private static readonly int[] ColourSprayDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
            58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
            65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
            73, 73, 74, 75, 76, 76, 77, 78, 79, 79
        };
        [Spell(16, "Colour Spray", AbilityTargets.CharacterOffensive)]
        public void SpellColourSpray(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool savesSpell = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Light, ColourSprayDamageTable);
            if (!savesSpell)
                SpellBlindness(this["Blindness"], level / 2, caster, victim);
        }

        [Spell(17, "Continual Light", AbilityTargets.OptionalItemInventory)]
        public void SpellContinualLight(IAbility ability, int level, ICharacter caster, IItem item)
        {
            // item
            if (item != null)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Glowing))
                {
                    caster.Act(ActOptions.ToCharacter, "{0} is already glowing.", item);
                    return;
                }

                item.AddBaseItemFlags(ItemFlags.Glowing);
                caster.Act(ActOptions.ToAll, "{0} glows with a white light.", item);
                return;
            }
            // create item
            IItem light = World.AddItem(Guid.NewGuid(), Settings.LightBallBlueprintId, caster.Room);
            caster.Act(ActOptions.ToAll, "{0} twiddle{0:v} {0:s} thumbs and {1} appears.", caster, light);
        }

        [Spell(18, "Control Weather", AbilityTargets.Custom)]
        public void SpellControlWeather(IAbility ability, int level, ICharacter caster, string parameter)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            //TODO: no weather implemented
            //if (!str_cmp(target_name, "better"))
            //    weather_info.change += dice(level / 3, 4);
            //else if (!str_cmp(target_name, "worse"))
            //    weather_info.change -= dice(level / 3, 4);
            //else
            //    send_to_char("Do you want it to get better or worse?\n\r", ch);

            //send_to_char("Ok.\n\r", ch);
            //return;
        }

        [Spell(19, "Create Food", AbilityTargets.None)]
        public void SpellCreateFood(IAbility ability, int level, ICharacter caster)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            //TODO: no food implemented
        }

        [Spell(20, "Create Rose", AbilityTargets.None)]
        public void SpelLCreateRose(IAbility ability, int level, ICharacter caster)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            //TODO: add rose blueprint
        }

        [Spell(21, "Create Spring", AbilityTargets.None)]
        public void SpellCreateSpring(IAbility ability, int level, ICharacter caster)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            //TODO: no water implemented
        }

        // TODO: no drink container implemented
        //[Spell(22, "Create Food", AbilityTargets.None)]
        //public void SpellCreateWater(IAbility ability, int level, ICharacter caster, IItemDrinkContainer container)
        //{
        //}

        [Spell(23, "Cure Blindness", AbilityTargets.CharacterDefensive)]
        public void SpellCureBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Blindness", level, caster, victim, "You aren't blind.", "{0:N} doesn't appear to be blinded.", "Spell failed.", "Your vision returns!", "{0:N} is no longer blinded.");
        }

        [Spell(24, "Cure Critical", AbilityTargets.CharacterDefensive)]
        public void SpellCureCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(3, 8) + level - 6;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(25, "Cure Disease", AbilityTargets.CharacterDefensive)]
        public void SpellCureDisease(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Plague", level, caster, victim, "You aren't ill.", "{0:N} doesn't appear to be diseased.", "Spell failed.", "Your sores vanish.", "{0:N} looks relieved as $s sores vanish.");
        }

        [Spell(26, "Cure Light", AbilityTargets.CharacterDefensive)]
        public void SpellCureLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(1, 8) + level / 3;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(27, "Cure Poison", AbilityTargets.CharacterDefensive)]
        public void SpellCurePoison(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Poison", level, caster, victim, "You aren't poisoned.", "{0:N} doesn't appear to be poisoned.", "Spell failed.", "A warm feeling runs through your body.", "{0:N} looks much better.");
        }

        [Spell(28, "Cure Serious", AbilityTargets.CharacterDefensive)]
        public void SpellCureSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(2, 8) + level / 2;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(29, "Curse", AbilityTargets.ItemHereOrCharacterOffensive, CharacterDispelMessage = "The curse wears off.", ItemDispelMessage = "{0} is no longer impure.")]
        public void SpellCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Evil))
                {
                    caster.Act(ActOptions.ToCharacter, "{0} is already filled with evil.", item);
                    return;
                }
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                {
                    IAura blessAura = item.GetAura("Bless");
                    if (!SavesDispel(level, blessAura?.Level ?? item.Level, 0))
                    {
                        if (blessAura != null)
                            item.RemoveAura(blessAura, false);
                        caster.Act(ActOptions.ToAll, "{0} glows with a red aura.", item);
                        item.RemoveBaseItemFlags(ItemFlags.Bless);
                        return;
                    }
                    else
                        caster.Act(ActOptions.ToCharacter, "The holy aura of {0} is too powerful for you to overcome.");
                    return;
                }
                World.AddAura(item, ability, caster, level, TimeSpan.FromMinutes(2 * level), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = 1, Operator = AffectOperators.Add },
                    new ItemFlagsAffect { Modifier = ItemFlags.Evil, Operator = AffectOperators.Or });
                caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                IAura curseAura = victim.GetAura("Curse");
                if (curseAura != null || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Curse) || victim.SavesSpell(level, SchoolTypes.Negative))
                    return;
                victim.Send("You feel unclean.");
                if (caster != victim)
                    caster.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
                int duration = 2 * level;
                int modifier = level / 8;
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -modifier, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Curse, Operator = AffectOperators.Or });
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "SpellCurse: invalid target type {0}", target.GetType());
        }

        [Spell(30, "Demonfire", AbilityTargets.CharacterOffensive)]
        public void SpellDemonfire(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (caster is IPlayableCharacter && victim.IsEvil)
            {
                victim = caster;
                caster.Send("The demons turn upon you!");
            }

            caster.UpdateAlignment(-50);

            if (victim != caster)
            {
                caster.Act(ActOptions.ToRoom, "{0} calls forth the demons of Hell upon {1}!", caster, victim);
                victim.Act(ActOptions.ToCharacter, "{0} has assailed you with the demons of Hell!", caster);
                caster.Send("You conjure forth the demons of hell!");
            }

            int damage = RandomManager.Dice(level, 10);
            if (victim.SavesSpell(level, SchoolTypes.Negative))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
            SpellCurse(this["Curse"], 3*level/4, caster, victim);
        }

        [Spell(31, "Detect Evil", AbilityTargets.CharacterSelf, CharacterDispelMessage = "The red in your vision disappears.")]
        public void SpellDetectEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectEvil, level, "You can already sense evil.", "{0:N} can already detect evil.", "Your eyes tingle.", "Ok.");
        }

        [Spell(32, "Detect Good", AbilityTargets.CharacterSelf, CharacterDispelMessage = "The gold in your vision disappears.")]
        public void SpellDetectGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectGood, level, "You can already sense good.", "{0:N} can already detect good.", "Your eyes tingle.", "Ok.");
        }

        [Spell(33, "Detect Hidden", AbilityTargets.CharacterSelf, CharacterDispelMessage = "You feel less aware of your surroundings.")]
        public void SpellDetectHidden(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectHidden, level, "You are already as alert as you can be.", "{0:N} can already sense hidden lifeforms.", "Your awareness improves.", "Ok.");
        }

        [Spell(34, "Detect Invis", AbilityTargets.CharacterSelf, CharacterDispelMessage = "You no longer see invisible objects.")]
        public void SpellDetectInvis(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectInvis, level, "You can already see invisible.", "{0:N} can already see invisible things.", "Your eyes tingle.", "Ok.");
        }

        [Spell(35, "Detect Magic", AbilityTargets.CharacterSelf, CharacterDispelMessage = "The detect magic wears off.")]
        public void SpellDetectMagic(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectMagic, level, "You can already sense magical auras.", "{0:N} can already detect magic.", "Your eyes tingle.", "Ok.");
        }

        [Spell(36, "Detect Poison", AbilityTargets.ItemInventory)]
        public void SpellDetectPoison(IAbility ability, int level, ICharacter caster, IItem item)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            // TODO: food and drink container are not yet implemented
        }

        [Spell(37, "Dispel Evil", AbilityTargets.CharacterOffensive)]
        public void SpellDispelEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (caster is IPlayableCharacter && victim.IsEvil)
                victim = caster;
            if (victim.IsGood)
            {
                caster.Act(ActOptions.ToAll, "Mota protects {0}.", victim);
                return;
            }
            if (victim.IsNeutral)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} does not seem to be affected.", victim);
                return;
            }
            int damage = victim.HitPoints >= caster.Level * 4
                ? RandomManager.Dice(level, 4)
                : Math.Max(victim.HitPoints, RandomManager.Dice(level, 4));
            if (victim.SavesSpell(level, SchoolTypes.Holy))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
        }

        [Spell(38, "Dispel Good", AbilityTargets.CharacterOffensive)]
        public void SpellDispelGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (caster is IPlayableCharacter && victim.IsGood)
                victim = caster;
            if (victim.IsEvil)
            {
                caster.Act(ActOptions.ToAll, "{0} is protected by {0:s} evil.", victim);
                return;
            }
            if (victim.IsNeutral)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} does not seem to be affected.", victim);
                return;
            }
            int damage = victim.HitPoints >= caster.Level * 4
                ? RandomManager.Dice(level, 4)
                : Math.Max(victim.HitPoints, RandomManager.Dice(level, 4));
            if (victim.SavesSpell(level, SchoolTypes.Negative))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
        }

        [Spell(39, "Dispel Magic", AbilityTargets.CharacterOffensive)]
        public void SpellDispelMagic(IAbility ability, int level, ICharacter caster, ICharacter victim) // TODO: check difference with cancellation
        {
            if (victim.SavesSpell(level, SchoolTypes.Other))
            {
                victim.Send("You feel a brief tingling sensation.");
                caster.Send("You failed.");
                return;
            }

            bool found = TryDispels(level, victim);
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Sanctuary) && !SavesDispel(level, victim.Level, -1) && victim.GetAura("Sanctuary") == null)
            {
                victim.RemoveBaseCharacterFlags(CharacterFlags.Sanctuary);
                victim.Act(ActOptions.ToRoom, "The white aura around {0:n}'s body vanishes.", victim);
                found = true;
            }

            if (found)
                caster.Send("Ok.");
            else
                caster.Send("Spell failed.");
        }

        [Spell(40, "Earthquake", AbilityTargets.None)]
        public void SpellEarthquake(IAbility ability, int level, ICharacter caster)
        {
            caster.Send("The earth trembles beneath your feet!");
            caster.Act(ActOptions.ToRoom, "{0:N} makes the earth tremble and shiver.", caster);

            // Inform people in area
            foreach (ICharacter character in caster.Room.Area.Characters.Where(x => x.Room != caster.Room))
                character.Send("The earth trembles and shivers.");

            // Damage people in room
            foreach (ICharacter victim in caster.Room.People.Where(x => x != caster && !x.IsSafeSpell(caster, true)))
            {
                int damage = victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Flying)
                    ? 0 // no damage but starts fight
                    : level + RandomManager.Dice(2, 8);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Bash, true);
            }
        }

        [Spell(41, "Enchant Armor", AbilityTargets.ArmorInventory)]
        public void SpellEnchantArmor(IAbility ability, int level, ICharacter caster, IItemArmor armor)
        {
            //if (item.EquipedBy == null)
            //{
            //    caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingAura = null;
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in armor.Auras)
            {
                if (aura.Ability == ability)
                    existingAura = aura;
                bool found = false;
                foreach (CharacterAttributeAffect characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.AllArmor))
                {
                    fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                    found = true;
                }
                if (!found) // things get a little harder
                    fail += 20;
            }
            // apply other modifiers
            fail -= level;
            if (armor.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (armor.CurrentItemFlags.HasFlag(ItemFlags.Glowing))
                fail -= 5;
            fail = fail.Range(5, 85);
            // the moment of truth
            int result = RandomManager.Range(1,100);
            if (result < fail / 5) // item destroyed
            {
                caster.Act(ActOptions.ToAll, "{0} flares blindingly... and evaporates!", armor);
                World.RemoveItem(armor);
                return;
            }
            if (result < fail / 3) // item disenchanted
            {
                caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", armor);
                caster.Act(ActOptions.ToRoom, "{0} glows brightly, then fades.", armor);
                armor.RemoveAuras(_ => true, false);
                armor.RemoveBaseItemFlags(armor.BaseItemFlags); // clear
                armor.Recompute();
                return;
            }
            if (result <= fail) // failed, no bad result
            {
                caster.Send("Nothing seemed to happen.");
                return;
            }
            int amount;
            if (result <= (90 - level / 5)) // success
            {
                caster.Act(ActOptions.ToAll, "{0} shimmers with a gold aura.", armor);
                amount = -1;
            }
            else // exceptional enchant
            {
                caster.Act(ActOptions.ToAll, "{0} glows a brillant gold!", armor);
                armor.AddBaseItemFlags(ItemFlags.Glowing);
                amount = -2;
            }
            armor.IncreaseLevel();
            armor.AddBaseItemFlags(ItemFlags.Magic); // Permanently change item flags
            if (existingAura != null)
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add },
                        x => x.Modifier += amount);
            else
                World.AddAura(armor, ability, caster, level, Pulse.Infinite, AuraFlags.Permanent, false,
                   new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add });
            armor.Recompute();
        }

        [Spell(42, "Enchant Weapon", AbilityTargets.WeaponInventory)]
        public void SpellEnchantWeapon(IAbility ability, int level, ICharacter caster, IItemWeapon weapon)
        {
            //if (weapon.EquipedBy == null)
            //{
            //    caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingAura = null;
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in weapon.Auras)
            {
                if (aura.Ability == ability)
                    existingAura = aura;
                bool found = false;
                foreach (CharacterAttributeAffect characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.HitRoll || x.Location == CharacterAttributeAffectLocations.DamRoll))
                {
                    fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                    found = true;
                }
                if (!found) // things get a little harder
                    fail += 20;
            }
            // apply other modifiers
            fail -= 3 * level / 2;
            if (weapon.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (weapon.CurrentItemFlags.HasFlag(ItemFlags.Glowing))
                fail -= 5;
            fail = fail.Range(5, 95);
            // the moment of truth
            int result = RandomManager.Range(1, 100);
            if (result < fail / 5) // item destroyed
            {
                caster.Act(ActOptions.ToAll, "{0:N} flares blindingly... and evaporates!", weapon);
                World.RemoveItem(weapon);
                return;
            }
            if (result < fail / 3) // item disenchanted
            {
                caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", weapon);
                caster.Act(ActOptions.ToRoom, "{0:N} glows brightly, then fades.", weapon);
                weapon.RemoveAuras(_ => true, false);
                weapon.RemoveBaseItemFlags(weapon.BaseItemFlags); // clear
                weapon.Recompute();
                return;
            }
            if (result <= fail) // failed, no bad result
            {
                caster.Send("Nothing seemed to happen.");
                return;
            }
            int amount;
            if (result <= (90 - level / 5)) // success
            {
                caster.Act(ActOptions.ToAll, "{0:N} glows blue.", weapon);
                amount = 1;
            }
            else // exceptional enchant
            {
                caster.Act(ActOptions.ToAll, "{0:N} glows a brillant blue!", weapon);
                weapon.AddBaseItemFlags(ItemFlags.Glowing);
                amount = 2;
            }
            weapon.IncreaseLevel();
            weapon.AddBaseItemFlags(ItemFlags.Magic); // Permanently change item flags
            if (existingAura != null)
            {
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.HitRoll,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                        x => x.Modifier += amount);
                existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.DamRoll,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add },
                        x => x.Modifier += amount);
            }
            else
                World.AddAura(weapon, ability, caster, level, Pulse.Infinite, AuraFlags.Permanent, false,
                   new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                   new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add });
            weapon.Recompute();
        }

        [Spell(43, "Energy Drain", AbilityTargets.CharacterOffensive)]
        public void SpellEnergyDrain(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim != caster)
                caster.UpdateAlignment(-50);

            if (victim.SavesSpell(level, SchoolTypes.Negative))
            {
                victim.Send("You feel a momentary chill.");
                return;
            }

            int damage;
            if (victim.Level <= 2)
                damage = victim.HitPoints + 1;
            else
            {
                damage = RandomManager.Dice(1, level);
                // TODO: negative experience gain gain_exp( victim, 0 - number_range( level/2, 3 * level / 2 ) );
                victim.UpdateResource(ResourceKinds.Mana, -victim[ResourceKinds.Mana] / 2); // half mana
                victim.UpdateMovePoints(-victim.MovePoints / 2); // half move
                caster.Heal(caster, ability, damage, false);
            }

            victim.Send("You feel your life slipping away!");
            caster.Send("Wow....what a rush!");

            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
        }

        [Spell(44, "Faerie Fire", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "The pink aura around you fades away.")]
        public void SpellFaerieFire(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.FaerieFire))
                return;
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 2 * level, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.FaerieFire, Operator = AffectOperators.Or });
            victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", victim);
        }

        [Spell(45, "Faerie Fog", AbilityTargets.None)]
        public void SpellFaerieFog(IAbility ability, int level, ICharacter caster)
        {
            caster.Act(ActOptions.ToAll, "{0} conjures a cloud of purple smoke.", caster);

            IAbility invis = this["Invis"];
            IAbility massInvis = this["Mass Invis"];
            IAbility sneak = this["Sneak"];
            foreach (ICharacter victim in caster.Room.People.Where(x => x != caster && !x.SavesSpell(level, SchoolTypes.Other))) // && ich->invis_level <= 0
            {
                victim.RemoveAuras(x => x.Ability == invis || x.Ability == massInvis || x.Ability == sneak, false);
                victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak); // TODO: what if it's a racial ?
                victim.Recompute();
                victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
            }
        }

        [Spell(46, "Farsight", AbilityTargets.None)]
        public void SpellFarsight(IAbility ability, int level, ICharacter caster)
        {
            // TODO: simple call to command scan
            caster.Send(StringHelpers.NotYetImplemented);
        }

        private static readonly int[] FireballDamageTable =
        {
            0,
            0,   0,   0,   0,   0,      0,   0,   0,   0,   0,
            0,   0,   0,   0,  30,     35,  40,  45,  50,  55,
            60,  65,  70,  75,  80,     82,  84,  86,  88,  90,
            92,  94,  96,  98, 100,    102, 104, 106, 108, 110,
            112, 114, 116, 118, 120,    122, 124, 126, 128, 130
        };
        [Spell(47, "Fireball", AbilityTargets.CharacterOffensive)]
        public void SpellFireball(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Fire, FireballDamageTable);
        }

        [Spell(48, "Fireproof", AbilityTargets.CharacterOffensive, ItemDispelMessage = "{0}'s protective aura fades.")]
        public void SpellFireproof(IAbility ability, int level, ICharacter caster, IItem item)
        {
            if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof))
            {
                caster.Act(ActOptions.ToCharacter, "{0} is already protected from burning.", item);
                return;
            }

            int duration = RandomManager.Fuzzy(level / 4);
            World.AddAura(item, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new ItemFlagsAffect { Modifier = ItemFlags.BurnProof, Operator = AffectOperators.Or });
            caster.Act(ActOptions.ToCharacter, "You protect {0} from fire.", item);
            caster.Act(ActOptions.ToRoom, "{0} is surrounded by a protective aura.", item);
        }

        [Spell(49, "Flamestrike", AbilityTargets.CharacterOffensive)]
        public void SpellFlamestrike(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(6 + level / 2, 8);
            if (victim.SavesSpell(level, SchoolTypes.Fire))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
        }

        [Spell(50, "Fly", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You slowly float to the ground.")]
        public void SpellFly(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Flying))
            {
                if (victim == caster)
                    caster.Send("You are already airborne.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} doesn't need your help to fly.");
                return;
            }
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level + 3), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Flying, Operator = AffectOperators.Or });
            caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", victim);
        }

        [Spell(51, "Floating Disc", AbilityTargets.None)]
        public void SpellFloatingDisc(IAbility ability, int level, ICharacter caster)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            // TODO: floating equipment location not implemented
        }

        [Spell(52, "Frenzy", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "Your rage ebbs.")]
        public void SpellFrenzy(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.GetAura("Frenzy") != null)
            {
                if (victim == caster)
                    caster.Send("You are already in a frenzy.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", victim);
                return;
            }

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Calm") != null)
            {
                if (victim == caster)
                    caster.Send("Why don't you just relax for a while?");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", victim);
                return;
            }

            if ((caster.IsGood && !victim.IsGood)
                || (caster.IsNeutral && !victim.IsNeutral)
                || (caster.IsEvil && !victim.IsEvil))
            {
                caster.Act(ActOptions.ToCharacter, "Your god doesn't seem to like {0:N}.", victim);
                return;
            }

            int duration = level / 3;
            int modifier = level / 6;
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = (10 * level) / 12, Operator = AffectOperators.Add });

            victim.Send("You are filled with holy wrath!");
            victim.Act(ActOptions.ToRoom, "{0:N} gets a wild look in $s eyes!", victim);
        }

        [Spell(53, "Gate", AbilityTargets.CharacterWorldwide)]
        public void SpellGate(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
            if (victim == caster
                || victim.Room == null
                || !caster.CanSee(victim.Room)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Private)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Solitary)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall)
                || caster.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall)
                || victim.Level >= level + 3
                // TODO: clan check
                // TODO: hero level check 
                || (npcVictim != null && npcVictim.CurrentImmunities.HasFlag(IRVFlags.Summon))
                || (npcVictim != null && victim.SavesSpell(level, SchoolTypes.Other)))
            {
                caster.Send("You failed.");
                return;
            }

            bool gatePet = caster.Slave?.Room == caster.Room; // if has slave in same room

            caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", caster);
            caster.ChangeRoom(victim.Room);
            caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", caster);
            caster.AutoLook();

            // slave follows
            if (gatePet)
            {
                caster.Slave.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", caster.Slave);
                caster.Slave.ChangeRoom(victim.Room);
                caster.Slave.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", caster.Slave);
                caster.Slave.AutoLook();
            }
        }

        [Spell(54, "Giant Strength", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You feel weaker.")]
        public void SpellGiantStrength(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Giant Strength") != null)
            {
                if (victim == caster)
                    caster.Send("You are already as strong as you can get!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} can't get any stronger.", victim);
                return;
            }
            int modifier = 1 + (level >= 18 ? 1 : 0) + (level >= 25 ? 1 : 0) + (level >= 32 ? 1 : 0);
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = modifier, Operator = AffectOperators.Add });
            victim.Act(ActOptions.ToAll, "{0:P} muscles surge with heightened power.", victim);
        }

        [Spell(55, "Harm", AbilityTargets.CharacterOffensive)]
        public void SpellHarm(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = Math.Max(20, victim.HitPoints - RandomManager.Dice(1, 4));
            if (victim.SavesSpell(level, SchoolTypes.Harm))
                damage = Math.Min(50, damage / 2);
            damage = Math.Min(100, damage);
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(56, "Haste", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You feel yourself slow down.")]
        public void SpellHaste(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste)
                || victim.GetAura("Haste") != null
                || (victim is INonPlayableCharacter npcVictim && npcVictim.OffensiveFlags.HasFlag(OffensiveFlags.Fast)))
            {
                if (victim == caster)
                    caster.Send("You can't move any faster!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", victim);
                return;
            }
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                if (TryDispel(level, victim, this["Slow"]) != CheckDispelReturnValues.Dispelled)
                {
                    if (victim != caster)
                        caster.Send("Spell failed.");
                    victim.Send("You feel momentarily faster.");
                    return;
                }
                victim.Act(ActOptions.ToRoom, "{0:N} is moving less slowly.", victim);
                return;
            }
            int duration = victim == caster
                ? level / 2
                : level / 4;
            int modifier = 1 + (level >= 18 ? 1 : 0) + (level >= 25 ? 1 : 0) + (level >= 32 ? 1 : 0);
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Haste, Operator = AffectOperators.Or });
            victim.Send("You feel yourself moving more quickly.");
            victim.Act(ActOptions.ToRoom, "{0:N} is moving more quickly.", victim);
            if (caster != victim)
                caster.Send("Ok.");
        }

        [Spell(57, "Heal", AbilityTargets.CharacterDefensive)]
        public void SpellHeal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            victim.Heal(caster, ability, 100, false);
            victim.Send("A warm feeling fills your body.");
            if (caster != victim)
                caster.Send("Ok.");
        }

        [Spell(58, "Heat Metal", AbilityTargets.CharacterOffensive)]
        public void SpellHeatMetal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool fail = true;
            int damage = 0;
            if (!victim.SavesSpell(level + 2, SchoolTypes.Fire) && !victim.CurrentImmunities.HasFlag(IRVFlags.Fire))
            {
                // Check equipments
                foreach (EquipedItem equipedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    IEquipableItem item = equipedItem.Item;
                    if (!item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                        && !item.CurrentItemFlags.HasFlag(ItemFlags.NonMetal)
                        && RandomManager.Range(1, 2 * level) > item.Level
                        && !victim.SavesSpell(level, SchoolTypes.Fire))
                    {
                        switch (item)
                        {
                            case ItemArmor itemArmor:
                                if (!itemArmor.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) // remove the item
                                    && !itemArmor.CurrentItemFlags.HasFlag(ItemFlags.NoRemove)
                                    && itemArmor.Weight / 10 < RandomManager.Range(1, 2 * victim.CurrentAttribute(CharacterAttributes.Dexterity)))
                                {
                                    itemArmor.ChangeEquipedBy(null);
                                    itemArmor.ChangeContainer(victim.Room);
                                    victim.Act(ActOptions.ToRoom, "{0:N} yelps and throws {1} to the ground!", victim, itemArmor);
                                    victim.Act(ActOptions.ToCharacter, "You remove and drop {0} before it burns you.", itemArmor);
                                    damage += RandomManager.Range(1, itemArmor.Level) / 3;
                                    fail = false;
                                }
                                else // stuck on the body! ouch! 
                                {
                                    victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemArmor);
                                    damage += RandomManager.Range(1, itemArmor.Level);
                                    fail = false;
                                }
                                break;
                            case IItemWeapon itemWeapon:
                                if (itemWeapon.DamageType != SchoolTypes.Fire)
                                {
                                    if (!itemWeapon.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) // remove the item
                                        && !itemWeapon.CurrentItemFlags.HasFlag(ItemFlags.NoRemove))
                                    {
                                        itemWeapon.ChangeEquipedBy(null);
                                        itemWeapon.ChangeContainer(victim.Room);
                                        victim.Act(ActOptions.ToRoom, "{0:N} is burned by {1}, and throws it to the ground.", victim, itemWeapon);
                                        victim.Send("You throw your red-hot weapon to the ground!");
                                        damage += 1;
                                        fail = false;
                                    }
                                    else // YOWCH
                                    {
                                        victim.Send("Your weapon sears your flesh!");
                                        damage += RandomManager.Range(1, itemWeapon.Level);
                                        fail = false;
                                    }
                                }
                                break;
                        }
                    }
                }
                // Check inventory
                foreach (IItem item in victim.Inventory)
                {
                    if (!item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                        && !item.CurrentItemFlags.HasFlag(ItemFlags.NonMetal)
                        && RandomManager.Range(1, 2 * level) > item.Level
                        && !victim.SavesSpell(level, SchoolTypes.Fire))
                    {
                        switch (item)
                        {
                            case ItemArmor itemArmor:
                                if (!itemArmor.CurrentItemFlags.HasFlag(ItemFlags.NoDrop)) // drop it if we can
                                {
                                    itemArmor.ChangeContainer(victim.Room);
                                    victim.Act(ActOptions.ToRoom, "{0:N} yelps and throws {1} to the ground!", victim, itemArmor);
                                    victim.Act(ActOptions.ToCharacter, "You remove and drop {0} before it burns you.", itemArmor);
                                    damage += RandomManager.Range(1, itemArmor.Level) / 6;
                                    fail = false;
                                }
                                else // cannot drop
                                {
                                    victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemArmor);
                                    damage += RandomManager.Range(1, itemArmor.Level) / 2;
                                    fail = false;
                                }
                                break;
                            case IItemWeapon itemWeapon:
                                if (!itemWeapon.CurrentItemFlags.HasFlag(ItemFlags.NoDrop)) // drop it if we can
                                {
                                    itemWeapon.ChangeContainer(victim.Room);
                                    victim.Act(ActOptions.ToRoom, "{0:N} throws a burning hot {1} to the ground!", victim, itemWeapon);
                                    victim.Act(ActOptions.ToCharacter, "You and drop {0} before it burns you.", itemWeapon);
                                    damage += RandomManager.Range(1, itemWeapon.Level) / 6;
                                    fail = false;
                                }
                                else // cannot drop
                                {
                                    victim.Act(ActOptions.ToCharacter, "Your skin is seared by {0}!", itemWeapon);
                                    damage += RandomManager.Range(1, itemWeapon.Level) / 2;
                                    fail = false;
                                }
                                break;
                        }
                    }
                }
            }
            if (fail)
            {
                caster.Send("Your spell had no effect.");
                victim.Send("You feel momentarily warmer.");
                return;
            }
            // damage
            if (victim.SavesSpell(level, SchoolTypes.Fire))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
        }

        [Spell(59, "Holy Word", AbilityTargets.CharacterOffensive)]
        public void SpellHolyWord(IAbility ability, int level, ICharacter caster)
        {
            IAbility bless = this["Bless"];
            IAbility curse = this["Curse"];
            IAbility frenzy = this["Frenzy"];

            caster.Act(ActOptions.ToRoom, "{0:N} utters a word of divine power!");
            caster.Send("You utter a word of divine power.");

            foreach (ICharacter victim in caster.Room.People)
            {
                if ((caster.IsGood && victim.IsGood)
                    || (caster.IsNeutral && victim.IsNeutral)
                    || (caster.IsEvil && victim.IsEvil))
                {
                    victim.Send("You feel full more powerful.");
                    SpellFrenzy(frenzy, level, caster, victim);
                    SpellBless(bless, level, caster, victim);
                }
                else if ((caster.IsGood && victim.IsEvil)
                        || (caster.IsEvil && victim.IsGood))
                {
                    if (!victim.SavesSpell(level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        SpellCurse(curse, level, caster, victim);
                        int damage = RandomManager.Dice(level, 6);
                        victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
                    }
                }
                else if (caster.IsNeutral)
                {
                    if (!victim.SavesSpell(level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        SpellCurse(curse, level/2, caster, victim);
                        int damage = RandomManager.Dice(level, 4);
                        victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
                    }
                }
            }

            caster.Send("You feel drained.");
            caster.UpdateMovePoints(-caster.MovePoints); // set to 0
            //TODO: ch->hit /= 2;
        }

        [Spell(60, "Identify", AbilityTargets.ItemInventory)]
        public void SpellIdentify(IAbility ability, int level, ICharacter caster, IItem item)
        {
            // TODO
        }

        [Spell(61, "Infravision", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You no longer see in the dark.")]
        public void SpellInfravision(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Infrared, 2*level, "You can already see in the dark", "{0} already has infravision.", "Your eyes glow red.", "{0:P} eyes glow red.");
        }

        [Spell(62, "Invisibility", AbilityTargets.ItemInventoryOrCharacterDefensive, CharacterDispelMessage = "You are no longer invisible.", ItemDispelMessage = "{0} fades into view.")]
        public void SpellIInvisibilitys(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Invis))
                {
                    caster.Send("{0} is already invisible.", item);
                    return;
                }

                caster.Act(ActOptions.ToAll, "{0} fades out of sight.", item);
                World.AddAura(item, ability, caster, level, TimeSpan.FromMinutes(level + 12), AuraFlags.None, true,
                    new ItemFlagsAffect { Modifier = ItemFlags.Invis, Operator = AffectOperators.Or });
                return;
            }
            if (target is ICharacter victim)
            {
                if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Invisible))
                    return;

                victim.Act(ActOptions.ToRoom, "{0:N} fades out of existence.", victim);
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level + 12), AuraFlags.None, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
                victim.Send("You fade out of existence.");
            }
        }

        [Spell(63, "Know Alignment", AbilityTargets.CharacterDefensive)]
        public void SpellKnowAlignment(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int ap = victim.Alignment;
            string msg;
            if (ap > 700) msg = "{0:N} has a pure and good aura.";
            else if (ap > 350) msg = "{0:N} is of excellent moral character.";
            else if (ap > 100) msg = "{0:N} is often kind and thoughtful.";
            else if (ap > -100) msg = "{0:N} doesn't have a firm moral commitment.";
            else if (ap > -350) msg = "{0:N} lies to $S friends.";
            else if (ap > -700) msg = "{0:N} is a black-hearted murderer.";
            else msg = "{0:N} is the embodiment of pure evil!.";
            caster.Act(ActOptions.ToCharacter, msg, victim);
        }

        private static readonly int[] LightningBoltDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0,  0,  0, 25, 28,
            31, 34, 37, 40, 40, 41, 42, 42, 43, 44,
            44, 45, 46, 46, 47, 48, 48, 49, 50, 50,
            51, 52, 52, 53, 54, 54, 55, 56, 56, 57,
            58, 58, 59, 60, 60, 61, 62, 62, 63, 64
        };
        [Spell(64, "Lightning Bolt", AbilityTargets.CharacterOffensive)]
        public void SpellLightningBolt(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Lightning, LightningBoltDamageTable);
        }

        [Spell(65, "Locate Object", AbilityTargets.Custom)]
        public void SpellLocateObject(IAbility ability, int level, ICharacter caster, string parameter)
        {
            StringBuilder sb = new StringBuilder();
            int maxFound = (caster as IPlayableCharacter)?.ImpersonatedBy is IAdmin
                ? 200
                : level * 2;
            int number = 0;
            IEnumerable<IItem> foundItems = FindHelpers.FindAllByName(World.Items.Where(x => caster.CanSee(x) && !x.CurrentItemFlags.HasFlag(ItemFlags.NoLocate) && x.Level <= caster.Level && RandomManager.Range(1,100) <= 2*level), parameter);
            foreach (IItem item in foundItems)
            {
                IItem outOfItemContainer = item;
                // Get container until container is not an item anymore (room or character)
                int maxDepth = 500;
                while(outOfItemContainer.ContainedInto is IItem container)
                {
                    outOfItemContainer = container;
                    maxDepth--;
                    if (maxDepth <= 0) // avoid infinite loop if something goes wrong in container
                        break;
                }

                if (item.ContainedInto is IRoom room)
                    sb.AppendFormatLine("One is in {0}", room.DisplayName);
                else if (item.ContainedInto is ICharacter character && caster.CanSee(character))
                    sb.AppendFormatLine("One is carried by {0}", character.DisplayName);
                else if (item is IEquipableItem equipable && equipable.EquipedBy != null && caster.CanSee(equipable.EquipedBy))
                    sb.AppendFormatLine("One is carried by {0}", equipable.EquipedBy.DisplayName);

                number++;
                if (number >= maxFound)
                    break;
            }
            if (number == 0)
                caster.Send("Nothing like that in heaven or earth.");
            else
                caster.Page(sb);
        }

        private static readonly int[] MagicMissileDamageTable =
        {
            0,
            3,  3,  4,  4,  5,  6,  6,  6,  6,  6,
            7,  7,  7,  7,  7,  8,  8,  8,  8,  8,
            9,  9,  9,  9,  9, 10, 10, 10, 10, 10,
            11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
            13, 13, 13, 13, 13, 14, 14, 14, 14, 14
        };
        [Spell(66, "Magic Missile", AbilityTargets.CharacterOffensive)]
        public void SpellMagicMissile(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Energy, MagicMissileDamageTable);
        }

        [Spell(67, "Mass Healing", AbilityTargets.None)]
        public void SpellMassHealing(IAbility ability, int level, ICharacter caster)
        {
            IAbility heal = this["Heal"];
            IAbility refresh = this["Refresh"];

            foreach (ICharacter victim in caster.Room.People)
            {
                if ((caster is IPlayableCharacter && victim is IPlayableCharacter)
                    || (caster is INonPlayableCharacter && victim is INonPlayableCharacter))
                {
                    SpellHeal(heal, level, caster, victim);
                    SpellRefresh(refresh, level, caster, victim);
                }
            }
        }

        [Spell(68, "Mass Invis", AbilityTargets.None, CharacterDispelMessage = "You are no longer invisible.")]
        public void SpellMassInvis(IAbility ability, int level, ICharacter caster)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            // TODO: group is important
        }

        [Spell(69, "Nexus", AbilityTargets.CharacterWorldwide)]
        public void SpellNexus(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            // TODO
        }

        [Spell(70, "Pass Door", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "You feel solid again.")]
        public void SpellPassDoor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int duration = RandomManager.Fuzzy(level / 4);
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.PassDoor, duration, "You are already out of phase.", "{0:N} is already shifted out of phase.", "You turn translucent.", "{0} turns translucent.");
        }

        [Spell(71, "Plague", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "Your sores vanish.")]
        public void SpellPlague(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.SavesSpell(level, SchoolTypes.Disease)
                || (victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
            {
                if (victim == caster)
                    caster.Send("You feel momentarily ill, but it passes.");
                else
                    caster.Send("{0:N} seems to be unaffected.", victim);
            }

            World.AddAura(victim, ability, caster, (3 * level) / 4, TimeSpan.FromMinutes(level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Plague, Operator = AffectOperators.Or });
            victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", victim);
        }

        [Spell(72, "Poison", AbilityTargets.ItemHereOrCharacterOffensive, CharacterDispelMessage = "You feel less sick.", ItemDispelMessage = "The poison on {0} dries up.")]
        public void SpellPoison(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                // TODO: food/drink container not yet implemented
                if (item is IItemWeapon itemWeapon)
                {
                    if (itemWeapon.CurrentWeaponFlags == WeaponFlags.Poison)
                    {
                        caster.Act(ActOptions.ToCharacter, "{0} is already envenomed.", itemWeapon);
                        return;
                    }
                    if (itemWeapon.CurrentWeaponFlags != WeaponFlags.None)
                    {
                        caster.Act(ActOptions.ToCharacter, "You can't seem to envenom {0}.", itemWeapon);
                        return;
                    }
                    int duration = level / 8;
                    World.AddAura(itemWeapon, ability, caster, level / 2, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                        new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or});
                    caster.Act(ActOptions.ToCharacter, "{0} is coated with deadly venom.", itemWeapon);
                    return;
                }
                caster.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                if (victim.SavesSpell(level, SchoolTypes.Poison))
                {
                    victim.Act(ActOptions.ToRoom, "{0:N} turns slightly green, but it passes.", victim);
                    victim.Send("You feel momentarily ill, but it passes.");
                    return;
                }

                int duration = level;
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                victim.Send("You feel very sick.");
                victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);
            }
        }

        [Spell(73, "Portal", AbilityTargets.CharacterWorldwide)]
        public void SpellPortal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.Send(StringHelpers.NotYetImplemented);
            // TODO
        }

        [Spell(74, "Protection Evil", AbilityTargets.CharacterSelf, CharacterDispelMessage = "You feel less protected.")]
        public void SpellProtectionEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already protected.", victim);
                return;
            }

            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.ProtectEvil, Operator = AffectOperators.Or });
            victim.Send("You feel holy and pure.");
            caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} protected from evil.", victim);
        }

        [Spell(75, "Protection Good", AbilityTargets.CharacterSelf, CharacterDispelMessage = "You feel less protected.")]
        public void SpellProtectionGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already protected.", victim);
                return;
            }

            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.ProtectGood, Operator = AffectOperators.Or });
            victim.Send("You feel aligned with darkness.");
            caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} protected from good.", victim);
        }

        [Spell(76, "Ray of Truth", AbilityTargets.CharacterOffensive)]
        public void SpellRayOfTruth(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (caster.IsEvil)
            {
                victim = caster;
                caster.Send("The energy explodes inside you!");
            }

            if (victim != caster)
                caster.Act(ActOptions.ToAll, "{0:N} raise{0:v} {0:s} hand, and a blinding ray of light shoots forth!");

            if (victim.IsGood)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} seems unharmed by the light.", victim);
                victim.Send("The light seems powerless to affect you.");
                return;
            }

            int damage = RandomManager.Dice(level, 10);
            if (victim.SavesSpell(level, SchoolTypes.Holy))
                damage /= 2;

            int alignment = victim.Alignment - 350;
            if (alignment < -1000)
                alignment = -1000 + (alignment + 1000) / 3;

            damage = (damage * alignment * alignment) / (1000*1000);

            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
            SpellBlindness(this["Blindness"], (3 * level) / 4, caster, victim);
        }

        //[Spell(77, "Recharge", AbilityTargets.ItemChargeInventory)]
        //TODO: public void SpellRecharge(IAbility ability, int level, ICharacter caster, IItemCharge item)
        // TODO: staff/wand not yet implemented

        [Spell(78, "Refresh", AbilityTargets.CharacterDefensive)]
        public void SpellRefresh(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            victim.UpdateMovePoints(level);
            if (victim.MovePoints == victim.CurrentAttribute(CharacterAttributes.MaxMovePoints))
                victim.Send("You feel fully refreshed!");
            else
                victim.Send("You feel less tired.");
            if (caster != victim)
                caster.Send("Ok");
        }

        [Spell(79, "Remove Curse", AbilityTargets.ItemInventoryOrCharacterDefensive)]
        public void SpellRemoveCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) || item.CurrentItemFlags.HasFlag(ItemFlags.NoRemove))
                {
                    if (!item.CurrentItemFlags.HasFlag(ItemFlags.NoUncurse) && !SavesDispel(level + 2, item.Level, 0))
                    {
                        item.RemoveBaseItemFlags(ItemFlags.NoRemove);
                        item.RemoveBaseItemFlags(ItemFlags.NoDrop);
                        caster.Act(ActOptions.ToAll, "{0:N} glows blue.", item);
                        return;
                    }
                    caster.Act(ActOptions.ToCharacter, "The curse on {0} is beyond your power.", item);
                    return;
                }
                caster.Act(ActOptions.ToCharacter, "There doesn't seem to be a curse on {0}.", item);
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                if (TryDispel(level, victim, this["Curse"]) == CheckDispelReturnValues.Dispelled)
                {
                    victim.Send("You feel better.");
                    victim.Act(ActOptions.ToRoom, "{0:N} looks more relaxed.", victim);
                }

                // attempt to remove curse on one item in inventory or equipment
                foreach (IItem carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).Where(x => (x.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) || x.CurrentItemFlags.HasFlag(ItemFlags.NoRemove)) && !x.CurrentItemFlags.HasFlag(ItemFlags.NoUncurse)))
                    if (!SavesDispel(level, carriedItem.Level, 0))
                    {
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoRemove);
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoDrop);
                        victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                        break;
                    }
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "SpellRemoveCurse: invalid target type {0}", target.GetType());
        }

        [Spell(80, "Sanctuary", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "The white aura around your body fades.")]
        public void SpellSanctuary(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int duration = level / 6;
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Sanctuary, duration, "You are already in sanctuary.", "{0:N} is already in sanctuary.", "You are surrounded by a white aura.", "{0:N} is surrounded by a white aura.");
        }

        [Spell(81, "Shield", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "Your force shield shimmers then fades away.")]
        public void SpellShield(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura(ability) != null)
            {
                if (victim != caster)
                    caster.Send("You are already shielded from harm.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already protected by a shield.", victim);
                return;
            }

            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(8+level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add });
            caster.Send("You are surrounded by a force shield.");
            caster.Act(ActOptions.ToRoom, "{0:N} {0:b} surrounded by a force shield.", victim);
        }

        private static readonly int[] ShockingGraspDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0, 20, 25, 29, 33,
            36, 39, 39, 39, 40, 40, 41, 41, 42, 42,
            43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
            48, 48, 49, 49, 50, 50, 51, 51, 52, 52,
            53, 53, 54, 54, 55, 55, 56, 56, 57, 57
        };
        [Spell(82, "Shocking Grasp", AbilityTargets.CharacterOffensive)]
        public void SpellShockingGrasp(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Lightning, ShockingGraspDamageTable);
        }

        [Spell(83, "Sleep", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "You feel less tired.")]
        public void SpellSleep(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Sleep)
                || (victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead))
                || level + 2 < victim.Level
                || victim.SavesSpell(level - 4, SchoolTypes.Charm))
                return;

            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(4 + level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -10, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Sleep, Operator = AffectOperators.Or });

            if (victim.Position > Positions.Sleeping)
            {
                victim.Send("You feel very sleepy ..... zzzzzz.");
                victim.Act(ActOptions.ToRoom, "{0:N} goes to sleep.", victim);
                victim.ChangePosition(Positions.Sleeping);
            }
        }

        [Spell(84, "Slow", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "You feel yourself speed up.")]
        public void SpellSlow(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Slow)
                || victim.GetAura(ability) != null)
            {
                if (victim == caster)
                    caster.Send("You can't move any slower!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} can't get any slower than that.", victim);
                return;
            }

            if (victim.CurrentImmunities.HasFlag(IRVFlags.Magic)
                || victim.SavesSpell(level, SchoolTypes.Other))
            {
                if (victim != caster)
                    caster.Send("Nothing seemed to happen.");
                victim.Send("You feel momentarily lethargic.");
                return;
            }

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
            {
                if (TryDispel(level, victim, this["Haste"]) != CheckDispelReturnValues.Dispelled)
                {
                    if (victim != caster)
                        caster.Send("Spell failed.");
                    victim.Send("You feel momentarily slower.");
                    return;
                }
                victim.Act(ActOptions.ToRoom, "{0:N} is moving less quickly.", victim);
                return;
            }

            int duration = level / 2;
            int modifier = -1 - (level >= 18 ? 1 : 0) - (level >= 25 ? 1 : 0) - (level >= 32 ? 1 : 0);
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Slow, Operator = AffectOperators.Or });
            victim.Recompute();
            victim.Send("You feel yourself slowing d o w n...");
            caster.Act(ActOptions.ToRoom, "{0} starts to move in slow motion.", victim);
        }

        [Spell(85, "Stone Skin", AbilityTargets.CharacterDefensive, CharacterDispelMessage = "Your skin feels soft again.")]
        public void SpellStoneSkin(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura(ability) != null)
            {
                if (victim == caster)
                    caster.Send("Your skin is already as hard as a rock.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already as hard as can be.", victim);
                return;
            }

            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -40, Operator = AffectOperators.Add });
            caster.Act(ActOptions.ToAll, "{0:P} skin turns to stone.", victim);
        }

        [Spell(86, "Summon", AbilityTargets.CharacterWorldwide)]
        public void SpellSummon(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
            IPlayableCharacter pcVictim = victim as IPlayableCharacter;
            if (victim == caster
                || victim.Room == null
                || caster.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Private)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Solitary)
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall)
                || npcVictim?.ActFlags.HasFlag(ActFlags.Aggressive) == true
                || victim.Level >= level+3
                || pcVictim?.ImpersonatedBy is IAdmin
                || victim.Fighting != null
                || npcVictim?.CurrentImmunities.HasFlag(IRVFlags.Summon) == true
                //TODO: shop || nonPlayableCharacterVictim
                //TODO: plr_nosummon || playableCharacterVictim
                || (npcVictim != null && victim.SavesSpell(level, SchoolTypes.Other)))
            {
                caster.Send("You failed.");
                return;
            }

            victim.Act(ActOptions.ToRoom, "{0:N} disappears suddenly.", victim);
            victim.ChangeRoom(caster.Room);
            caster.Act(ActOptions.ToRoom, "{0:N} arrives suddenly", victim);
            victim.Act(ActOptions.ToCharacter, "{0:N} has summoned you!", caster);
            victim.AutoLook();
        }

        [Spell(87, "Teleport", AbilityTargets.CharacterSelf)]
        public void SpellTeleport(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.Room == null
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall)
                || (victim != caster && victim.CurrentImmunities.HasFlag(IRVFlags.Summon))
                || (victim is IPlayableCharacter pcVictim && pcVictim.Fighting != null)
                || (victim != caster && victim.SavesSpell(level - 5, SchoolTypes.Other)))
            {
                caster.Send("Spell failed.");
                return;
            }

            IRoom destination = World.GetRandomRoom(caster);
            if (destination == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "SpellTeleport: no random room available for {0}", victim.DebugName);
                caster.Send("Spell failed.");
                return;
            }

            if (victim != caster)
                victim.Send("You have been teleported!");

            victim.Act(ActOptions.ToRoom, "{0:N} vanishes", victim);
            victim.ChangeRoom(destination);
            victim.Act(ActOptions.ToRoom, "{0:N} slowly fades into existence.", victim);
            victim.AutoLook();
        }

        [Spell(88, "Ventriloquate", AbilityTargets.None)] // TODO: which AbilityTargets
        public void SpellVentriloquate(IAbility ability, int level, ICharacter caster, ICharacter victim, string parameter)
        {
            string phraseSuccess = $"%g%{victim.DisplayName} says '%x%{parameter ?? ""}%g%'%x%.";
            string phraseFail = $"Someone makes %g%{victim.DisplayName} say '%x%{parameter ?? ""}%g%'%x%.";

            foreach (ICharacter character in caster.Room.People.Where(x => x != victim && x.Position > Positions.Sleeping))
            {
                if (character.SavesSpell(level, SchoolTypes.Other))
                    character.Send(phraseFail);
                else
                    character.Send(phraseSuccess);
            }
        }

        [Spell(89, "Weaken", AbilityTargets.CharacterOffensive, CharacterDispelMessage = "You feel stronger.")]
        public void SpellWeaken(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Weaken) || victim.GetAura(ability) != null || victim.SavesSpell(level, SchoolTypes.Other))
                return;

            int duration = level / 2;
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -level/5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Weaken, Operator = AffectOperators.Or });
            victim.Recompute();
            victim.Send("You feel your strength slip away.");
            victim.Act(ActOptions.ToRoom, "{0:N} looks tired and weak.", victim);
        }

        [Spell(90, "Word of Recall", AbilityTargets.CharacterSelf)]
        public void SpellWordOfRecall(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            IPlayableCharacter pcVictim = victim as IPlayableCharacter;
            if (pcVictim == null)
                return;

            IRoom recallRoom = pcVictim.RecallRoom;
            if (recallRoom == null)
            {
                pcVictim.Send("You are completely lost.");
                return;
            }

            if (pcVictim.CurrentCharacterFlags.HasFlag(CharacterFlags.Curse)
                || pcVictim.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall))
            {
                pcVictim.Send("Spell failed.");
                return;
            }

            if (pcVictim.Fighting != null)
                pcVictim.StopFighting(true);

            pcVictim.UpdateMovePoints(-pcVictim.CurrentAttribute(CharacterAttributes.MaxMovePoints) / 2);
            pcVictim.Act(ActOptions.ToRoom, "{0:N} disappears", pcVictim);
            pcVictim.ChangeRoom(recallRoom);
            pcVictim.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcVictim);
            pcVictim.AutoLook();
        }

        // NPC Spells

        [Spell(500, "Acid Breath", AbilityTargets.CharacterOffensive)]
        public void SpellAcidBreath(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.ActToNotVictim(victim, "{0} spits acid at {1}.", caster, victim);
            victim.Act(ActOptions.ToCharacter, "{0} spits a stream of corrosive acid at you.", caster);
            caster.Act(ActOptions.ToCharacter, "You spit acid at {0}.", victim);

            int hp = Math.Max(12, victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
            int diceDamage = RandomManager.Dice(level, 16);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            if (victim.SavesSpell(level, SchoolTypes.Acid))
            {
                AcidEffect(victim, ability, caster, level / 2, damage / 4);
                victim.AbilityDamage(caster, ability, damage / 2, SchoolTypes.Acid, true);
            }
            else
            {
                AcidEffect(victim, ability, caster, level, damage);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Acid, true);
            }
        }

        [Spell(501, "Fire Breath", AbilityTargets.CharacterOffensive)]
        public void SpellFireBreath(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.ActToNotVictim(victim, "{0} breathes forth a cone of fire.", caster);
            victim.Act(ActOptions.ToCharacter, "{0} breathes a cone of hot fire over you!", caster);
            caster.Send("You breath forth a cone of fire.");

            int hp = Math.Max(10, victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
            int diceDamage = RandomManager.Dice(level, 20);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            BreathAreaEffect(victim, ability, caster, level, damage, SchoolTypes.Fire, FireEffect);
        }

        [Spell(502, "Frost Breath", AbilityTargets.CharacterOffensive)]
        public void SpellFrostBreath(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.ActToNotVictim(victim, "{0} breathes out a freezing cone of frost!", caster);
            victim.Act(ActOptions.ToCharacter, "{0} breathes a freezing cone of frost over you!", caster);
            caster.Send("You breath out a cone of frost.");

            int hp = Math.Max(12, victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
            int diceDamage = RandomManager.Dice(level, 18);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            BreathAreaEffect(victim, ability, caster, level, damage, SchoolTypes.Cold, ColdEffect);
        }

        [Spell(503, "Gas Breath", AbilityTargets.None)]
        public void SpellGasBreath(IAbility ability, int level, ICharacter caster)
        {
            caster.Act(ActOptions.ToRoom, "{0} breathes out a cloud of poisonous gas!", caster);
            caster.Send("You breath out a cloud of poisonous gas.");

            int hp = Math.Max(16, caster.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 15, hp / 8);
            int diceDamage = RandomManager.Dice(level, 12);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            BreathAreaEffect(caster, ability, caster, level, damage, SchoolTypes.Poison, PoisonEffect);
        }

        [Spell(504, "Lightning Breath", AbilityTargets.None)]
        public void SpellLightningBreath(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.ActToNotVictim(victim, "{0} breathes a bolt of lightning at {1}.", caster, victim);
            victim.Act(ActOptions.ToCharacter, "{0} breathes a bolt of lightning at you!", caster);
            caster.Act(ActOptions.ToCharacter, "You breathe a bolt of lightning at {0}.", victim);

            int hp = Math.Max(10, victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
            int diceDamage = RandomManager.Dice(level, 20);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            if (victim.SavesSpell(level, SchoolTypes.Lightning))
            {
                AcidEffect(victim, ability, caster, level / 2, damage / 4);
                victim.AbilityDamage(caster, ability, damage / 2, SchoolTypes.Lightning, true);
            }
            else
            {
                AcidEffect(victim, ability, caster, level, damage);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
            }
        }

        // TODO: general purpose, high explosive

        #region Effects

        public void AcidEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "AcidEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    AcidEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    AcidEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only corpse, container, armor, clothing, wand, staff and scroll
                if (!(item is IItemCorpse || item is IItemContainer || item is IItemArmor)) // TODO: wand, staff, scroll, clothing
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                // TODO: if staff/wand -> chance-=10
                // TODO: if scroll -> chance+=10
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                string msg;
                switch (item)
                {
                    case IItemCorpse _:
                    case IItemContainer _:
                        msg = "{0} fumes and dissolves.";
                        break;
                    case IItemArmor _:
                        msg = "{0} is pitted and etched.";
                        break;
                    // TODO: clothing "$p is corroded into scrap."
                    // TODO: staff, wand  "$p corrodes and breaks."
                    // TODO: scroll  "$p is burned into waste."
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "AcidEffect: default message for unexpected item type {0}", item.GetType());
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipableItem)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                if (item is IItemArmor) // etch it
                {
                    IAura existingAura = item.GetAura(ability);
                    if (existingAura != null)
                        existingAura.AddOrUpdateAffect(
                            x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add },
                            x => x.Modifier += 1);
                    else
                        World.AddAura(item, ability, source, level, Pulse.Infinite, AuraFlags.Permanent, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add });
                    item.Recompute();
                    return;
                }
                // destroy container, dump the contents and apply fire effect on them
                if (item is IContainer container) // get rid of content and apply acid effect on it
                {
                    IRoom dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
                    if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                        dropItemTargetRoom = roomContainer;
                    else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                        dropItemTargetRoom = character.Room;
                    else if (item is IEquipableItem equipable && equipable.EquipedBy.Room != null) // if container is in equipment, unequip and drop content to room
                    {
                        equipable.ChangeEquipedBy(null);
                        dropItemTargetRoom = equipable.EquipedBy.Room;
                    }
                    foreach (IItem itemInContainer in container.Content)
                    {
                        if (dropItemTargetRoom != null) // drop and apply acid effect
                        {
                            itemInContainer.ChangeContainer(dropItemTargetRoom);
                            AcidEffect(itemInContainer, ability, source, level / 2, damage / 2);
                        }
                        else // if item is nowhere, destroy it
                            World.RemoveItem(itemInContainer);
                    }
                    World.RemoveItem(item); // destroy item
                    dropItemTargetRoom?.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "AcidEffect: invalid target type {0}", target.GetType());
        }

        public void ColdEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ColdEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ColdEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // whack a character
            {
                // chill touch effect
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Cold))
                {
                    victim.Send("A chill sinks deep into your bones.");
                    victim.Act(ActOptions.ToRoom, "{0:N} turns blue and shivers.", victim);
                    IAbility chillTouchAbility = this["Chill Touch"];
                    IAura chillTouchAura = victim.GetAura(chillTouchAbility);
                    if (chillTouchAura != null)
                        chillTouchAura.AddOrUpdateAffect( // TODO: update duration
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                    else
                        World.AddAura(victim, chillTouchAbility, source, level, TimeSpan.FromMinutes(6), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
                }
                // hunger! (warmth sucked out)
                // TODO: gain_condition(victim,COND_HUNGER,dam/20); if NPC
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    ColdEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only potion and drink container
                if (true) //if (!(item is IItemPotion || item is IDrinkContainer)) // TODO: potion and drink container
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                //if (item is IItemPotion)
                //    chance += 25;
                //if (item is IDrinkContainer)
                //    chance += 5;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // display msg
                string msg = "{0} freezes and shatters!";
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipableItem)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                // unequip and destroy item
                IEntity itemContainedInto = null;
                if (item is IEquipableItem equipable && equipable.EquipedBy != null) // if item equiped: unequip 
                {
                    equipable.ChangeEquipedBy(null);
                    itemContainedInto = equipable.EquipedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                itemContainedInto?.Recompute();
            }
            Log.Default.WriteLine(LogLevels.Error, "ColdEffect: invalid target type {0}", target.GetType());
        }

        public void FireEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "FireEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    FireEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of blindness
                if (!victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Blind) && !victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Fire))
                {
                    victim.Send("Your eyes tear up from smoke...you can't see a thing!");
                    victim.Act(ActOptions.ToRoom, "{0} is blinded by smoke!", victim);
                    IAbility fireBreath = this["Fire Breath"];
                    int duration = RandomManager.Range(1, level / 10);
                    World.AddAura(victim, fireBreath, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterAttributeAffect { Operator = AffectOperators.Add, Modifier = -4, Location = CharacterAttributeAffectLocations.HitRoll },
                        new CharacterFlagsAffect { Operator = AffectOperators.Or, Modifier = CharacterFlags.Blind });
                }
                // getting thirsty
                // TODO: gain_condition(victim,COND_THIRST,dam/20); if NPC
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    FireEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only container, potion, scroll, staff, wand, food, pill
                if (!(item is IItemContainer)) // TODO: potion, scroll, staff, wand, food, pill
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                //TODO: IItemPotion chance += 25
                //TODO: IItemScroll chance += 50
                //TODO: IItemStaff chance += 10
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // display msg
                string msg;
                switch (item)
                {
                    case IItemContainer _:
                        msg = "{0} ignites and burns!";
                        break;
                    //TODO case IItemPotion _: msg = "{0} bubbles and boils!"
                    //TODO case IItemScroll _: msg = "{0} crackles and burns!"
                    //TODO case IItemStaff _: msg = "{0} smokes and chars!"
                    //TODO case IItemWand _: msg = "{0} sparks and sputters!"
                    //TODO case IItemFood _: msg = "{0} blackens and crisps!"
                    //TODO case IItemPill _: msg = "{0} melts and drips!"
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "FireEffect: default message for unexpected item type {0}", item.GetType());
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipableItem)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                // destroy container, dump the contents and apply fire effect on them
                if (item is IItemContainer itemContainer) // get rid of content and apply fire effect on it
                {
                    IRoom dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
                    if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                        dropItemTargetRoom = roomContainer;
                    else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                        dropItemTargetRoom = character.Room;
                    else if (item is IEquipableItem equipable && equipable.EquipedBy.Room != null) // if container is equiped, unequip and drop content to room
                    {
                        equipable.ChangeEquipedBy(null);
                        dropItemTargetRoom = equipable.EquipedBy.Room;
                    }
                    foreach (IItem itemInContainer in itemContainer.Content)
                    {
                        if (dropItemTargetRoom != null) // drop and apply acid effect
                        {
                            itemInContainer.ChangeContainer(dropItemTargetRoom);
                            FireEffect(itemInContainer, ability, source, level / 2, damage / 2);
                        }
                        else // if item is nowhere, destroy it
                            World.RemoveItem(itemInContainer);
                    }
                    World.RemoveItem(item); // destroy item
                    dropItemTargetRoom?.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "FireEffect: invalid target type {0}", target.GetType());
        }

        public void PoisonEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "PoisonEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    PoisonEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of poisoning
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Poison))
                {
                    victim.Send("You feel poison coursing through your veins.");
                    victim.Act(ActOptions.ToRoom, "{0} looks very ill.", victim);
                    int duration = level / 2;
                    IAbility poisonAbility = this["Poison"];
                    IAura poisonAura = victim.GetAura(poisonAbility);
                    if (poisonAura != null) // TODO: update duration
                    {
                        poisonAura.AddOrUpdateAffect(
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                        poisonAura.AddOrUpdateAffect(
                            x => x.Modifier == CharacterFlags.Poison,
                            () => new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                            null);
                    }
                    else
                        World.AddAura(victim, ability, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                }
                // equipment
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    PoisonEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // do some poisoning
            {
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // TODO: Food/DrinkContainer not implemented -> poison food or drink container
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "PoisonEffect: invalid target type {0}", target.GetType());
        }

        public void ShockEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ShockEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ShockEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // daze and confused?
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Lightning))
                {
                    victim.Send("Your muscles stop responding.");
                    // TODO: set Daze to Math.Max(12, level/4 + damage/20)
                }
                // toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    PoisonEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // unequip and destroy item
                IEntity itemContainedInto;
                if (item is IEquipableItem equipable && equipable.EquipedBy != null) // if item is equiped: unequip 
                {
                    equipable.ChangeEquipedBy(null);
                    itemContainedInto = equipable.EquipedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                itemContainedInto?.Recompute();
            }
            Log.Default.WriteLine(LogLevels.Error, "ShockEffect: invalid target type {0}", target.GetType());
        }

        #endregion

        #region Helpers

        private void BreathAreaEffect(ICharacter victim, IAbility ability, ICharacter caster, int level, int damage, SchoolTypes damageType, Action<IEntity, IAbility, ICharacter, int, int> breathAction)
        {
            // Room content
            breathAction(caster.Room, ability, caster, level, damage / 2);
            // Room people
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.Where(x => !x.IsSafeSpell(caster, true) && !(x is INonPlayableCharacter && caster is INonPlayableCharacter && (caster.Fighting != x || x.Fighting != caster))).ToList());
            foreach (ICharacter coVictim in clone)
            {
                if (victim == coVictim) // full damage
                {
                    if (coVictim.SavesSpell(level, damageType))
                    {
                        breathAction(coVictim, ability, caster, level / 2, damage / 4);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                    else
                    {
                        breathAction(coVictim, ability, caster, level, damage);
                        coVictim.AbilityDamage(caster, ability, damage, damageType, true);
                    }
                }
                else // partial damage
                {
                    if (coVictim.SavesSpell(level - 2, damageType))
                    {
                        breathAction(coVictim, ability, caster, level / 4, damage / 8);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                    else
                    {
                        breathAction(coVictim, ability, caster, level / 2, damage / 4);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                }
            }
        }

        private bool TableBaseDamageSpell(IAbility ability, int level, ICharacter caster, ICharacter victim, SchoolTypes damageType, int[] table) // returns Rom24Common.SavesSpell result
        {
            int entry = level.Range(table);
            int minDamage = table[entry] / 2;
            int maxDamage = table[entry] * 2;
            int damage = RandomManager.Range(minDamage, maxDamage);
            bool savesSpell = victim.SavesSpell(level, damageType);
            if (savesSpell)
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
            return savesSpell;
        }

        private void GenericCharacterFlagsAbility(IAbility ability, int level, ICharacter caster, ICharacter victim, CharacterFlags characterFlags, int duration, string selfAlreadyAffected, string notSelfAlreadyAffected, string success, string notSelfSuccess)
        {
            if (victim.CurrentCharacterFlags.HasFlag(characterFlags))
            {
                if (victim == caster)
                    caster.Send(selfAlreadyAffected);
                else
                    caster.Act(ActOptions.ToCharacter, notSelfAlreadyAffected, victim);
                return;
            }
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = characterFlags, Operator = AffectOperators.Or });
            victim.Send(success);
            if (victim != caster)
                victim.Act(ActOptions.ToRoom, notSelfSuccess, victim);
        }

        private void GenericSpellCureAbility(string toCureAbilityName, int level, ICharacter caster, ICharacter victim, string selfNotFound, string notSelfNotFound, string noAction, string selfDispelled, string notSelfDispelled)
        {
            CheckDispelReturnValues dispel = TryDispel(level, victim, this[toCureAbilityName]);
            switch (dispel)
            {
                case CheckDispelReturnValues.NotFound:
                    if (victim == caster)
                        caster.Send(selfNotFound);
                    else
                        caster.Act(ActOptions.ToCharacter, notSelfNotFound, victim);
                    break;
                case CheckDispelReturnValues.FoundAndNotDispelled:
                    caster.Send(noAction);
                    break;
                case CheckDispelReturnValues.Dispelled:
                    victim.Send(selfDispelled);
                    victim.Act(ActOptions.ToRoom, notSelfDispelled, victim);
                    break;
            }
        }

        private bool TryDispels(int level, ICharacter victim)
        {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            bool found = false;
            if (TryDispel(level, victim, this["Armor"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Bless"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Blindness"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer blinded.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Calm"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N no longer looks so peaceful...", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Change Sex"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} looks more like {0:f} again.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Charm Person"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} regains {0:s} free will.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Chill Touch"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} looks warmer.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Curse"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Detect Evil"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Detect Good"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Detect Hidden"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Detect Invis"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Detect Magic"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Faerie Fire"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N}'s outline fades.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Fly"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} falls to the ground!", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Frenzy"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} no longer looks so wild.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Giant Strength"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} no longer looks so mighty.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Haste"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer moving so quickly.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Infravision"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Invis"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} fades into existance.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Mass invis"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} fades into existance.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Pass Door"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Protection Evil"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Protection Good"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Sanctuary"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "The white aura around {0:n}'s body vanishes.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Shield"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "The shield protecting {0:n} vanishes.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Sleep"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, this["Slow"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer moving so slowly.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Stone Skin"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N}'s skin regains its normal texture.", victim);
                found = true;
            }
            if (TryDispel(level, victim, this["Weaken"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} looks stronger.", victim);
                found = true;
            }
            return found;
        }

        public enum CheckDispelReturnValues
        {
            NotFound,
            Dispelled,
            FoundAndNotDispelled
        }
        public CheckDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, IAbility ability) // was called check_dispel in Rom24
        {
            bool found = false;
            foreach (IAura aura in victim.Auras.Where(x => x.Ability == ability)) // no need to clone because at most one entry will be removed
            {
                if (!SavesDispel(dispelLevel, aura.Level, aura.PulseLeft))
                {
                    // TODO: if a wears off message on spell, display it
                    //if (skill_table[sn].msg_off)
                    //{
                    //    send_to_char(skill_table[sn].msg_off, victim);
                    //    send_to_char("\n\r", victim);
                    //}
                    victim.RemoveAura(aura, true);
                    return CheckDispelReturnValues.Dispelled;
                }
                else
                    aura.DecreaseLevel();
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }

        public bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft)
        {
            if (pulseLeft == -1) // very hard to dispel permanent effects
                spellLevel += 5;

            int save = 50 + (spellLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        #endregion
    }
}