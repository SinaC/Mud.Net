using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.MobProgram.Interfaces;
using Mud.Server.Options;
using Mud.Server.Parser.Interfaces;
using System.Text;

namespace Mud.Server.MobProgram;

[Export(typeof(IMobProgramEvaluator)), Shared]
public class MobProgramEvaluator : IMobProgramEvaluator
{
    private readonly Dictionary<string, Func<IMobProgramExecutionContext, string, bool>> _singleArgumentConditions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IMobProgramExecutionContext, string, string, bool>> _twoArgumentsConditions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IMobProgramExecutionContext, long?>> _parameterLessIntegerFunctions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IMobProgramExecutionContext, string, long?>> _oneParameterIntegerFunctions = new(StringComparer.OrdinalIgnoreCase);

    private ILogger<MobProgramEvaluator> Logger { get; }
    private int MaxProgramDepth { get; }

    public MobProgramEvaluator(ILogger<MobProgramEvaluator> logger, IOptions<ServerOptions> serverOptions)
    {
        Logger = logger;
        MaxProgramDepth = serverOptions.Value.MaxProgramDepth;

        RegisterConditions();
        RegisterFunctions();
    }

    public bool Evaluate(IMobProgram mobProgram, IMobProgramExecutionContext ctx)
    {
        if (ctx.Self.MobProgramDepth >= MaxProgramDepth)
        {
            Logger.LogError("MOBPROGRAM: {debugName} [MP id {MPId}] max call depth reached", ctx.Self.DebugName, ctx.MobProgramId);
            return false;
        }

        ctx.Self.IncreaseMobProgramDepth();

        var shouldBreak = false;
        EvaluateNodes(mobProgram.Nodes, ctx, ref shouldBreak);

        ctx.Self.DecreaseMobProgramDepth();
        return true;
    }

    private void EvaluateNodes(IEnumerable<INode> nodes, IMobProgramExecutionContext ctx, ref bool shouldBreak)
    {
        foreach (var node in nodes)
        {
            if (shouldBreak)
                return;

            switch (node)
            {
                case CommandNode cmd:
                    ExecuteCommand(cmd.Command, ctx);
                    break;

                case IfNode ifNode:
                    if (EvaluateCondition(ifNode.Condition, ctx))
                        EvaluateNodes(ifNode.TrueBranch, ctx, ref shouldBreak);
                    else
                        EvaluateNodes(ifNode.FalseBranch, ctx, ref shouldBreak);
                    break;

                case BreakNode:
                    Logger.LogInformation("MOBPROGRAM: {debugName} [MP id {MPId}] BREAK", ctx.Self.DebugName, ctx.MobProgramId);
                    shouldBreak = true;
                    return;

                // EmptyNode, CommentNode are not evaluated
            }
        }
    }

    // =========================
    // CONDITION ENGINE
    // =========================
    public bool EvaluateCondition(BoolExpr boolExpr, IMobProgramExecutionContext ctx)
    {
        switch (boolExpr)
        {
            case AndExpr andExpr:
                if (!EvaluateCondition(andExpr.Left, ctx))
                    return false;

                return EvaluateCondition(andExpr.Right, ctx);

            case OrExpr orExpr:
                if (EvaluateCondition(orExpr.Left, ctx))
                    return true;

                return EvaluateCondition(orExpr.Right, ctx);

            case ConditionExpr conditionExpr:
                return EvaluateSimpleCondition(conditionExpr.Raw, ctx);
        }
        return false;
    }

    // =========================
    // SIMPLE CONDITION ENGINE
    // =========================

    public bool EvaluateSimpleCondition(string cond, IMobProgramExecutionContext ctx)
    {
        var tokens = cond.Tokenize(false).ToArray();
        if (tokens.Length == 0)
            return false;

        Logger.LogInformation("MOBPROGRAM: {debugName} [MP id {MPId}] simple condition {cond}", ctx.Self.DebugName, ctx.MobProgramId, cond);

        if (tokens.Length == 2)
        {
            if (_singleArgumentConditions.TryGetValue(tokens[0], out var evaluateFunc))
                return evaluateFunc(ctx, tokens[1]);
        }
        else if (tokens.Length == 3)
        {
            // try to evaluate as integer expression
            if (IsComparisonOperator(tokens[1]))
            {
                var leftExpr = tokens[0];
                var op = tokens[1];
                var rightExpr = tokens[2];

                return EvaluateIntegerComparison(leftExpr, op, rightExpr, ctx);
            }

            // try to evaluate as mob expression
            if (_twoArgumentsConditions.TryGetValue(tokens[0], out var evaluateFunc))
                return evaluateFunc(ctx, tokens[1], tokens[2]);
        }
        else if (tokens.Length == 4)
        {
            if (IsComparisonOperator(tokens[2]))
            {
                var leftExpr = $"{tokens[0]} '{tokens[1]}'";
                var op = tokens[2];
                var rightExpr = tokens[3];

                return EvaluateIntegerComparison(leftExpr, op, rightExpr, ctx);
            }
        }

        Logger.LogError("MOBPROGRAM: {debugName} [MP id {MPId}] unknown simple condition {cond}", ctx.Self.DebugName, ctx.MobProgramId, cond);

        return false;
    }

