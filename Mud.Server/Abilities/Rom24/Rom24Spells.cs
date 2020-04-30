using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Item;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.Server.Abilities.Rom24
{
    public class Rom24Spells
    {
        private ISettings Settings { get; }
        private IWorld World { get; }
        private IAbilityManager AbilityManager { get; }
        private IRandomManager RandomManager { get; }

        private Rom24Common Rom24Common { get; }
        private Rom24Effects Rom24Effects { get; }

        public Rom24Spells(ISettings settings, IWorld world, IAbilityManager abilityManager, IRandomManager randomManager)
        {
            Settings = settings;
            World = world;
            AbilityManager = abilityManager;
            RandomManager = randomManager;

            Rom24Common = new Rom24Common(randomManager);
            Rom24Effects = new Rom24Effects(world, abilityManager, randomManager);
        }

        public IAbility CreateDummyAbility(string name)
        {
            IAbility ability = AbilityManager.Abilities.FirstOrDefault(x => x.Name == name);
            if (ability == null)
            {
                ability = new Ability(AbilityManager.Abilities.Max(x => x.Id) + 1, name, AbilityTargets.Distant, AbilityBehaviors.Any, AbilityKinds.Spell, ResourceKinds.None, AmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.CannotBeUsed);
                AbilityManager.AddAbility(ability);
            }
            return ability;
        }

        public void SpellAcidBlast(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(level, 12);
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Acid))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Acid, true);
        }

        public void SpellArmor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.GetAura("Armor") != null)
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} is already armored.");
                return;
            }
            World.AddAura(victim, ability, caster, AuraModifiers.Armor, -20, AmountOperators.Fixed, 24, TimeSpan.FromMinutes(level), true);
        }

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
                    if (!Rom24Common.SavesDispel(level, evilAura?.Level ?? item.Level, 0))
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
                World.AddAura(item, ability, caster, AuraModifiers.SavingThrow, -1, AmountOperators.Fixed, level, TimeSpan.FromMinutes(6 + level), true);
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
                caster.Act(ActOptions.ToCharacter, "You grant {0} the favor of your god.", victim);
                int duration = 6 + level;
                World.AddAura(victim, ability, caster, AuraModifiers.HitRoll, level / 8, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                World.AddAura(victim, ability, caster, AuraModifiers.SavingThrow, -level / 8, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                victim.Recompute();
                return;
            }
        }

        public void SpellBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Blind) || Rom24Common.SavesSpell(level, victim, SchoolTypes.None))
                return;

            int duration = 1 + level;
            World.AddAura(victim, ability, caster, AuraModifiers.HitRoll, -4, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), true);
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Blind, level, TimeSpan.FromMinutes(duration), true);
            victim.Send("You are blinded!");
            victim.Act(ActOptions.ToRoom, "{0:N} appears to be blinded.", victim);
        }

        private readonly int[] BurningsHandsDamageTable =
        {
            0,
            0,  0,  0,  0, 14, 17, 20, 23, 26, 29,
            29, 29, 30, 30, 31, 31, 32, 32, 33, 33,
            34, 34, 35, 35, 36, 36, 37, 37, 38, 38,
            39, 39, 40, 40, 41, 41, 42, 42, 43, 43,
            44, 44, 45, 45, 46, 46, 47, 47, 48, 48
        };
        public void SpellBurningHands(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Fire, BurningsHandsDamageTable);
        }

        public void SpellCallLightning(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            //TODO: no weather implemented
        }

        public void SpellCalm(IAbility ability, int level, ICharacter caster)
        {
            // Stops all fighting in the room

            // Sum/Max/Count of fightning people in room
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
                if (victim != null && (victim.CurrentImmunities.HasFlag(IRVFlags.Magic) || npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
                    continue;

                // Is affected by berserk, calm or frenzy
                if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Berserk) || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Calm) || victim.GetAura("Frenzy") != null)
                    continue;

                victim.Send("A wave of calm passes over you.");

                if (victim.Fighting != null && victim.Position == Positions.Fighting)
                    victim.StopFighting(false);

                int modifier = victim != null
                    ? -5
                    : -2;
                int duration = level / 4;
                World.AddAura(victim, ability, caster, AuraModifiers.HitRoll, modifier, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                World.AddAura(victim, ability, caster, AuraModifiers.DamRoll, modifier, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Calm, level, TimeSpan.FromMinutes(duration), false);
                victim.Recompute();
            }
        }

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

        public void SpellCauseLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(1, 8) + level / 3;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        public void SpellCauseCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(3, 8) + level - 6;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        public void SpellCauseSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(2, 8) + level / 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        public void SpellChainLightning(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {

            caster.Act(ActOptions.ToRoom, "A lightning bolt leaps from {0}'s hand and arcs to {1}.", caster, victim);
            caster.Act(ActOptions.ToCharacter, "A lightning bolt leaps from your hand and arcs to {0}.", victim);
            victim.Act(ActOptions.ToCharacter, "A lightning bolt leaps from {0}'s hand and hits you!", caster);

            int damage = RandomManager.Dice(level, 6);
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Lightning))
                damage /= 3;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);

            // Hops from one victim to another
            ICharacter lastVictim = victim;
            level -= 4; // decrement damage
            while (level > 0)
            {
                // search a new victim
                ICharacter target = caster.Room.People.FirstOrDefault(x => x != lastVictim && Rom24Common.IsSafeSpell(caster, victim, true));
                if (target != null) // target found
                {
                    target.Act(ActOptions.ToRoom, "The bolt arcs to {0}!", target);
                    target.Send("The bolt hits you!");
                    damage = RandomManager.Dice(level, 6);
                    if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Lightning))
                        damage /= 3;
                    victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                    level -= 4; // decrement damage
                    lastVictim = target;
                }
                else // no target found, hits caster
                {
                    if (caster == null)
                        return;
                    if (lastVictim == caster) // no double hits
                    {
                        caster.Act(ActOptions.ToRoom, "The bolt seems to have fizzled out.");
                        caster.Send("The bolt grounds out through your body.");
                        return;
                    }
                    caster.Act(ActOptions.ToRoom, "The bolt arcs to {0}...whoops!", caster);
                    caster.Send("You are struck by your own lightning!");
                    damage = RandomManager.Dice(level, 6);
                    if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Lightning))
                        damage /= 3;
                    victim.AbilityDamage(caster, ability, damage, SchoolTypes.Lightning, true);
                    level -= 4; // decrement damage
                    lastVictim = caster;
                }
            }
        }

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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Other))
                return;
            Sex newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != victim.Sex));
            World.AddAura<Sex>(victim, ability, caster, AuraModifiers.Sex, newSex, level, TimeSpan.FromMinutes(2 * level), true);
            victim.Send("You feel different.");
            victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like $mself anymore...", victim);
        }

        public void SpellCharmPerson(IAbility ability, int level, ICharacter caster, INonPlayableCharacter victim)
        {
            if (Rom24Common.IsSafe(caster, victim))
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
                || Rom24Common.SavesSpell(level, victim, SchoolTypes.Charm))
                return;

            if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Law))
            {
                caster.Send("The mayor does not allow charming in the city limits.");
                return;
            }

            victim.ChangeController(caster);
            caster.ChangeSlave(victim);

            int duration = RandomManager.Fuzzy(level / 4);
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Charm, level, TimeSpan.FromMinutes(duration), true);

            victim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", caster);
            if (caster != victim)
                caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", victim);
            return;
        }

        private readonly int[] ChillTouchDamageTable =
        {
            0,
            0,  0,  6,  7,  8,  9, 12, 13, 13, 13,
            14, 14, 14, 15, 15, 15, 16, 16, 16, 17,
            17, 17, 18, 18, 18, 19, 19, 19, 20, 20,
            20, 21, 21, 21, 22, 22, 22, 23, 23, 23,
            24, 24, 24, 25, 25, 25, 26, 26, 26, 27
        };
        public void SpellChillTouch(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool savesSpell = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Cold, ChillTouchDamageTable);
            if (!savesSpell)
            {
                victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", victim);
                IAura existing = victim.GetAura(ability);
                if (existing != null)
                    existing.Modify(null, existing.Amount - 1, Pulse.FromPulse(existing.PulseLeft) + TimeSpan.FromMinutes(6));
                else
                    World.AddAura(victim, ability, caster, AuraModifiers.Strength, -1, AmountOperators.Fixed, level, TimeSpan.FromMinutes(6), true); // TODO: additional param to combine aura
            }
        }

        private readonly int[] ColourSprayDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
            30, 35, 40, 45, 50, 55, 55, 55, 56, 57,
            58, 58, 59, 60, 61, 61, 62, 63, 64, 64,
            65, 66, 67, 67, 68, 69, 70, 70, 71, 72,
            73, 73, 74, 75, 76, 76, 77, 78, 79, 79
        };
        public void SpellColourSpray(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool savesSpell = TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Light, ColourSprayDamageTable);
            if (!savesSpell)
                SpellBlindness(AbilityManager["Blindness"], level / 2, caster, victim);
        }

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

        public void SpellControlWeather(IAbility ability, int level, ICharacter caster, string parameter)
        {
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

        public void SpellCreateFood(IAbility ability, int level, ICharacter caster)
        {
            //TODO: no food implemented
        }

        public void SpelLCreateRose(IAbility ability, int level, ICharacter caster)
        {
            //TODO: add rose blueprint
        }

        public void SpellCreateSpring(IAbility ability, int level, ICharacter caster)
        {
            //TODO: no water implemented
        }

        // TODO: no drink container implemented
        //public void SpellCreateWater(IAbility ability, int level, ICharacter caster, IItemDrinkContainer container)
        //{
        //}

        public void SpellCureBlindness(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Blindness", level, caster, victim, "You aren't blind.", "{0:N} doesn't appear to be blinded.", "Spell failed.", "Your vision returns!", "{0:N} is no longer blinded.");
        }

        public void SpellCureCritical(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(3, 8) + level - 6;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        public void SpellCureDisease(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Plague", level, caster, victim, "You aren't ill.", "{0:N} doesn't appear to be diseased.", "Spell failed.", "Your sores vanish.", "{0:N} looks relieved as $s sores vanish.");
        }

        public void SpellCureLight(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(1, 8) + level / 3;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

        public void SpellCurePoison(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericSpellCureAbility("Poison", level, caster, victim, "You aren't poisoned.", "{0:N} doesn't appear to be poisoned.", "Spell failed.", "A warm feeling runs through your body.", "{0:N} looks much better.");
        }

        public void SpellCureSerious(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int heal = RandomManager.Dice(2, 8) + level / 2;
            victim.Heal(caster, ability, heal, false);
            victim.Send("You feel better!");
            if (victim != caster)
                caster.Send("Ok");
        }

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
                    if (!Rom24Common.SavesDispel(level, blessAura?.Level ?? item.Level, 0))
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
                World.AddAura(item, ability, caster, AuraModifiers.SavingThrow, 1, AmountOperators.Fixed, level, TimeSpan.FromMinutes(2 * level), true);
                caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                IAura curseAura = victim.GetAura("Curse");
                if (curseAura != null || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Curse) || Rom24Common.SavesSpell(level, victim, SchoolTypes.Negative))
                    return;
                victim.Send("You feel unclean.");
                if (caster != victim)
                    caster.Act(ActOptions.ToCharacter, "{0:N} looks very uncomfortable.", victim);
                int duration = 2 * level;
                World.AddAura(victim, ability, caster, AuraModifiers.HitRoll, -level / 8, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                World.AddAura(victim, ability, caster, AuraModifiers.SavingThrow, level / 8, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
                victim.Recompute();
                return;
            }
        }

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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Negative))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
            SpellCurse(AbilityManager["Curse"], 3*level/4, caster, victim);
        }

        public void SpellDetectEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectEvil, "You can already sense evil.", "{0:N} can already detect evil.", "Your eyes tingle.", "Ok.");
        }

        public void SpellDetectGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectGood, "You can already sense good.", "{0:N} can already detect good.", "Your eyes tingle.", "Ok.");
        }

        public void SpellDetectHidden(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectHidden, "You are already as alert as you can be.", "{0:N} can already sense hidden lifeforms.", "Your awareness improves.", "Ok.");
        }

        public void SpellDetectInvis(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectInvis, "You can already see invisible.", "{0:N} can already see invisible things.", "Your eyes tingle.", "Ok.");
        }

        public void SpellDetectMagic(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.DetectMagic, "You can already sense magical auras.", "{0:N} can already detect magic.", "Your eyes tingle.", "Ok.");
        }

        public void SpellDetectPoison(IAbility ability, int level, ICharacter caster, IItem item)
        {
            // TODO: food and drink container are not yet implemented
        }

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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Holy))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
        }

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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Negative))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
        }

        public void SpellDispelMagic(IAbility ability, int level, ICharacter caster, ICharacter victim) // TODO: check difference with cancellation
        {
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Other))
            {
                victim.Send("You feel a brief tingling sensation.");
                caster.Send("You failed.");
                return;
            }

            bool found = TryDispels(level, victim);
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Sanctuary) && !Rom24Common.SavesDispel(level, victim.Level, -1) && victim.GetAura("Sanctuary") == null)
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

        public void SpellEarthquake(IAbility ability, int level, ICharacter caster)
        {
            caster.Send("The earth trembles beneath your feet!");
            caster.Act(ActOptions.ToRoom, "{0:N} makes the earth tremble and shiver.", caster);

            // Inform people in area
            foreach (ICharacter character in caster.Room.Area.Characters.Where(x => x.Room != caster.Room))
                character.Send("The earth trembles and shivers.");

            // Damage people in room
            foreach (ICharacter victim in caster.Room.People.Where(x => x != caster && !Rom24Common.IsSafeSpell(caster, x, true)))
            {
                int damage = victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Flying)
                    ? 0 // no damage but starts fight
                    : level + RandomManager.Dice(2, 8);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Bash, true);
            }
        }

        public void SpellEnchantArmor(IAbility ability, int level, ICharacter caster, IItemArmor armor)
        {
            //if (item.EquipedBy == null)
            //{
            //    caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingAcAura = null;
            int acBonus = 0; // this means they have no bonus
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in armor.Auras)
                if (aura.Modifier == AuraModifiers.Armor)
                {
                    existingAcAura = aura;
                    acBonus += aura.Amount;
                    fail += 5 * (acBonus * acBonus);
                }
                else // things get a little harder
                    fail += 20;
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
            // TODO: change item level (+1)
            armor.AddBaseItemFlags(ItemFlags.Magic);
            if (existingAcAura != null)
                existingAcAura.Modify(Math.Max(existingAcAura.Level, level), existingAcAura.Amount + amount, null);
            else
                World.AddAura(armor, ability, caster, AuraModifiers.Armor, amount, AmountOperators.Fixed, level, Pulse.Infinite, false);
            armor.Recompute();
        }

        public void SpellEnchantWeapon(IAbility ability, int level, ICharacter caster, IItemWeapon weapon)
        {
            //if (weapon.EquipedBy == null)
            //{
            //    caster.Send("The item must be carried to be enchanted.");
            //    return;
            //}

            IAura existingHitRollAura = null;
            IAura existingDamRollAura = null;
            int hitBonus = 0; // this means they have no bonus
            int damBonus = 0; // this means they have no bonus
            int fail = 25; // base 25% chance of failure

            // find existing bonuses
            foreach (IAura aura in weapon.Auras)
                if (aura.Modifier == AuraModifiers.HitRoll)
                {
                    existingHitRollAura = aura;
                    hitBonus += aura.Amount;
                    fail += 5 * (hitBonus * hitBonus);
                }
                else if (aura.Modifier == AuraModifiers.DamRoll)
                {
                    existingDamRollAura = aura;
                    damBonus += aura.Amount;
                    fail += 5 * (damBonus * damBonus);
                }
                else // things get a little harder
                    fail += 20;
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
            // TODO: change item level (+1)
            weapon.AddBaseItemFlags(ItemFlags.Magic);
            if (existingHitRollAura != null)
                existingHitRollAura.Modify(Math.Max(existingHitRollAura.Level, level), existingHitRollAura.Amount + amount, Pulse.Infinite);
            else
                World.AddAura(weapon, ability, caster, AuraModifiers.HitRoll, amount, AmountOperators.Fixed, level, Pulse.Infinite, false);
            if (existingDamRollAura != null)
                existingDamRollAura.Modify(Math.Max(existingDamRollAura.Level, level), existingDamRollAura.Amount + amount, Pulse.Infinite);
            else
                World.AddAura(weapon, ability, caster, AuraModifiers.DamRoll, amount, AmountOperators.Fixed, level, Pulse.Infinite, false);
            weapon.Recompute();
        }

        public void SpellEnergyDrain(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim != caster)
                caster.UpdateAlignment(-50);

            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Negative))
            {
                victim.Send("You feel a momentary chill.");
                return;
            }

            int damage;
            if (victim.Level <= 2)
                damage = victim.HitPoints + 1;
            else
            {
                // TODO: negative experience gain gain_exp( victim, 0 - number_range( level/2, 3 * level / 2 ) );
                victim.UpdateResource(ResourceKinds.Mana, -victim[ResourceKinds.Mana] / 2); // half mana
                victim.UpdateMovePoints(-victim.MovePoints / 2); // half move
                damage = RandomManager.Dice(1, level);
                caster.Heal(caster, ability, damage, false);
            }

            victim.Send("You feel your life slipping away!");
            caster.Send("Wow....what a rush!");

            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Negative, true);
        }

        private readonly int[] FireballDamageTable =
        {
            0,
            0,   0,   0,   0,   0,      0,   0,   0,   0,   0,
            0,   0,   0,   0,  30,     35,  40,  45,  50,  55,
            60,  65,  70,  75,  80,     82,  84,  86,  88,  90,
            92,  94,  96,  98, 100,    102, 104, 106, 108, 110,
            112, 114, 116, 118, 120,    122, 124, 126, 128, 130
        };
        public void SpellFireball(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Fire, FireballDamageTable);
        }

        public void SpellFireproof(IAbility ability, int level, ICharacter caster, IItem item)
        {
            if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof))
            {
                caster.Act(ActOptions.ToCharacter, "{0} is already protected from burning.", item);
                return;
            }

            int duration = RandomManager.Fuzzy(level / 4);
            World.AddAura<ItemFlags>(item, ability, caster, AuraModifiers.ItemFlags, ItemFlags.BurnProof, level, TimeSpan.FromMinutes(duration), true);

            caster.Act(ActOptions.ToCharacter, "You protect {0} from fire.", item);
            caster.Act(ActOptions.ToRoom, "{0} is surrounded by a protective aura.", item);
        }

        public void SpellFlamestrike(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = RandomManager.Dice(6 + level / 2, 8);
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Fire))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
        }

        public void SpellFaerieFire(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.FaerieFire))
                return;
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.FaerieFire, level, TimeSpan.FromMinutes(level), true);
            victim.Act(ActOptions.ToAll, "{0:N} are surrounded by a pink outline.", victim);
        }

        public void SpellFaerieFog(IAbility ability, int level, ICharacter caster)
        {
            caster.Act(ActOptions.ToAll, "{0} conjures a cloud of purple smoke.", caster);

            IAbility invis = AbilityManager["Invis"];
            IAbility massInvis = AbilityManager["Mass Invis"];
            IAbility sneak = AbilityManager["Sneak"];
            foreach (ICharacter victim in caster.Room.People.Where(x => x != caster && !Rom24Common.SavesSpell(level, x, SchoolTypes.Other))) // && ich->invis_level <= 0
            {
                victim.RemoveAuras(x => x.Ability == invis || x.Ability == massInvis || x.Ability == sneak, false);
                victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak);
                victim.Recompute();
                victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
            }
        }

        public void SpellFloatingDisc(IAbility ability, int level, ICharacter caster)
        {
            // TODO: floating equipment location not implemented
        }

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
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Flying, level, TimeSpan.FromMinutes(level + 3), true);
            caster.Act(ActOptions.ToAll, "{0:P} feet rise off the ground.", victim);
        }

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
            int amount = level / 6;
            World.AddAura(victim, ability, caster, AuraModifiers.HitRoll, amount, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura(victim, ability, caster, AuraModifiers.DamRoll, amount, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura(victim, ability, caster, AuraModifiers.Armor, (10 * level) / 12, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();

            victim.Send("You are filled with holy wrath!");
            victim.Act(ActOptions.ToRoom, "{0:N} gets a wild look in $s eyes!", victim);
        }

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
                || (npcVictim != null && Rom24Common.SavesSpell(level, victim, SchoolTypes.Other)))
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
            World.AddAura(victim, ability, caster, AuraModifiers.Strength, modifier, AmountOperators.Fixed, level, TimeSpan.FromMinutes(level), true);
            victim.Act(ActOptions.ToAll, "{0:P} muscles surge with heightened power.", victim);
        }

        public void SpellHarm(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            int damage = Math.Max(20, victim.HitPoints - RandomManager.Dice(1, 4));
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Harm))
                damage = Math.Min(50, damage / 2);
            damage = Math.Min(100, damage);
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Harm, true);
        }

        public void SpellHaste(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste) || victim.GetAura("Haste") != null)  // TODO: Npc OffFlags.Fast
            {
                if (victim == caster)
                    caster.Send("You can't move any faster!");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", victim);
                return;
            }
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                if (TryDispel(level, victim, AbilityManager["Slow"]) != CheckDispelReturnValues.Dispelled)
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
            int amount = 1 + (level >= 18 ? 1 : 0) + (level >= 25 ? 1 : 0) + (level >= 32 ? 1 : 0);
            World.AddAura(victim, ability, caster, AuraModifiers.Dexterity, amount, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Haste, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();
            victim.Send("You feel yourself moving more quickly.");
            victim.Act(ActOptions.ToRoom, "{0:N} is moving more quickly.", victim);
            if (caster != victim)
                caster.Send("Ok.");
        }

        public void SpellHeal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            victim.Heal(caster, ability, 100, true);
            victim.Send("A warm feeling fills your body.");
            if (caster != victim)
                caster.Send("Ok.");
        }

        public void SpellHeatMetal(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            bool fail = true;
            int damage = 0;
            if (!Rom24Common.SavesSpell(level + 2, victim, SchoolTypes.Fire) && !victim.CurrentImmunities.HasFlag(IRVFlags.Fire))
            {
                // Check equipments
                foreach (IItem item in victim.Equipments.Where(x => x.Item != null))
                {
                    if (!item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                        && !item.CurrentItemFlags.HasFlag(ItemFlags.NonMetal)
                        && RandomManager.Range(1, 2 * level) > item.Level
                        && !Rom24Common.SavesSpell(level, victim, SchoolTypes.Fire))
                    {
                        switch (item)
                        {
                            case ItemArmor itemArmor:
                                if (!itemArmor.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) // remove the item
                                    && !itemArmor.CurrentItemFlags.HasFlag(ItemFlags.NoRemove)
                                    && itemArmor.Weight / 10 < RandomManager.Range(1, 2 * victim.CurrentAttributes(CharacterAttributes.Dexterity)))
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
                        && !Rom24Common.SavesSpell(level, victim, SchoolTypes.Fire))
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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Fire))
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
        }

        public void SpellHolyWord(IAbility ability, int level, ICharacter caster)
        {
            IAbility bless = AbilityManager["Bless"];
            IAbility curse = AbilityManager["Curse"];
            IAbility frenzy = AbilityManager["Frenzy"];

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
                    SpellBless(frenzy, level, caster, victim);
                }
                else if ((caster.IsGood && victim.IsEvil)
                        || (caster.IsEvil && victim.IsGood))
                {
                    if (!Rom24Common.SavesSpell(level, victim, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        SpellCurse(curse, level, caster, victim);
                        int damage = RandomManager.Dice(level, 6);
                        victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
                    }
                }
                else if (caster.IsNeutral)
                {
                    if (!Rom24Common.SavesSpell(level, victim, SchoolTypes.Holy))
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

        public void SpellIdentify(IAbility ability, int level, ICharacter caster, IItem item)
        {
            // TODO
        }

        public void SpellInfravision(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Infrared, "You can already see in the dark", "{0} already has infravision.", "Your eyes glow red.", "{0:P} eyes glow red.");
        }

        public void SpellInvis(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Invis))
                {
                    caster.Send("{0} is already invisible.", item);
                    return;
                }

                caster.Act(ActOptions.ToAll, "{0} fades out of sight.", item);
                World.AddAura<ItemFlags>(item, ability, caster, AuraModifiers.ItemFlags, ItemFlags.Invis, level, TimeSpan.FromMinutes(level + 12), true);
                return;
            }
            if (target is ICharacter victim)
            {
                if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Invisible))
                    return;

                victim.Act(ActOptions.ToRoom, "{0:N} fades out of existence.", victim);
                World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Invisible, level, TimeSpan.FromMinutes(level + 12), true);
                victim.Send("You fade out of existence.");
            }
        }

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

        private readonly int[] LightningBoltDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0,  0,  0, 25, 28,
            31, 34, 37, 40, 40, 41, 42, 42, 43, 44,
            44, 45, 46, 46, 47, 48, 48, 49, 50, 50,
            51, 52, 52, 53, 54, 54, 55, 56, 56, 57,
            58, 58, 59, 60, 60, 61, 62, 62, 63, 64
        };
        public void SpellLightningBolt(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Lightning, LightningBoltDamageTable);
        }

        public void SpellLocateObject(IAbility ability, int level, ICharacter caster, string parameter)
        {
            StringBuilder sb = new StringBuilder();
            int maxFound = (caster as IPlayableCharacter)?.ImpersonatedBy is IAdmin
                ? 200
                : level * 2;
            int number = 0;
            IEnumerable<IItem> foundItems = FindHelpers.FindAllByName(World.Items.Where(x => caster.CanSee(x) && !x.CurrentItemFlags.HasFlag(ItemFlags.NoLocate) && x.Level <= caster.Level && RandomManager.Range(1,100) <= 2*level), parameter, false);
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
                else if (item is IEquipable equipable && equipable.EquipedBy != null && caster.CanSee(equipable.EquipedBy))
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

        private readonly int[] MagicMissileDamageTable =
        {
            0,
            3,  3,  4,  4,  5,  6,  6,  6,  6,  6,
            7,  7,  7,  7,  7,  8,  8,  8,  8,  8,
            9,  9,  9,  9,  9, 10, 10, 10, 10, 10,
            11, 11, 11, 11, 11, 12, 12, 12, 12, 12,
            13, 13, 13, 13, 13, 14, 14, 14, 14, 14
        };
        public void SpellMagicMissile(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Energy, MagicMissileDamageTable);
        }

        public void SpellMassHealing(IAbility ability, int level, ICharacter caster)
        {
            IAbility heal = AbilityManager["Heal"];
            IAbility refresh = AbilityManager["Refresh"];

            foreach (ICharacter victim in caster.Room.People)
            {
                if ((caster is IPlayableCharacter && victim is IPlayableCharacter)
                    || (caster is INonPlayableCharacter && victim is INonPlayableCharacter))
                {
                    SpellHeal(heal, level, caster, victim);
                    SpellRefresh(heal, level, caster, victim);
                }
            }
        }

        public void SpellMassInvis(IAbility ability, int level, ICharacter caster)
        {
            // TODO: group is important
        }

        public void SpellPassDoor(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.PassDoor, "You are already out of phase.", "{0:N} is already shifted out of phase.", "You turn translucent.", "{0} turns translucent.");
        }

        public void SpellPlague(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Disease)
                || (victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead)))
            {
                if (victim == caster)
                    caster.Send("You feel momentarily ill, but it passes.");
                else
                    caster.Send("{0:N} seems to be unaffected.", victim);
            }

            World.AddAura(victim, ability, caster, AuraModifiers.Strength, -5, AmountOperators.Fixed, (3 * level) / 4, TimeSpan.FromMinutes(level), true);
            victim.Act(ActOptions.ToAll, "{0:N} scream{0:V} in agony as plague sores erupt from {0:s} skin.", victim);
        }

        public void SpellPoison(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                // TODO: food/drink container not yet implemented
                // TODO: poisoned weapon not yet implemented
                return;
            }
            // character
            if (target is ICharacter victim)
            {
                if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Poison))
                {
                    victim.Act(ActOptions.ToRoom, "{0:N} turns slightly green, but it passes.", victim);
                    victim.Send("You feel momentarily ill, but it passes.");
                    return;
                }

                int duration = level;
                World.AddAura(victim, ability, caster, AuraModifiers.Strength, -2, AmountOperators.Fixed, level, TimeSpan.FromMinutes(level), false);
                World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Poison, level, TimeSpan.FromMinutes(level), false);
                victim.Send("You feel very sick.");
                victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);
            }
        }

        public void SpellProtectionEvil(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already protected.", victim);
                return;
            }

            int duration = 24;
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.ProtectEvil, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura(victim, ability, caster, AuraModifiers.SavingThrow, -1, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();

            victim.Send("You feel holy and pure.");
            caster.Act(ActOptions.ToCharacter, "{0:N} is protected from evil.", victim);
        }

        public void SpellProtectionGood(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectGood)
                || victim.CurrentCharacterFlags.HasFlag(CharacterFlags.ProtectEvil))
            {
                caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already protected.", victim);
                return;
            }

            int duration = 24;
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.ProtectGood, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura(victim, ability, caster, AuraModifiers.SavingThrow, -1, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();

            victim.Send("You feel aligned with darkness.");
            caster.Act(ActOptions.ToCharacter, "{0:N} is protected from good.", victim);
        }

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
            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Holy))
                damage /= 2;

            int alignment = victim.Alignment - 350;
            if (alignment < -1000)
                alignment = -1000 + (alignment + 1000) / 3;

            damage = (damage * alignment * alignment) / (1000*1000);

            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Holy, true);
            SpellBlindness(AbilityManager["Blindness"], (3 * level) / 4, caster, victim);
        }

        //TODO: public void SpellRecharge(IAbility ability, int level, ICharacter caster, IItemCharge item)
        // TODO: staff/wand not yet implemented

        public void SpellRefresh(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            victim.UpdateMovePoints(level);
            if (victim.MovePoints == victim.CurrentAttributes(CharacterAttributes.MaxMovePoints))
                victim.Send("You feel fully refreshed!");
            else
                victim.Send("You feel less tired.");
            if (caster != victim)
                caster.Send("Ok");
        }

        public void SpellRemoveCurse(IAbility ability, int level, ICharacter caster, IEntity target)
        {
            // item
            if (target is IItem item)
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) || item.CurrentItemFlags.HasFlag(ItemFlags.NoRemove))
                {
                    if (!item.CurrentItemFlags.HasFlag(ItemFlags.NoUncurse) && !Rom24Common.SavesDispel(level + 2, item.Level, 0))
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
                if (TryDispel(level, victim, AbilityManager["Curse"]) == CheckDispelReturnValues.Dispelled)
                {
                    victim.Send("You feel better.");
                    victim.Act(ActOptions.ToRoom, "{0:N} looks more relaxed.", victim);
                }

                // attempt to remove curse on one item in inventory or equipment
                foreach (IItem carriedItem in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)).Where(x => (x.CurrentItemFlags.HasFlag(ItemFlags.NoDrop) || x.CurrentItemFlags.HasFlag(ItemFlags.NoRemove)) && !x.CurrentItemFlags.HasFlag(ItemFlags.NoUncurse)))
                    if (!Rom24Common.SavesDispel(level, carriedItem.Level, 0))
                    {
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoRemove);
                        carriedItem.RemoveBaseItemFlags(ItemFlags.NoDrop);
                        victim.Act(ActOptions.ToAll, "{0:P} {1} glows blue.", victim, carriedItem);
                        break;
                    }
                return;
            }
        }

        public void SpellSanctuary(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            GenericCharacterFlagsAbility(ability, level, caster, victim, CharacterFlags.Sanctuary, "You are already in sanctuary.", "{0:N} is already in sanctuary.", "You are surrounded by a white aura.", "{0:N} is surrounded by a white aura.");
        }

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

            World.AddAura(victim, ability, caster, AuraModifiers.Armor, -20, AmountOperators.Fixed, level, TimeSpan.FromMinutes(8 + level), true);
            caster.Act(ActOptions.ToRoom, "{0:N} {0:b} surrounded by a force shield.", victim);
        }

        private int[] ShockingGraspDamageTable =
        {
            0,
            0,  0,  0,  0,  0,  0, 20, 25, 29, 33,
            36, 39, 39, 39, 40, 40, 41, 41, 42, 42,
            43, 43, 44, 44, 45, 45, 46, 46, 47, 47,
            48, 48, 49, 49, 50, 50, 51, 51, 52, 52,
            53, 53, 54, 54, 55, 55, 56, 56, 57, 57
        };
        public void SpellShockingGrasp(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            TableBaseDamageSpell(ability, level, caster, victim, SchoolTypes.Lightning, ShockingGraspDamageTable);
        }

        public void SpellSleep(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Sleep)
                || (victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead))
                || level + 2 < victim.Level
                || Rom24Common.SavesSpell(level - 4, victim, SchoolTypes.Charm))
                return;

            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Sleep, level, TimeSpan.FromMinutes(4 + level), true);

            if (victim.Position > Positions.Sleeping)
            {
                victim.Send("You feel very sleepy ..... zzzzzz.");
                victim.Act(ActOptions.ToRoom, "{0:N} goes to sleep.", victim);
                victim.ChangePosition(Positions.Sleeping);
            }
        }

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
                || Rom24Common.SavesSpell(level, victim, SchoolTypes.Other))
            {
                if (victim != caster)
                    caster.Send("Nothing seemed to happen.");
                victim.Send("You feel momentarily lethargic.");
                return;
            }

            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Haste))
            {
                if (TryDispel(level, victim, AbilityManager["Haste"]) != CheckDispelReturnValues.Dispelled)
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
            int amount = -1 - (level >= 18 ? 1 : 0) - (level >= 25 ? 1 : 0) - (level >= 32 ? 1 : 0);
            World.AddAura(victim, ability, caster, AuraModifiers.Dexterity, amount, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Slow, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();
        }

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

            World.AddAura(victim, ability, caster, AuraModifiers.Armor, -40, AmountOperators.Fixed, level, TimeSpan.FromMinutes(level), true);
            caster.Act(ActOptions.ToAll, "{0:P} skin turns to stone.", victim);
        }

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
                || (npcVictim != null && Rom24Common.SavesSpell(level, victim, SchoolTypes.Other)))
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

        public void SpellTeleport(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.Room == null
                || victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.NoRecall)
                || (victim != caster && victim.CurrentImmunities.HasFlag(IRVFlags.Summon))
                || (victim is IPlayableCharacter pcVictim && pcVictim.Fighting != null)
                || (victim != caster && Rom24Common.SavesSpell(level - 5, victim, SchoolTypes.Other)))
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

        public void SpellVentriloquate(IAbility ability, int level, ICharacter caster, ICharacter victim, string parameter)
        {
            string phraseSuccess = $"%g%{victim.DisplayName} says '%x%{parameter ?? ""}%g%'%x%.";
            string phraseFail = $"Someone makes %g%{victim.DisplayName} say '%x%{parameter ?? ""}%g%'%x%.";

            foreach (ICharacter character in caster.Room.People.Where(x => x != victim && x.Position > Positions.Sleeping))
            {
                if (Rom24Common.SavesSpell(level, character, SchoolTypes.Other))
                    character.Send(phraseFail);
                else
                    character.Send(phraseSuccess);
            }
        }

        public void SpellWeaken(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            if (victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Weaken) || victim.GetAura(ability) != null || Rom24Common.SavesSpell(level, victim, SchoolTypes.Other))
                return;

            int duration = level / 2;
            World.AddAura(victim, ability, caster, AuraModifiers.Strength, -level / 5, AmountOperators.Fixed, level, TimeSpan.FromMinutes(duration), false);
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, CharacterFlags.Weaken, level, TimeSpan.FromMinutes(duration), false);
            victim.Recompute();
            victim.Send("You feel your strength slip away.");
            victim.Act(ActOptions.ToRoom, "{0:N} looks tired and weak.", victim);
        }

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

            pcVictim.UpdateMovePoints(-pcVictim.CurrentAttributes(CharacterAttributes.MaxMovePoints) / 2);
            pcVictim.Act(ActOptions.ToRoom, "{0:N} disappears", pcVictim);
            pcVictim.ChangeRoom(recallRoom);
            pcVictim.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pcVictim);
            pcVictim.AutoLook();
        }

        // NPC Spells

        public void SpellAcidBreath(IAbility ability, int level, ICharacter caster, ICharacter victim)
        {
            caster.ActToNotVictim(victim, "{0} spits acid at {1}.", caster, victim);
            victim.Act(ActOptions.ToCharacter, "{0} spits a stream of corrosive acid at you.", caster);
            caster.Act(ActOptions.ToCharacter, "You spit acid at {0}.", victim);

            int hp = Math.Max(12, victim.HitPoints);
            int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
            int diceDamage = RandomManager.Dice(level, 16);
            int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

            if (Rom24Common.SavesSpell(level, victim, SchoolTypes.Acid))
            {
                Rom24Effects.AcidEffect(victim, ability, caster, level / 2, damage / 4);
                victim.AbilityDamage(caster, ability, damage / 2, SchoolTypes.Acid, true);
            }
            else
            {
                Rom24Effects.AcidEffect(victim, ability, caster, level, damage);
                victim.AbilityDamage(caster, ability, damage, SchoolTypes.Acid, true);
            }
        }


        // TODO: acid breath, fire breath, frost breath, gas breath, lightning breath
        // TODO: general purpose, high explosive

        #region Helpers

        private bool TableBaseDamageSpell(IAbility ability, int level, ICharacter caster, ICharacter victim, SchoolTypes damageType, int[] table) // returns Rom24Common.SavesSpell result
        {
            int entry = level.Range(table);
            int minDamage = table[entry] / 2;
            int maxDamage = table[entry] * 2;
            int damage = RandomManager.Range(minDamage, maxDamage);
            bool savesSpell = Rom24Common.SavesSpell(level, victim, damageType);
            if (savesSpell)
                damage /= 2;
            victim.AbilityDamage(caster, ability, damage, SchoolTypes.Fire, true);
            return savesSpell;
        }

        private void GenericCharacterFlagsAbility(IAbility ability, int level, ICharacter caster, ICharacter victim, CharacterFlags characterFlags, string selfAlreadyAffected, string notSelfAlreadyAffected, string success, string notSelfSuccess)
        {
            if (victim.CurrentCharacterFlags.HasFlag(characterFlags))
            {
                if (victim == caster)
                    caster.Send(selfAlreadyAffected);
                else
                    caster.Act(ActOptions.ToCharacter, notSelfAlreadyAffected, victim);
                return;
            }
            World.AddAura<CharacterFlags>(victim, ability, caster, AuraModifiers.CharacterFlags, characterFlags, level, TimeSpan.FromMinutes(level), true);
            victim.Send(success);
            if (victim != caster)
                victim.Act(ActOptions.ToRoom, notSelfSuccess, victim);
        }

        private void GenericSpellCureAbility(string toCureAbilityName, int level, ICharacter caster, ICharacter victim, string selfNotFound, string notSelfNotFound, string noAction, string selfDispelled, string notSelfDispelled)
        {
            CheckDispelReturnValues dispel = TryDispel(level, victim, AbilityManager[toCureAbilityName]);
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
            bool found = false;
            if (TryDispel(level, victim, AbilityManager["Armor"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Bless"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Blindness"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer blinded.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Calm"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N no longer looks so peaceful...", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Change Sex"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} looks more like {0:f} again.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Charm Person"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} regains {0:s} free will.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Chill Touch"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} looks warmer.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Curse"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Detect Evil"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Detect Good"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Detect Hidden"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Detect Invis"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Detect Magic"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Faerie Fire"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N}'s outline fades.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Fly"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} falls to the ground!", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Frenzy"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} no longer looks so wild.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Giant Strength"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} no longer looks so mighty.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Haste"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer moving so quickly.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Infravision"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Invis"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} fades into existance.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Mass invis"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} fades into existance.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Pass Door"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Protection Evil"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Protection Good"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Sanctuary"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "The white aura around {0:n}'s body vanishes.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Shield"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "The shield protecting {0:n} vanishes.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Sleep"]) == CheckDispelReturnValues.Dispelled)
                found = true;
            if (TryDispel(level, victim, AbilityManager["Slow"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N} is no longer moving so slowly.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Stone Skin"]) == CheckDispelReturnValues.Dispelled)
            {
                victim.Act(ActOptions.ToRoom, "{0:N}'s skin regains its normal texture.", victim);
                found = true;
            }
            if (TryDispel(level, victim, AbilityManager["Weaken"]) == CheckDispelReturnValues.Dispelled)
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
                if (!Rom24Common.SavesDispel(dispelLevel, aura.Level, aura.PulseLeft))
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
                    aura.Modify(aura.Level - 1, null, null);
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }

        #endregion
    }
}
