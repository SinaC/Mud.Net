using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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

        public override bool IsFinalStateReached => State == LoginStates.LoggedIn || State == LoginStates.Disconnected;

        public LoginStateMachine()
        {
            PreserveInput = false;
            StateMachine = new Dictionary<LoginStates, Func<IClient, string, LoginStates>>
            {
                {LoginStates.Username, ProcessUsername},
                {LoginStates.Password, ProcessPassword},
                {LoginStates.UsernameConfirm, ProcessUsernameConfirm},
                {LoginStates.NewPassword1, ProcessNewPassword1},
                {LoginStates.NewPassword2, ProcessNewPassword2},
                {LoginStates.LoggedIn, ProcessConnected},
                {LoginStates.Disconnected, ProcessDisconnect}
            };
            State = LoginStates.Username;
        }

        private LoginStates ProcessUsername(IClient client, string input)
        {
            // Reset password tries
            _invalidPasswordTries = 0;
            //
            if (!string.IsNullOrWhiteSpace(input))
            {
                _username = input;
                bool isAdmin;
                bool known = Repository.LoginManager.CheckUsername(input, out isAdmin);

                // If known, greets and asks for password
                // Else, name confirmation
                if (known)
                {
                    Send(client, "Welcome back, {0}! Please enter your password:", StringHelpers.UpperFirstLetter(_username));
                    _isAdmin = isAdmin;
                    _isNewPlayer = false;
                    EchoOff(client);
                    PreserveInput = true;
                    return LoginStates.Password;
                }
                // TODO: check name validity
                Send(client, "Are you sure this is the account name you wish to use? (y/n)");
                _isAdmin = false;
                _isNewPlayer = true;
                PreserveInput = false;
                return LoginStates.UsernameConfirm;
            }
            //
            Send(client, "Please enter a name:");
            PreserveInput = false;
            return LoginStates.Username;
        }

        private LoginStates ProcessPassword(IClient client, string input)
        {
            // If password is correct, go to final state
            // Else, 
            //      If too many try, disconnect
            //      Else, retry password
            bool passwordCorrect = Repository.LoginManager.CheckPassword(_username, input);
            if (passwordCorrect)
            {
                Send(client, "Password correct." + Environment.NewLine);
                EchoOn(client);
                LoginSuccessfull(client);
                PreserveInput = false;
                return LoginStates.LoggedIn;
            }
            //
            _invalidPasswordTries++;
            if (_invalidPasswordTries < MaxPasswordTries)
            {
                Send(client, "Password invalid, please try again:");
                PreserveInput = true;
                return LoginStates.Password;
            }
            //
            Send(client, "Maximum login attempts reached, disconnecting.");
            Disconnect(client);
            return LoginStates.Disconnected;
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
            //
            Send(client, "Ok, what name would you like to use?");
            PreserveInput = false;
            return LoginStates.Username;
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
                Repository.LoginManager.InsertLogin(_username, _password); // add login in DB
                EchoOn(client);
                LoginSuccessfull(client);
                return LoginStates.LoggedIn;
            }
            //
            Send(client, "Password do not match, please choose a password:");
            PreserveInput = true;
            return LoginStates.NewPassword1;
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
            LoginSuccessful?.Invoke(client, _username, _isAdmin, _isNewPlayer);
            //
            State = LoginStates.LoggedIn;
        }

        private void Disconnect(IClient client)
        {
            LoginFailed?.Invoke(client);
            //
            State = LoginStates.Disconnected;
        }

        private static void Send(IClient client, string format, params object[] parameters)
        {
            string message = string.Format(format, parameters);
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

        private static string CryptPassword(string password)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(password)));
        }
    }
}