    private bool EvaluateIntegerComparison(string leftExpr, string op, string rightExpr, IMobProgramExecutionContext ctx)
    {
        var left = ResolveIntegerValue(leftExpr, ctx);
        var right = ResolveIntegerValue(rightExpr, ctx);

        var result = op switch
        {
            "<" => left < right,
            ">" => left > right,
            "<=" => left <= right,
            ">=" => left >= right,
            "==" => left == right,
            "!=" => left != right,
            _ => false
        };

        Logger.LogInformation("MOBPROGRAM: {debugName} [MP id {MPId}] COND {leftExpr} {op} {rightExpr} => {result} ({left} {op} {right}", ctx.Self.DebugName, ctx.MobProgramId, leftExpr, op, rightExpr, result, left, op, right);
        return result;
    }

    private long? ResolveIntegerValue(string expr, IMobProgramExecutionContext ctx)
    {
        expr = expr.Trim();

        Logger.LogInformation("MOBPROGRAM: {debugName} [MP id {MPId}] INT VALUE {expr}", ctx.Self.DebugName, ctx.MobProgramId, expr);

        // direct number
        if (long.TryParse(expr, out var literal))
            return literal;

        var tokens = expr.Tokenize(false).ToArray();

        // parameter-less function
        if (tokens.Length == 1)
        {
            if (_parameterLessIntegerFunctions.TryGetValue(expr, out var function))
                return function(ctx);
        }

        // 1-parameter function
        if (tokens.Length == 2)
        {
            if (_oneParameterIntegerFunctions.TryGetValue(tokens[0], out var function))
                return function(ctx, tokens[1]);
        }

        Logger.LogError("MOBPROGRAM: {debugName} [MP id {MPId}] unknown integer expression: {expr}", ctx.Self.DebugName, ctx.MobProgramId, expr);
        return null;
    }

    private static bool IsComparisonOperator(string op)
        => op is "<" or ">" or "<=" or ">=" or "==" or "!=";

    // =========================
    // COMMAND EXECUTION
    // =========================

    private void ExecuteCommand(string command, IMobProgramExecutionContext ctx)
    {
        command = ReplaceVariables(command, ctx);
        var processed = ctx.Self.ProcessInput(command);
        if (!processed)
            Logger.LogError("MOBPROGRAM: {debugName} [MP id {MPId}] execution failed for instruction {instruction}", ctx.Self.DebugName, ctx.MobProgramId, command);
    }

    // =========================
    // VARIABLE RESOLUTION
    // =========================

    private ICharacter? ResolveCharacterVariable(string name, IMobProgramExecutionContext ctx)
    {
        return name switch
        {
            "$i" => ctx.Self,
            "$n" => ctx.Triggerer,
            "$t" => ctx.Secondary,
            "$q" => ctx.Target,
            // TODO: handle $r
            _ => null
        };
    }
    private IPlayableCharacter? ResolvePlayerVariable(string name, IMobProgramExecutionContext ctx)
    {
        return name switch
        {
            // $i is always a mob
            "$n" => ctx.Triggerer as IPlayableCharacter,
            "$t" => ctx.Secondary as IPlayableCharacter,
            "$q" => ctx.Target as IPlayableCharacter,
            // TODO: handle $r
            _ => null
        };
    }

    private INonPlayableCharacter? ResolveMobVariable(string name, IMobProgramExecutionContext ctx)
    {
        return name switch
        {
            "$i" => ctx.Self,
            "$n" => ctx.Triggerer as INonPlayableCharacter,
            "$t" => ctx.Secondary as INonPlayableCharacter,
            "$q" => ctx.Target as INonPlayableCharacter,
            // TODO: handle $r
            _ => null
        };
    }

    private IItem? ResolveItemVariable(string name, IMobProgramExecutionContext ctx)
    {
        return name switch
        {
            "$o" => ctx.PrimaryObject,
            "$p" => ctx.SecondaryObject,
            _ => null
        };
    }

