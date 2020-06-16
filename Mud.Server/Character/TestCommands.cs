using System.Linq;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    // TODO: remove   test commands
    {
        [Command("test", "!!Test!!")]
        protected virtual bool DoTest(string rawParameters, params ICommandParameter[] parameters)
        {
            return true;
            //Send("Character: DoTest");

            //Send("==> TESTING DAMAGE/PERIODIC AURAS/AURAS/ABILITIES");
            //if (parameters.Length == 0)
            //{
            //    //AbilityDamage(this, null, 500, SchoolTypes.Fire, true);
            //    //World.AddAura(this, null, this, AuraModifiers.Dodge, 200 /*to be sure :p*/, AmountOperators.Fixed, 60*60, true);
            //    World.AddAura(this, null, this, AuraModifiers.Dodge, 30, AmountOperators.Fixed, 60 * 60, TimeSpan.FromSeconds(40), true);
            //    World.AddAura(this, null, this, AuraModifiers.Parry, 30, AmountOperators.Fixed, 60 * 60, TimeSpan.FromSeconds(40), true);
            //    World.AddAura(this, null, this, AuraModifiers.Block, 30, AmountOperators.Fixed, 60 * 60, TimeSpan.FromSeconds(40), true);
            //    //World.AddAura(this, null, this, AuraModifiers.Armor, 100000, AmountOperators.Fixed, 60 * 60, true);
            //}
            //else
            //{
            //    ICharacter victim = null;
            //    if (parameters.Length > 1)
            //        victim = FindHelpers.FindByName(Room.People, parameters[1]);
            //    victim = victim ?? Fighting ?? this;
            //    //CommandParameter abilityTarget = victim == this
            //    //    ? new CommandParameter
            //    //    {
            //    //        Count = 1,
            //    //        Value = Name
            //    //    }
            //    //    : parameters[1];
            //    if (parameters[0].Value == "a")
            //    {
            //        foreach (IAbility ability in AbilityManager.Abilities)
            //        {
            //            Send("[{0}]{1} [{2}] [{3}|{4}|{5}] [{6}|{7}|{8}] [{9}|{10}|{11}] {12} {13}",
            //                ability.Id, ability.Name,
            //                ability.Target,
            //                ability.ResourceKind, ability.CostType, ability.CostAmount,
            //                ability.GlobalCooldown, ability.Cooldown, ability.Duration,
            //                ability.School, ability.Mechanic, ability.DispelType,
            //                ability.Flags,
            //                ability.Effects == null || ability.Effects.Count == 0
            //                    ? string.Empty
            //                    : string.Join(" | ", ability.Effects.Select(x => "[" + x.GetType().Name + "]")));
            //        }
            //    }
            //    else if (parameters[0].Value == "0")
            //    {
            //        World.AddPeriodicAura(victim, null, this, SchoolTypes.Mental, 75, AmountOperators.Fixed, 40, true, 3, 8);
            //        World.AddPeriodicAura(victim, null, this, SchoolTypes.Fire, 75, AmountOperators.Fixed, 40, true, 3, 8);
            //        World.AddPeriodicAura(victim, null, this, SchoolTypes.Cold, 75, AmountOperators.Fixed, 40, true, 3, 8);
            //        World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, 40, true, 3, 8);
            //        World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, 40, true, 3, 8);
            //        World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, 40, true, 3, 8);
            //    }
            //    else if (parameters[0].Value == "1")
            //        victim.UnknownSourceDamage(null, 100, SchoolTypes.Lightning, true);
            //    else if (parameters[0].Value == "2")
            //        victim.UnknownSourceDamage(null, 100, SchoolTypes.Sound, true);
            //    else if (parameters[0].Value == "3")
            //        World.AddPeriodicAura(victim, null, this, SchoolTypes.Holy, 75, AmountOperators.Fixed, 40, true, 3, 8);
            //    else if (parameters[0].Value == "4")
            //        World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, 40, true, 3, 8);
            //    else if (parameters[0].Value == "5")
            //    {
            //        World.AddAura(victim, null, this, AuraModifiers.Stamina, 15, AmountOperators.Percentage, 70, TimeSpan.FromSeconds(40), true);
            //        World.AddAura(victim, null, this, AuraModifiers.Characteristics, -10, AmountOperators.Fixed, 30, TimeSpan.FromSeconds(40), true);
            //        World.AddAura(victim, null, this, AuraModifiers.AttackPower, 150, AmountOperators.Fixed, 90, TimeSpan.FromSeconds(40), true);
            //    }
            //    else if (parameters[0].Value == "6")
            //    {
            //        AbilityManager.Process(this, victim, AbilityManager["Shadow Word: Pain"], 40);
            //    }
            //    else if (parameters[0].Value == "7")
            //    {
            //        AbilityManager.Process(this, victim, AbilityManager["Rupture"], 40);
            //    }
            //    else if (parameters[0].Value == "8")
            //    {
            //        AbilityManager.Process(this, victim, AbilityManager["Trash"], 40);
            //    }
            //    else
            //    {
            //        AbilityManager.Process(this, parameters);
            //    }
            //}
            //return true;
        }

        [Command("act", "!!Test!!")]
        protected virtual bool DoAct(string rawParameters, params CommandParameter[] parameters)
        {
            ICharacter victim = Room.People.FirstOrDefault(x => x != this);
            if (victim == null)
                return true;
            IItem item = Inventory.FirstOrDefault();
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

        [Command("rom24", "!!Test!!")]
        protected virtual bool DoRom24(string rawParameters, params CommandParameter[] parameters)
        {
            //AbilityManager.Cast(this, rawParameters, parameters);
            //Rom24Spells rom24Spells = new Rom24Spells(DependencyContainer.Current.GetInstance<ISettings>(), DependencyContainer.Current.GetInstance<IWorld>(), DependencyContainer.Current.GetInstance<IAbilityManager>(), DependencyContainer.Current.GetInstance<IRandomManager>());

            //// no param: Earthquake
            //if (parameters.Length == 0)
            //{
            //    rom24Spells.SpellEarthquake(rom24Spells.CreateDummyAbility("earthquake"), Level, this);
            //    return true;
            //}

            //// 1 item param: 
            //IItem item = FindHelpers.FindItemHere(this, parameters[0]);
            //if (item != null)
            //{
            //    rom24Spells.SpellContinualLight(rom24Spells.CreateDummyAbility("continual light"), Level, this, item);
            //    return true;
            //}

            //// 1 character param:
            //ICharacter victim = FindHelpers.FindByName(Room.NonPlayableCharacters, parameters[0]);
            //if (victim != null)
            //{
            //    rom24Spells.SpellAcidBlast(rom24Spells.CreateDummyAbility("acid blast"), Level, this, victim);
            //    return true;
            //}

            return true;
        }
    }
}
