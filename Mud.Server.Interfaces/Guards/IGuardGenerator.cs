namespace Mud.Server.Interfaces.Guards;

public interface IGuardGenerator
{
    ICharacterGuard[] GenerateCharacterGuards(Type type);
    IPlayerGuard[] GeneratePlayerGuards(Type type);
    IAdminGuard[] GenerateAdminGuards(Type type);
}