using System;
using System.Linq;
using System.Text;
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
        [AdminCommand("promote", Category = "Admin", Priority = 999, NoShortcut = true, MinLevel = AdminLevels.Supremacy, CannotBeImpersonated = true)]
        protected virtual bool DoPromote(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length != 2)
            {
                Send("Syntax: promote <player> <level>");
                return true;
            }

            // whom
            IPlayer player = FindHelpers.FindByName(PlayerManager.Players, parameters[0], true);
            if (player == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (player == this)
            {
                Send("You cannot promote yourself.");
                return true;
            }

            if (player is IAdmin)
            {
                Send("{0} is already Admin", player.DisplayName);
                return true;
            }

            // what
            AdminLevels level;
            if (!EnumHelpers.TryFindByName(parameters[1].Value, out level))
            {
                Send("{0} is not a valid admin levels. Values are : {1}", parameters[1].Value, string.Join(",", EnumHelpers.GetValues<AdminLevels>().Select(x => x.ToString())));
                return true;
            }

            ServerAdminCommand.Promote(player, level);

            return true;
        }

        [AdminCommand("shutdown", Category = "Admin", Priority = 999 /*low priority*/, NoShortcut = true, MinLevel = AdminLevels.Implementor, CannotBeImpersonated = true)]
        protected virtual bool DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                Send("Syntax: shutdown <delay>");
            else if (seconds < 30)
                Send("You cannot shutdown that fast.");
            else
                ServerAdminCommand.Shutdown(seconds);
            return true;
        }

        [AdminCommand("cload", Category = "Admin", Priority = 10, MustBeImpersonated = true)]
        [AdminCommand("mload", Category = "Admin", Priority = 10, MustBeImpersonated = true)]
        protected virtual bool DoCload(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 || !parameters[0].IsNumber)
            {
                Send("Syntax: cload <id>");
                return true;
            }

            CharacterBlueprintBase characterBlueprint = World.GetCharacterBlueprint(parameters[0].AsNumber);
            if (characterBlueprint == null)
            {
                Send("No character with that id.");
                return true;
            }

            INonPlayableCharacter character = World.AddNonPlayableCharacter(Guid.NewGuid(), characterBlueprint, Impersonating.Room);
            if (character == null)
            {
                Send("Character cannot be created.");
                Wiznet.Wiznet($"DoCload: character with id {parameters[0].AsNumber} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                return true;
            }

            Wiznet.Wiznet($"{DisplayName} loads {character.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1:n}!", Impersonating, character);
            Send("Ok.");

            return true;
        }

        [AdminCommand("iload", Category = "Admin", Priority = 10, MustBeImpersonated = true)]
        [AdminCommand("oload", Category = "Admin", Priority = 10, MustBeImpersonated = true)]
        protected virtual bool DoIload(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0 || !parameters[0].IsNumber)
            {
                Send("Syntax: iload <id>");
                return true;
            }

            ItemBlueprintBase itemBlueprint = World.GetItemBlueprint(parameters[0].AsNumber);
            if (itemBlueprint == null)
            {
                Send("No item with that id.");
                return true;
            }

            IContainer container = itemBlueprint.WearLocation == WearLocations.None
                ? Impersonating.Room as IContainer
                : Impersonating as IContainer;
            IItem item = World.AddItem(Guid.NewGuid(), itemBlueprint, container);
            if (item == null)
            {
                Send("Item cannot be created.");
                Wiznet.Wiznet($"DoIload: item with id {parameters[0].AsNumber} cannot be created", WiznetFlags.Bugs, AdminLevels.Implementor);
                return true;
            }

            Wiznet.Wiznet($"{DisplayName} loads {item.DebugName}.", WiznetFlags.Load);

            Impersonating.Act(ActOptions.ToAll, "{0:N} {0:h} created {1}!", Impersonating, item);
            Send("Ok.");

            return true;
        }

        [AdminCommand("slay", Category = "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
        protected virtual bool DoSlay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Slay whom?");
                return true;
            }
            ICharacter victim = FindHelpers.FindByName(Impersonating.Room.People, parameters[0]);
            if (victim == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }
            if (victim == Impersonating)
            {
                Send("Suicide is a mortal sin.");
                return true;
            }

            Wiznet.Wiznet($"{DisplayName} slayed {victim.DebugName}.", WiznetFlags.Punish);

            victim.Act(ActOptions.ToAll, "{0:N} slay{0:v} {1} in cold blood!", Impersonating, victim);
            victim.Slay(Impersonating);

            return true;
        }

        [AdminCommand("purge", Category = "Admin", Priority = 999, NoShortcut = true, MustBeImpersonated = true)]
        protected virtual bool DoPurge(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Purge what?");
                return true;
            }

            IItem item = FindHelpers.FindItemHere(Impersonating, parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemNotFound);
                return true;
            }

            Wiznet.Wiznet($"{DisplayName} purges {item.DebugName}.", WiznetFlags.Punish);

            Impersonating.Act(ActOptions.ToAll, "{0:N} purge{0:v} {1}!", Impersonating, item);
            World.RemoveItem(item);

            return true;
        }

        [AdminCommand("goto", Category = "Admin", MustBeImpersonated = true)]
        protected virtual bool DoGoto(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Goto where?");
                return true;
            }

            //
            IRoom where = FindHelpers.FindLocation(Impersonating, parameters[0]);
            if (where == null)
            {
                Send("No such location.");
                return true;
            }

            if (Impersonating.Fighting != null)
                Impersonating.StopFighting(true);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} leaves in a swirling mist.", Impersonating); // Don't display 'Someone leaves ...' if Impersonating is not visible
            Impersonating.ChangeRoom(where);
            Impersonating.Act(Impersonating.Room.People.Where(x => x != Impersonating && x.CanSee(Impersonating)), "{0} appears in a swirling mist.", Impersonating);
            Impersonating.AutoLook();

            return true;
        }

        [Command("xpbonus", Category = "Admin")]
        protected virtual bool DpXpBonus(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Syntax: xpgain <character> <experience>");
                return true;
            }

            IPlayableCharacter victim = FindHelpers.FindByName(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), parameters[0]);
            if (victim == null)
            {
                Send("That impersonated player is not here.");
                return true;
            }

            int experience = parameters[1].AsNumber;
            if (experience < 1)
            {
                Send("Experience must be greater than 1.");
                return true;
            }

            if (victim.Level >= Settings.MaxLevel)
            {
                Send($"{DisplayName} is already at max level.");
                return true;
            }

            Wiznet.Wiznet($"{DisplayName} give experience [{experience}] to {victim.DebugName}.", WiznetFlags.Help);

            victim.Send("You have received an experience boost.");
            victim.GainExperience(experience);

            //
            victim.ImpersonatedBy.Save();
            return true;
        }

        [Command("transfer", Category = "Admin")]
        protected virtual bool DoTransfer(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Transfer whom (and where)?");
                return true;
            }
            if (Impersonating == null && parameters.Length == 1)
            {
                Send("Transfer without specifying location can only be used when impersonating.");
                return true;
            }

            // TODO: IsAll ?

            IRoom where;
            if (Impersonating != null)
            {
                where = parameters.Length == 1
                    ? Impersonating.Room
                    : FindHelpers.FindLocation(Impersonating, parameters[1]);
            }
            else
                where = FindHelpers.FindLocation(parameters[1]);
            if (where == null)
            {
                Send("No such location.");
                return true;
            }

            ICharacter whom;
            if (Impersonating != null)
                whom = FindHelpers.FindChararacterInWorld(Impersonating, parameters[0]);
            else
                whom = FindHelpers.FindByName(World.Characters, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
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
            return true;
        }

        [Command("sanitycheck", Category = "Admin")]
        protected virtual bool DoSanityCheck(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Perform sanity check on which player/admin?");
                return true;
            }

            IPlayer whom = FindHelpers.FindByName(PlayerManager.Players, parameters[0]);

            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            StringBuilder info = whom.PerformSanityCheck();
            Page(info);

            return true;
        }
    }
}
