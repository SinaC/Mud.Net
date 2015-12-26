using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mud.Server.Character
{
    public partial class Character : EntityBase, ICharacter
    {
        private static readonly IReadOnlyDictionary<string, MethodInfo> CharacterCommands;

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

        public override IReadOnlyDictionary<string, MethodInfo> Commands
        {
            get { return CharacterCommands; }
        }

        public override void Send(string format, params object[] parameters)
        {
            base.Send(format, parameters);
            if (ImpersonatedBy != null)
                ImpersonatedBy.Send("<IMP|"+Name+">"+format, parameters);
            // TODO: do we really need to receive message sent to slave ?
            //if (ControlledBy != null)
            //    ControlledBy.Send("<CTRL|" + Name + ">" + format, parameters);
        }

        #endregion

        #region IEntity

        public override void OnRemoved()
        {
            base.OnRemoved();
            // TODO: warn ImpersonatedBy about removing
        }

        #endregion

        #region ICharacter

        public IRoom Room { get; private set; }

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

        #endregion

        [Command("look")]
        protected virtual bool Look(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("kill")]
        protected virtual bool Kill(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("test")]
        protected virtual bool Test(string rawParameters, CommandParameter[] parameters)
        {
            Send("Sending myself [{0}] a message", Name);

            return true;
        }
    }
}
