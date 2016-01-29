using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public enum AvatarCreationStates
    {
        NameChoice, // -> NameConfirmation | NameChoice
        NameConfirmation, // -> NameChoice | RaceChoice
        RaceChoice, // -> RaceChoice | ClassChoice
        ClassChoice, // -> ClassChoice | AvatarCreated | ImmediateImpersonate
        ImmediateImpersonate, // -> CreationComplete
        CreationComplete
    }
    public class AvatarCreationStateMachine : InputTrapBase<IPlayer, AvatarCreationStates>
    {
        private string _name;
        private IRace _race;
        private IClass _class;

        public override bool IsFinalStateReached
        {
            get { return State == AvatarCreationStates.CreationComplete; }
        }

        public AvatarCreationStateMachine()
        {
            PreserveInput = false;
            StateMachine = new Dictionary<AvatarCreationStates, Func<IPlayer, string, AvatarCreationStates>>
            {
                {AvatarCreationStates.NameChoice, ProcessNameChoice},
                {AvatarCreationStates.NameConfirmation, ProcessNameConfirmation},
                {AvatarCreationStates.RaceChoice, ProcessRaceChoice},
                {AvatarCreationStates.ClassChoice, ProcessClassChoice},
                {AvatarCreationStates.ImmediateImpersonate, ProcessImmediateImpersonate},
                {AvatarCreationStates.CreationComplete, ProcessCreationComplete}
            };
            State = AvatarCreationStates.NameChoice;
        }

        private AvatarCreationStates ProcessNameChoice(IPlayer player, string input)
        {
            // TODO: name validation
            if (!String.IsNullOrWhiteSpace(input))
            {
                _name = input;
                player.Send("Are you sure '{0}' is the name of your avatar? (y/n)" + Environment.NewLine, StringHelpers.UpperFirstLetter(_name));
                return AvatarCreationStates.NameConfirmation;
            }
            player.Send("Please enter a name:" + Environment.NewLine);
            return AvatarCreationStates.NameChoice;
        }

        private AvatarCreationStates ProcessNameConfirmation(IPlayer player, string input)
        {
            if (input == "y" || input == "yes")
            {
                player.Send("Great! Please choose a race."+Environment.NewLine);
                DisplayRaceList(player, false);
                return AvatarCreationStates.RaceChoice;
            }
            player.Send("Ok, what name would you give to your avatar?");
            return AvatarCreationStates.NameChoice;
        }

        private AvatarCreationStates ProcessRaceChoice(IPlayer player, string input)
        {
            List<IRace> races = Repository.RaceManager.Races.Where(x => FindHelpers.StringStartWith(x.Name, input)).ToList();
            if (races.Count == 1)
            {
                _race = races[0];
                player.Send("Good choice! Now, please choose a class."+Environment.NewLine);
                DisplayClassList(player, false);
                return AvatarCreationStates.ClassChoice;
            }
            DisplayRaceList(player);
            return AvatarCreationStates.RaceChoice;
        }

        private AvatarCreationStates ProcessClassChoice(IPlayer player, string input)
        {
            List<IClass> classes = Repository.ClassManager.Classes.Where(x => FindHelpers.StringStartWith(x.Name, input)).ToList();
            if (classes.Count == 1)
            {
                _class = classes[0];
                // TODO: Add character to impersonate list
                player.Save();
                // TODO: better wording
                player.Send("Your avatar is created. Name: {0} Race: {1} Class: {2}."+Environment.NewLine, StringHelpers.UpperFirstLetter(_name), _race.DisplayName, _class.DisplayName);
                player.Send("Would you like to impersonate it now? (y/n)"+Environment.NewLine);
                return AvatarCreationStates.ImmediateImpersonate;
            }
            DisplayClassList(player);
            return AvatarCreationStates.ClassChoice;
        }

        private AvatarCreationStates ProcessImmediateImpersonate(IPlayer player, string input)
        {
            if (input == "y" || input == "yes")
            {
                // TODO: impersonate character
                // TODO: return AvatarCreationStates.CreationComplete;
            }
            player.Send("Avatar {0} created but not impersonated. Use /impersonate {0} to enter game or use /list to see your avatar list."+Environment.NewLine, StringHelpers.UpperFirstLetter(_name));
            // else, NOP
            return AvatarCreationStates.CreationComplete;
        }

        private AvatarCreationStates ProcessCreationComplete(IPlayer player, string input)
        {
            // fall-thru
            return AvatarCreationStates.CreationComplete;
        }

        private void DisplayRaceList(IPlayer player, bool displayHeader = true)
        {
            if (displayHeader)
                player.Send("Please choose a race." + Environment.NewLine);
            string races = String.Join(" | ", Repository.RaceManager.Races.Select(x => x.DisplayName));
            player.Send(races + Environment.NewLine);
        }

        private void DisplayClassList(IPlayer player, bool displayHeader = true)
        {
            if (displayHeader)
                player.Send("Please choose a class." + Environment.NewLine);
            string classes = String.Join(" | ", Repository.ClassManager.Classes.Select(x => x.DisplayName));
            player.Send(classes + Environment.NewLine);
        }
    }
}
