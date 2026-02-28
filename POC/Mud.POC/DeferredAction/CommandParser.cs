using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public static class CommandParser
{
    public static IGameAction Parse(string input, Mob player, World world)
    {
        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var cmd = parts[0].ToLower();
        var arg = parts.Length > 1 ? parts[1] : null;

        return cmd switch
        {
            "attack" when arg != null => ParseAttack(player, arg, world),
            "cast" when arg != null => ParseCast(player, arg, world),
            "move" when arg != null => ParseMove(player, arg, world),
            "steal" when arg != null => ParseSkill(player, "steal", arg, world),
            "disarm" when arg != null => ParseSkill(player, "disarm", arg, world),
            "rescue" when arg != null => ParseSkill(player, "rescue", arg, world),
            "bash" when arg != null => ParseSkill(player, "bash", arg, world),
            "trip" when arg != null => ParseSkill(player, "trip", arg, world),
            "hide" => ParseSkill(player, "hide", null, world),
            "sneak" => ParseSkill(player, "sneak", null, world),
            _ => null
        };
    }

    private static IGameAction ParseAttack(Mob player, string targetName, World world)
    {
        var target = world.FindMobInRoom(player.CurrentRoom, targetName);
        if (target == null) return null;

        return new AttackAction(player, target);
    }

    private static IGameAction ParseCast(Mob player, string arg, World world)
    {
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var skillName = parts[0].ToLower();
        var targetName = parts.Length > 1 ? parts[1] : null;

        var skill = SkillBook.GetSkillByName(skillName);
        if (skill == null) return null;

        var target = targetName != null ? world.FindMobInRoom(player.CurrentRoom, targetName) : null;
        return new SkillAction(player, skill, target);
    }

    private static IGameAction ParseMove(Mob player, string roomName, World world)
    {
        var destination = world.FindRoom(roomName);
        if (destination == null) return null;

        return new EnterRoomAction(player, destination);
    }

    private static IGameAction ParseSkill(Mob player, string skillName, string targetName, World world)
    {
        var skill = SkillBook.GetSkillByName(skillName);
        if (skill == null) return null;

        Mob target = null;
        if (targetName != null)
            target = world.FindMobInRoom(player.CurrentRoom, targetName);

        return new SkillAction(player, skill, target);
    }
}

/*
public static class CommandParser
{
    public static IGameAction Parse(string input, Mob player, World world)
    {
        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var cmd = parts[0].ToLower();
        var arg = parts.Length > 1 ? parts[1] : null;

        return cmd switch
        {
            "attack" when arg != null => new AttackAction(player, world.FindMobInRoom(player.CurrentRoom, arg)),
            "cast" when arg != null => ParseCast(player, arg, world),
            "move" when arg != null => new EnterRoomAction(player, world.FindRoom(arg)),
            "steal" when arg != null => new StealAction(player, world.FindMobInRoom(player.CurrentRoom, arg)),
            "disarm" when arg != null => new DisarmAction(player, world.FindMobInRoom(player.CurrentRoom, arg)),
            "rescue" when arg != null => new RescueAction(player, world.FindMobInRoom(player.CurrentRoom, arg)),
            _ => null
        };
    }

    private static IGameAction ParseCast(Mob player, string arg, World world)
    {
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var skillName = parts[0].ToLower();
        var targetName = parts.Length > 1 ? parts[1] : null;

        var skill = SkillBook.GetSkillByName(skillName);
        if (skill == null) return null;

        var target = targetName != null ? world.FindMobInRoom(player.CurrentRoom, targetName) : null;
        return new CastSkillAction(player, skill, target);
    }
}
*/
/*
public static class CommandParser
{
    public static IGameAction Parse(string input, Mob player, World world)
    {
        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var cmd = parts[0].ToLower();
        var arg = parts.Length > 1 ? parts[1] : null;

        return cmd switch
        {
            "attack" when arg != null => ParseAttack(player, arg, world),
            "cast" when arg != null => ParseCast(player, arg, world),
            "move" when arg != null => ParseMove(player, arg, world),
            "steal" when arg != null => ParseSteal(player, arg, world),
            _ => null
        };
    }

    private static IGameAction ParseAttack(Mob player, string targetName, World world)
    {
        var target = world.FindMobInRoom(player.CurrentRoom, targetName);
        if (target == null) return null;

        return new AttackAction(player, target);
    }

    private static IGameAction ParseCast(Mob player, string arg, World world)
    {
        var parts = arg.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var skillName = parts[0].ToLower();
        var targetName = parts.Length > 1 ? parts[1] : null;

        var skill = SkillBook.GetSkillByName(skillName);
        if (skill == null) return null;

        Mob target = targetName != null ? world.FindMobInRoom(player.CurrentRoom, targetName) : null;

        return new CastSkillAction(player, skill, target);
    }

    private static IGameAction ParseMove(Mob player, string roomName, World world)
    {
        var destination = world.FindRoom(roomName);
        if (destination == null) return null;

        return new EnterRoomAction(player, destination);
    }

    private static IGameAction ParseSteal(Mob player, string targetName, World world)
    {
        var target = world.FindMobInRoom(player.CurrentRoom, targetName);
        if (target == null) return null;

        var stealSkill = SkillBook.GetSkillByName("steal");
        if (stealSkill == null) return null;

        return new CastSkillAction(player, stealSkill, target);
    }
}
*/