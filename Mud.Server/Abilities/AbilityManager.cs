﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Abilities
{
    public class AbilityManager : IAbilityManager
    {
        private const int WeakenedSoulAbilityId = 999;
        private const int ParryAbilityId = 1000;
        private const int DodgeAbilityId = 1001;
        private const int ShieldBlockAbilityId = 1002;
        private const int DualWieldAbilityId = 1003;
        private const int ThirdWieldAbilityId = 1004;
        private const int FourthWieldAbilityId = 1005;

        private readonly List<IAbility> _abilities = new List<IAbility> // TODO: dictionary on id + Trie on name
        {
            //// Linked to Power Word: Shield (cannot be used/casted)
            //new Ability(WeakenedSoulAbilityId, "Weakened Soul", AbilityTargets.Target, AbilityBehaviors.None, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.CannotBeUsed),
            ////
            //new Ability(10, "Bear Form", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.AuraIsHidden, new ChangeFormEffect(Forms.Bear)),
            //new Ability(11, "Cat Form", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.AuraIsHidden, new ChangeFormEffect(Forms.Cat)),
            //new Ability(19, "Shadow Form", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.AuraIsHidden, new ChangeFormEffect(Forms.Shadow)),
            ////
            //new Ability(100, "Wrath", AbilityTargets.Target, AbilityBehaviors.Harmful, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 4, 4, 0, 0, SchoolTypes.Energy, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageAbilityEffect(149, CharacterAttributes.Intelligence, SchoolTypes.Energy)),
            //new Ability(101, "Thrash(bear)", AbilityTargets.Room, AbilityBehaviors.Harmful, AbilityKinds.Skill, ResourceKinds.Rage, CostAmountOperators.Fixed, 50, 4, 0, 15, SchoolTypes.Slash, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.RequireBearForm, new DamageAbilityEffect(513, CharacterAttributes.Strength, SchoolTypes.Slash), new DotAbilityEffect(365, CharacterAttributes.Strength, SchoolTypes.Pierce, 3)),
            //new Ability(102, "Thrash(cat)", AbilityTargets.Room, AbilityBehaviors.Harmful, AbilityKinds.Skill, ResourceKinds.Energy, CostAmountOperators.Fixed, 50, 4, 0, 15, SchoolTypes.Pierce, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.RequireCatForm, new DamageAbilityEffect(513, CharacterAttributes.Strength, SchoolTypes.Pierce), new DotAbilityEffect(365, CharacterAttributes.Strength, SchoolTypes.Pierce, 3)),
            //new Ability(103, "Shadow Word: Pain", AbilityTargets.Target, AbilityBehaviors.Harmful, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 1, 1, 0, 18, SchoolTypes.Negative, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new DamageAbilityEffect(475, CharacterAttributes.Intelligence, SchoolTypes.Negative), new DotAbilityEffect(475, CharacterAttributes.Intelligence, SchoolTypes.Negative, 3)),
            //new Ability(104, "Rupture", AbilityTargets.Target, AbilityBehaviors.Harmful, AbilityKinds.Skill, ResourceKinds.Energy, CostAmountOperators.Fixed, 25, 4, 0, 8 /* TODO: multiplied by combo*/, SchoolTypes.Pierce, AbilityMechanics.Bleeding, DispelTypes.None, AbilityFlags.None, new DotAbilityEffect(685, CharacterAttributes.Strength, SchoolTypes.Pierce, 2)),
            //new Ability(105, "Renew", AbilityTargets.TargetOrSelf, AbilityBehaviors.Friendly, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 2, 4, 0, 12, SchoolTypes.Holy, AbilityMechanics.None, DispelTypes.Magic, AbilityFlags.None, new HealAbilityEffect(22, CharacterAttributes.Intelligence), new HotAbilityEffect(44, CharacterAttributes.Intelligence, 3)),
            //new Ability(106, "Power Word: Shield", AbilityTargets.TargetOrSelf, AbilityBehaviors.Friendly, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 2, 1, 6, 15, SchoolTypes.Holy, AbilityMechanics.Shielded, DispelTypes.Magic, AbilityFlags.None, new PowerWordShieldEffect()),
            //// TODO: + %maxHP should only be done on friendly target
            //new Ability(107, "Death Coil", AbilityTargets.TargetOrSelf, AbilityBehaviors.Any, AbilityKinds.Spell, ResourceKinds.Runic, CostAmountOperators.Fixed, 30, 4, 0, 30, SchoolTypes.Negative, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new DamageOrHealEffect(0.88m, 0.88m*5, CharacterAttributes.Strength, SchoolTypes.Negative), new AuraAbilityEffect(AuraModifiers.MaxHitPoints, 3, CostAmountOperators.Percentage)),
            //new Ability(108, "Berserking", AbilityTargets.Self, AbilityBehaviors.Friendly, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 4, 3*60, 10, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackSpeed, 15, CostAmountOperators.Percentage)),
            //new Ability(109, "Battle Shout", AbilityTargets.Group, AbilityBehaviors.Friendly, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 4, 0, 1*60*60, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.AttackPower, 10, CostAmountOperators.Percentage)),
            //new Ability(110, "Swiftmend", AbilityTargets.TargetOrSelf, AbilityBehaviors.Friendly, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 14, 1, 30, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.None, new HealAbilityEffect(700,CharacterAttributes.Intelligence)),

            //new Ability(ParryAbilityId, "Parry", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            //new Ability(DodgeAbilityId, "Dodge", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            //new Ability(ShieldBlockAbilityId, "Shield Block", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            //new Ability(DualWieldAbilityId, "Dual wield", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            //new Ability(ThirdWieldAbilityId, "Third wield", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),
            //new Ability(FourthWieldAbilityId, "Fourth wield", AbilityTargets.Self, AbilityBehaviors.None, AbilityKinds.Skill, ResourceKinds.None, CostAmountOperators.None, 0, 0, 0, 0, SchoolTypes.None, AbilityMechanics.None, DispelTypes.None, AbilityFlags.Passive),

            //new Ability(8888, "Smite", AbilityTargets.Target, AbilityBehaviors.Harmful, AbilityKinds.Spell, ResourceKinds.Mana, CostAmountOperators.Percentage, 1, 1, 2, 0, SchoolTypes.Holy, AbilityMechanics.None, DispelTypes.None, AbilityFlags.CannotMiss, new DamageRangeAbilityEffect(516, 577, SchoolTypes.Holy), new HealSourceAbilityEffect(0.69m, CharacterAttributes.MaxHitPoints)),

            //new Ability(999999, "Test", AbilityTargets.TargetOrSelf, AbilityBehaviors.Harmful, AbilityKinds.Spell, ResourceKinds.None, CostAmountOperators.None, 0, 5, 0, 60, SchoolTypes.Negative, AbilityMechanics.Shielded, DispelTypes.Magic, AbilityFlags.None, new AuraAbilityEffect(AuraModifiers.HealAbsorb, 200000, CostAmountOperators.Fixed))
        };

        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        public AbilityManager()
        {
            //WeakenedSoulAbility = this[WeakenedSoulAbilityId];
            //ParryAbility = this[ParryAbilityId];
            //DodgeAbility = this[DodgeAbilityId];
            //ShieldBlockAbility = this[ShieldBlockAbilityId];
            //DualWieldAbility = this[DualWieldAbilityId];
            //ThirdWieldAbility = this[ThirdWieldAbilityId];
            //FourthWieldAbility = this[FourthWieldAbilityId];
        }

        #region IAbilityManager

        public IAbility WeakenedSoulAbility { get; }
        public IAbility ParryAbility { get; }
        public IAbility DodgeAbility { get; }
        public IAbility ShieldBlockAbility { get; }
        public IAbility DualWieldAbility { get; }
        public IAbility ThirdWieldAbility { get; }
        public IAbility FourthWieldAbility { get; }

        public IEnumerable<IAbility> Abilities => _abilities.AsReadOnly();

        public IAbility this[int id] =>_abilities.FirstOrDefault(x => x.Id == id);

        public IAbility this[string name] => _abilities.FirstOrDefault(x => FindHelpers.StringEquals(x.Name, name));

        public IAbility Search(CommandParameter parameter, bool includePassive = false)
        {
            return _abilities.Where(x =>
                (x.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed
                && (!includePassive || (x.Flags & AbilityFlags.Passive) != AbilityFlags.Passive)
                && FindHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
        }

        public bool Process(ICharacter source, params CommandParameter[] parameters)
        {
            //0/ Search ability (in known abilities)
            if (parameters.Length == 0)
            {
                source.Send("Cast/Use what ?");
                return false;
            }
            IAbility ability = Search(source.KnownAbilities, source.Level, parameters[0]);
            if (ability == null)
            {
                source.Send("You don't know any abilities of that name.");
                return false;
            }
            IPlayableCharacter playableCharacterSource = source as IPlayableCharacter;
            //1/ Check flags
            if ((ability.Flags & AbilityFlags.RequiresMainHand) == AbilityFlags.RequiresMainHand && !source.Equipments.Any(x => x.Slot == EquipmentSlots.MainHand && x.Item != null))
            {
                source.Send("You must be wielding in main-hand something prior using {0}", ability.Name);
                return false;
            }
            if ((ability.Flags & AbilityFlags.RequireBearForm) == AbilityFlags.RequireBearForm && source.Form != Forms.Bear)
            {
                source.Send("You must be in Bear form prior using {0}", ability.Name);
                return false;
            }
            if ((ability.Flags & AbilityFlags.RequireCatForm) == AbilityFlags.RequireCatForm && source.Form != Forms.Cat)
            {
                source.Send("You must be in Cat form prior using {0}", ability.Name);
                return false;
            }
            if ((ability.Flags & AbilityFlags.RequireMoonkinForm) == AbilityFlags.RequireMoonkinForm && source.Form != Forms.Moonkin)
            {
                source.Send("You must be in Moonkin form prior using {0}", ability.Name);
                return false;
            }
            if ((ability.Flags & AbilityFlags.RequireShadowForm) == AbilityFlags.RequireShadowForm && source.Form != Forms.Shadow)
            {
                source.Send("You must be in Shadow form prior using {0}", ability.Name);
                return false;
            }
            // TODO: shapeshift, combo
            //2/ Check cooldown
            int cooldownSecondsLeft = source.CooldownSecondsLeft(ability);
            if (cooldownSecondsLeft > 0)
            {
                source.Send("{0} is in cooldown for {1}.", ability.Name, StringHelpers.FormatDelay(cooldownSecondsLeft));
                return false;
            }
            //3/ Check resource
            int cost = ability.CostAmount; // default value (always overwritten if significant)
            if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != CostAmountOperators.None)
            {
                if (!source.CurrentResourceKinds.Contains(ability.ResourceKind)) // TODO: not sure about this test
                {
                    source.Send("You can't use {0} as resource for the moment.", ability.ResourceKind);
                    return false;
                }
                int resourceLeft = source[ability.ResourceKind];
                if (ability.CostType == CostAmountOperators.Fixed)
                    cost = ability.CostAmount;
                else //ability.CostType == CostAmountOperators.Percentage
                {
                    int maxResource = source.GetMaxResource(ability.ResourceKind);
                    cost = maxResource*ability.CostAmount/100;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                {
                    source.Send("You don't have enough {0}.", ability.ResourceKind);
                    return false;
                }
            }
            //4/ Check target(s)
            List<ICharacter> targets;
            switch (ability.Target)
            {
                case AbilityTargets.Self:
                    targets = new List<ICharacter> {source};
                    break;
                case AbilityTargets.Target:
                {
                    ICharacter target;
                    if (parameters.Length == 1 && ability.Behavior == AbilityBehaviors.Harmful && source.Fighting != null)
                        target = source.Fighting;
                    else if (parameters.Length == 1)
                    {
                        source.Send("{0} on whom ?", ability.Name);
                        return false;
                    }
                    else
                    {
                        target = FindHelpers.FindByName(source.Room.People, parameters[1]);
                        if (target == null)
                        {
                            source.Send(StringHelpers.CharacterNotFound);
                            return false;
                        }
                    }
                    targets = new List<ICharacter> {target};
                    break;
                }
                case AbilityTargets.TargetOrSelf:
                {
                    if (parameters.Length == 1)
                        targets = new List<ICharacter> {source};
                    else
                    {
                        ICharacter target = FindHelpers.FindByName(source.Room.People, parameters[1]);
                        if (target == null)
                        {
                            source.Send(StringHelpers.CharacterNotFound);
                            return false;
                        }
                        targets = new List<ICharacter> {target};
                    }
                    break;
                }
                case AbilityTargets.Group:
                {
                    // Source + group members
                    targets = new List<ICharacter>
                    {
                        source
                    };
                    if (playableCharacterSource != null)
                        targets.AddRange(playableCharacterSource.GroupMembers);
                    break;
                }
                case AbilityTargets.Room:
                {
                    // Friendly -> everyone in room
                    // Harmful -> everyone not in group
                    // Any -> everyone in room
                    if (ability.Behavior == AbilityBehaviors.Friendly)
                        targets = source.Room.People.ToList();
                    else if (ability.Behavior == AbilityBehaviors.Harmful)
                    {
                        if (playableCharacterSource != null)
                            targets = source.Room.People.Where(x => x != source && !playableCharacterSource.GroupMembers.Contains(x)).ToList();
                        else
                            targets = source.Room.People.Where(x => x != source).ToList();
                    }
                    else
                        targets = source.Room.People.ToList();
                    break;
                }
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown target {0} for ability {1}[{2}]!!!", ability.Target, ability.Name, ability.Id);
                    targets = Enumerable.Empty<ICharacter>().ToList();
                    break;
            }
            //5/ Say ability
            SayAbility(ability, source);
            //6/ Pay resource cost
            if (ability.ResourceKind != ResourceKinds.None && ability.CostAmount > 0 && ability.CostType != CostAmountOperators.None)
                source.UpdateResource(ability.ResourceKind, -cost);
            //7/ Perform effect(s) on target(s)
            int level = source.Level;
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(targets);
            foreach (ICharacter target in clone)
                ProcessOnOneTarget(source, target, ability, level, (ability.Flags & AbilityFlags.CannotMiss) == AbilityFlags.CannotMiss, (ability.Flags & AbilityFlags.CannotBeDodgedParriedBlocked) == AbilityFlags.CannotBeDodgedParriedBlocked);
            //8/ Set global cooldown
            if (playableCharacterSource != null)
                playableCharacterSource.ImpersonatedBy?.SetGlobalCooldown(ability.GlobalCooldown);
            // TODO: if ability cannot be used because an effect cannot be casted (ex. power word: shield with weakened soul is still affecting)
            //9/ Set cooldown
            if (ability.Cooldown > 0)
                source.SetCooldown(ability);
            return true;
        }

        #endregion

        #region TODO: TEST TO REMOVE

        public void AddAbility(IAbility ability) 
        {
            _abilities.Add(ability);
        }

        public bool Process(ICharacter source, ICharacter target, IAbility ability, int level)
        {
            ProcessOnOneTarget(source, target, ability, level, false, false);
            return true;
        }

        #endregion

        private void ProcessOnOneTarget(ICharacter source, ICharacter victim, IAbility ability, int level, bool cannotMiss, bool cannotBeDodgedParriedBlocked)
        {
            return;
            if (!source.IsValid || !victim.IsValid)
                return;
            // TODO
            //if (ability?.Effects == null || ability.Effects.Count == 0 || !source.IsValid || !victim.IsValid)
            //    return;

            // Miss/Dodge/Parray/Block check (only for harmful ability)
            CombatHelpers.AttackResults attackResult = CombatHelpers.AttackResults.Hit;
            if (ability.Behavior == AbilityBehaviors.Harmful)
            {
                // Starts fight if needed (if A attacks B, A fights B and B fights A)
                if (source != victim)
                {
                    if (source.Fighting == null)
                        source.StartFighting(victim);
                    if (victim.Fighting == null)
                        victim.StartFighting(source);
                    // TODO: Cannot attack slave without breaking slavery
                }
                if (ability.Kind == AbilityKinds.Skill)
                {
                    // TODO: refactor same code in Character.OneHit
                    // Miss, dodge, parry, ...
                    attackResult = CombatHelpers.YellowMeleeAttack(source, victim, cannotMiss, cannotBeDodgedParriedBlocked);
                    Log.Default.WriteLine(LogLevels.Debug, $"{source.DebugName} -> {victim.DebugName} : attack result = {attackResult}");
                    switch (attackResult)
                    {
                        case CombatHelpers.AttackResults.Miss:
                            victim.Act(ActOptions.ToCharacter, "{0} misses you.", source);
                            source.Act(ActOptions.ToCharacter, "You miss {0}.", victim);
                            return; // no effect applied
                        case CombatHelpers.AttackResults.Dodge:
                            victim.Act(ActOptions.ToCharacter, "You dodge {0}'s {1}.", source, ability.Name);
                            source.Act(ActOptions.ToCharacter, "{0} dodges your {1}.", victim, ability.Name);
                            return; // no effect applied
                        case CombatHelpers.AttackResults.Parry:
                            victim.Act(ActOptions.ToCharacter, "You parry {0}'s {1}.", source, ability.Name);
                            source.Act(ActOptions.ToCharacter, "{0} parries your {1}.", victim, ability.Name);
                            return; // no effect applied
                        case CombatHelpers.AttackResults.Block:
                            EquipedItem victimShield = victim.Equipments.FirstOrDefault(x => x.Item != null && x.Item is IItemShield && x.Slot == EquipmentSlots.OffHand);
                            if (victimShield != null) // will never be null because MeleeAttack will not return Block if no shield
                            {
                                victim.Act(ActOptions.ToCharacter, "You block {0}'s {1} with {2}.", source, ability.Name, victimShield.Item);
                                source.Act(ActOptions.ToCharacter, "{0} blocks your {1} with {2}.", victim, ability.Name, victimShield.Item);
                            }
                            // effect applied
                            break;
                        case CombatHelpers.AttackResults.Critical:
                        case CombatHelpers.AttackResults.CrushingBlow:
                        case CombatHelpers.AttackResults.Hit:
                            // effect applied
                            break;
                        default:
                            Log.Default.WriteLine(LogLevels.Error, $"Ability {ability.Name}[{ability.Kind}] returned an invalid attack result: {attackResult}");
                            break;
                    }
                }
                else if (ability.Kind == AbilityKinds.Spell && ability.Behavior == AbilityBehaviors.Harmful)
                {
                    // Miss/Hit/Critical
                    attackResult = CombatHelpers.SpellAttack(source, victim, cannotMiss);
                    switch (attackResult)
                    {
                        case CombatHelpers.AttackResults.Miss:
                            victim.Act(ActOptions.ToCharacter, "{0} misses you.", source);
                            source.Act(ActOptions.ToCharacter, "You miss {0}.", victim);
                            return; // no effect applied
                        case CombatHelpers.AttackResults.Hit:
                        case CombatHelpers.AttackResults.Critical:
                            // effect applied
                            break;
                        default:
                            Log.Default.WriteLine(LogLevels.Error, $"Ability {ability.Name}[{ability.Kind}] returned an invalid attack result: {attackResult}");
                            break;
                    }
                }
                else
                    Log.Default.WriteLine(LogLevels.Error, $"Ability {ability.Name} has an invalid kind: {ability.Kind}");
            }
            // TODO
            //// Apply effects
            //foreach (AbilityEffect effect in ability.Effects)
            //    effect.Process(source, victim, ability, level, attackResult);
        }

        private IAbility Search(IEnumerable<AbilityAndLevel> abilities, int level, CommandParameter parameter)
        {
            return abilities.Where(x =>
                (x.Ability.Flags & AbilityFlags.CannotBeUsed) != AbilityFlags.CannotBeUsed
                && (x.Ability.Flags & AbilityFlags.Passive) != AbilityFlags.Passive
                && x.Level <= level
                && FindHelpers.StringStartsWith(x.Ability.Name, parameter.Value))
                .Select(x => x.Ability)
                .ElementAtOrDefault(parameter.Count - 1);
        }

        private static readonly Dictionary<string, string> SyllableTable = new Dictionary<string, string> // TODO: use Trie ?
        {
            { " ",      " "     },
            { "ar",     "abra"      },
            { "au",     "kada"      },
            { "bless",  "fido"      },
            { "blind",  "nose"      },
            { "bur",    "mosa"      },
            { "cu",     "judi"      },
            { "de",     "oculo"     },
            { "en",     "unso"      },
            { "light",  "dies"      },
            { "lo",     "hi"        },
            { "mor",    "zak"       },
            { "move",   "sido"      },
            { "ness",   "lacri"     },
            { "ning",   "illa"      },
            { "per",    "duda"      },
            { "ra",     "gru"       },
            { "fresh",  "ima"       },
            { "re",     "candus"    },
            { "son",    "sabru"     },
            { "tect",   "infra"     },
            { "tri",    "cula"      },
            { "ven",    "nofo"      },
            { "a", "a" }, { "b", "b" }, { "c", "q" }, { "d", "e" },
            { "e", "z" }, { "f", "y" }, { "g", "o" }, { "h", "p" },
            { "i", "u" }, { "j", "y" }, { "k", "t" }, { "l", "r" },
            { "m", "w" }, { "n", "i" }, { "o", "a" }, { "p", "s" },
            { "q", "d" }, { "r", "f" }, { "s", "g" }, { "t", "h" },
            { "u", "j" }, { "v", "z" }, { "w", "x" }, { "x", "n" },
            { "y", "l" }, { "z", "k" }
        };

        // TODO: maybe a table should be constructed for each spell to avoid computing at each cast
        private void SayAbility(IAbility ability, ICharacter source)
        {
            if (ability.Kind == AbilityKinds.Spell)
            {
                source.Send("You cast '{0}'.", ability.Name);

                // Build mystical words for spell
                StringBuilder mysticalWords = new StringBuilder();
                string abilityName = ability.Name.ToLowerInvariant();
                string remaining = abilityName;
                while (remaining.Length > 0)
                {
                    bool found = false;
                    foreach (var syllable in SyllableTable)
                    {
                        if (remaining.StartsWith(syllable.Key))
                        {
                            mysticalWords.Append(syllable.Value);
                            remaining = remaining.Substring(syllable.Key.Length);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        mysticalWords.Append('?');
                        remaining = remaining.Substring(1);
                        Logger.Log.Default.WriteLine(LogLevels.Warning, "Spell {0} contains a character which is not found in syllable table", ability.Name);
                    }
                }

                // Say to people in room except source
                foreach (ICharacter target in source.Room.People.Where(x => x != source))
                {
                    if (target.KnownAbilities.Any(x => x.Ability == ability && x.Level < target.Level))
                        target.Act(ActOptions.ToCharacter, "{0} casts the spell '{1}'.", source, ability.Name);
                    else
                    {

                        target.Act(ActOptions.ToCharacter, "{0} utters the words, '{1}'.", source, mysticalWords);
                    }
                }
            }
            else if (ability.Kind == AbilityKinds.Skill)
            {
                source.Send("You use '{0}'.", ability.Name);
                source.Act(ActOptions.ToRoom, "{0} uses '{1}'.", source, ability.Name);
            }
            else
            {
                source.Send("You use '{0}'.", ability.Name);
                source.Act(ActOptions.ToRoom, "{0} uses '{1}'.", source, ability.Name);
                Log.Default.WriteLine(LogLevels.Error, "Ability {0} has unknown type {1}!", ability.Name, ability.Kind);
                Wiznet.Wiznet($"Ability {ability.Name} has unknown type {ability.Kind}!", WiznetFlags.Bugs);
            }
        }
        //            /*
        // * Utter mystical words for an sn.
        // */
        //void say_spell(CHAR_DATA* ch, int sn)
        //        {
        //            char buf[MAX_STRING_LENGTH];
        //            char buf2[MAX_STRING_LENGTH];
        //            CHAR_DATA* rch;
        //            const char* pName;
        //            int iSyl;
        //            int length;

        //  struct syl_type
        //        {
        //            char* old;
        //            char* newsyl;
        //        };

        //        static const struct syl_type syl_table[] =
        //  {
        //    { " ",		" "		},
        //    { "ar",		"abra"		},
        //    { "au",		"kada"		},
        //    { "bless",	"fido"		},
        //    { "blind",	"nose"		},
        //    { "bur",	"mosa"		},
        //    { "cu",		"judi"		},
        //    { "de",		"oculo"		},
        //    { "en",		"unso"		},
        //    { "light",	"dies"		},
        //    { "lo",		"hi"		},
        //    { "mor",	"zak"		},
        //    { "move",	"sido"		},
        //    { "ness",	"lacri"		},
        //    { "ning",	"illa"		},
        //    { "per",	"duda"		},
        //    { "ra",		"gru"		},
        //    { "fresh",	"ima"		},
        //    { "re",		"candus"	},
        //    { "son",	"sabru"		},
        //    { "tect",	"infra"		},
        //    { "tri",	"cula"		},
        //    { "ven",	"nofo"		},
        //    { "a", "a" }, { "b", "b" }, { "c", "q" }, { "d", "e" },
        //    { "e", "z" }, { "f", "y" }, { "g", "o" }, { "h", "p" },
        //    { "i", "u" }, { "j", "y" }, { "k", "t" }, { "l", "r" },
        //    { "m", "w" }, { "n", "i" }, { "o", "a" }, { "p", "s" },
        //    { "q", "d" }, { "r", "f" }, { "s", "g" }, { "t", "h" },
        //    { "u", "j" }, { "v", "z" }, { "w", "x" }, { "x", "n" },
        //    { "y", "l" }, { "z", "k" },
        //    { "", "" }
        //  };

        //  buf[0]	= '\0';
        //  for (pName = ability_table[sn].name; *pName != '\0'; pName += length ) {
        //    for (iSyl = 0; (length = strlen(syl_table[iSyl].old)) != 0; iSyl++ ) {
        //      if ( !str_prefix(syl_table[iSyl].old, pName ) ) {
        //	strcat(buf, syl_table[iSyl].newsyl );
        //	break;
        //      }
        //}

        //    if (length == 0 )
        //      length = 1;
        //  }

        //  sprintf(buf2, "$n utters the words, '%s'.", buf );
        //sprintf(buf,  "$n casts the '%s' spell.", ability_table[sn].name );

        //  for (rch = ch->in_room->people; rch; rch = rch->next_in_room ) {
        //    if (rch != ch )
        //      // Modified by SinaC 2000, was rch->pcdata->learned[sn] before
        //      act(( !IS_NPC(rch) && (get_ability(rch, sn) > 0) ) ? buf : buf2,
        //	   ch, NULL, rch, TO_VICT);
        //  }

        //  return;
        //}
    }
}