    private IEntity? ResolveEntityVariable(string name, IMobProgramExecutionContext ctx)
    {
        return name switch
        {
            "$i" => ctx.Self,
            "$n" => ctx.Triggerer,
            "$t" => ctx.Secondary,
            "$q" => ctx.Target,
            // TODO: handle $r
            "$o" => ctx.PrimaryObject,
            "$p" => ctx.SecondaryObject,
            _ => null
        };
    }

    private string ReplaceVariables(string command, IMobProgramExecutionContext ctx)
    {
        var sb = new StringBuilder(command);

        var randomPC = ctx.RandomManager.Random(ctx.Self.Room.PlayableCharacters.Where(x => x != ctx.Self && ctx.Self.CanSee(x)));

        // name/display name
        sb.Replace("$i", ctx.Self.Keywords.First());
        sb.Replace("$I", ctx.Self.DisplayName);
        sb.Replace("$n", GetName(ctx.Self, ctx.Triggerer));
        sb.Replace("$N", GetDisplayName(ctx.Self, ctx.Triggerer));
        sb.Replace("$t", GetName(ctx.Self, ctx.Secondary));
        sb.Replace("$T", GetDisplayName(ctx.Self, ctx.Secondary));
        sb.Replace("$r", GetName(ctx.Self, randomPC));
        sb.Replace("$R", GetDisplayName(ctx.Self, randomPC));
        sb.Replace("$q", GetName(ctx.Self, ctx.Target));
        sb.Replace("$Q", GetDisplayName(ctx.Self, ctx.Target));

        // subjects
        sb.Replace("$j", StringHelpers.Subjects[ctx.Self.Sex]);
        sb.Replace("$e", GetValueFromSex(ctx.Self, ctx.Triggerer, StringHelpers.Subjects));
        sb.Replace("$E", GetValueFromSex(ctx.Self, ctx.Secondary, StringHelpers.Subjects));
        sb.Replace("$J", GetValueFromSex(ctx.Self, randomPC, StringHelpers.Subjects));
        sb.Replace("$X", GetValueFromSex(ctx.Self, ctx.Target, StringHelpers.Subjects));

        // objectives
        sb.Replace("$k", StringHelpers.Objectives[ctx.Self.Sex]);
        sb.Replace("$m", GetValueFromSex(ctx.Self, ctx.Triggerer, StringHelpers.Objectives));
        sb.Replace("$M", GetValueFromSex(ctx.Self, ctx.Secondary, StringHelpers.Objectives));
        sb.Replace("$K", GetValueFromSex(ctx.Self, randomPC, StringHelpers.Objectives));
        sb.Replace("$Y", GetValueFromSex(ctx.Self, ctx.Target, StringHelpers.Objectives));

        // possessives
        sb.Replace("$l", StringHelpers.Possessives[ctx.Self.Sex]);
        sb.Replace("$s", GetValueFromSex(ctx.Self, ctx.Triggerer, StringHelpers.Possessives));
        sb.Replace("$S", GetValueFromSex(ctx.Self, ctx.Secondary, StringHelpers.Possessives));
        sb.Replace("$L", GetValueFromSex(ctx.Self, randomPC, StringHelpers.Possessives));
        sb.Replace("$Z", GetValueFromSex(ctx.Self, ctx.Target, StringHelpers.Possessives));

        // item name/display name
        sb.Replace("$o", GetName(ctx.Self, ctx.PrimaryObject));
        sb.Replace("$O", GetDisplayName(ctx.Self, ctx.PrimaryObject));
        sb.Replace("$p", GetName(ctx.Self, ctx.SecondaryObject));
        sb.Replace("$P", GetDisplayName(ctx.Self, ctx.Secondary));

        return sb.ToString();
    }

