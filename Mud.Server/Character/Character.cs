using System;
using System.Reflection;
using Mud.DataStructures;
using Mud.DataStructures.Trie;
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

        #endregion

        protected void ChangeRoom(IRoom destination)
        {
            Room.Leave(this);
            Room = destination;
            destination.Enter(this);
        }

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
}
