namespace Mud.Server.Interfaces.Guards;

public interface IGuardGenerator
{
    IActorGuard[] GenerateActorGuards(Type type);
    ICharacterGuard[] GenerateCharacterGuards(Type type);
    IPlayerGuard[] GeneratePlayerGuards(Type type);
    IAdminGuard[] GenerateAdminGuards(Type type);
}