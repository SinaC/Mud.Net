using Mud.Common.Attributes;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Attributes;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.Guards;
using System.Reflection;

namespace Mud.Server.Guards;

[Export(typeof(IGuardGenerator)), Shared]
public class GuardGenerator : IGuardGenerator
{
    public ICharacterGuard[] GenerateCharacterGuards(Type type)
    {
        var guardAttributes = type.GetCustomAttributes<CharacterGuardAttributeBase>();
        List<ICharacterGuard> guards = [];
        foreach (var guardAttribute in guardAttributes)
        {
            if (guardAttribute is MinPositionAttribute minPositionAttribute)
            {
                guards.Add(new MinPositionGuard(minPositionAttribute.MinimumPosition));
            }
            else if (guardAttribute is NotInCombatAttribute notInCombatAttribute)
            {
                guards.Add(new NotInCombatGuard(notInCombatAttribute.Message));
            }
            else if (guardAttribute is InCombatAttribute inCombatAttribute)
            {
                guards.Add(new InCombatGuard(inCombatAttribute.Message));
            }
            else if (guardAttribute is ShapesAttribute shapesAttribute)
            {
                guards.Add(new ShapesGuard(shapesAttribute.Shapes));
            }
            else if (guardAttribute is NoShapeshiftAttribute)
            {
                guards.Add(new NoShapeshiftGuard());
            }
        }
        return guards.ToArray();
    }

    public IPlayerGuard[] GeneratePlayerGuards(Type type)
    {
        var guardAttributes = type.GetCustomAttributes<PlayerGuardAttributeBase>();
        List<IPlayerGuard> guards = [];
        foreach (var guardAttribute in guardAttributes)
        {
            if (guardAttribute is MustBeImpersonatedAttribute)
            {
                guards.Add(new MustBeImpersonatedGuard());
            }
            else if (guardAttribute is CannotBeImpersonatedAttribute)
            {
                guards.Add(new CannotBeImpersonatedGuard());
            }
        }
        return guards.ToArray();
    }

    public IAdminGuard[] GenerateAdminGuards(Type type)
    {
        var guardAttributes = type.GetCustomAttributes<AdminGuardAttributeBase>();
        List<IAdminGuard> guards = [];
        foreach (var guardAttribute in guardAttributes)
        {
            if (guardAttribute is MinAdminLevelAttribute minAdminLevelAttribute)
            {
                guards.Add(new MinAdminLevelGuard(minAdminLevelAttribute.MinLevel));
            }
        }
        return guards.ToArray();
    }
}
