using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("commanddebug", "Admin", Priority = 10)]
        [Syntax(
            "[cmd] admin", 
            "[cmd] player",
            "[cmd] pc",
            "[cmd] npc",
            "[cmd] item",
            "[cmd] room")]
        protected virtual CommandExecutionResults DoCommandDebug(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            string specifier = parameters[0].Value.ToLowerInvariant();
            Type type;
            if ("admin".StartsWith(specifier))
                type = typeof(Admin);
            else if ("player".StartsWith(specifier))
                type = typeof(Player.Player);
            else if ("pc".StartsWith(specifier))
                type = typeof(Character.PlayableCharacter.PlayableCharacter);
            else if ("npc".StartsWith(specifier))
                type = typeof(Character.NonPlayableCharacter.NonPlayableCharacter);
            else if ("item".StartsWith(specifier))
                type = typeof(Item.ItemBase<,>);
            else if ("room".StartsWith(specifier))
                type = typeof(Room.Room);
            else
                return CommandExecutionResults.SyntaxError;
            IReadOnlyTrie<ICommandExecutionInfo> commands = GameAction.GameActionManager.GetCommands(type);
            // Filter?
            var query = parameters.Length > 1
                // Filter using Trie, then order by priority
                ? commands.GetByPrefix(parameters[1].Value).OfType<ICommandMethodInfo>().OrderBy(x => x.Priority)
                // No filter and order by MethodInfo name
                : commands.Values.OfType<ICommandMethodInfo>().OrderBy(x => x.MethodInfo.Name);
            // Display
            StringBuilder sb = TableGenerators.CommandMethodInfoTableGenerator.Value.Generate($"Commands for {type.Name}", query);
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("resetarea", "Admin")]
        [Syntax(
            "[cmd] <area>",
            "[cmd] (if impersonated)")]
        protected virtual CommandExecutionResults DoResetArea(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 && Impersonating == null)
                return CommandExecutionResults.SyntaxError;

            IArea area;
            if (parameters.Length == 0)
                area = Impersonating.Room.Area;
            else
                area = World.Areas.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.DisplayName, parameters[0].Value));

            if (area == null)
            {
                Send("Area not found.");
                return CommandExecutionResults.TargetNotFound;
            }

            area.ResetArea();

            Send($"{area.DisplayName} resetted.");
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("peace", "Admin", MustBeImpersonated = true)]
        [Syntax("[cmd]")]
        protected virtual CommandExecutionResults DoPeace(string rawParameters, params CommandParameter[] parameters)
        {
            foreach (ICharacter character in Impersonating.Room.People)
            {
                character.StopFighting(true);
                // Needed ?
                //if (character is INonPlayableCharacter npc && npc.ActFlags.HasFlag(ActFlags.Aggressive))
                //    npc.Remove
            }
            Send("Ok.");

            return CommandExecutionResults.Ok;
        }
    }
}
