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
using Mud.Server.Input;
using Mud.Server.Blueprints.Character;

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

        [Spell(2, "Armor", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You feel less armored.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellArmor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Armor") != null)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} is already armored.", victim);
                return;
            }
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add });
            victim.Send("You feel someone protecting you.");
            if (victim != caster)
                caster.Act(ActOptions.ToCharacter, "{0} is protected by your magic.", victim);
        }

        [Spell(3, "Bless", AbilityTargets.ItemInventoryOrCharacterDefensive, CharacterWearOffMessage = "You feel less righteous.", ItemWearOffMessage = "{0}'s holy aura fades.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellBless(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
                {
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already blessed.", item);
                    return;
                }
                if (item.ItemFlags.HasFlag(ItemFlags.Evil))
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
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(6 + level), AuraFlags.None, true,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = level / 8, Operator = AffectOperators.Add },
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -level / 8, Operator = AffectOperators.Add });
                return;
            }
            Wiznet.Wiznet($"SpellBless: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        [Spell(4, "Blindness", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You can see again.", DispelRoomMessage = "{0:N} is no longer blinded.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Blind) || victim.SavesSpell(level, SchoolTypes.None))
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

        [Spell(6, "Call Lightning", AbilityTargets.None, DamageNoun = "lightning bolt")]
        public void SpellCallLightning(IAbility ability, int level, ICharacter caster)
        {
            if (caster.Room == null)
                return;
            if (caster.Room.RoomFlags.HasFlag(RoomFlags.Indoors))
            {
                caster.Send("You must be out of doors.");
                return;
            }

            if (TimeManager.SkyState < SkyStates.Raining)
            {
                caster.Send("You need bad weather.");
                return;
            }

            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
            int damage = RandomManager.Dice(level / 2, 8);
            caster.Send("Mota's lightning strikes your foes!");
            caster.Act(ActOptions.ToRoom, "{0:N} calls Mota's lightning to strike {0:s} foes!", caster);
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.Where(x => x != caster).ToList()); // clone because damage could kill and remove character from list
            foreach (ICharacter victim in clone)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
                if (npcCaster != null ? npcVictim == null : npcVictim != null) // NPC on PC and PC on NPC
                {
                    if (victim.SavesSpell(level, SchoolTypes.Lightning))
                        victim.AbilityDamage(caster, ability, damage / 2, SchoolTypes.Lightning, true);
                    else
                        victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                }
            }
            // Inform in area about it
            foreach(ICharacter character in caster.Room.Area.Characters.Where(x => x.Position > Positions.Sleeping && !x.Room.RoomFlags.HasFlag(RoomFlags.Indoors)))
                character.Send("Lightning flashes in the sky.");
        }

        [Spell(7, "Calm", AbilityTargets.None, CharacterWearOffMessage = "You have lost your peace of mind.", DispelRoomMessage = "{0:N} no longer looks so peaceful...", Flags = AbilityFlags.CanBeDispelled)]
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
            if (caster is IPlayableCharacter pcCaster && pcCaster.IsImmortal)
                sumLevel = 0;
            // Harder to stop large fights
            if (RandomManager.Range(0, chance) < sumLevel)
                return;
            //
            foreach (ICharacter victim in caster.Room.People)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;

                // IsNpc, immune magic or undead
                if (npcVictim != null && (npcVictim.Immunities.HasFlag(IRVFlags.Magic) || npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
                    continue;

                // Is affected by berserk, calm or frenzy
                if (victim.CharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.CharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Frenzy") != null)
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
            if ((caster is IPlayableCharacter && victim is INonPlayableCharacter npcVictim && !caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcVictim.Master == caster)
                || (caster is INonPlayableCharacter && victim is IPlayableCharacter))
            {
                caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = TryDispels(level+2, victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                caster.Send("Ok.");
            else
                caster.Send("Spell failed.");
        }

        [Spell(9, "Cause Critical", AbilityTargets.CharacterOffensive, DamageNoun = "spell")]
        public void SpellCauseCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(3, 8) + level - 6;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(10, "Cause Light", AbilityTargets.CharacterOffensive, DamageNoun = "spell")]
        public void SpellCauseLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(1, 8) + level / 3;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(11, "Cause Serious", AbilityTargets.CharacterOffensive, DamageNoun = "spell")]
        public void SpellCauseSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(2, 8) + level / 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(12, "Chain Lightning", AbilityTargets.CharacterOffensive, DamageNoun = "lightning")]
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
                    target.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
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
                    if (caster.SavesSpell(level, SchoolTypes.Lightning))
                        damage /= 3;
                    caster.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                    level -= 4; // decrement damage
                    lastVictim = caster;
                }
            }
        }

        [Spell(13, "Change Sex", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "Your body feels familiar again.", DispelRoomMessage = "{0:N} looks more like {0:f} again.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellChangeSex(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Change Sex") != null)
            {
                if (victim == caster)
                    caster.Send("You've already been changed.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} has already had {0:s} sex changed.", victim);
                return;
            }

            if (victim.SavesSpell(level, SchoolTypes.Other))
                return;

            Sex newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != victim.Sex));
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(2 * level), AuraFlags.None, true,
                new CharacterSexAffect { Value = newSex });
            victim.Send("You feel different.");
            victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like {0:m}self anymore...", victim);
        }

        [Spell(14, "Charm Person", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You feel more self-confident.", DispelRoomMessage = "{0:N} regains {0:s} free will.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellCharmPerson(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.IsSafe(caster))
                return;

            if (caster == victim)
            {
                caster.Send("You like yourself even better!");
                return;
            }

            IPlayableCharacter pcCaster = caster as IPlayableCharacter;
            if (pcCaster == null)
            {
                caster.Send("You can't charm!");
                return;
            }

            INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
            if (npcVictim == null)
            {
                caster.Send("You can't charm players!");
                return;
            }

            if (npcVictim.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || caster.CharacterFlags.HasFlag(CharacterFlags.Charm)
                || level < npcVictim.Level
                || npcVictim.Immunities.HasFlag(IRVFlags.Charm)
                || npcVictim.SavesSpell(level, SchoolTypes.Charm))
                return;

            if (npcVictim.Room.RoomFlags.HasFlag(RoomFlags.Law))
            {
                caster.Send("The mayor does not allow charming in the city limits.");
                return;
            }

            npcVictim.Master?.RemoveFollower(npcVictim);
            pcCaster.AddFollower(npcVictim);
            npcVictim.ChangeMaster(pcCaster);

            int duration = RandomManager.Fuzzy(level / 4);
            World.AddAura(npcVictim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Charm, Operator = AffectOperators.Or });

            npcVictim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", caster);
            if (caster != npcVictim)
                caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", npcVictim);
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
        [Spell(15, "Chill Touch", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You feel less cold.", DispelRoomMessage = "{0:N} looks warmer.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellChillTouch(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            var result = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Cold, ChillTouchDamageTable);
            if (!result.savesSpell && result.damageResult == DamageResults.Damaged)
            {
                victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", victim);
                IAura existingAura = victim.GetAura(ability);
                if (existingAura != null)
                {
                    existingAura.Update(level, TimeSpan.FromMinutes(6));
                    existingAura.AddOrUpdateAffect(
                        x => x.Location == CharacterAttributeAffectLocations.Strength,
                        () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                        x => x.Modifier -= 1);
                }
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
            var result = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Light, ColourSprayDamageTable);
            if (!result.savesSpell && result.damageResult == DamageResults.Damaged)
                SpellBlindness(this["Blindness"], level / 2, caster, victim);
        }

        [Spell(17, "Continual Light", AbilityTargets.OptionalItemInventory)]
        public void SpellContinualLight(IAbility ability, int level, ICharacter caster, IItem item)
        {
            // item
            if (item != null)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.Glowing))
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
        public void SpellControlWeather(IAbility ability, int level, ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            if (StringCompareHelpers.StringEquals(rawParameters, "better"))
                TimeManager.ChangePressure(RandomManager.Dice(level / 3, 4));
            else if (StringCompareHelpers.StringEquals(rawParameters, "worse"))
                TimeManager.ChangePressure(-RandomManager.Dice(level / 3, 4));
            else
                caster.Send("Do you want it to get better or worse?");
            caster.Send("Ok");
        }

        [Spell(19, "Create Food", AbilityTargets.None)]
        public void SpellCreateFood(IAbility ability, int level, ICharacter caster)
        {
            IItemFood mushroom = World.AddItem(Guid.NewGuid(), Settings.MushroomBlueprintId, caster.Room) as IItemFood;
            mushroom?.SetHours(level / 2, level);
            caster.Act(ActOptions.ToAll, "{0} suddenly appears.", mushroom);
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
            IItemFountain fountain = World.AddItem(Guid.NewGuid(), Settings.SpringBlueprintId, caster.Room) as IItemFountain;
            int duration = level;
            fountain?.SetTimer(TimeSpan.FromMinutes(duration));
            caster.Act(ActOptions.ToAll, "{0} flows from the ground.", fountain);
        }

        [Spell(22, "Create Water", AbilityTargets.ItemInventory)]
        public void SpellCreateWater(IAbility ability, int level, ICharacter caster, IItem target)
        {
            IItemDrinkContainer drinkContainer = target as IItemDrinkContainer;
            if (drinkContainer == null)
            {
                caster.Send("It is unable to hold water.");
                return;
            }
            if (drinkContainer.LiquidName != "water" && !drinkContainer.IsEmpty)
            {
                caster.Send("It contains some other liquid.");
                return;
            }

            int multiplier = TimeManager.SkyState == SkyStates.Raining
                ? 4
                : 2;
            int water = Math.Min(level * multiplier, drinkContainer.MaxLiquid - drinkContainer.LiquidLeft);
            if (water > 0)
            {
                drinkContainer.Fill("water", water);
                caster.Act(ActOptions.ToCharacter, "{0:N} is filled.", drinkContainer);
            }
        }

        [Spell(23, "Cure Blindness", AbilityTargets.CharacterDefensive)]
        public void SpellCureBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Blindness", level, caster, victim, "You aren't blind.", "{0:N} doesn't appear to be blinded.");
        }

        [Spell(24, "Cure Critical", AbilityTargets.CharacterDefensive)]
        public void SpellCureCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(3, 8) + level - 6;
            victim.UpdateHitPoints(heal);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(25, "Cure Disease", AbilityTargets.CharacterDefensive)]
        public void SpellCureDisease(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Plague", level, caster, victim, "You aren't ill.", "{0:N} doesn't appear to be diseased.");
        }

        [Spell(26, "Cure Light", AbilityTargets.CharacterDefensive)]
        public void SpellCureLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(1, 8) + level / 3;
            victim.UpdateHitPoints(heal);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(27, "Cure Poison", AbilityTargets.CharacterDefensive)]
        public void SpellCurePoison(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Poison", level, caster, victim, "You aren't poisoned.", "{0:N} doesn't appear to be poisoned.");
        }

        [Spell(28, "Cure Serious", AbilityTargets.CharacterDefensive)]
        public void SpellCureSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(2, 8) + level / 2;
            victim.UpdateHitPoints(heal);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        [Spell(29, "Curse", AbilityTargets.ItemHereOrCharacterOffensive, CharacterWearOffMessage = "The curse wears off.", ItemWearOffMessage = "{0} is no longer impure.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.Evil))
                {
                    caster.Act(ActOptions.ToCharacter, "{0} is already filled with evil.", item);
                    return;
                }
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
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
                if (curseAura != null || victim.CharacterFlags.HasFlag(CharacterFlags.Curse) || victim.SavesSpell(level, SchoolTypes.Negative))
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
            Wiznet.Wiznet($"SpellCurse: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        [Spell(30, "Demonfire", AbilityTargets.CharacterOffensive, DamageNoun = "torments")]
        public void SpellDemonfire(IAbility ability, int level, ICharacter caster, ICharacter victim) 
        {
            if (caster is IPlayableCharacter && !caster.IsEvil)
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
            DamageResults damageResult = victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
            if (damageResult == DamageResults.Damaged)
                SpellCurse(this["Curse"], 3*level/4, caster, victim);
        }

        [Spell(31, "Detect Evil", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "The red in your vision disappears.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellDetectEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectEvil, level, "You can already sense evil.", "{0:N} can already detect evil.", "Your eyes tingle.", "Ok.");
        }

        [Spell(32, "Detect Good", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "The gold in your vision disappears.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellDetectGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectGood, level, "You can already sense good.", "{0:N} can already detect good.", "Your eyes tingle.", "Ok.");
        }

        [Spell(33, "Detect Hidden", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "You feel less aware of your surroundings.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellDetectHidden(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectHidden, level, "You are already as alert as you can be.", "{0:N} can already sense hidden lifeforms.", "Your awareness improves.", "Ok.");
        }

        [Spell(34, "Detect Invis", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "You no longer see invisible objects.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellDetectInvis(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectInvis, level, "You can already see invisible.", "{0:N} can already see invisible things.", "Your eyes tingle.", "Ok.");
        }

        [Spell(35, "Detect Magic", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "The detect magic wears off.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellDetectMagic(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectMagic, level, "You can already sense magical auras.", "{0:N} can already detect magic.", "Your eyes tingle.", "Ok.");
        }

        [Spell(36, "Detect Poison", AbilityTargets.ItemInventory)]
        public void SpellDetectPoison(IAbility ability, int level, ICharacter caster, IItem item)
        {
            if (item is IItemPoisonable poisonable)
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (poisonable.IsPoisoned)
                    caster.Send("You smell poisonous fumes.");
                else
                    caster.Send("It looks delicious.");
            }
            else
                caster.Send("It doesn't look poisoned.");
        }

        [Spell(37, "Dispel Evil", AbilityTargets.CharacterOffensive)]
        public void SpellDispelEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (caster is IPlayableCharacter && caster.IsEvil)
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
            if (caster is IPlayableCharacter && caster.IsGood)
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
        public void SpellDispelMagic(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.SavesSpell(level, SchoolTypes.Other))
            {
                victim.Send("You feel a brief tingling sensation.");
                caster.Send("You failed.");
                return;
            }

            bool found = TryDispels(level, victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
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
                int damage = victim.CharacterFlags.HasFlag(CharacterFlags.Flying)
                    ? 0 // no damage but starts fight
                    : level + RandomManager.Dice(2, 8);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Bash, true);
            }
        }

        [Spell(41, "Enchant Armor", AbilityTargets.ArmorInventory, PulseWaitTime = 24)]
        public void SpellEnchantArmor(IAbility ability, int level, ICharacter caster, IItemArmor armor)
        {
            //if (item.EquippedBy == null)
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
            if (armor.ItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (armor.ItemFlags.HasFlag(ItemFlags.Glowing))
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

        [Spell(42, "Enchant Weapon", AbilityTargets.WeaponInventory, PulseWaitTime = 24)]
        public void SpellEnchantWeapon(IAbility ability, int level, ICharacter caster, IItemWeapon weapon)
        {
            //if (weapon.EquippedBy == null)
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
            if (weapon.ItemFlags.HasFlag(ItemFlags.Bless))
                fail -= 15;
            if (weapon.ItemFlags.HasFlag(ItemFlags.Glowing))
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
                weapon.AddBaseItemFlags(ItemFlags.Glowing); // permanently change item flags
                amount = 2;
            }
            weapon.IncreaseLevel();
            weapon.AddBaseItemFlags(ItemFlags.Magic); // permanently change item flags
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
                if (victim is IPlayableCharacter pcVictim)
                {
                    int lose = RandomManager.Range(level / 2, 3 * level / 2);
                    pcVictim.GainExperience(-lose);
                }
                victim.UpdateResource(ResourceKinds.Mana, -victim[ResourceKinds.Mana] / 2); // half mana
                victim.UpdateMovePoints(-victim.MovePoints / 2); // half move
                caster.UpdateHitPoints(damage);
            }

            victim.Send("You feel your life slipping away!");
            caster.Send("Wow....what a rush!");

            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
        }

        [Spell(44, "Faerie Fire", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "The pink aura around you fades away.", DispelRoomMessage = "{0:N}'s outline fades.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellFaerieFire(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.FaerieFire))
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

            IAbility invis = this["Invisiblity"];
            IAbility massInvis = this["Mass Invis"];
            IAbility sneak = this["Sneak"];
            foreach (ICharacter victim in caster.Room.People.Where(x => x != caster && !x.SavesSpell(level, SchoolTypes.Other))) // && ich->invis_level <= 0
            {
                victim.RemoveAuras(x => x.Ability == invis || x.Ability == massInvis || x.Ability == sneak, false);
                if (victim is INonPlayableCharacter)
                    victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak);
                victim.Recompute();
                victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
            }
        }

        [Spell(46, "Farsight", AbilityTargets.None)]
        public void SpellFarsight(IAbility ability, int level, ICharacter caster)
        {
            // TODO: see Rot -> give an affect which is used in scan command
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

        [Spell(48, "Fireproof", AbilityTargets.ItemInventory, ItemWearOffMessage = "{0}'s protective aura fades.")]
        public void SpellFireproof(IAbility ability, int level, ICharacter caster, IItem item)
        {
            if (item.ItemFlags.HasFlag(ItemFlags.BurnProof))
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

        [Spell(50, "Fly", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You slowly float to the ground.", DispelRoomMessage = "{0:N} falls to the ground!", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 18)]
        public void SpellFly(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Flying))
            {
                if (victim == caster)
                    caster.Send("You are already airborne.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} doesn't need your help to fly.", victim);
                return;
            }
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level + 3), AuraFlags.None, true,
                new CharacterFlagsAffect { Modifier = CharacterFlags.Flying, Operator = AffectOperators.Or });
            caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", victim);
        }

        [Spell(51, "Floating Disc", AbilityTargets.None, PulseWaitTime = 24)]
        public void SpellFloatingDisc(IAbility ability, int level, ICharacter caster)
        {
            // TODO: using data is kindy hacky to perform a custom level item
            IItem item = World.AddItem(Guid.NewGuid(), Settings.FloatingDiscBlueprintId, caster);
            if (!(item is IItemContainer floatingDisc))
            {
                caster.Send("Somehing went wrong.");
                Wiznet.Wiznet($"SpellFloatingDisc: blueprint {Settings.FloatingDiscBlueprintId} is not a container", WiznetFlags.Bugs, AdminLevels.Implementor);
                World.RemoveItem(item); // destroy it if invalid
                return;
            }
            int maxWeight = level * 10;
            int maxWeightPerItem = level * 5;
            int duration = level * 2 - RandomManager.Range(0, level / 2);
            floatingDisc.SetTimer(TimeSpan.FromMinutes(duration));
            floatingDisc.SetCustomValues(level, maxWeight, maxWeightPerItem);

            caster.Act(ActOptions.ToGroup, "{0} has created a floating black disc.", caster);
            caster.Send("You create a floating disc.");
            // TODO: Try to equip it ?
        }

        [Spell(52, "Frenzy", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "Your rage ebbs.", DispelRoomMessage = "{0:N} no longer looks so wild.", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 24)]
        public void SpellFrenzy(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.GetAura("Frenzy") != null)
            {
                if (victim == caster)
                    caster.Send("You are already in a frenzy.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} doesn't look like $e wants to fight anymore.", victim);
                return;
            }

            if (victim.CharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Calm") != null)
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
            if (!IsValidGateTarget(caster, victim, level))
            {
                caster.Send("You failed.");
                return;
            }

            caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", caster);
            caster.ChangeRoom(victim.Room);
            caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", caster);
            caster.AutoLook();

            // pets follows
            if (caster is IPlayableCharacter pcCaster)
            {
                foreach (INonPlayableCharacter pet in pcCaster.Pets)
                {
                    pet.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", pet);
                    pet.ChangeRoom(victim.Room);
                    pet.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", pet);
                    pet.AutoLook();
                }
            }
        }

        [Spell(54, "Giant Strength", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You feel weaker.", DispelRoomMessage = "{0:N} no longer looks so mighty.", Flags = AbilityFlags.CanBeDispelled)]
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

        [Spell(55, "Harm", AbilityTargets.CharacterOffensive, DamageNoun = "harm spell")]
        public void SpellHarm(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = Math.Max(20, victim.HitPoints - RandomManager.Dice(1, 4));
            if (victim.SavesSpell(level, SchoolTypes.Harm))
                damage = Math.Min(50, damage / 2);
            damage = Math.Min(100, damage);
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        [Spell(56, "Haste", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You feel yourself slow down.", DispelRoomMessage = "{0:N} is no longer moving so quickly.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellHaste(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Haste)
                || victim.GetAura("Haste") != null
                || (victim is INonPlayableCharacter npcVictim && npcVictim.OffensiveFlags.HasFlag(OffensiveFlags.Fast)))
            {
                if (victim == caster)
                    caster.Send("You can't move any faster!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", victim);
                return;
            }
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Slow))
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
            victim.UpdateHitPoints(100);
            victim.Send("A warm feeling fills your body.");
            if (caster != victim)
                caster.Send("Ok.");
        }

        [Spell(58, "Heat Metal", AbilityTargets.CharacterOffensive, PulseWaitTime = 18)]
        public void SpellHeatMetal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool fail = true;
            int damage = 0;
            if (!victim.SavesSpell(level + 2, SchoolTypes.Fire) && !victim.Immunities.HasFlag(IRVFlags.Fire))
            {
                bool recompute = false;
                // Check equipments
                foreach (EquippedItem equippedItem in victim.Equipments.Where(x => x.Item != null))
                {
                    IItem item = equippedItem.Item;
                    if (!item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                        && !item.ItemFlags.HasFlag(ItemFlags.NonMetal)
                        && RandomManager.Range(1, 2 * level) > item.Level
                        && !victim.SavesSpell(level, SchoolTypes.Fire))
                    {
                        switch (item)
                        {
                            case IItemArmor itemArmor:
                                if (!itemArmor.ItemFlags.HasFlag(ItemFlags.NoDrop) // remove the item
                                    && !itemArmor.ItemFlags.HasFlag(ItemFlags.NoRemove)
                                    && itemArmor.Weight / 10 < RandomManager.Range(1, 2 * victim[CharacterAttributes.Dexterity]))
                                {
                                    itemArmor.ChangeEquippedBy(null, false);
                                    itemArmor.ChangeContainer(victim.Room);
                                    victim.Act(ActOptions.ToRoom, "{0:N} yelps and throws {1} to the ground!", victim, itemArmor);
                                    victim.Act(ActOptions.ToCharacter, "You remove and drop {0} before it burns you.", itemArmor);
                                    damage += RandomManager.Range(1, itemArmor.Level) / 3;
                                    fail = false;
                                    recompute = true;
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
                                    if (!itemWeapon.ItemFlags.HasFlag(ItemFlags.NoDrop) // remove the item
                                        && !itemWeapon.ItemFlags.HasFlag(ItemFlags.NoRemove))
                                    {
                                        itemWeapon.ChangeEquippedBy(null, false);
                                        itemWeapon.ChangeContainer(victim.Room);
                                        victim.Act(ActOptions.ToRoom, "{0:N} is burned by {1}, and throws it to the ground.", victim, itemWeapon);
                                        victim.Send("You throw your red-hot weapon to the ground!");
                                        damage += 1;
                                        fail = false;
                                        recompute = true;
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
                    if (!item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                        && !item.ItemFlags.HasFlag(ItemFlags.NonMetal)
                        && RandomManager.Range(1, 2 * level) > item.Level
                        && !victim.SavesSpell(level, SchoolTypes.Fire))
                    {
                        switch (item)
                        {
                            case IItemArmor itemArmor:
                                if (!itemArmor.ItemFlags.HasFlag(ItemFlags.NoDrop)) // drop it if we can
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
                                if (!itemWeapon.ItemFlags.HasFlag(ItemFlags.NoDrop)) // drop it if we can
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
                if (recompute)
                    victim.Recompute();
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

        [Spell(59, "Holy Word", AbilityTargets.None, PulseWaitTime = 24, DamageNoun = "divine wrath")]
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
            caster.UpdateHitPoints(-caster.HitPoints/2);
        }

        [Spell(60, "Identify", AbilityTargets.ItemInventory, PulseWaitTime = 24)]
        public void SpellIdentify(IAbility ability, int level, ICharacter caster, IItem item)
        {
            caster.Send(StringHelpers.NotYetImplemented);
        }

        [Spell(61, "Infravision", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You no longer see in the dark.", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 18)]
        public void SpellInfravision(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Infrared, 2*level, "You can already see in the dark", "{0} already has infravision.", "Your eyes glow red.", "{0:P} eyes glow red.");
        }

        [Spell(62, "Invisibility", AbilityTargets.ItemInventoryOrCharacterDefensive, CharacterWearOffMessage = "You are no longer invisible.", ItemWearOffMessage = "{0} fades into view.", DispelRoomMessage = "{0:N} fades into existance.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellInvisibility(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            if (target is IItem item)
            {
                if (item.ItemFlags.HasFlag(ItemFlags.Invis))
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
                if (victim.CharacterFlags.HasFlag(CharacterFlags.Invisible))
                    return;

                victim.Act(ActOptions.ToAll, "{0:N} fade{0:v} out of existence.", victim);
                World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(level + 12), AuraFlags.None, true,
                    new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
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
            else if (ap > -350) msg = "{0:N} lies to {0:s} friends.";
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

        [Spell(65, "Locate Object", AbilityTargets.Custom, PulseWaitTime = 18)]
        public void SpellLocateObject(IAbility ability, int level, ICharacter caster, string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            int maxFound = (caster as IPlayableCharacter)?.IsImmortal == true
                ? 200
                : level * 2;
            int number = 0;
            IEnumerable<IItem> foundItems = FindHelpers.FindAllByName(World.Items.Where(x => caster.CanSee(x) && !x.ItemFlags.HasFlag(ItemFlags.NoLocate) && x.Level <= caster.Level && RandomManager.Range(1,100) <= 2*level), rawParameters);
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
                {
                    if ((caster as IPlayableCharacter)?.IsImmortal == true)
                        sb.AppendFormatLine("One is in {0} (room {1})", room.DisplayName, room.Blueprint?.Id.ToString() ?? "???");
                    else
                        sb.AppendFormatLine("One is in {0}", room.DisplayName);
                }
                else if (item.ContainedInto is ICharacter character && caster.CanSee(character))
                    sb.AppendFormatLine("One is carried by {0}", character.DisplayName);
                else if (item.EquippedBy != null && caster.CanSee(item.EquippedBy))
                    sb.AppendFormatLine("One is carried by {0}", item.EquippedBy.DisplayName);

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

        [Spell(67, "Mass Healing", AbilityTargets.None, PulseWaitTime = 36)]
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

        [Spell(68, "Mass Invis", AbilityTargets.None, CharacterWearOffMessage = "You are no longer invisible.", DispelRoomMessage = "{0:N} fades into existance.", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 24)]
        public void SpellMassInvis(IAbility ability, int level, ICharacter caster)
        {
            bool IsSameGroupOrPet(ICharacter ch1, ICharacter ch2)
            {
                IPlayableCharacter pcCh1 = ch1 as IPlayableCharacter;
                IPlayableCharacter pcCh2 = ch2 as IPlayableCharacter;
                return (pcCh1 != null && pcCh1.IsSameGroupOrPet(ch2)) || (pcCh2 != null && pcCh2.IsSameGroupOrPet(ch1));
            }

            foreach (ICharacter victim in caster.Room.People)
            {
                if (IsSameGroupOrPet(caster, victim))
                {
                    victim.Act(ActOptions.ToAll, "{0:N} slowly fade{0:v} out of existence.", victim);

                    World.AddAura(victim, ability, caster, level/2, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
                }
            }
            caster.Send("Ok.");
        }

        [Spell(69, "Nexus", AbilityTargets.CharacterWorldwide, PulseWaitTime = 36)]
        public void SpellNexus(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (!IsValidGateTarget(caster, victim, level))
            {
                caster.Send("You failed.");
                return;
            }

            // search warpstone
            IItemWarpstone stone = caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
            if (stone == null && (caster as IPlayableCharacter)?.IsImmortal != true)
            {
                caster.Send("You lack the proper component for this spell.");
                return;
            }

            // destroy warpstone
            if (stone != null)
            {
                caster.Act(ActOptions.ToCharacter, "You draw upon the power of {0}.", stone);
                caster.Act(ActOptions.ToCharacter, "It flares brightly and vanishes!");
                World.RemoveItem(stone);
            }

            int duration = 1 + level / 10;

            // create portal one (caster -> victim)
            IItemPortal portal1 = World.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, caster.Room) as IItemPortal;
            if (portal1 != null)
            {
                portal1.SetTimer(TimeSpan.FromMinutes(duration));
                portal1.ChangeDestination(victim.Room);
                portal1.SetCharge(1, 1);

                caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal1);
                caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal1);
            }

            if (caster.Room == victim.Room)
                return; // no second portal if rooms are the same

            // create portal two (victim -> caster)
            IItemPortal portal2 = World.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, victim.Room) as IItemPortal;
            if (portal2 != null)
            {
                portal2.SetTimer(TimeSpan.FromMinutes(duration));
                portal2.ChangeDestination(caster.Room);
                portal2.SetCharge(1, 1);

                victim.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal2);
                victim.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal2);
            }
        }

        [Spell(70, "Pass Door", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "You feel solid again.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellPassDoor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int duration = RandomManager.Fuzzy(level / 4);
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.PassDoor, duration, "You are already out of phase.", "{0:N} is already shifted out of phase.", "You turn translucent.", "{0} turns translucent.");
        }

        [Spell(71, "Plague", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "Your sores vanish.", DamageNoun = "sickness", Flags = AbilityFlags.CanBeDispelled)]
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
                new CharacterFlagsAffect { Modifier = CharacterFlags.Plague, Operator = AffectOperators.Or },
                new PlagueSpreadAndDamageAffect());
            victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", victim);
        }

        [Spell(72, "Poison", AbilityTargets.ItemHereOrCharacterOffensive, CharacterWearOffMessage = "You feel less sick.", ItemWearOffMessage = "The poison on {0} dries up.")]
        public void SpellPoison(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                // food/drink container
                if (item is IItemPoisonable poisonable)
                {
                    if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless) || poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof))
                    {
                        caster.Act(ActOptions.ToCharacter, "Your spell fails to corrupt {0}.", poisonable);
                        return;
                    }
                    poisonable.Poison();
                    caster.Act(ActOptions.ToCharacter, "{0} is infused with poisonous vapors.", poisonable);
                    return;
                }
                // weapon
                if (item is IItemWeapon weapon)
                {
                    if (weapon.WeaponFlags == WeaponFlags.Poison)
                    {
                        caster.Act(ActOptions.ToCharacter, "{0} is already envenomed.", weapon);
                        return;
                    }
                    if (weapon.WeaponFlags != WeaponFlags.None)
                    {
                        caster.Act(ActOptions.ToCharacter, "You can't seem to envenom {0}.", weapon);
                        return;
                    }
                    int duration = level / 8;
                    World.AddAura(weapon, ability, caster, level / 2, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                        new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Poison, Operator = AffectOperators.Or});
                    caster.Act(ActOptions.ToCharacter, "{0} is coated with deadly venom.", weapon);
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
                IAura poisonAura = victim.GetAura(ability);
                if (poisonAura != null)
                    poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                else
                    World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                        new PoisonDamageAffect());
                victim.Send("You feel very sick.");
                victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);
            }
        }

        [Spell(73, "Portal", AbilityTargets.CharacterWorldwide, PulseWaitTime = 24)]
        public void SpellPortal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (!IsValidGateTarget(caster, victim, level))
            {
                caster.Send("You failed.");
                return;
            }

            // search warpstone
            IItemWarpstone stone = caster.GetEquipment(EquipmentSlots.OffHand) as IItemWarpstone;
            if (stone == null && (caster as IPlayableCharacter)?.IsImmortal != true)
            {
                caster.Send("You lack the proper component for this spell.");
                return;
            }

            // destroy warpsone
            if (stone != null)
            {
                caster.Act(ActOptions.ToCharacter, "You draw upon the power of {0}.", stone);
                caster.Act(ActOptions.ToCharacter, "It flares brightly and vanishes!");
                World.RemoveItem(stone);
            }

            // create portal
            IItemPortal portal = World.AddItem(Guid.NewGuid(), Settings.PortalBlueprintId, caster.Room) as IItemPortal;
            if (portal != null)
            {
                int duration = 2 + level / 25;
                portal.SetTimer(TimeSpan.FromMinutes(duration));
                portal.ChangeDestination(victim.Room);
                portal.SetCharge(1 + level / 25, 1 + level / 25);

                caster.Act(ActOptions.ToCharacter, "{0:N} rises up before you.", portal);
                caster.Act(ActOptions.ToRoom, "{0:N} rises up from the ground.", portal);
            }
        }

        [Spell(74, "Protection Evil", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "You feel less protected.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellProtectionEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
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

        [Spell(75, "Protection Good", AbilityTargets.CharacterSelf, CharacterWearOffMessage = "You feel less protected.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellProtectionGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
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
                caster.Act(ActOptions.ToAll, "{0:N} raise{0:v} {0:s} hand, and a blinding ray of light shoots forth!", caster);

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

        [Spell(77, "Recharge", AbilityTargets.ItemInventory, PulseWaitTime = 24)]
        public void SpellRecharge(IAbility ability, int level, ICharacter caster, IItem item)
        {
            IItemCastSpellsCharge chargeable = item as IItemCastSpellsCharge;
            if (chargeable == null)
            {
                caster.Send("That item does not carry charges.");
                return;
            }

            if (chargeable.SpellLevel >= (3 * level) / 2)
            {
                caster.Send("Your skills are not great enough for that");
                return;
            }

            if (chargeable.AlreadyRecharged)
            {
                caster.Send("That item has already been recharged once.");
                return;
            }

            int chance = 40 + 2 * level;
            chance -= chargeable.SpellLevel;
            chance -= (chargeable.MaxChargeCount - chargeable.CurrentChargeCount) * (chargeable.MaxChargeCount - chargeable.CurrentChargeCount);
            chance = Math.Max(level / 2, chance);
            int percent = RandomManager.Range(1, 100);

            if (percent < chance / 2)
            {
                caster.Act(ActOptions.ToAll, "{0:N} glows softly.", chargeable);
                int current = Math.Max(chargeable.CurrentChargeCount, chargeable.MaxChargeCount);
                chargeable.Recharge(current, chargeable.MaxChargeCount);
                return;
            }

            if (percent <= chance)
            {
                caster.Act(ActOptions.ToAll, "{0:N} glows softly.", chargeable);
                int chargeMax = chargeable.MaxChargeCount - chargeable.CurrentChargeCount;
                int chargeBack = chargeMax > 0
                    ? Math.Max(1, (chargeMax * percent) / 100)
                    : 0;
                chargeable.Recharge(chargeable.CurrentChargeCount+chargeBack, chargeable.MaxChargeCount);
                return;
            }

            if (percent <= Math.Min(95, (3 * chance) / 2))
            {
                caster.Send("Nothing seems to happen.");
                if (chargeable.MaxChargeCount > 1)
                    chargeable.Recharge(chargeable.CurrentChargeCount, chargeable.MaxChargeCount-1);
                return;
            }

            caster.Act(ActOptions.ToAll, "{0:N} glows brightly and explodes!", chargeable);
            World.RemoveItem(item);
        }

        [Spell(78, "Refresh", AbilityTargets.CharacterDefensive, PulseWaitTime = 18)]
        public void SpellRefresh(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            victim.UpdateMovePoints(level);
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (victim.MovePoints == victim.MaxMovePoints)
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
                if (item.ItemFlags.HasFlag(ItemFlags.NoDrop) || item.ItemFlags.HasFlag(ItemFlags.NoRemove))
                {
                    if (!item.ItemFlags.HasFlag(ItemFlags.NoUncurse) && !SavesDispel(level + 2, item.Level, 0))
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
                foreach (IItem carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).Where(x => (x.ItemFlags.HasFlag(ItemFlags.NoDrop) || x.ItemFlags.HasFlag(ItemFlags.NoRemove)) && !x.ItemFlags.HasFlag(ItemFlags.NoUncurse)))
                    if (!SavesDispel(level, carriedItem.Level, 0))
                    {
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoRemove);
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoDrop);
                        victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                        break;
                    }
                return;
            }
            Wiznet.Wiznet($"SpellRemoveCurse: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        [Spell(80, "Sanctuary", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "The white aura around your body fades.", DispelRoomMessage = "The white aura around {0:n}'s body vanishes.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellSanctuary(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int duration = level / 6;
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Sanctuary, duration, "You are already in sanctuary.", "{0:N} is already in sanctuary.", "You are surrounded by a white aura.", "{0:N} is surrounded by a white aura.");
        }

        [Spell(81, "Shield", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "Your force shield shimmers then fades away.", DispelRoomMessage = "The shield protecting {0:n} vanishes.", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 18)]
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

        [Spell(83, "Sleep", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You feel less tired.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellSleep(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Sleep)
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

        [Spell(84, "Slow", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You feel yourself speed up.", DispelRoomMessage = "{0:N} is no longer moving so slowly.", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellSlow(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Slow)
                || victim.GetAura(ability) != null)
            {
                if (victim == caster)
                    caster.Send("You can't move any slower!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} can't get any slower than that.", victim);
                return;
            }

            if (victim.Immunities.HasFlag(IRVFlags.Magic)
                || victim.SavesSpell(level, SchoolTypes.Other))
            {
                if (victim != caster)
                    caster.Send("Nothing seemed to happen.");
                victim.Send("You feel momentarily lethargic.");
                return;
            }

            if (victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
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

        [Spell(85, "Stone Skin", AbilityTargets.CharacterDefensive, CharacterWearOffMessage = "Your skin feels soft again.", DispelRoomMessage = "{0:N}'s skin regains its normal texture.", Flags = AbilityFlags.CanBeDispelled, PulseWaitTime = 18)]
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
                || caster.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || caster.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Private)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Solitary)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.ImpOnly)
                || npcVictim?.ActFlags.HasFlag(ActFlags.Aggressive) == true
                || victim.Level >= level+3
                || pcVictim?.IsImmortal == true
                || victim.Fighting != null
                || npcVictim?.Immunities.HasFlag(IRVFlags.Summon) == true
                || (npcVictim?.Blueprint is CharacterShopBlueprint) == true
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
                || victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || (victim != caster && victim.Immunities.HasFlag(IRVFlags.Summon))
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
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (character.SavesSpell(level, SchoolTypes.Other))
                    character.Send(phraseFail);
                else
                    character.Send(phraseSuccess);
            }
        }

        [Spell(89, "Weaken", AbilityTargets.CharacterOffensive, CharacterWearOffMessage = "You feel stronger.", DispelRoomMessage = "{0:N} looks stronger.", DamageNoun = "spell", Flags = AbilityFlags.CanBeDispelled)]
        public void SpellWeaken(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Weaken) || victim.GetAura(ability) != null || victim.SavesSpell(level, SchoolTypes.Other))
                return;

            int duration = level / 2;
            World.AddAura(victim, ability, caster, level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -level/5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Weaken, Operator = AffectOperators.Or });
            victim.Recompute();
            victim.Send("You feel your strength slip away.");
            victim.Act(ActOptions.ToRoom, "{0:N} looks tired and weak.", victim);
        }

        [Spell(90, "Word of recall", AbilityTargets.CharacterSelf)]
        public void SpellWordOfRecall(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            IPlayableCharacter pcVictim = victim as IPlayableCharacter;
            if (pcVictim == null)
                return;

            IRoom recallRoom = pcVictim.RecallRoom;
            if (recallRoom == null)
            {
                pcVictim.Send("You are completely lost.");
                Log.Default.WriteLine(LogLevels.Error, "No recall room found for {0}", pcVictim.ImpersonatedBy.DisplayName);
                return;
            }

            if (pcVictim.CharacterFlags.HasFlag(CharacterFlags.Curse)
                || pcVictim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall))
            {
                pcVictim.Send("Spell failed.");
                return;
            }

            if (pcVictim.Fighting != null)
                pcVictim.StopFighting(true);

            pcVictim.UpdateMovePoints(-pcVictim.MovePoints / 2); // half move
            pcVictim.Act(ActOptions.ToRoom, "{0:N} disappears", pcVictim);
            pcVictim.ChangeRoom(recallRoom);
            pcVictim.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcVictim);
            pcVictim.AutoLook();

            // Pets follows
            foreach (INonPlayableCharacter pet in pcVictim.Pets)
            {
                // no recursive call because DoRecall has been coded for IPlayableCharacter
                if (pet.CharacterFlags.HasFlag(CharacterFlags.Curse))
                    continue; // pet failing doesn't impact return value
                if (pet.Fighting != null)
                {
                    if (!RandomManager.Chance(80))
                        continue;// pet failing doesn't impact return value
                    pet.StopFighting(true);
                }

                pet.Act(ActOptions.ToRoom, "{0:N} disappears", pet);
                pet.ChangeRoom(recallRoom);
                pet.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pet);
                pet.AutoLook();
            }
        }

        // NPC Spells

        [Spell(500, "Acid breath", AbilityTargets.CharacterOffensive, PulseWaitTime = 24, DamageNoun = "blast of acid")]
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

        [Spell(501, "Fire breath", AbilityTargets.CharacterOffensive, PulseWaitTime = 24, DamageNoun = "blast of fire")]
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

        [Spell(502, "Frost breath", AbilityTargets.CharacterOffensive, PulseWaitTime = 24, DamageNoun = "blast of frost")]
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

        [Spell(503, "Gas breath", AbilityTargets.None, PulseWaitTime = 24, DamageNoun = "blast of gas")]
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

        [Spell(504, "Lightning breath", AbilityTargets.CharacterOffensive, PulseWaitTime = 24, DamageNoun = "blast of lightning")]
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

        // NPC spells for mega.are

        [Spell(600, "General Purpose", AbilityTargets.CharacterOffensive, DamageNoun = "general purpose ammo")]
        public void SpellGeneralPurpose(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Range(25, 100);
            if (victim.SavesSpell(level, SchoolTypes.Pierce))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Pierce, true);
        }

        [Spell(601, "High explosive", AbilityTargets.CharacterOffensive, DamageNoun = "high explosive ammo")]
        public void SpellHighExplosive(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Range(30, 120);
            if (victim.SavesSpell(level, SchoolTypes.Pierce))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Pierce, true);
        }

        #region Helpers

        private bool IsValidGateTarget(ICharacter caster, ICharacter victim, int level)
        {
            INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
            if (victim == caster
                || victim.Room == null
                || !caster.CanSee(victim.Room)
                || caster.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || caster.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Private)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.Solitary)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || victim.Room.RoomFlags.HasFlag(RoomFlags.ImpOnly)
                || victim.Level >= level + 3
                // TODO: clan check
                // TODO: hero level check 
                || (npcVictim != null && npcVictim.Immunities.HasFlag(IRVFlags.Summon))
                || (npcVictim != null && victim.SavesSpell(level, SchoolTypes.Other)))
                return false;
            return true;
        }

        private void BreathAreaEffect(ICharacter victim, IAbility ability, ICharacter caster, int level, int damage, SchoolTypes damageType, Action<IEntity, ICharacter, int, int> breathAction)
        {
            // Room content
            breathAction(caster.Room, caster, level, damage / 2);
            // Room people
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.Where(x => !x.IsSafeSpell(caster, true) && !(x is INonPlayableCharacter && caster is INonPlayableCharacter && (caster.Fighting != x || x.Fighting != caster))).ToList());
            foreach (ICharacter coVictim in clone)
            {
                if (victim == coVictim) // full damage
                {
                    if (coVictim.SavesSpell(level, damageType))
                    {
                        breathAction(coVictim, caster, level / 2, damage / 4);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                    else
                    {
                        breathAction(coVictim, caster, level, damage);
                        coVictim.AbilityDamage(caster, ability, damage, damageType, true);
                    }
                }
                else // partial damage
                {
                    if (coVictim.SavesSpell(level - 2, damageType))
                    {
                        breathAction(coVictim, caster, level / 4, damage / 8);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                    else
                    {
                        breathAction(coVictim, caster, level / 2, damage / 4);
                        coVictim.AbilityDamage(caster, ability, damage / 2, damageType, true);
                    }
                }
            }
        }

        private (bool savesSpell, DamageResults damageResult) TableBaseDamageSpell(IAbility ability, int level, ICharacter caster, ICharacter victim, SchoolTypes damageType, int[] table) // returns Rom24Common.SavesSpell result
        {
            int baseDamage = table.Get(level);
            int minDamage = baseDamage / 2;
            int maxDamage = baseDamage * 2;
            int damage = RandomManager.Range(minDamage, maxDamage);
            bool savesSpell = victim.SavesSpell(level, damageType);
            if (savesSpell)
                damage /= 2;
            DamageResults damageResult = victim.AbilityDamage(caster, ability, damage, damageType, true);
            return (savesSpell, damageResult);
        }

        private void GenericCharacterFlagsAbility(IAbility ability, int level, ICharacter caster, ICharacter victim, CharacterFlags characterFlags, int duration, string selfAlreadyAffected, string notSelfAlreadyAffected, string success, string notSelfSuccess)
        {
            if (victim.CharacterFlags.HasFlag(characterFlags))
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

        private void GenericSpellCureAbility(string toCureAbilityName, int level, ICharacter caster, ICharacter victim, string selfNotFound, string notSelfNotFound)
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
                    caster.Send("Spell failed.");
                    break;
            }
        }

        private bool TryDispels(int dispelLevel, ICharacter victim) // dispels every spells
        {
            bool found = false;
            IReadOnlyCollection<IAura> clone = new ReadOnlyCollection<IAura>(victim.Auras.Where(x => x.IsValid).ToList());
            foreach (IAura aura in clone)
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    found = true;
                    victim.RemoveAura(aura, false); // RemoveAura will display DispelMessage
                    if (aura.Ability != null && !string.IsNullOrWhiteSpace(aura.Ability.DispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, aura.Ability.DispelRoomMessage, victim);
                }
                else
                    aura.DecreaseLevel();
            }
            if (found)
                victim.Recompute();
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
                if (!SavesDispel(dispelLevel, aura))
                {
                    victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                    if (aura.Ability != null && !string.IsNullOrWhiteSpace(aura.Ability.DispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, aura.Ability.DispelRoomMessage, victim);
                    return CheckDispelReturnValues.Dispelled; // stop at first aura dispelled
                }
                else
                    aura.DecreaseLevel();
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }

        public bool SavesDispel(int dispelLevel, IAura aura)
        {
            if (aura.AuraFlags.HasFlag(AuraFlags.NoDispel))
                return true;
            int auraLevel = aura.Level;
            if (aura.AuraFlags.HasFlag(AuraFlags.Permanent)
                || aura.PulseLeft < 0) // very hard to dispel permanent effects
                auraLevel += 5;

            int save = 50 + (auraLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        public bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft)
        {
            if (pulseLeft < 0) // very hard to dispel permanent effects
                spellLevel += 5;

            int save = 50 + (spellLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        #endregion
    }
}
