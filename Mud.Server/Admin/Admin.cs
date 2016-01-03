using System;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    // TODO: override ProcessCommand  if Incarnating, process command by Incarnation instead of this
    public class Admin : Player.Player, IAdmin
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> AdminCommands;

        static Admin()
        {
            AdminCommands = CommandHelpers.GetCommands(typeof(Admin));
        }

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return AdminCommands; }
        }

        #endregion

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

        public IEntity Incarnating { get; private set; }

        #endregion

        [Command("incarnate")]
        protected virtual bool DoIncarnate(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Incarnating != null)
                {
                    Send("You stop incarnating {0}." + Environment.NewLine, Incarnating.DisplayName);
                    Incarnating.ChangeIncarnation(null);
                }
                else
                    Send("Syntax: Incarnate <kind> <name|id>"+Environment.NewLine);
            }
            else if (parameters.Length == 1)
                Send("Syntax: Incarnate <kind> <name|id>" + Environment.NewLine);
            else if (parameters.Length == 2)
            {
                IEntity incarnateTarget = null;
                string kind = parameters[0].Value;
                if ("room".StartsWith(kind))
                    incarnateTarget = FindHelpers.FindByName(World.World.Instance.GetRooms(), parameters[1]);
                else if ("item".StartsWith(kind))
                    incarnateTarget = FindHelpers.FindByName(World.World.Instance.GetItems(), parameters[1]);
                else if ("mob".StartsWith(kind))
                    incarnateTarget = FindHelpers.FindByName(World.World.Instance.GetCharacters(), parameters[1]);
                if (incarnateTarget == null)
                    Send("Target not found");
                else
                {
                    if (Incarnating != null)
                    {
                        Send("You stop incarnating {0}." + Environment.NewLine, Incarnating.DisplayName);
                        Incarnating.ChangeIncarnation(null);
                    }
                    Send("%M%You start incarnating %C%{0}%x%." + Environment.NewLine, incarnateTarget.DisplayName);
                    incarnateTarget.ChangeIncarnation(this);
                    Incarnating = incarnateTarget;
                    PlayerState = PlayerStates.Impersonating;
                }
            }
            return true;
        }

        [Command("shutdow", Hidden = true)] // TODO: add an option in CommandAttribute to force full command to be type
        protected virtual bool DoShutdow(string rawParameters, params CommandParameter[] parameters)
        {
            Send("If you want to SHUTDOWN, spell it out." + Environment.NewLine);
            return true;
        }

        [Command("shutdown")]
        protected virtual bool DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds = 0;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                Send("Syntax: shutdown xxx  where xxx is a delay in seconds." + Environment.NewLine);
            else if (seconds < 30)
                Send("You cannot shutdown that fast." + Environment.NewLine);
            else
                Server.Server.Instance.Shutdown(seconds);
            return true;
        }

        [Command("who")]
        protected override bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Players:" + Environment.NewLine);
            foreach (IPlayer player in Server.Server.Instance.GetPlayers())
            {
                StringBuilder sb = new StringBuilder();
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} playing {1}", player.DisplayName, player.Impersonating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} playing ???", player.DisplayName);
                        break;
                    default:
                        sb.AppendFormat("[OOG] {0} {1}", player.DisplayName, player.PlayerState);
                        break;
                }
                sb.AppendLine();
                Send(sb);
            }
            Send("Admins" + Environment.NewLine);
            foreach (IAdmin admin in Server.Server.Instance.GetAdmins())
            {
                StringBuilder sb = new StringBuilder();
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} impersonating {1}", admin.DisplayName, admin.Impersonating.Name);
                        else if (admin.Incarnating != null)
                            sb.AppendFormat("[ IG] {0} incarnating {1}", admin.DisplayName, admin.Incarnating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} nor playing nor incarnating !!!", admin.DisplayName);
                        break;
                    default:
                        sb.AppendFormat("[OOG] {0} {1}", admin.DisplayName, admin.PlayerState);
                        break;
                }
                sb.AppendLine();
                Send(sb);
            }
            return true;
        }
    }
}
