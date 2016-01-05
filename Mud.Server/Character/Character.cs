using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Server;

namespace Mud.Server.Character
{
    public partial class Character : EntityBase, ICharacter
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> CharacterCommands;

        private readonly List<IItem> _inventory;
        private readonly List<EquipmentSlot> _equipments;

        static Character()
        {
            CharacterCommands = CommandHelpers.GetCommands(typeof (Character));
        }

        public Character(Guid guid, string name, IRoom room) // playable
            : base(guid, name)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipmentSlot>();
            BuildEquipmentLocation();
            Sex = Sex.Neutral;
            HitPoints = 1000; // TODO:
            Impersonable = true;
            Room = room;
            room.Enter(this);
        }

        public Character(Guid guid, CharacterBlueprint blueprint, IRoom room) // non-playable
            :base(guid, blueprint.Name, blueprint.Description)
        {
            Blueprint = blueprint;
            _inventory = new List<IItem>();
            _equipments = new List<EquipmentSlot>();
            BuildEquipmentLocation();
            Sex = blueprint.Sex;
            HitPoints = 1000; // TODO:
            Impersonable = false;
            Room = room;
            room.Enter(this);
        }

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return CharacterCommands; }
        }

        public override void Send(string message)
        {
            // TODO: use Act formatter ?
            base.Send(message);
            if (ImpersonatedBy != null)
            {
                if (ServerOptions.PrefixForwardedMessages)
                    message= "<IMP|" + Name + ">" + message;
                ImpersonatedBy.Send(message);
            }
            // TODO: do we really need to receive message sent to slave ?
            if (ServerOptions.ForwardSlaveMessages && ControlledBy != null)
            {
                if (ServerOptions.PrefixForwardedMessages)
                    message = "<CTRL|" + Name + ">" + message;
                ControlledBy.Send(message);
            }
        }

        public override void Page(StringBuilder text)
        {
            base.Page(text);
            if (ImpersonatedBy != null)
                ImpersonatedBy.Page(text);
            if (ServerOptions.ForwardSlaveMessages && ControlledBy != null)
                ControlledBy.Page(text);
        }

        #endregion

        public override string DisplayName
        {
            get { return Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription; }
        }

        public override void OnRemoved() // called before removing an item from the game
        {
            base.OnRemoved();
            StopFighting(true);
            if (Slave != null)
                Slave.ChangeController(null);
            if (ImpersonatedBy != null)
            {
                // TODO: warn ImpersonatedBy
                ImpersonatedBy = null;
            }
            if (ControlledBy != null)
            {
                // TODO: warn ControlledBy
                ControlledBy = null;
            }
            _inventory.Clear();
            _equipments.Clear();
            Blueprint = null;
            Room = null;
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _inventory.AsReadOnly(); }
        }

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _inventory.Add(obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _inventory.Remove(obj);
            return removed;
        }

        #endregion

        public CharacterBlueprint Blueprint { get; private set; }

        public IRoom Room { get; private set; }
        
        public ICharacter Fighting { get; private set; }

        public IReadOnlyList<EquipmentSlot> Equipments
        {
            get
            {
                return _equipments.AsReadOnly();
            }
        }

        public Sex Sex { get; private set; }
        public int HitPoints { get; private set; }

        public bool Impersonable { get; private set; }
        public IPlayer ImpersonatedBy { get; private set; }

        public ICharacter Slave { get; private set; } // who is our slave (related to charm command/spell)
        public ICharacter ControlledBy { get; private set; } // who is our master (related to charm command/spell)

        public bool ChangeImpersonation(IPlayer player) // if non-null, start impersonation, else, stop impersonation
        {
            Log.Default.WriteLine(LogLevels.Debug, "ChangeImpersonation: {0} old: {1}; new {2}", Name, ImpersonatedBy == null ? "<<none>>" : ImpersonatedBy.Name, player == null ? "<<none>>" : player.Name);
            // TODO: check if not already impersonated, if impersonable, ...
            ImpersonatedBy = player;
            return true;
        }

        public bool ChangeController(ICharacter master) // if non-null, start slavery, else, stop slavery
        {
            Log.Default.WriteLine(LogLevels.Debug, "ChangeController: {0} master: old: {1}; new {2}", Name, ControlledBy == null ? "<<none>>" : ControlledBy.Name, master == null ? "<<none>>" : master.Name);
            // TODO: check if already slave, ...
            if (master == null) // TODO: remove display ???
            {
                if (ControlledBy != null)
                {
                    Act(ActOptions.ToCharacter, "You stop following {0}.", ControlledBy);
                    Act(ActOptions.ToVictim, ControlledBy, "{0} stops following you.", this);
                }
            }
            else
            {
                Act(ActOptions.ToCharacter, "You now follow {0}.", master);
                Act(ActOptions.ToVictim, master, "{0} now follows you.", this);
            }
            ControlledBy = master;
            return true;
        }

        public bool CanSee(ICharacter character)
        {
            return true; // TODO
        }

        public bool CanSee(IItem obj)
        {
            return true; // TODO
        }

        public void ChangeRoom(IRoom destination)
        {
            Log.Default.WriteLine(LogLevels.Debug, "ChangeRoom: {0} from: {1} to {2}", Name, Room == null ? "<<no room>>" : Room.Name, destination == null ? "<<no room>>" : destination.Name);
            if (Room != null)
                Room.Leave(this);
            Room = destination;
            if (destination != null)
                destination.Enter(this);
        }

        public bool MultiHit(ICharacter enemy)
        {
            Log.Default.WriteLine(LogLevels.Debug, "MultiHit: {0} -> {1}", Name, enemy.Name);

            if (this == enemy || Room != enemy.Room)
                return false;

            // TODO: check if wielding a weapon, ...
            // TODO: secondary, haste, ...
            IItemWeapon wielded = (Equipments.FirstOrDefault(x => x.WearLocation == WearLocations.Wield) ?? EquipmentSlot.NullObject).Item as IItemWeapon;
            DamageTypes damageType = DamageTypes.Arcane; // TODO
            if (ImpersonatedBy == null)
            {
                OneHit(enemy, wielded, damageType);
                //
                if (Fighting != enemy)
                    return true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    OneHit(enemy, wielded, damageType);
                    //
                    if (Fighting != enemy)
                        return true;
                }
            }

            return true;
        }

        public bool StartFighting(ICharacter enemy) // equivalent to set_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} starts fighting {1}", Name, enemy.Name);

            Fighting = enemy;
            return true;
        }

        public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} stops fighting {1}", Name, Fighting == null ? "<<no enemy>>" : Fighting.Name);

            Fighting = null;
            // TODO: change/update pos
            if (both)
                foreach (ICharacter enemy in World.World.Instance.GetCharacters().Where(x => x.Fighting == this))
                {
                    enemy.StopFighting(false);
                    // TODO: change/update pos
                }
            return true;
        }

        public bool TakeCombatDamage(ICharacter damager, int damage, DamageTypes damageType, bool visible) // TODO:    check combat_damage in fight.C:1940
        {
            // TODO: damage reduction

            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != damager)
            {
                if (Fighting == null)
                    StartFighting(damager);
                if (damager.Fighting == null)
                    damager.StartFighting(this);
                // TODO: Cannot attack slave without breaking slavery
            }
            // TODO: if invisible, remove invisibility
            // TODO: damage modifier
            // TODO: check parry, dodge, shield block, ...
            // TODO: check immunity/resist/vuln

            if (visible) // equivalent to dam_message in fight.C:4381
            {
                string damagePhrase1 = "damage";
                string damagePhrase2 = "damages";
                if (damage == 0)
                {
                    damagePhrase1 = "miss";
                    damagePhrase2 = "misses";
                }
                else // TODO: other flavor messages (see fight.C:4429)
                {
                    damagePhrase1 = "%b%--*-- --*-- RUPTURE --*-- --*--%x%";
                    damagePhrase2 = "%b%--*-- --*-- RUPTURES --*-- --*--%x%";
                }

                Act(ActOptions.ToVictim, damager, "You {0} {1}.", damagePhrase1, this);
                Act(ActOptions.ToNotVictim, damager, "{0} {1} {2}.", this, damagePhrase2, this);
                if (damager != this)
                    Act(ActOptions.ToCharacter, "{0} {1} you.", damager, damagePhrase2);
            }
            // No damage -> stop here
            if (damage == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", damager.Name, Name);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", damager.Name, damage, Name);

            // Apply damage
            bool dead = false;
            HitPoints -= damage;
            if (HitPoints < 1)
            {
                HitPoints = 1;
                dead = true;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP: {1}", Name, HitPoints);

            //update_pos(victim);
            //position_msg(victim, dam);
            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", Name, damager.Name);

                Act(ActOptions.ToCharacter, "{0} is dead.", this);
                Act(ActOptions.ToNotVictim, this, "{0} is dead.", this);
                Send("You have been KILLED!!"+Environment.NewLine);

                StopFighting(false);
                damager.KillingPayoff(this);
                return true;
            }

            // TODO: wimpy, ... // fight.C:2264

            return true;
        }

        public bool KillingPayoff(ICharacter victim)
        {
            // TODO: gain/lose xp/reputation   damage.C:32
            IItemCorpse corpse = RawKill(victim);
            // TODO: autoloot, autosac  damage.C:96
            return true;
        }

        public IItemCorpse RawKill(ICharacter victim) // returns ItemCorpse
        {
            victim.StopFighting(true);
            // Death cry
            //we should not hear our death cry Act(ActOptions.ToCharacter, "You hear {0}'s death cry.", victim);
            Act(ActOptions.ToNotVictim, this, "You hear {0}'s death cry.", victim);
            // Create corpse
            IItemCorpse corpse = World.World.Instance.AddItemCorpse(Guid.NewGuid(), ServerOptions.CorpseBlueprint, Room, victim);
            if (victim.ImpersonatedBy != null) // If impersonated, no real death
            {
                // TODO: reset hit/mana/...
                // TODO: teleport player to hall room/graveyard  see fight.C:3952
            }
            else // If not impersonated, remove from game
            {
                World.World.Instance.RemoveCharacter(victim);
            }
            return corpse;
        }

        #endregion

        protected void BuildEquipmentLocation()
        {
            // TODO: depend on race+affects+...
            _equipments.Add(new EquipmentSlot(WearLocations.Light));
            _equipments.Add(new EquipmentSlot(WearLocations.Head));
            _equipments.Add(new EquipmentSlot(WearLocations.Amulet));
            _equipments.Add(new EquipmentSlot(WearLocations.Shoulders));
            _equipments.Add(new EquipmentSlot(WearLocations.Chest));
            _equipments.Add(new EquipmentSlot(WearLocations.Cloak));
            _equipments.Add(new EquipmentSlot(WearLocations.Waist));
            _equipments.Add(new EquipmentSlot(WearLocations.Wrists));
            _equipments.Add(new EquipmentSlot(WearLocations.Hands));
            _equipments.Add(new EquipmentSlot(WearLocations.RingLeft));
            _equipments.Add(new EquipmentSlot(WearLocations.RingRight));
            _equipments.Add(new EquipmentSlot(WearLocations.Legs));
            _equipments.Add(new EquipmentSlot(WearLocations.Feet));
            _equipments.Add(new EquipmentSlot(WearLocations.Trinket1));
            _equipments.Add(new EquipmentSlot(WearLocations.Trinket2));
            _equipments.Add(new EquipmentSlot(WearLocations.Wield));
            _equipments.Add(new EquipmentSlot(WearLocations.Shield));
            _equipments.Add(new EquipmentSlot(WearLocations.Hold));
        }

        private bool OneHit(ICharacter victim, IItemWeapon weapon, DamageTypes damageType) // TODO: skill    check fight.C:1394
        {
            if (this == victim || Room != victim.Room)
                return false;
            // TODO: skill percentage
            // TODO: check wield  fight.C:1595

            int damage = 0;

            if (weapon != null)
                damage = RandomizeHelpers.Instance.Dice(weapon.DiceCount, weapon.DiceValue);
            else
            {
                // TEST
                if (ImpersonatedBy != null)
                    damage = 75;
                else
                    damage = 10000;
            }
            // TODO: damage modifier  fight.C:1693

            victim.TakeCombatDamage(this, damage, damageType, true);

            return true;
        }

        #region Act  TODO in EntityBase ???

        protected enum ActOptions
        {
            ToRoom, // everyone in the room except origin
            ToNotVictim, // everyone on in the room except Character and Target
            ToVictim, // only to Target
            ToCharacter, // only to Character
            ToAll // everyone in the room
        }

        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        protected void Act(ActOptions options, string format, params object[] arguments)
        {
            if (options == ActOptions.ToNotVictim || options == ActOptions.ToVictim) // TODO: exception ???
                Log.Default.WriteLine(LogLevels.Error, "Act: victim must be specified to use ToNotVictim or ToVictim");
            else if (options == ActOptions.ToAll || options == ActOptions.ToRoom)
                foreach (ICharacter to in Room.People)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != this))
                    {
                        string phrase = FormatActOneLine(to, format, arguments);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = FormatActOneLine(this, format, arguments);
                Send(phrase);
            }
        }

        protected void Act(ActOptions options, ICharacter victim, string format, params object[] arguments)
        {
            if (options == ActOptions.ToAll || options == ActOptions.ToRoom || options == ActOptions.ToNotVictim)
                foreach (ICharacter to in Room.People)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != this)
                        || (options == ActOptions.ToNotVictim && to != victim && to != this))
                    {
                        string phrase = FormatActOneLine(to, format, arguments);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = FormatActOneLine(this, format, arguments);
                Send(phrase);
            }
            else if (options == ActOptions.ToVictim)
            {
                string phrase = FormatActOneLine(victim, format, arguments);
                victim.Send(phrase);
            }
        }

        // Recreate behaviour of String.Format with maximum 10 arguments
        // If an argument is ICharacter, IItem, IExit special formatting is applied (depending on who'll receive the message)
        private enum ActParsingStates
        {
            Normal,
            OpeningBracketFound,
            ArgumentFound,
            FormatSeparatorFound,
        }
        private static string FormatActOneLine(ICharacter target, string format, params object[] arguments)
        {
            StringBuilder result = new StringBuilder();

            ActParsingStates state = ActParsingStates.Normal;
            object currentArgument = null;
            StringBuilder argumentFormat = null;
            for (int i = 0; i < format.Length; i++)
            {
                char c = format[i];

                switch (state)
                {
                    case ActParsingStates.Normal: // searching for {
                        if (c == '{')
                        {
                            state = ActParsingStates.OpeningBracketFound;
                            currentArgument = null;
                            argumentFormat = new StringBuilder();
                        }
                        else
                            result.Append(c);
                        break;
                    case ActParsingStates.OpeningBracketFound: // searching for a number
                        if (c == '{') // {{ -> {
                        {
                            result.Append('{');
                            state = ActParsingStates.Normal;
                        }
                        else if (c == '}') // {} -> nothing
                        {
                            state = ActParsingStates.Normal;
                        }
                        else if (c >= '0' && c <= '9') // {x -> argument found
                        {
                            currentArgument = arguments[c - '0'];
                            state = ActParsingStates.ArgumentFound;
                        }
                        break;
                    case ActParsingStates.ArgumentFound: // searching for } or :
                        if (c == '}')
                        {
                            FormatActOneArgument(target, result, null, currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else if (c == ':')
                            state = ActParsingStates.FormatSeparatorFound;
                        break;
                    case ActParsingStates.FormatSeparatorFound: // searching for }
                        if (c == '}')
                        {
                            FormatActOneArgument(target, result, argumentFormat.ToString(), currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else
                        {
                            // argumentFormat cannot be null
                            argumentFormat.Append(c);
                        }
                        break;
                }
            }
            if (result.Length > 0)
                result[0] = Char.ToUpperInvariant(result[0]);
            result.AppendLine();
            return result.ToString();
        }

        // Formatting
        //  ICharacter
        //      default: same as n, N
        //      n, N: argument.name if visible by target, someone otherwise
        //      e, E: he/she/it, depending on argument.sex
        //      m, M: him/her/it, depending on argument.sex
        //      s, S: his/her/its, depending on argument.sex
        ////      r, R: you or argument.name if visible by target, someone otherwise
        ////      v, V: add 's' at the end of a verb if argument is different than target
        // IItem
        //      argument.Name if visible by target, something otherwhise
        // IExit
        //      exit name
        private static void FormatActOneArgument(ICharacter target, StringBuilder result, string format, object argument)
        {
            ICharacter character = argument as ICharacter;
            if (character != null)
            {
                char letter = format == null ? 'n' : format[0]; // if no format, n
                switch (letter)
                {
                    case 'n':
                    case 'N':
                        result.Append(target.CanSee(character)
                            ? character.DisplayName
                            : "someone");
                        break;
                    case 'e':
                    case 'E':
                        result.Append(StringHelpers.Subjects[character.Sex]);
                        break;
                    case 'm':
                    case 'M':
                        result.Append(StringHelpers.Objectives[character.Sex]);
                        break;
                    case 's':
                    case 'S':
                        result.Append(StringHelpers.Possessives[character.Sex]);
                        break;
                        // TODO:
                        //case 'r':
                        //case 'R': // transforms '$r' into 'you' or '<name>' depending if target is the same as argument
                        //    if (character == target)
                        //        result.Append("you");
                        //    else
                        //        result.Append(target.CanSee(character)
                    //            ? character.DisplayName
                        //            : "someone");
                        //    break;
                        //case 'v':
                        //case 'V': // transforms 'look$s' into 'look' and 'looks' depending if target is the same as argument
                        //    if (character != target)
                        //        result.Append('s');
                        //    break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Act: invalid format {0} for ICharacter", format);
                        result.Append("<???>");
                        break;
                }
            }
            else
            {
                IItem item = argument as IItem;
                if (item != null)
                {
                    // no specific format
                    result.Append(target.CanSee(item)
                        ? item.DisplayName
                        : "something");
                }
                else
                {
                    IExit exit = argument as IExit;
                    if (exit != null)
                        result.Append(exit.Name);
                    else
                    {
                        if (format == null)
                            result.Append(argument);
                        else if (argument is IFormattable)
                        {
                            IFormattable formattable = argument as IFormattable;
                            result.Append(formattable.ToString(format, null));
                        }
                        else
                            result.Append(argument);
                    }
                }
            }
        }

        #endregion

        [Command("kill")]
        protected virtual bool DoKill(string rawParameters, params CommandParameter[] parameters)
        {
            Send("DoKill: NOT YET IMPLEMENTED" + Environment.NewLine);
            Send("==> TESTING MULTIHIT"+Environment.NewLine);

            if (parameters.Length > 0)
            {
                ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);

                if (target != null)
                    MultiHit(target);
            }
            return true;
        }

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Character: DoTest" + Environment.NewLine);
            return true;
        }
    }


    //#region IFormattable

    ////http://www.informit.com/articles/article.aspx?p=1567486
    //public string ToString(string format, IFormatProvider formatProvider)
    //{
    //    //"G" is .Net's standard for general formatting--all
    //    //types should support it
    //    if (format == null) 
    //        format = "G";

    //    // is the user providing their own format provider?
    //    if (formatProvider != null)
    //    {
    //        ICustomFormatter formatter = formatProvider.GetFormat(this.GetType()) as ICustomFormatter;
    //        if (formatter != null)
    //            return formatter.Format(format, this, formatProvider);
    //    }

    //    if (format == "G")
    //        return Name;
    //    StringBuilder sb = new StringBuilder();
    //    int sourceIndex = 0;
    //    while (sourceIndex < format.Length)
    //    {
    //        switch (format[sourceIndex])
    //        {
    //            case 'n': case 'N':
    //                sb.Append(DisplayName); // TODO: short description
    //                break;
    //            case 'e': case 'E':
    //                sb.Append(StringHelpers.Subjects[Sex]);
    //                break;
    //            case 'm': case 'M':
    //                sb.Append(StringHelpers.Objectives[Sex]);
    //                break;
    //            case 's': case 'S':
    //                sb.Append(StringHelpers.Possessives[Sex]);
    //                break;
    //            default:
    //                sb.Append(format[sourceIndex]);
    //                break;
    //        }
    //        sourceIndex++;
    //    }
    //    return sb.ToString();
    //}

    //#endregion
}
