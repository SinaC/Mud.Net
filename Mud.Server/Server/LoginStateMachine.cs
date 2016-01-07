using System;
using System.Collections.Generic;
using Mud.Network;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Server
{
    internal enum LoginStates
    {
        Username, // -> Username | Password | UsernameConfirm
        Password, // -> Password | LoggedIn | Disconnected
        UsernameConfirm, // -> NewPassword1
        NewPassword1, // -> NewPassword2
        NewPassword2, // -> Username | LoggedIn
        LoggedIn,
        Disconnected,
    }

    internal delegate void LoginSuccessfulEventHandler(IClient client, string username, bool isAdmin, bool isNewPlayer);
    internal delegate void LoginFailedEventHandler(IClient client);

    internal class LoginStateMachine : InputTrapBase<IClient, LoginStates>
    {
        public const int MaxPasswordTries = 3;
        private int _invalidPasswordTries;

        private string _username;
        private string _password; // todo: encryption
        private bool _isAdmin;
        private bool _isNewPlayer;

        public event LoginSuccessfulEventHandler LoginSuccessful;
        public event LoginFailedEventHandler LoginFailed;

        public override bool IsFinalStateReached
        {
            get { return State == LoginStates.LoggedIn || State == LoginStates.Disconnected; }
        }

        public LoginStateMachine()
        {
            PreserveInput = false;
            StateMachine = new Dictionary<LoginStates, Func<IClient, string, LoginStates>>
            {
                { LoginStates.Username, ProcessUsername },
                { LoginStates.Password, ProcessPassword },
                { LoginStates.UsernameConfirm, ProcessUsernameConfirm },
                { LoginStates.NewPassword1, ProcessNewPassword1 },
                { LoginStates.NewPassword2, ProcessNewPassword2 },
                { LoginStates.LoggedIn, ProcessConnected },
                { LoginStates.Disconnected, ProcessDisconnect }
            };
            State = LoginStates.Username;
        }

        private LoginStates ProcessUsername(IClient client, string input)
        {
            // Reset password tries
            _invalidPasswordTries = 0;
            //
            if (!String.IsNullOrWhiteSpace(input))
            {
                _username = input;
                bool isAdmin;
                bool known = CheckUsername(input, out isAdmin);

                // If known, greets and asks for password
                // Else, name confirmation
                if (known)
                {
                    Send(client,"Welcome back, {0}! Please enter your password:", StringHelpers.UpperFirstLetter(_username));
                    _isAdmin = isAdmin;
                    _isNewPlayer = false;
                    EchoOff(client);
                    PreserveInput = true;
                    return LoginStates.Password;
                }
                else
                {
                    Send(client, "Are you sure this is the account name you wish to use? (y/n)");
                    _isAdmin = false;
                    _isNewPlayer = true;
                    PreserveInput = false;
                    return LoginStates.UsernameConfirm;
                }
            }
            else
            {
                Send(client, "Please enter a name:");
                PreserveInput = false;
                return LoginStates.Username;
            }
        }

        private LoginStates ProcessPassword(IClient client, string input)
        {
            // If password is correct, go to final state
            // Else, 
            //      If too many try, disconnect
            //      Else, retry password
            bool passwordCorrect = input == "password"; // TODO: check password + encryption
            if (passwordCorrect)
            {
                Send(client, "Password correct." + Environment.NewLine);
                EchoOn(client);
                LoginSuccessfull(client);
                PreserveInput = false;
                return LoginStates.LoggedIn;
            }
            else
            {
                _invalidPasswordTries++;
                if (_invalidPasswordTries < MaxPasswordTries)
                {
                    Send(client, "Password invalid, please try again:");
                    PreserveInput = true;
                    return LoginStates.Password;
                }
                else
                {
                    Send(client, "Maximum login attempts reached, disconnecting.");
                    Disconnect(client);
                    return LoginStates.Disconnected;
                }
            }
        }

        private LoginStates ProcessUsernameConfirm(IClient client, string input)
        {
            // If confirmed, ask for password
            // Else, ask name again
            if (input == "y" || input == "yes")
            {
                Send(client, "Great! Please enter a password.");
                EchoOff(client);
                PreserveInput = true;
                return LoginStates.NewPassword1;
            }
            else
            {
                Send(client, "Ok, what name would you like to use?");
                PreserveInput = false;
                return LoginStates.Username;
            }
        }

        private LoginStates ProcessNewPassword1(IClient client, string input)
        {
            // Save password to compare
            _password = input;
            // Ask confirmation
            Send(client, "Please reenter your password:");
            PreserveInput = true;
            return LoginStates.NewPassword2;
        }

        private LoginStates ProcessNewPassword2(IClient client, string input)
        {
            // If password is the same, go to final
            // Else, restart password selection
            // TODO: encryption
            if (input == _password)
            {
                Send(client, "Your new account with username {0} has been created" + Environment.NewLine, _username);
                EchoOn(client);
                LoginSuccessfull(client);
                return LoginStates.LoggedIn;
            }
            else
            {
                Send(client, "Passwords do not match, please choose a password:");
                PreserveInput = true;
                return LoginStates.NewPassword1;
            }
        }

        private static LoginStates ProcessConnected(IClient client, string input)
        {
            // Fall-thru
            return LoginStates.LoggedIn;
        }

        private static LoginStates ProcessDisconnect(IClient client, string input)
        {
            // Fall-thru
            return LoginStates.Disconnected;
        }

        private void LoginSuccessfull(IClient client)
        {
            if (LoginSuccessful != null)
                LoginSuccessful(client, _username, _isAdmin, _isNewPlayer);
            //
            State = LoginStates.LoggedIn;
        }

        private void Disconnect(IClient client)
        {
            if (LoginFailed != null)
                LoginFailed(client);
            //
            State = LoginStates.Disconnected;
        }

        private static void Send(IClient client, string format, params object[] parameters)
        {
            string message = String.Format(format, parameters);
            client.WriteData(message);
        }

        private static void EchoOff(IClient client)
        {
            client.EchoOff();
        }

        private static void EchoOn(IClient client)
        {
            client.EchoOn();
        }

        // TODO: remove fake client
        private readonly Dictionary<string, Tuple<string, bool>> _fakeUsernameTable = new Dictionary<string, Tuple<string, bool>>
        {
            {"sinac", new Tuple<string, bool>("password", true)},
            {"player", new Tuple<string, bool>("password", false)}
        };

        private bool CheckUsername(string username, out bool isAdmin)
        {
            isAdmin = false;
            Tuple<string, bool> passwordInDb;
            bool found = _fakeUsernameTable.TryGetValue(username, out passwordInDb);
            if (found)
                isAdmin = passwordInDb.Item2;
            return found;
        }

        private bool CheckPassword(string name, string password)
        {
            Tuple<string, bool> passwordInDb;
            _fakeUsernameTable.TryGetValue(name, out passwordInDb);
            return passwordInDb != null && password == passwordInDb.Item1;
        }
    }
}
