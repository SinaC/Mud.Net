using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mud.Server.Admin
{
    public class Admin : Player.Player, IAdmin
    {
        private static readonly IReadOnlyDictionary<string, MethodInfo> AdminCommands;

        static Admin()
        {
            AdminCommands = CommandHelpers.GetCommands(typeof(Admin));
        }

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        #region IActor

        public override IReadOnlyDictionary<string, MethodInfo> Commands
        {
            get { return AdminCommands; }
        }

        #endregion

        #region IPlayer

        public override void OnDisconnected()
        {
            base.OnDisconnected();
            
            // Stop incarnation if any
            if (Incarnating != null)
            {
                Incarnating.ChangeIncarnation(null);
                Incarnating = null;
            }
        }

        #endregion

        #region IAdmin

        public IEntity Incarnating { get; private set; }

        #endregion

        [Command("incarnate")]
        protected virtual bool Incarnate(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }
    }
}