    private static string GetName(INonPlayableCharacter npc, ICharacter? character)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return character.Keywords.First().UpperFirstLetter();
    }

    private static string GetDisplayName(INonPlayableCharacter npc, ICharacter? character)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return character.DisplayName;
    }

    private static string GetName(INonPlayableCharacter npc, IItem? item)
    {
        if (item == null || !npc.CanSee(item))
            return "something";
        return item.Keywords.First().UpperFirstLetter();
    }

    private static string GetDisplayName(INonPlayableCharacter npc, IItem? item)
    {
        if (item == null || !npc.CanSee(item))
            return "something";
        return item.DisplayName;
    }

    private static string GetValueFromSex(INonPlayableCharacter npc, ICharacter? character, IDictionary<Sex, string> valueBySex)
    {
        if (character == null || !npc.CanSee(character))
            return "someone";
        return valueBySex[character.Sex];
    }

    private void RegisterConditions()
    {
        // single argument
        _singleArgumentConditions["rand"] = (ctx, param) => ctx.RandomManager.Next(100) < ResolveIntegerValue(param, ctx);
        _singleArgumentConditions["mobhere"] = (ctx, param) =>
            int.TryParse(param, out var vnum)
                ? ctx.Self.Room.NonPlayableCharacters.Any(x => x.Blueprint.Id == vnum && ctx.Self.CanSee(x))
                : ctx.Self.Room.NonPlayableCharacters.Any(x => StringCompareHelpers.AnyStringEquals(x.Keywords, param) && ctx.Self.CanSee(x));
        _singleArgumentConditions["objhere"] = (ctx, param) =>
            int.TryParse(param, out var vnum)
                ? ctx.Self.Room.Content.Any(x => x.Blueprint.Id == vnum && ctx.Self.CanSee(x))
                : ctx.Self.Room.Content.Any(x => StringCompareHelpers.AnyStringEquals(x.Keywords, param) && ctx.Self.CanSee(x));
        _singleArgumentConditions["mobexists"] = (ctx, param) => ctx.CharacterManager.Characters.OfType<INonPlayableCharacter>().Any(x => StringCompareHelpers.AnyStringEquals(x.Keywords, param) && ctx.Self.CanSee(x));
        _singleArgumentConditions["isnpc"] = (ctx, param) => ResolveMobVariable(param, ctx) is not null;
        _singleArgumentConditions["ispc"] = (ctx, param) => ResolvePlayerVariable(param, ctx) is not null;
        _singleArgumentConditions["isgood"] = (ctx, param) => ResolveCharacterVariable(param, ctx)?.Alignment >= 350;
        _singleArgumentConditions["isneutral"] = (ctx, param) =>
        {
            var align = ResolveCharacterVariable(param, ctx)?.Alignment;
            return align > -350 && align < 350;
        };
        _singleArgumentConditions["isevil"] = (ctx, param) => ResolveCharacterVariable(param, ctx)?.Alignment <= -350;
        _singleArgumentConditions["isimmort"] = (ctx, param) => ResolvePlayerVariable(param, ctx)?.ImpersonatedBy is IAdmin;
        _singleArgumentConditions["ischarm"] = (ctx, param) => ResolveMobVariable(param, ctx)?.CharacterFlags.IsSet("charm") == true; // only mob can be charmed
        _singleArgumentConditions["isfollow"] = (ctx, param) => false; // TODO Is $* a follower with their master in the room
        _singleArgumentConditions["isactive"] = (ctx, param) => ResolveCharacterVariable(param, ctx)?.Position > Positions.Sleeping;
        _singleArgumentConditions["isdelay"] = (ctx, param) => ResolveMobVariable(param, ctx)?.MobProgramDelay > 0;
        _singleArgumentConditions["isvisible"] = (ctx, param) =>
        {
            var mob = ResolveCharacterVariable(param, ctx);
            if (mob != null)
                return ctx.Self.CanSee(mob);
            var item = ResolveItemVariable(param, ctx);
            if (item != null)
                return ctx.Self.CanSee(item);
            return false;
        };
        _singleArgumentConditions["hastarget"] = (ctx, param) =>
        {
            var mob = ResolveMobVariable(param, ctx);
            if (mob == null)
                return false;
            return mob.MobProgramTarget?.Room == mob.Room;
        };
        _singleArgumentConditions["istarget"] = (ctx, param) =>
        {
            if (ctx.Target == null)
                return false;
            var ch = ResolveCharacterVariable(param, ctx);
            if (ch == null)
                return false;
            return ctx.Target == ch;
        };

        // two arguments
        // character
        _twoArgumentsConditions["affected"] = (ctx, var, affect) => ResolveCharacterVariable(var, ctx)?.CharacterFlags.IsSet(affect) == true; // TODO: could also be used for items
        _twoArgumentsConditions["act"] = (ctx, var, act) => ResolveMobVariable(var, ctx)?.ActFlags.IsSet(act) == true;
        _twoArgumentsConditions["off"] = (ctx, var, off) => ResolveMobVariable(var, ctx)?.OffensiveFlags.IsSet(off) == true;
        _twoArgumentsConditions["imm"] = (ctx, var, imm) => ResolveCharacterVariable(var, ctx)?.Immunities.IsSet(imm) == true;
        _twoArgumentsConditions["carries"] = (ctx, var, itemName) => ResolveCharacterVariable(var, ctx)?.Inventory.Any(x => StringCompareHelpers.AnyStringEquals(x.Keywords, itemName)) == true;
        _twoArgumentsConditions["wears"] = (ctx, var, itemName) => ResolveCharacterVariable(var, ctx)?.Equipments.Any(x => x.Item is not null && StringCompareHelpers.AnyStringEquals(x.Item.Keywords, itemName)) == true;
        _twoArgumentsConditions["has"] = (ctx, var, itemType) => ResolveCharacterVariable(var, ctx)?.Inventory.Any(x => StringCompareHelpers.StringEquals(x.ItemType, itemType)) == true;
        _twoArgumentsConditions["uses"] = (ctx, var, itemType) => ResolveCharacterVariable(var, ctx)?.Equipments.Any(x => x.Item is not null && StringCompareHelpers.StringEquals(x.Item.ItemType, itemType)) == true;
        _twoArgumentsConditions["name"] = (ctx, var, name) => StringCompareHelpers.AnyStringEquals(ResolveEntityVariable(var, ctx)?.Keywords ?? [], name) == true;
        _twoArgumentsConditions["pos"] = (ctx, var, position) => StringCompareHelpers.StringEquals(ResolveCharacterVariable(var, ctx)?.Position.ToString() ?? string.Empty, position);
        _twoArgumentsConditions["clan"] = (ctx, var, clanName) => false; // TODO
        _twoArgumentsConditions["race"] = (ctx, var, raceName) => StringCompareHelpers.StringEquals(ResolveCharacterVariable(var, ctx)?.Race?.Name ?? string.Empty, raceName);
        _twoArgumentsConditions["class"] = (ctx, var, className) => StringCompareHelpers.AnyStringEquals(ResolveCharacterVariable(var, ctx)?.Classes?.Select(x => x.Name) ?? [], className);
        // item
        _twoArgumentsConditions["objtype"] = (ctx, item, itemType) => StringCompareHelpers.StringEquals(ResolveItemVariable(item, ctx)?.ItemType ?? string.Empty, itemType);
    }

    private void RegisterFunctions()
    {
        _parameterLessIntegerFunctions["rand"] = (ctx) => ctx.RandomManager.Next(100);
        _parameterLessIntegerFunctions["people"] = (ctx) => ctx.Self.Room.People.Count(x => x != ctx.Self);
        _parameterLessIntegerFunctions["players"] = (ctx) => ctx.Self.Room.PlayableCharacters.Count();
        _parameterLessIntegerFunctions["mobs"] = (ctx) => ctx.Self.Room.NonPlayableCharacters.Count(x => x != ctx.Self);
        _parameterLessIntegerFunctions["clones"] = (ctx) => ctx.Self.Room.NonPlayableCharacters.Count(x => x != ctx.Self && x.Blueprint.Id == ctx.Self.Blueprint.Id);
        _parameterLessIntegerFunctions["order"] = (ctx) => Array.IndexOf(ctx.Self.Room.NonPlayableCharacters.ToArray(), ctx.Self);
        _parameterLessIntegerFunctions["hour"] = (ctx) => ctx.TimeManager.Hour;

        // character
        _oneParameterIntegerFunctions["vnum"] = (ctx, ch) => ResolveMobVariable(ch, ctx)?.Blueprint?.Id;
        _oneParameterIntegerFunctions["hpcnt"] = (ctx, ch) =>
        {
            var character = ResolveCharacterVariable(ch, ctx);
            if (character == null)
                return null;
            return (100 * character[ResourceKinds.HitPoints]) / character.MaxResource(ResourceKinds.HitPoints);
        };
        _oneParameterIntegerFunctions["room"] = (ctx, var) => ResolveMobVariable(var, ctx)?.Room?.Blueprint?.Id;
        _oneParameterIntegerFunctions["sex"] = (ctx, var) => (long?)ResolveMobVariable(var, ctx)?.Sex;
        _oneParameterIntegerFunctions["level"] = (ctx, var) => ResolveMobVariable(var, ctx)?.Level;
        _oneParameterIntegerFunctions["align"] = (ctx, var) => ResolveMobVariable(var, ctx)?.Alignment;
        _oneParameterIntegerFunctions["money"] = (ctx, var) => ResolveMobVariable(var, ctx)?.Wealth;
        // item
        _oneParameterIntegerFunctions["objval0"] = (ctx, var) => 0; // TODO: not available for the moment in ItemBlueprintBase
        _oneParameterIntegerFunctions["objval1"] = (ctx, var) => 0; // TODO
        _oneParameterIntegerFunctions["objval2"] = (ctx, var) => 0; // TODO
        _oneParameterIntegerFunctions["objval3"] = (ctx, var) => 0; // TODO
        _oneParameterIntegerFunctions["objval4"] = (ctx, var) => 0; // TODO
    }
}