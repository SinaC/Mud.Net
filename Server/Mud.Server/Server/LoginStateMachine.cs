using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Network.Interfaces;
using Mud.Repository.Interfaces;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Options;

namespace Mud.Server.Server;

public enum LoginStates
{
    Username, // -> Username | Password | UsernameConfirm
    Password, // -> Password | LoggedIn | Disconnected
    UsernameConfirm, // -> NewPassword1
    NewPassword1, // -> NewPassword2
    NewPassword2, // -> Username | LoggedIn
    LoggedIn,
    Disconnected,
}

public delegate void LoginSuccessfulEventHandler(IClient client, string username, bool isAdmin, bool isNewPlayer);

public delegate void LoginFailedEventHandler(IClient client);

[Export]
public class LoginStateMachine : InputTrapBase<IClient, LoginStates>
{
    private const int MaxPasswordTries = 3;
    private int _invalidPasswordTries;

    private string? _username;
    private string? _password; // todo: encryption
    private bool _isAdmin;
    private bool _isNewPlayer;

    private IAccountRepository AccountRepository { get; }
    private IUniquenessManager UniquenessManager { get; }
    private bool CheckPassword { get; }


    public event LoginSuccessfulEventHandler? LoginSuccessful;
    public event LoginFailedEventHandler? LoginFailed;

    public override bool IsFinalStateReached => State == LoginStates.LoggedIn || State == LoginStates.Disconnected;

    public LoginStateMachine(IAccountRepository accountRepository, IUniquenessManager uniquenessManager, IOptions<ServerOptions> options)
    {
        AccountRepository = accountRepository;
        UniquenessManager = uniquenessManager;
        CheckPassword = options.Value.CheckPassword;

        KeepInputAsIs = false;
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

    public void Initialize(IClient client)
    {
        Send(client, "Why don't you login or tell us the name you wish to be known by?");
    }

    private LoginStates ProcessUsername(IClient client, string input)
    {
        // Reset password tries
        _invalidPasswordTries = 0;
        //
        if (!string.IsNullOrWhiteSpace(input))
        {
            var accountData = AccountRepository.Load(input);

            // If known, greets and asks for password
            // Else, name confirmation
            if (accountData != null)
            {
                Send(client, "Welcome back, {0}! Please enter your password:", input.UpperFirstLetter());
                _username = input;
                _password = accountData.Password;
                _isAdmin = accountData.AdminData != null;
                _isNewPlayer = false;
                EchoOff(client);
                KeepInputAsIs = true;
                return LoginStates.Password;
            }

            // If account name is available, create
            if (UniquenessManager.IsAccountNameAvailable(input))
            {
                Send(client, "Are you sure this is the account name you wish to use? (y/n)");
                _username = input;
                _password = null;
                _isAdmin = false;
                _isNewPlayer = true;
                KeepInputAsIs = false;
                return LoginStates.UsernameConfirm;
            }

            Send(client, "This name is not available for creation. Please enter a valid name:");
            KeepInputAsIs = false;
            return LoginStates.Username;
        }
        //
        Send(client, "Please enter a name:");
        KeepInputAsIs = false;
        return LoginStates.Username;
    }

    private LoginStates ProcessPassword(IClient client, string input)
    {
        // If password is correct, go to final state
        // Else, 
        //      If too many try, disconnect
        //      Else, retry password
        if (!CheckPassword || PasswordHelpers.Check(input, _password!))
        {
            Send(client, "Password correct." + Environment.NewLine);
            EchoOn(client);
            LoginSuccessfull(client);
            KeepInputAsIs = false;
            return LoginStates.LoggedIn;
        }
        //
        _invalidPasswordTries++;
        if (_invalidPasswordTries < MaxPasswordTries)
        {
            Send(client, "Password invalid, please try again:");
            KeepInputAsIs = true;
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
            KeepInputAsIs = true;
            return LoginStates.NewPassword1;
        }
        //
        Send(client, "Ok, what name would you like to use?");
        KeepInputAsIs = false;
        return LoginStates.Username;
    }

    private LoginStates ProcessNewPassword1(IClient client, string input)
    {
        // Save password to compare
        _password = PasswordHelpers.Crypt(input);
        // Ask confirmation
        Send(client, "Please reenter your password:");
        KeepInputAsIs = true;
        return LoginStates.NewPassword2;
    }

    private LoginStates ProcessNewPassword2(IClient client, string input)
    {
        // If password is the same, go to final
        // Else, restart password selection
        // TODO: encryption
        if (!CheckPassword || PasswordHelpers.Check(input, _password!))
        {
            Send(client, "Your new account with username {0} has been created" + Environment.NewLine, _username!);
            EchoOn(client);
            LoginSuccessfull(client);
            return LoginStates.LoggedIn;
        }
        //
        Send(client, "Password do not match, please choose a password:");
        KeepInputAsIs = true;
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
        LoginSuccessful?.Invoke(client, _username!, _isAdmin, _isNewPlayer);
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
}
