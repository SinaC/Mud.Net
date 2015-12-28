using System;
using System.Collections.Generic;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public enum LoginStates
    {
        Username, // -> Username | Password | UsernameConfirm
        Password, // -> Password | Connected | Disconnect
        UsernameConfirm, // -> NewPassword1
        NewPassword1, // -> NewPassword2
        NewPassword2, // -> Username | Connected
        Connected,
        Disconnect,
    }

    public class LoginStateMachine : InputTrapBase<IPlayer, LoginStates>
    {
        public const int MaxPasswordTries = 3;
        private int _invalidPasswordTries = 0;

        private string _username;
        private string _password; // todo: encryption

        public bool IsRunning // true is not in final state
        {
            get { return State != LoginStates.Connected && State != LoginStates.Disconnect; }
        }

        public bool IsLoginSuccessfull
        {
            get { return State == LoginStates.Connected; }
        }

        public LoginStateMachine()
        {
            PreserveInput = false;
            StateMachine = new Dictionary<LoginStates, Func<IPlayer, string, LoginStates>>
            {
                { LoginStates.Username, ProcessUsername },
                { LoginStates.Password, ProcessPassword },
                { LoginStates.UsernameConfirm, ProcessUsernameConfirm },
                { LoginStates.NewPassword1, ProcessNewPassword1 },
                { LoginStates.NewPassword2, ProcessNewPassword2 },
                { LoginStates.Connected, ProcessConnected },
                { LoginStates.Disconnect, ProcessDisconnect }
            };
            State = LoginStates.Username;
        }

        private LoginStates ProcessUsername(IPlayer player, string input)
        {
            // Reset password tries
            _invalidPasswordTries = 0;
            //
            if (!String.IsNullOrWhiteSpace(input))
            {
                _username = input;
                // TODO: Check if name exists
                bool known = false;
                if (_username == "sinac")
                {
                    known = true;
                }

                // If known, greets and asks for password
                // Else, name confirmation
                if (known)
                {
                    player.Send("Welcome back, {0}! Please enter your password:", StringHelpers.UpperFirstLetter(_username));
                    PreserveInput = true;
                    return LoginStates.Password;
                }
                else
                {
                    player.Send("Are you sure this is the account name you wish to use? (y/n)");
                    PreserveInput = false;
                    return LoginStates.UsernameConfirm;
                }
            }
            else
            {
                player.Send("Please enter a name:");
                PreserveInput = false;
                return LoginStates.Username;
            }
        }

        private LoginStates ProcessPassword(IPlayer player, string input)
        {
            // If password is correct, go to final state
            // Else, 
            //      If too many try, disconnect
            //      Else, retry password
            bool passwordCorrect = input == "password"; // TODO: check password + encryption
            if (passwordCorrect)
            {
                player.Send("Password correct.");
                PreserveInput = false;
                LoginSuccessfull(player);
                return LoginStates.Connected;
            }
            else
            {
                _invalidPasswordTries++;
                if (_invalidPasswordTries < MaxPasswordTries)
                {
                    player.Send("Password invalid, please try again:");
                    PreserveInput = true;
                    return LoginStates.Password;
                }
                else
                {
                    player.Send("Maximum login attempts reached, disconnecting.");
                    Disconnect(player);
                    return LoginStates.Disconnect;
                }
            }
        }

        private LoginStates ProcessUsernameConfirm(IPlayer player, string input)
        {
            // If confirmed, ask for password
            // Else, ask name again
            if (input == "y" || input == "yes")
            {
                player.Send("Great! Please enter a password.");
                PreserveInput = true;
                return LoginStates.NewPassword1;
            }
            else
            {
                player.Send("Ok, what name would you like to use?");
                PreserveInput = false;
                return LoginStates.Username;
            }
        }

        private LoginStates ProcessNewPassword1(IPlayer player, string input)
        {
            // Save password to compare
            _password = input;
            // Ask confirmation
            player.Send("Please reenter your password:");
            PreserveInput = true;
            return LoginStates.NewPassword2;
        }

        private LoginStates ProcessNewPassword2(IPlayer player, string input)
        {
            // If password is the same, go to final
            // Else, restart password selection
            // TODO: encryption
            if (input == _password)
            {
                player.Send("Your new account with username {0} has been created", _username);
                LoginSuccessfull(player);
                return LoginStates.Connected;
            }
            else
            {
                player.Send("Passwords do not match, please choose a password:");
                PreserveInput = true;
                return LoginStates.NewPassword1;
            }
        }

        private LoginStates ProcessConnected(IPlayer player, string input)
        {
            // Fall-thru
            return LoginStates.Connected;
        }

        private LoginStates ProcessDisconnect(IPlayer player, string input)
        {
            // Fall-thru
            return LoginStates.Disconnect;
        }

        private void LoginSuccessfull(IPlayer player)
        {
            player.Send("Welcome to Mud.Net!!");

            // Load player
            player.Load(_username);
            // Add player to world
            WorldTest.Instance.AddPlayer(player);
            //
            State = LoginStates.Connected;
        }

        private void Disconnect(IPlayer player)
        {
            // TODO: disconnect player and stop state machine
            State = LoginStates.Disconnect;
        }
    }
}
