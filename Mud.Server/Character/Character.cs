using System;
using System.Reflection;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Blueprints;
using Mud.Server.Input;

namespace Mud.Server.Character
{
    public partial class Character : EntityBase, ICharacter
    {
        private static readonly Trie<MethodInfo> CharacterCommands;

        static Character()
        {
            CharacterCommands = CommandHelpers.GetCommands(typeof (Character));
        }

        #region IActor

        public Character(Guid guid, string name, IRoom room) 
            : base(guid, name)
        {
            Room = room;
            room.Enter(this);
        }

        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return CharacterCommands; }
        }

        public override void Send(string format, params object[] parameters)
        {
            base.Send(format, parameters);
            if (ImpersonatedBy != null)
            {
                if (ServerOptions.Instance.PrefixForwardedMessages)
                    format = "<IMP|" + Name + ">" + format;
                ImpersonatedBy.Send(format, parameters);
            }
            // TODO: do we really need to receive message sent to slave ?
            if (ServerOptions.Instance.ForwardSlaveMessages && ControlledBy != null)
            {
                if (ServerOptions.Instance.PrefixForwardedMessages)
                    format = "<CTRL|" + Name + ">" + format;
                ControlledBy.Send(format, parameters);
            }
        }

        #endregion

        #region ICharacter

        public CharacterBlueprint Blueprint { get; private set; } // TODO: 1st parameter in ctor

        public IRoom Room { get; private set; }

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

        public bool CanSee(IObject obj)
        {
            return true; // TODO
        }

        #endregion

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

        private enum ActParsingStates
        {
            Normal,
            OpeningBracketFound,
            ArgumentFound,
            FormatSeparatorFound,
        }

        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        public void Act(ActOptions options, string format, params object[] arguments)
        {
            if (options == ActOptions.ToNotVictim || options == ActOptions.ToVictim) // TODO: exception ???
                Log.Default.WriteLine(LogLevels.Error, "Act: victim must be specified to use ToNotVictim or ToVictim");
            else if (options == ActOptions.ToAll || options == ActOptions.ToRoom)
                foreach (ICharacter to in Room.CharactersInRoom)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != this))
                    {
                        string phrase = FormatOneLine(to, format, arguments);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = FormatOneLine(this, format, arguments);
                Send(phrase);
            }
        }

        public void Act(ActOptions options, ICharacter victim, string format, params object[] arguments)
        {
            if (options == ActOptions.ToAll || options == ActOptions.ToRoom || options == ActOptions.ToNotVictim)
                foreach (ICharacter to in Room.CharactersInRoom)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != this)
                        || (options == ActOptions.ToNotVictim && to != victim))
                    {
                        string phrase = FormatOneLine(to, format, arguments);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = FormatOneLine(this, format, arguments);
                Send(phrase);
            }
            else if (options == ActOptions.ToVictim)
            {
                string phrase = FormatOneLine(victim, format, arguments);
                victim.Send(phrase);
            }
        }

        // Recreate behaviour of String.Format with maximum 10 arguments
        // If one argument is ICharacter, IObject, IExit special formatting is applied (depending on who'll receive the message)
        private static string FormatOneLine(ICharacter target, string format, params object[] arguments)
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
                            FormatOneArgument(target, result, null, currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else if (c == ':')
                            state = ActParsingStates.FormatSeparatorFound;
                        break;
                    case ActParsingStates.FormatSeparatorFound: // searching for }
                        if (c == '}')
                        {
                            FormatOneArgument(target, result, argumentFormat.ToString(), currentArgument);
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
            return result.ToString();
        }

        private static void FormatOneArgument(ICharacter target, StringBuilder result, string format, object argument)
        {
            if (argument is ICharacter)
            {
                ICharacter character = argument as ICharacter;
                char letter = format == null ? 'n' : format[0]; // if no format, n
                switch (letter)
                {
                    case 'n':
                    case 'N':
                        result.Append(target.CanSee(character)
                            ? character.Name // TODO: short description
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
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Act: invalid format {0} for ICharacter", format);
                        result.Append("<???>");
                        break;
                }
            }
            else if (argument is IObject)
            {
                IObject obj = argument as IObject;
                // no specific format
                result.Append(target.CanSee(obj)
                    ? obj.Name // TODO: short description
                    : "something");
            }
            else if (argument is IExit)
            {
                IExit exit = argument as IExit;
                result.Append(exit.Name);
            }
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
        #endregion

        [Command("look")]
        protected virtual bool DoLook(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("kill")]
        protected virtual bool DoKill(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, CommandParameter[] parameters)
        {
            Send("Sending myself [{0}] a message", Name);

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
    //                sb.Append(Name); // TODO: short description
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
