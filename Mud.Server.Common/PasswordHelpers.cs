namespace Mud.Server.Common;

public static class PasswordHelpers
{
    public static string Crypt(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Check(string password, string cryptedPassword)
        => BCrypt.Net.BCrypt.Verify(password, cryptedPassword);
}
