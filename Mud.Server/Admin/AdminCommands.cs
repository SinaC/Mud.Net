using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin
    {
        [AdminCommand("promote", "Admin", Priority = 999, NoShortcut = true, MinLevel = AdminLevels.Supremacy, CannotBeImpersonated = true)]
        [Syntax("[cmd] <player name> <level>")]
        protected virtual CommandExecutionResults DoPromote(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                return CommandExecutionResults.SyntaxError;

            // whom
            IPlayer player = FindHelpers.FindByName(PlayerManager.Players, parameters[0], true);
            if (player == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            if (player == this)
            {
                Send("You cannot promote yourself.");
                return CommandExecutionResults.InvalidTarget;
            }

            if (player is IAdmin)
            {
                Send("{0} is already Admin", player.DisplayName);
                return CommandExecutionResults.InvalidTarget;
            }

            // what
            AdminLevels level;
            if (!EnumHelpers.TryFindByName(parameters[1].Value, out level))
            {
                Send("{0} is not a valid admin levels. Values are : {1}", parameters[1].Value, string.Join(", ", EnumHelpers.GetValues<AdminLevels>().Select(x => x.ToString())));
                return CommandExecutionResults.InvalidParameter;
            }

            ServerAdminCommand.Promote(player, level);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("shutdown", "Admin", Priority = 999 /*low priority*/, NoShortcut = true, MinLevel = AdminLevels.Implementor, CannotBeImpersonated = true)]
        [Syntax("[cmd] <delay>")]
        protected virtual CommandExecutionResults DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                return CommandExecutionResults.SyntaxError;
            if (seconds < 30)
            {
                Send("You cannot shutdown that fast.");
                return CommandExecutionResults.InvalidParameter;
            }
            ServerAdminCommand.Shutdown(seconds);
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("cload", "Admin", Priority = 10, MustBeImpersonated = true)]
        [AdminCommand("mload", "Admin", Priority = 10, MustBeImpersonated = true)]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoCload(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 || !parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            CharacterBlueprintBase characterBlueprint = World.GetCharacterBlueprint(parameters[0].AsNumber);
            if (characterBlueprint == null)
            {
                Send("No character with that id.");
                return CommandExecutionResults.TargetNotFound;
            }

            INonPlayableCharacter character = World.AddNonPlayableCharacter(Guid.NewGuid(), characterBlueprint, Impersonating.Room);
            if (character == null)
            {
                Send("Character cannot be created.");
                Wiznet.Wiznet($"DoCload: character with id {parameters[0].AsNumber} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                return CommandExecutionResults.Error;
            }

            Wiznet.Wiznet($"{DisplayName} loads {character.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1:n}!", Impersonating, character);
            Send("Ok.");

            return CommandExecutionResults.Ok;
        }

        [AdminCommand("iload", "Admin", Priority = 10, MustBeImpersonated = true)]
        [AdminCommand("oload", "Admin", Priority = 10, MustBeImpersonated = true)]
        [Syntax("[cmd] <id>")]
        protected virtual CommandExecutionResults DoIload(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 || !parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;

            ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(parameters[0].AsNumber);
            if (itemBlueprint == null)
            {
                Send("No item with that id.");
                return CommandExecutionResults.TargetNotFound;
            }

            IContainer container = itemBlueprint.WearLocation == WearLocations.None
                ? Impersonating.Room as IContainer
                : Impersonating as IContainer;
            IItem item = World.AddItem(Guid.NewGuid(), itemBlueprint, container);
            if (item == null)
            {
                Send("Item cannot be created.");
                Wiznet.Wiznet($"DoIload: item with id {parameters[0].AsNumber} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                return CommandExecutionResults.Error;
            }

            Wiznet.Wiznet($"{DisplayName} loads {item.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1}!", Impersonating, item);
            Send("Ok.");
            return CommandExecutionResults.Ok;
        }

        [AdminCommand("slay", "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
        [Syntax("[cmd] <character>")]
        protected virtual CommandExecutionResults DoSlay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            ICharacter victim = FindHelpers.FindByName(Impersonating.Room.People, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            if (victim == Impersonating)
            {
                Send("Suicide is a mortal sin.");
                return CommandExecutionResults.InvalidTarget;
            }

            Wiznet.Wiznet($"{DisplayName} slayed {victim.DebugName}.", WiznetFlags.Punish);

            victim.Act(ActOptions.ToAll, "{0:N} slay{0:v} {1} in cold blood!", Impersonating, victim);
            victim.Slay(Impersonating);

            return CommandExecutionResults.Ok;
        }

        [AdminCommand("purge", "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
        [Syntax(
            "[cmd] all",
            "[cmd] <character>",
            "[cmd] <item>")]
        protected virtual CommandExecutionResults DoPurge(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            // TODO: room NoPurge

            // purge room
            if (parameters[0].IsAll)
            {
                // Purge non playable characters (TODO: what if npc was wearing NoPurge items?)
                IReadOnlyCollection<INonPlayableCharacter> nonPlayableCharacters = new ReadOnlyCollection<INonPlayableCharacter>(Impersonating.Room.NonPlayableCharacters.ToList()); // clone
                foreach (INonPlayableCharacter nonPlayableCharacter in nonPlayableCharacters)
                    World.RemoveCharacter(nonPlayableCharacter);
                // Purge items (with NoPurge flag)
                IReadOnlyCollection<IItem> items = new ReadOnlyCollection<IItem>(Impersonating.Room.Content.Where(x => !x.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)).ToList()); // clone
                foreach (IItem itemToPurge in items)
                    World.RemoveItem(itemToPurge);
                Impersonating.Act(ActOptions.ToRoom, "{0} purge{0:v} the room!", Impersonating);
                Send("Ok.");
                return CommandExecutionResults.Ok;
            }

            // non playable character
            INonPlayableCharacter nonPlayableCharacterVictim = FindHelpers.FindNonPlayableChararacterInWorld(Impersonating, parameters[0]);
            if (nonPlayableCharacterVictim != null)
            {
                nonPlayableCharacterVictim.Act(ActOptions.ToRoom, "{0} purge{0:v} {1}.", Impersonating, nonPlayableCharacterVictim);
                World.RemoveCharacter(nonPlayableCharacterVictim);
            }

            // playable character
            IPlayableCharacter playableCharacterVictim = FindHelpers.FindPlayableChararacterInWorld(Impersonating, parameters[0]);
            if (playableCharacterVictim != null)
            {
                if (playableCharacterVictim == this)
                {
                    Send("Ho ho ho.");
                    return CommandExecutionResults.InvalidTarget;
                }
                playableCharacterVictim.Act(ActOptions.ToRoom, "{0} disintegrate{0:v} {1}.", Impersonating, playableCharacterVictim);
                playableCharacterVictim.StopFighting(true);
                if (playableCharacterVictim.ImpersonatedBy != null)
                    playableCharacterVictim.ChangeImpersonation(null);
                World.RemoveCharacter(playableCharacterVictim);
            }

            // item
            IItem item = FindHelpers.FindItemHere(Impersonating, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge))
            {
                Send("It can't be purged.");
                return CommandExecutionResults.InvalidTarget;
            }

            Wiznet.Wiznet($"{DisplayName} purges {item.DebugName}.", WiznetFlags.Punish);

            Impersonating.Act(ActOptions.ToAll, "{0:N} purge{0:v} {1}.", Impersonating, item);
            World.RemoveItem(item);

            return CommandExecutionResults.Ok;
        }

        [AdminCommand("goto", "Admin", MustBeImpersonated = true)]
        [Syntax("[cmd] <location>")]
        protected virtual CommandExecutionResults DoGoto(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            //
            IRoom where = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (where == null)
            {
                Send("No such location.");
                return CommandExecutionResults.TargetNotFound;
            }

            if (Impersonating.Fighting != null)
                Impersonating.StopFighting(true);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} leaves in a swirling mist.", Impersonating); // Don't display 'Someone leaves ...' if Impersonating is not visible
            Impersonating.ChangeRoom(where);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} appears in a swirling mist.", Impersonating);
            Impersonating.AutoLook();

            return CommandExecutionResults.Ok;
        }

        [Command("xpbonus", "Admin")]
        [Syntax("[cmd] <player name> <experience>")]
        protected virtual CommandExecutionResults DpXpBonus(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                return CommandExecutionResults.SyntaxError;

            IPlayableCharacter victim = FindHelpers.FindByName(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), parameters[0]);
            if (victim == null)
            {
                Send("That impersonated player is not here.");
                return CommandExecutionResults.TargetNotFound;
            }

            int experience = parameters[1].AsNumber;
            if (experience < 1)
            {
                Send("Experience must be greater than 1.");
                return CommandExecutionResults.InvalidParameter;
            }

            if (victim.Level >= Settings.MaxLevel)
            {
                Send($"{DisplayName} is already at max level.");
                return CommandExecutionResults.InvalidTarget;
            }

            Wiznet.Wiznet($"{DisplayName} give experience [{experience}] to {victim.DebugName}.", WiznetFlags.Help);

            victim.Send("You have received an experience boost.");
            victim.GainExperience(experience);

            //
            victim.ImpersonatedBy.Save();
            return CommandExecutionResults.Ok;
        }

        [Command("transfer", "Admin")]
        [Syntax(
            "[cmd] <character> (if impersonated)",
            "[cmd] <character> <location>")]
        protected virtual CommandExecutionResults DoTransfer(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;
            if (Impersonating == null && parameters.Length == 1)
            {
                Send("Transfer without specifying location can only be used when impersonating.");
                return CommandExecutionResults.InvalidParameter;
            }

            // TODO: IsAll ?

            IRoom where;
            if (Impersonating != null)
                where = parameters.Length == 1
                    ? Impersonating.Room
                    : FindHelpers.FindLocation(Impersonating, parameters[1]);
            else
                where = FindHelpers.FindLocation(parameters[1]);
            if (where == null)
            {
                Send("No such location.");
                return CommandExecutionResults.TargetNotFound;
            }

            ICharacter whom;
            if (Impersonating != null)
                whom = FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            else
                whom = FindHelpers.FindByName(World.Characters, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (whom.Fighting != null)
                whom.StopFighting(true);
            whom.Act(ActOptions.ToRoom, "{0:N} disappears in a mushroom cloud.", whom);
            whom.ChangeRoom(where);
            whom.Act(ActOptions.ToRoom, "{0:N} appears from a puff of smoke.", whom);
            if (whom != Impersonating)
            {
                if (Impersonating != null)
                    whom.Act(ActOptions.ToCharacter, "{0:N} has transferred you.", Impersonating);
                else
                    whom.Act(ActOptions.ToCharacter, "Someone has transferred you.");
            }
            whom.AutoLook();

            Send("Ok");
            return CommandExecutionResults.Ok;
        }

        [Command("sanitycheck", "Admin")]

        protected virtual CommandExecutionResults DoSanityCheck(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            IPlayer whom = FindHelpers.FindByName(PlayerManager.Players, parameters[0]);

            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            StringBuilder info = whom.PerformSanityCheck();
            Page(info);

            return CommandExecutionResults.Ok;
        }

        [Command("commanddebug", "Admin", Priority = 10)]
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
                type = typeof(Item.ItemBase<>);
            else if ("room".StartsWith(specifier))
                type = typeof(Room.Room);
            else
                return CommandExecutionResults.SyntaxError;
            IReadOnlyTrie<CommandMethodInfo> commands = CommandHelpers.GetCommands(type);
            // Filter?
            IEnumerable<CommandMethodInfo> query;
            if (parameters.Length > 1)
            {
                // Filter using Trie, then order by priority
                query = commands.GetByPrefix(parameters[1].Value).OrderBy(x => x.Value.Attribute.Priority).Select(x => x.Value);
            }
            else
            {
                // No filter and order by MethodInfo name
                query = commands.Values.OrderBy(x => x.MethodInfo.Name);
            }

            // Display
            StringBuilder sb = TableGenerators.CommandMethodInfoTableGenerator.Value.Generate($"Commands for {type.Name}", query);
            Page(sb);
            return CommandExecutionResults.Ok;
        }
    }
}
