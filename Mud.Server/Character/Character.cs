using System;
using System.Collections.Generic;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;
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
            Sex = (Sex)(int)blueprint.Sex; // TODO: better conversion
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

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Content
        {
            get { return _inventory.AsReadOnly(); }
        }

        public bool Put(IItem obj)
        {
            // TODO: check if already in a container
            _inventory.Add(obj);
            return true;
        }

        public bool Get(IItem obj)
        {
            bool removed = _inventory.Remove(obj);
            return removed;
        }

        #endregion

        public CharacterBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public IRoom Room { get; private set; }

        public IReadOnlyList<EquipmentSlot> Equipments
        {
            get
            {
                return _equipments.AsReadOnly();
            }
        }

        public Sex Sex { get; private set; }

        public bool Impersonable { get; private set; }
        public IPlayer ImpersonatedBy { get; private set; }

        public ICharacter Slave { get; private set; } // who is our slave (related to charm command/spell)
        public ICharacter ControlledBy { get; private set; } // who is our master (related to charm command/spell)

        public bool ChangeImpersonation(IPlayer player) // if non-null, start impersonation, else, stop impersonation
        {
            // TODO: check if not already impersonated, if impersonable, ...
            ImpersonatedBy = player;
            return true;
        }

        public bool ChangeController(ICharacter master) // if non-null, start slavery, else, stop slavery
        {
            // TODO: check if already slave, ...
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

        #endregion

        protected void BuildEquipmentLocation()
        {
            // TODO: depend on race+affects+...
            _equipments.Add(new EquipmentSlot(WearLocations.Light));
            _equipments.Add(new EquipmentSlot(WearLocations.Head));
            _equipments.Add(new EquipmentSlot(WearLocations.Eyes));
            _equipments.Add(new EquipmentSlot(WearLocations.Ear));
            _equipments.Add(new EquipmentSlot(WearLocations.Ear));
            _equipments.Add(new EquipmentSlot(WearLocations.Neck));
            _equipments.Add(new EquipmentSlot(WearLocations.Arms));
            _equipments.Add(new EquipmentSlot(WearLocations.Wrist));
            _equipments.Add(new EquipmentSlot(WearLocations.Wrist));
            _equipments.Add(new EquipmentSlot(WearLocations.Finger));
            _equipments.Add(new EquipmentSlot(WearLocations.Finger));
            _equipments.Add(new EquipmentSlot(WearLocations.Wield));
            _equipments.Add(new EquipmentSlot(WearLocations.Offhand));
            _equipments.Add(new EquipmentSlot(WearLocations.Body));
            _equipments.Add(new EquipmentSlot(WearLocations.About));
            _equipments.Add(new EquipmentSlot(WearLocations.Waist));
            _equipments.Add(new EquipmentSlot(WearLocations.Legs));
            _equipments.Add(new EquipmentSlot(WearLocations.Feet));
            _equipments.Add(new EquipmentSlot(WearLocations.Float));
        }

        protected void ChangeRoom(IRoom destination)
        {
            Room.Leave(this);
            Room = destination;
            destination.Enter(this);
        }

        #region Act  TODO in EntityBase ???

        public enum ActOptions
        {
            ToRoom, // everyone in the room except origin
            ToNotVictim, // everyone on in the room except Character and Target
            ToVictim, // only to Target
            ToCharacter, // only to Character
            ToAll // everyone in the room
        }

        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        public void Act(ActOptions options, string format, params object[] arguments)
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

        public void Act(ActOptions options, ICharacter victim, string format, params object[] arguments)
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
