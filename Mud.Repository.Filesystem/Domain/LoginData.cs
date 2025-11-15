namespace Mud.Repository.Filesystem.Domain;

public class LoginData
{
    public string Username { get; set; }

    public string Password { get; set; } // TODO: crypt

    public bool IsAdmin { get; set; }
}
