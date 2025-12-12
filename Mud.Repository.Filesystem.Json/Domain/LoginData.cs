namespace Mud.Repository.Filesystem.Json.Domain;

public class LoginData
{
    public required string Username { get; set; }

    public required string Password { get; set; } // TODO: crypt

    public required bool IsAdmin { get; set; }
}
