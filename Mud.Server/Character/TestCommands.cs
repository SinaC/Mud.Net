using System.Linq;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character
    // TODO: remove   test commands
    {
        [Command("test", Category = "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Character: DoTest");

            Send("==> TESTING DAMAGE/PERIODIC AURAS/AURAS/ABILITIES");
            if (parameters.Length == 0)
            {
                //AbilityDamage(this, null, 500, SchoolTypes.Fire, true);
                //World.AddAura(this, null, this, AuraModifiers.Dodge, 200 /*to be sure :p*/, AmountOperators.Fixed, 60*60, true);
                World.AddAura(this, null, this, AuraModifiers.Dodge, 30, AmountOperators.Fixed, 60 * 60, true);
                World.AddAura(this, null, this, AuraModifiers.Parry, 30, AmountOperators.Fixed, 60 * 60, true);
                World.AddAura(this, null, this, AuraModifiers.Block, 30, AmountOperators.Fixed, 60 * 60, true);
                //World.AddAura(this, null, this, AuraModifiers.Armor, 100000, AmountOperators.Fixed, 60 * 60, true);
            }
            else
            {
                ICharacter victim = null;
                if (parameters.Length > 1)
                    victim = FindHelpers.FindByName(Room.People, parameters[1]);
                victim = victim ?? Fighting ?? this;
                //CommandParameter abilityTarget = victim == this
                //    ? new CommandParameter
                //    {
                //        Count = 1,
                //        Value = Name
                //    }
                //    : parameters[1];
                if (parameters[0].Value == "a")
                {
                    foreach (IAbility ability in AbilityManager.Abilities)
                    {
                        Send("[{0}]{1} [{2}] [{3}|{4}|{5}] [{6}|{7}|{8}] [{9}|{10}|{11}] {12} {13}",
                            ability.Id, ability.Name,
                            ability.Target,
                            ability.ResourceKind, ability.CostType, ability.CostAmount,
                            ability.GlobalCooldown, ability.Cooldown, ability.Duration,
                            ability.School, ability.Mechanic, ability.DispelType,
                            ability.Flags,
                            ability.Effects == null || ability.Effects.Count == 0
                                ? string.Empty
                                : string.Join(" | ", ability.Effects.Select(x => "[" + x.GetType().Name + "]")));
                    }
                }
                else if (parameters[0].Value == "0")
                {
                    World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                    World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                    World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                }
                else if (parameters[0].Value == "1")
                    victim.UnknownSourceDamage(null, 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "2")
                    victim.UnknownSourceDamage(null, 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "3")
                    World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                else if (parameters[0].Value == "4")
                    World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                else if (parameters[0].Value == "5")
                {
                    World.AddAura(victim, null, this, AuraModifiers.Stamina, 15, AmountOperators.Percentage, 70, true);
                    World.AddAura(victim, null, this, AuraModifiers.Characteristics, -10, AmountOperators.Fixed, 30, true);
                    World.AddAura(victim, null, this, AuraModifiers.AttackPower, 150, AmountOperators.Fixed, 90, true);
                }
                else if (parameters[0].Value == "6")
                {
                    AbilityManager.Process(this, victim, AbilityManager["Shadow Word: Pain"]);
                }
                else if (parameters[0].Value == "7")
                {
                    AbilityManager.Process(this, victim, AbilityManager["Rupture"]);
                }
                else if (parameters[0].Value == "8")
                {
                    AbilityManager.Process(this, victim, AbilityManager["Trash"]);
                }
                else
                {
                    AbilityManager.Process(this, parameters);
                }
            }
            return true;
        }

        [Command("act", Category = "!!Test!!")]
        protected virtual bool DoAct(string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter victim = Room.People.FirstOrDefault(x => x != this);
            if (victim == null)
                return true;
            IItem item = Content.FirstOrDefault();
            if (item == null)
                return true;
            Send("Victim: {0}", victim.DisplayName);
            Send("Item: {0}", item.DisplayName);
            //Act(ActOptions.ToAll, "{0} slay{0:v} {1} in cold blood!", this, victim);
            //Act(ActOptions.ToAll, "{0:P} {1} is looking at {2}!", this, item, victim);
            //Act(ActOptions.ToAll, "{0} {0:b} looking at {1}", this, victim);
            //Act(ActOptions.ToAll, "{0} {0:h} frown at {1}", this, victim);
            //Act(ActOptions.ToAll, "{0:P} {1} {2} {0:f}.[{3}]", this, "lightning bolt", "nukes", 10);
            //Act(ActOptions.ToAll, "{0:P} {1} {2} {0:f}.[{3}]", victim, "lightning bolt", "nukes", 10);
            Act(ActOptions.ToAll, "{0:P} {1} {2} {3}.[{4}]", victim, "lightning bolt", "nuke", this, 10);
            return true;
        }

        [Command("charm", Category = "!!Test!!")]
        protected virtual bool DoCharm(string rawParameters, params CommandParameter[] parameters)
        {
            if (ControlledBy != null)
                Send("You feel like taking, not giving, orders.");
            else if (parameters.Length == 0)
            {
                if (Slave != null)
                {
                    Send("You stop controlling {0}.", Slave.DisplayName);
                    Slave.ChangeController(null);
                    Slave = null;
                }
                else
                    Send("Try controlling something before trying to un-control.");
            }
            else
            {
                ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);
                if (target != null)
                {
                    if (target == this)
                        Send("You like yourself even better!");
                    else
                    {
                        target.ChangeController(this);
                        Slave = target;
                        Send("{0} looks at you with adoring eyes.", target.DisplayName);
                        target.Send("Isn't {0} so nice?", DisplayName);
                    }
                }
                else
                    Send(StringHelpers.CharacterNotFound);
            }

            return true;
        }
    }
}
