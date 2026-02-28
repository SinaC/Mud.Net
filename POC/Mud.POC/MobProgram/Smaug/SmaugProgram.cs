using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mud.POC.MobProgram.Smaug;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

    #region World Simulation
    public class Item { public string Name { get; set; } = ""; }
    public class Room
    {
        public int Vnum { get; set; }
        public List<Mob> Mobs { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public List<Item> Objects { get; set; } = new();
        public int CountMob(string name) => Mobs.Count(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public class Mob
    {
        public string Name { get; set; } = "";
        public bool IsNpc { get; set; } = true;
        public int HitPoints { get; set; } = 100;
        public Room CurrentRoom { get; set; } = null!;
        public bool Mounted { get; set; } = false;
        public bool InCombat { get; set; } = false;
        public int Alignment { get; set; } = 0;
        public bool IsImmortal { get; set; } = false;
        public List<Item> Inventory { get; set; } = new();
        public void ReceiveDamage(int amount)
        {
            HitPoints -= amount;
            Console.WriteLine($"{Name} takes {amount} damage. HP={HitPoints}");
            if (HitPoints <= 0) Console.WriteLine($"{Name} has died!");
        }
        public void Say(string msg) => Console.WriteLine($"{Name} says: {msg}");
        public void Act(string msg) => Console.WriteLine($"{Name} acts: {msg}");
        public void Emote(string msg) => Console.WriteLine($"{Name} emotes: {msg}");
        public bool CanSee(Mob target) => true;
    }

    public class Player : Mob
    {
        public Player(string name) { Name = name; IsNpc = false; }
    }
    #endregion

    #region MobProg AST
    public enum SmaugTriggerType { Act, Fight, Rand, Greet, AllGreet }
    public record SmaugTriggerHeader { public SmaugTriggerType Type { get; init; } public char? ActFlag { get; init; } public string? ActPattern { get; init; } }
    public abstract record MpStatement;
    public record MpCommand(string CommandName, List<string> Args) : MpStatement;
    public record MpIf(string Condition, List<MpStatement> ThenBlock, List<(string, List<MpStatement>)> ElseIfBlocks, List<MpStatement> ElseBlock) : MpStatement;
    public record SmaugMobProgBlock { public SmaugTriggerHeader Header { get; init; } public List<MpStatement> Statements { get; init; } = new(); }
    #endregion

    #region Parser
    public static class SmaugTriggerHeaderParser
    {
        public static SmaugTriggerHeader Parse(string line)
        {
            line = line.Trim('>', '~').Trim();
            var parts = line.Split(' ', 3);
            SmaugTriggerType type = parts[0].ToLower() switch
            {
                "act_prog" => SmaugTriggerType.Act,
                "fight_prog" => SmaugTriggerType.Fight,
                "rand_prog" => SmaugTriggerType.Rand,
                "greet_prog" => SmaugTriggerType.Greet,
                "all_greet_prog" => SmaugTriggerType.AllGreet,
                _ => throw new Exception($"Unknown trigger type {parts[0]}")
            };
            char? flag = null;
            string? pattern = null;
            if (parts.Length > 1)
            {
                if (parts[1].Length == 1 && type == SmaugTriggerType.Act)
                {
                    flag = parts[1][0];
                    pattern = parts.Length > 2 ? parts[2] : null;
                }
                else
                    pattern = string.Join(' ', parts.Skip(1));
            }
            return new SmaugTriggerHeader { Type = type, ActFlag = flag, ActPattern = pattern };
        }
    }

//public static class SmaugMobProgParser
//{
//    public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
//    {
//        var enumerator = lines.GetEnumerator();
//        if (!enumerator.MoveNext()) throw new Exception("Empty block");
//        var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);
//        var statements = new List<MpStatement>();
//        ParseStatements(enumerator, statements);
//        return new SmaugMobProgBlock { Header = header, Statements = statements };
//    }

//    private static void ParseStatements(IEnumerator<string> lines, List<MpStatement> output)
//    {
//        while (lines.MoveNext())
//        {
//            var line = lines.Current.Trim();
//            if (line == "~") return;
//            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
//            {
//                string condition = line.Substring(3).Trim();
//                var thenBlock = new List<MpStatement>();
//                var elseIfBlocks = new List<(string, List<MpStatement>)>();
//                var elseBlock = new List<MpStatement>();
//                ParseStatements(lines, thenBlock);

//                bool done = false;
//                while (!done && lines.Current != null)
//                {
//                    var l = lines.Current.Trim();
//                    if (l.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
//                    {
//                        string elseifCond = l.Substring(7).Trim();
//                        var elseifBlock = new List<MpStatement>();
//                        ParseStatements(lines, elseifBlock);
//                        elseIfBlocks.Add((elseifCond, elseifBlock));
//                    }
//                    else if (l.Equals("else", StringComparison.OrdinalIgnoreCase))
//                    {
//                        ParseStatements(lines, elseBlock);
//                    }
//                    else if (l.Equals("endif", StringComparison.OrdinalIgnoreCase))
//                    {
//                        done = true;
//                        break;
//                    }
//                    else break;
//                }
//                output.Add(new MpIf(condition, thenBlock, elseIfBlocks, elseBlock));
//            }
//            else
//            {
//                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
//                if (parts.Count > 0) output.Add(new MpCommand(parts[0], parts.Skip(1).ToList()));
//            }
//        }
//    }
//}
public static class SmaugMobProgParser
{
    public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
    {
        using var enumerator = lines.GetEnumerator();
        if (!enumerator.MoveNext()) throw new Exception("Empty block");

        var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);
        var statements = ParseStatements(enumerator).ToList();

        return new SmaugMobProgBlock { Header = header, Statements = statements };
    }

    private static IEnumerable<MpStatement> ParseStatements(IEnumerator<string> lines)
    {
        while (lines.Current != null)
        {
            string line = lines.Current.Trim();

            if (line == "~") yield break;

            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
            {
                string condition = line.Substring(3).Trim();

                // Consume then block
                var thenBlock = new List<MpStatement>();
                while (lines.MoveNext())
                {
                    string nextLine = lines.Current.Trim();
                    if (nextLine.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase) ||
                        nextLine.Equals("else", StringComparison.OrdinalIgnoreCase) ||
                        nextLine.Equals("endif", StringComparison.OrdinalIgnoreCase))
                        break;
                    thenBlock.AddRange(ParseStatements(lines));
                }

                // Consume elseif blocks
                var elseIfBlocks = new List<(string, List<MpStatement>)>();
                while (lines.Current != null && lines.Current.Trim().StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
                {
                    string elseifCond = lines.Current.Trim().Substring(7).Trim();
                    lines.MoveNext();
                    var elseifBlock = ParseStatements(lines).ToList();
                    elseIfBlocks.Add((elseifCond, elseifBlock));
                }

                // Consume else block
                List<MpStatement> elseBlock = new();
                if (lines.Current != null && lines.Current.Trim().Equals("else", StringComparison.OrdinalIgnoreCase))
                {
                    lines.MoveNext();
                    elseBlock = ParseStatements(lines).ToList();
                }

                // Skip endif
                if (lines.Current != null && lines.Current.Trim().Equals("endif", StringComparison.OrdinalIgnoreCase))
                    lines.MoveNext();

                yield return new MpIf(condition, thenBlock, elseIfBlocks, elseBlock);
            }
            else
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    yield return new MpCommand(parts[0], parts.Skip(1).ToList());
                lines.MoveNext();
            }
        }
    }
}
#endregion

#region Variables
public static class VariableSubstitutor
    {
        public static string Resolve(string text, MobExecutionContext ctx)
        {
            return text.Replace("$n", ctx.Target?.Name ?? "")
                       .Replace("$r", ctx.Victim?.Name ?? "")
                       .Replace("$i", ctx.Self?.Name ?? "")
                       .Replace("$t", ctx.Secondary?.Name ?? "")
                       .Replace("$I", ctx.Self?.Name.ToUpper() ?? "")
                       .Replace("$N", ctx.Target?.Name.ToUpper() ?? "")
                       .Replace("$R", ctx.Victim?.Name.ToUpper() ?? "")
                       .Replace("$T", ctx.Secondary?.Name.ToUpper() ?? "");
        }
    }
    #endregion

    #region Expression Engine with Extended Smaug Functions
    public class ExpressionEngine
    {
        private readonly Dictionary<string, Func<MobExecutionContext, string[], bool>> _functions;
        private readonly Random _rand = new();

        public ExpressionEngine()
        {
            _functions = new Dictionary<string, Func<MobExecutionContext, string[], bool>>(StringComparer.OrdinalIgnoreCase)
            {
                ["rand"] = (ctx, args) => args.Length > 0 && int.TryParse(args[0], out int p) && _rand.Next(100) < p,
                ["ispc"] = (ctx, args) => ctx.Target != null && !ctx.Target.IsNpc,
                ["isnpc"] = (ctx, args) => ctx.Target != null && ctx.Target.IsNpc,
                ["mobinroom"] = (ctx, args) =>
                {
                    if (args.Length < 2) return false;
                    string targetName = args[1].Trim('\'');
                    int count = ctx.Self.CurrentRoom.Mobs.Count(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    return int.TryParse(args[0], out int expected) && count == expected;
                },
                ["ismounted"] = (ctx, args) => ctx.Target?.Mounted ?? false,
                ["cansee"] = (ctx, args) => ctx.Target != null && ctx.Self.CanSee(ctx.Target),
                ["isfighting"] = (ctx, args) => ctx.Target?.InCombat ?? false,
                ["isimmort"] = (ctx, args) => ctx.Target?.IsImmortal ?? false,
                ["isgood"] = (ctx, args) => ctx.Target != null && ctx.Target.Alignment > 1000,
                ["isevil"] = (ctx, args) => ctx.Target != null && ctx.Target.Alignment < -1000,
                ["isneutral"] = (ctx, args) => ctx.Target != null && ctx.Target.Alignment >= -1000 && ctx.Target.Alignment <= 1000,
                ["objhere"] = (ctx, args) => ctx.Self.CurrentRoom.Objects.Any(o => o.Name.Equals(args[0].Trim('\''), StringComparison.OrdinalIgnoreCase)),
                ["hasitem"] = (ctx, args) => ctx.Target != null && ctx.Target.Inventory.Any(i => i.Name.Equals(args[0].Trim('\''), StringComparison.OrdinalIgnoreCase))
            };
        }

        public bool Evaluate(MobExecutionContext ctx, string expr)
        {
            expr = expr.Trim();
            if (expr.StartsWith("!")) return !Evaluate(ctx, expr.Substring(1));

            int andIndex = expr.IndexOf("&&", StringComparison.Ordinal);
            if (andIndex >= 0)
            {
                var left = expr.Substring(0, andIndex);
                var right = expr.Substring(andIndex + 2);
                return Evaluate(ctx, left) && Evaluate(ctx, right);
            }

            int orIndex = expr.IndexOf("||", StringComparison.Ordinal);
            if (orIndex >= 0)
            {
                var left = expr.Substring(0, orIndex);
                var right = expr.Substring(orIndex + 2);
                return Evaluate(ctx, left) || Evaluate(ctx, right);
            }

            string[] numericOps = new[] { ">=", "<=", "==", "!=", "<", ">" };
            foreach (var op in numericOps)
            {
                int idx = expr.IndexOf(op, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    string left = expr.Substring(0, idx).Trim();
                    string right = expr.Substring(idx + op.Length).Trim();
                    if (double.TryParse(ResolveValue(left, ctx), out double lVal) && double.TryParse(ResolveValue(right, ctx), out double rVal))
                    {
                        return op switch
                        {
                            ">" => lVal > rVal,
                            "<" => lVal < rVal,
                            ">=" => lVal >= rVal,
                            "<=" => lVal <= rVal,
                            "==" => lVal == rVal,
                            "!=" => lVal != rVal,
                            _ => false
                        };
                    }
                    return ResolveValue(left, ctx).Equals(ResolveValue(right, ctx), StringComparison.OrdinalIgnoreCase);
                }
            }

            var match = Regex.Match(expr, @"(\w+)\((.*)\)");
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string[] args = match.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
                if (_functions.TryGetValue(name, out var func)) return func(ctx, args);
            }

            return bool.TryParse(expr, out bool bVal) && bVal;
        }

        private string ResolveValue(string s, MobExecutionContext ctx)
            => s.Replace("$n", ctx.Target?.Name ?? "")
                .Replace("$i", ctx.Self?.Name ?? "")
                .Replace("$r", ctx.Victim?.Name ?? "")
                .Replace("$t", ctx.Secondary?.Name ?? "");

        public void RegisterFunction(string name, Func<MobExecutionContext, string[], bool> func) => _functions[name] = func;
    }
    #endregion

    #region Commands
    public interface IMobCommand { void Execute(MobExecutionContext ctx, List<string> args); }
    public class MobExecutionContext { public Mob Self { get; set; } = null!; public Mob? Target { get; set; } public Mob? Victim { get; set; } public Mob? Secondary { get; set; } }
    public class CommandRegistry
    {
        private readonly Dictionary<string, IMobCommand> _commands = new();
        public void Register(string name, IMobCommand cmd) => _commands[name.ToLower()] = cmd;
        public bool TryGet(string name, out IMobCommand cmd) => _commands.TryGetValue(name.ToLower(), out cmd);
    }

    public static class SmaugCommands
    {
        public static void RegisterAll(CommandRegistry registry)
        {
            registry.Register("say", new SayCommand());
            registry.Register("snicker", new SimpleActionCommand("snickers"));
            registry.Register("chuckle", new SimpleActionCommand("chuckles"));
            registry.Register("mpslay", new MpSlayCommand());
            registry.Register("mpdamage", new MpDamageCommand());
            registry.Register("mpe", new MpeCommand());
        }

        private class SimpleActionCommand : IMobCommand { private readonly string _action; public SimpleActionCommand(string a) => _action = a; public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Act(_action); }
        private class SayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Say(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); }
        private class MpSlayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null) return; Console.WriteLine($"{ctx.Self.Name} slays {ctx.Target.Name}!"); ctx.Target.HitPoints = 0; } }
        private class MpDamageCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null || args.Count < 1) return; if (int.TryParse(args[0], out int dmg)) ctx.Target.ReceiveDamage(dmg); } }
        private class MpeCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { ctx.Self.Emote(VariableSubstitutor.Resolve(string.Join(' ', args), ctx)); } }
    }
    #endregion

    #region Trigger Dispatcher
    public static class ActPatternMatcher
    {
        public static bool Match(string actText, SmaugTriggerHeader header, MobExecutionContext ctx)
        {
            if (header.ActPattern == null) return false;
            string pattern = Regex.Escape(header.ActPattern).Replace("\\*", ".*");
            bool match = Regex.IsMatch(actText, pattern, RegexOptions.IgnoreCase);
            if (!match) return false;
            if (header.ActFlag == 'p' && ctx.Target != null && !actText.Contains(ctx.Target.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if (header.ActFlag == 'r' && ctx.Self != null && !actText.Contains(ctx.Self.Name, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
    }

    public class MobProgExecutor
    {
        private readonly CommandRegistry _registry;
        private readonly ExpressionEngine _engine = new ExpressionEngine();
        public MobProgExecutor(CommandRegistry registry) => _registry = registry;
        public void ExecuteBlock(SmaugMobProgBlock block, MobExecutionContext ctx) => ExecuteStatements(block.Statements, ctx);
        private void ExecuteStatements(List<MpStatement> statements, MobExecutionContext ctx)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case MpCommand cmd:
                        if (_registry.TryGet(cmd.CommandName, out var command)) command.Execute(ctx, cmd.Args);
                        else Console.WriteLine($"Unknown command: {cmd.CommandName}");
                        break;
                    case MpIf ifStmt:
                        if (_engine.Evaluate(ctx, ifStmt.Condition)) ExecuteStatements(ifStmt.ThenBlock, ctx);
                        else
                        {
                            bool executed = false;
                            foreach (var (e, blk) in ifStmt.ElseIfBlocks)
                                if (_engine.Evaluate(ctx, e)) { ExecuteStatements(blk, ctx); executed = true; break; }
                            if (!executed && ifStmt.ElseBlock.Count > 0) ExecuteStatements(ifStmt.ElseBlock, ctx);
                        }
                        break;
                }
            }
        }
    }

    public class TriggerDispatcher
    {
        private readonly List<SmaugMobProgBlock> _mobProgs = new();
        private readonly MobProgExecutor _executor;
        public TriggerDispatcher(MobProgExecutor executor) => _executor = executor;
        public void Register(SmaugMobProgBlock block) => _mobProgs.Add(block);
        public void Dispatch(string triggerType, string actText, MobExecutionContext ctx)
        {
            foreach (var prog in _mobProgs)
            {
                if (prog.Header.Type.ToString().Equals(triggerType, StringComparison.OrdinalIgnoreCase))
                {
                    if (prog.Header.Type == SmaugTriggerType.Act && !ActPatternMatcher.Match(actText, prog.Header, ctx)) continue;
                    _executor.ExecuteBlock(prog, ctx);
                }
            }
        }
    }
    #endregion

/*
    #region World Simulation
    public class Room
    {
        public int Vnum { get; set; }
        public List<Mob> Mobs { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public int CountMob(string name) => Mobs.Count(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public class Mob
    {
        public string Name { get; set; } = "";
        public bool IsNpc { get; set; } = true;
        public int HitPoints { get; set; } = 100;
        public Room CurrentRoom { get; set; } = null!;

        public void ReceiveDamage(int amount)
        {
            HitPoints -= amount;
            Console.WriteLine($"{Name} takes {amount} damage. HP={HitPoints}");
            if (HitPoints <= 0) Console.WriteLine($"{Name} has died!");
        }
        public void Say(string msg) => Console.WriteLine($"{Name} says: {msg}");
        public void Act(string msg) => Console.WriteLine($"{Name} acts: {msg}");
        public void Emote(string msg) => Console.WriteLine($"{Name} emotes: {msg}");
    }

    public class Player : Mob
    {
        public Player(string name)
        {
            Name = name;
            IsNpc = false;
        }
    }
    #endregion

    #region MobProg AST
    public enum SmaugTriggerType { Act, Fight, Rand, Greet, AllGreet }
    public record SmaugTriggerHeader { public SmaugTriggerType Type { get; init; } public char? ActFlag { get; init; } public string? ActPattern { get; init; } }
    public abstract record MpStatement;
    public record MpCommand(string CommandName, List<string> Args) : MpStatement;
    public record MpIf(string Condition, List<MpStatement> ThenBlock, List<(string, List<MpStatement>)> ElseIfBlocks, List<MpStatement> ElseBlock) : MpStatement;
    public record SmaugMobProgBlock { public SmaugTriggerHeader Header { get; init; } public List<MpStatement> Statements { get; init; } = new(); }
    #endregion

    #region Parser
    public static class SmaugTriggerHeaderParser
    {
        public static SmaugTriggerHeader Parse(string line)
        {
            line = line.Trim('>', '~').Trim();
            var parts = line.Split(' ', 3);
            SmaugTriggerType type = parts[0].ToLower() switch
            {
                "act_prog" => SmaugTriggerType.Act,
                "fight_prog" => SmaugTriggerType.Fight,
                "rand_prog" => SmaugTriggerType.Rand,
                "greet_prog" => SmaugTriggerType.Greet,
                "all_greet_prog" => SmaugTriggerType.AllGreet,
                _ => throw new Exception($"Unknown trigger type {parts[0]}")
            };
            char? flag = null;
            string? pattern = null;
            if (parts.Length > 1)
            {
                if (parts[1].Length == 1 && type == SmaugTriggerType.Act)
                {
                    flag = parts[1][0];
                    pattern = parts.Length > 2 ? parts[2] : null;
                }
                else
                    pattern = string.Join(' ', parts.Skip(1));
            }
            return new SmaugTriggerHeader { Type = type, ActFlag = flag, ActPattern = pattern };
        }
    }

    public static class SmaugMobProgParser
    {
        public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            if (!enumerator.MoveNext()) throw new Exception("Empty block");
            var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);
            var statements = new List<MpStatement>();
            ParseStatements(enumerator, statements);
            return new SmaugMobProgBlock { Header = header, Statements = statements };
        }

        private static void ParseStatements(IEnumerator<string> lines, List<MpStatement> output)
        {
            while (lines.MoveNext())
            {
                var line = lines.Current.Trim();
                if (line == "~") return;
                if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                {
                    string condition = line.Substring(3).Trim();
                    var thenBlock = new List<MpStatement>();
                    var elseIfBlocks = new List<(string, List<MpStatement>)>();
                    var elseBlock = new List<MpStatement>();
                    ParseStatements(lines, thenBlock);

                    bool done = false;
                    while (!done && lines.Current != null)
                    {
                        var l = lines.Current.Trim();
                        if (l.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
                        {
                            string elseifCond = l.Substring(7).Trim();
                            var elseifBlock = new List<MpStatement>();
                            ParseStatements(lines, elseifBlock);
                            elseIfBlocks.Add((elseifCond, elseifBlock));
                        }
                        else if (l.Equals("else", StringComparison.OrdinalIgnoreCase))
                        {
                            ParseStatements(lines, elseBlock);
                        }
                        else if (l.Equals("endif", StringComparison.OrdinalIgnoreCase))
                        {
                            done = true;
                            break;
                        }
                        else break;
                    }
                    output.Add(new MpIf(condition, thenBlock, elseIfBlocks, elseBlock));
                }
                else
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (parts.Count > 0) output.Add(new MpCommand(parts[0], parts.Skip(1).ToList()));
                }
            }
        }
    }
    #endregion

    #region Variables
    public static class VariableSubstitutor
    {
        public static string Resolve(string text, MobExecutionContext ctx)
        {
            return text.Replace("$n", ctx.Target?.Name ?? "")
                       .Replace("$r", ctx.Victim?.Name ?? "")
                       .Replace("$i", ctx.Self?.Name ?? "")
                       .Replace("$t", ctx.Secondary?.Name ?? "")
                       .Replace("$I", ctx.Self?.Name.ToUpper() ?? "")
                       .Replace("$N", ctx.Target?.Name.ToUpper() ?? "")
                       .Replace("$R", ctx.Victim?.Name.ToUpper() ?? "")
                       .Replace("$T", ctx.Secondary?.Name.ToUpper() ?? "");
        }
    }
    #endregion

    #region Expression Engine with Numeric & Boolean
    public class ExpressionEngine
    {
        private readonly Dictionary<string, Func<MobExecutionContext, string[], bool>> _functions;
        private readonly Random _rand = new Random();

        public ExpressionEngine()
        {
            _functions = new Dictionary<string, Func<MobExecutionContext, string[], bool>>(StringComparer.OrdinalIgnoreCase)
            {
                ["rand"] = (ctx, args) => args.Length > 0 && int.TryParse(args[0], out int p) && _rand.Next(100) < p,
                ["ispc"] = (ctx, args) => ctx.Target != null && !ctx.Target.IsNpc,
                ["isnpc"] = (ctx, args) => ctx.Target != null && ctx.Target.IsNpc,
                ["mobinroom"] = (ctx, args) =>
                {
                    if (args.Length < 2) return false;
                    string targetName = args[1].Trim('\'');
                    int count = ctx.Self.CurrentRoom.Mobs.Count(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                    return int.TryParse(args[0], out int expected) && count == expected;
                }
            };
        }

        public bool Evaluate(MobExecutionContext ctx, string expr)
        {
            expr = expr.Trim();

            if (expr.StartsWith("!")) return !Evaluate(ctx, expr.Substring(1));

            int andIndex = expr.IndexOf("&&", StringComparison.Ordinal);
            if (andIndex >= 0)
            {
                var left = expr.Substring(0, andIndex);
                var right = expr.Substring(andIndex + 2);
                return Evaluate(ctx, left) && Evaluate(ctx, right);
            }

            int orIndex = expr.IndexOf("||", StringComparison.Ordinal);
            if (orIndex >= 0)
            {
                var left = expr.Substring(0, orIndex);
                var right = expr.Substring(orIndex + 2);
                return Evaluate(ctx, left) || Evaluate(ctx, right);
            }

            string[] numericOps = new[] { ">=", "<=", "==", "!=", "<", ">" };
            foreach (var op in numericOps)
            {
                int idx = expr.IndexOf(op, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    string left = expr.Substring(0, idx).Trim();
                    string right = expr.Substring(idx + op.Length).Trim();
                    if (double.TryParse(ResolveValue(left, ctx), out double lVal) && double.TryParse(ResolveValue(right, ctx), out double rVal))
                    {
                        return op switch
                        {
                            ">" => lVal > rVal,
                            "<" => lVal < rVal,
                            ">=" => lVal >= rVal,
                            "<=" => lVal <= rVal,
                            "==" => lVal == rVal,
                            "!=" => lVal != rVal,
                            _ => false
                        };
                    }
                    return ResolveValue(left, ctx).Equals(ResolveValue(right, ctx), StringComparison.OrdinalIgnoreCase);
                }
            }

            var match = Regex.Match(expr, @"(\w+)\((.*)\)");
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string[] args = match.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
                if (_functions.TryGetValue(name, out var func)) return func(ctx, args);
            }

            return bool.TryParse(expr, out bool bVal) && bVal;
        }

        private string ResolveValue(string s, MobExecutionContext ctx)
        {
            return s.Replace("$n", ctx.Target?.Name ?? "")
                    .Replace("$i", ctx.Self?.Name ?? "")
                    .Replace("$r", ctx.Victim?.Name ?? "")
                    .Replace("$t", ctx.Secondary?.Name ?? "");
        }

        public void RegisterFunction(string name, Func<MobExecutionContext, string[], bool> func) => _functions[name] = func;
    }
    #endregion

    #region Commands
    public interface IMobCommand { void Execute(MobExecutionContext ctx, List<string> args); }
    public class MobExecutionContext { public Mob Self { get; set; } = null!; public Mob? Target { get; set; } public Mob? Victim { get; set; } public Mob? Secondary { get; set; } }
    public class CommandRegistry
    {
        private readonly Dictionary<string, IMobCommand> _commands = new();
        public void Register(string name, IMobCommand cmd) => _commands[name.ToLower()] = cmd;
        public bool TryGet(string name, out IMobCommand cmd) => _commands.TryGetValue(name.ToLower(), out cmd);
    }

    public static class SmaugCommands
    {
        public static void RegisterAll(CommandRegistry registry)
        {
            registry.Register("say", new SayCommand());
            registry.Register("snicker", new SimpleActionCommand("snickers"));
            registry.Register("chuckle", new SimpleActionCommand("chuckles"));
            registry.Register("mpslay", new MpSlayCommand());
            registry.Register("mpdamage", new MpDamageCommand());
            registry.Register("mpe", new MpeCommand());
        }

        private class SimpleActionCommand : IMobCommand { private readonly string _action; public SimpleActionCommand(string a) => _action = a; public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Act(_action); }
        private class SayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Say(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); }
        private class MpSlayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null) return; Console.WriteLine($"{ctx.Self.Name} slays {ctx.Target.Name}!"); ctx.Target.HitPoints = 0; } }
        private class MpDamageCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null || args.Count < 1) return; if (int.TryParse(args[0], out int dmg)) ctx.Target.ReceiveDamage(dmg); } }
        private class MpeCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { ctx.Self.Emote(VariableSubstitutor.Resolve(string.Join(' ', args), ctx)); } }
    }
    #endregion

    #region Trigger Dispatcher
    public static class ActPatternMatcher
    {
        public static bool Match(string actText, SmaugTriggerHeader header, MobExecutionContext ctx)
        {
            if (header.ActPattern == null) return false;
            string pattern = Regex.Escape(header.ActPattern).Replace("\\*", ".*");
            bool match = Regex.IsMatch(actText, pattern, RegexOptions.IgnoreCase);
            if (!match) return false;
            if (header.ActFlag == 'p' && ctx.Target != null && !actText.Contains(ctx.Target.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if (header.ActFlag == 'r' && ctx.Self != null && !actText.Contains(ctx.Self.Name, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
    }

    public class MobProgExecutor
    {
        private readonly CommandRegistry _registry;
        private readonly ExpressionEngine _engine = new ExpressionEngine();
        public MobProgExecutor(CommandRegistry registry) => _registry = registry;
        public void ExecuteBlock(SmaugMobProgBlock block, MobExecutionContext ctx) => ExecuteStatements(block.Statements, ctx);
        private void ExecuteStatements(List<MpStatement> statements, MobExecutionContext ctx)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case MpCommand cmd:
                        if (_registry.TryGet(cmd.CommandName, out var command)) command.Execute(ctx, cmd.Args);
                        else Console.WriteLine($"Unknown command: {cmd.CommandName}");
                        break;
                    case MpIf ifStmt:
                        if (_engine.Evaluate(ctx, ifStmt.Condition)) ExecuteStatements(ifStmt.ThenBlock, ctx);
                        else
                        {
                            bool executed = false;
                            foreach (var (e, blk) in ifStmt.ElseIfBlocks)
                                if (_engine.Evaluate(ctx, e)) { ExecuteStatements(blk, ctx); executed = true; break; }
                            if (!executed && ifStmt.ElseBlock.Count > 0) ExecuteStatements(ifStmt.ElseBlock, ctx);
                        }
                        break;
                }
            }
        }
    }

    public class TriggerDispatcher
    {
        private readonly List<SmaugMobProgBlock> _mobProgs = new();
        private readonly MobProgExecutor _executor;
        public TriggerDispatcher(MobProgExecutor executor) => _executor = executor;
        public void Register(SmaugMobProgBlock block) => _mobProgs.Add(block);
        public void Dispatch(string triggerType, string actText, MobExecutionContext ctx)
        {
            foreach (var prog in _mobProgs)
            {
                if (prog.Header.Type.ToString().Equals(triggerType, StringComparison.OrdinalIgnoreCase))
                {
                    if (prog.Header.Type == SmaugTriggerType.Act && !ActPatternMatcher.Match(actText, prog.Header, ctx)) continue;
                    _executor.ExecuteBlock(prog, ctx);
                }
            }
        }
    }
    #endregion
*/

/*
    #region World Simulation
    public class Room
    {
        public int Vnum { get; set; }
        public List<Mob> Mobs { get; set; } = new();
        public List<Player> Players { get; set; } = new();
        public int CountMob(string name) => Mobs.Count(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public class Mob
    {
        public string Name { get; set; } = "";
        public bool IsNpc { get; set; } = true;
        public int Level { get; set; } = 1;
        public int HitPoints { get; set; } = 100;
        public Room CurrentRoom { get; set; } = null!;
        public void ReceiveDamage(int amount)
        {
            HitPoints -= amount;
            Console.WriteLine($"{Name} takes {amount} damage. HP={HitPoints}");
            if (HitPoints <= 0) Console.WriteLine($"{Name} has died!");
        }
        public void Say(string msg) => Console.WriteLine($"{Name} says: {msg}");
        public void Act(string msg) => Console.WriteLine($"{Name} acts: {msg}");
        public void Emote(string msg) => Console.WriteLine($"{Name} emotes: {msg}");
    }

    public class Player : Mob
    {
        public Player(string name)
        {
            Name = name;
            IsNpc = false;
        }
    }
    #endregion

    #region MobProg AST
    public enum SmaugTriggerType { Act, Fight, Rand, Greet, AllGreet }
    public record SmaugTriggerHeader { public SmaugTriggerType Type { get; init; } public char? ActFlag { get; init; } public string? ActPattern { get; init; } }
    public abstract record MpStatement;
    public record MpCommand(string CommandName, List<string> Args) : MpStatement;
    public record MpIf(string Condition, List<MpStatement> ThenBlock, List<(string, List<MpStatement>)> ElseIfBlocks, List<MpStatement> ElseBlock) : MpStatement;
    public record SmaugMobProgBlock { public SmaugTriggerHeader Header { get; init; } public List<MpStatement> Statements { get; init; } = new(); }
    #endregion

    #region Parser
    public static class SmaugTriggerHeaderParser
    {
        public static SmaugTriggerHeader Parse(string line)
        {
            line = line.Trim('>', '~').Trim();
            var parts = line.Split(' ', 3);
            SmaugTriggerType type = parts[0].ToLower() switch
            {
                "act_prog" => SmaugTriggerType.Act,
                "fight_prog" => SmaugTriggerType.Fight,
                "rand_prog" => SmaugTriggerType.Rand,
                "greet_prog" => SmaugTriggerType.Greet,
                "all_greet_prog" => SmaugTriggerType.AllGreet,
                _ => throw new Exception($"Unknown trigger type {parts[0]}")
            };
            char? flag = null;
            string? pattern = null;
            if (parts.Length > 1)
            {
                if (parts[1].Length == 1 && type == SmaugTriggerType.Act)
                {
                    flag = parts[1][0];
                    pattern = parts.Length > 2 ? parts[2] : null;
                }
                else
                    pattern = string.Join(' ', parts.Skip(1));
            }
            return new SmaugTriggerHeader { Type = type, ActFlag = flag, ActPattern = pattern };
        }
    }

    public static class SmaugMobProgParser
    {
        public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            if (!enumerator.MoveNext()) throw new Exception("Empty block");
            var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);
            var statements = new List<MpStatement>();
            ParseStatements(enumerator, statements);
            return new SmaugMobProgBlock { Header = header, Statements = statements };
        }

        private static void ParseStatements(IEnumerator<string> lines, List<MpStatement> output)
        {
            while (lines.MoveNext())
            {
                var line = lines.Current.Trim();
                if (line == "~") return;
                if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                {
                    string condition = line.Substring(3).Trim();
                    var thenBlock = new List<MpStatement>();
                    var elseIfBlocks = new List<(string, List<MpStatement>)>();
                    var elseBlock = new List<MpStatement>();
                    ParseStatements(lines, thenBlock);

                    bool done = false;
                    while (!done && lines.Current != null)
                    {
                        var l = lines.Current.Trim();
                        if (l.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
                        {
                            string elseifCond = l.Substring(7).Trim();
                            var elseifBlock = new List<MpStatement>();
                            ParseStatements(lines, elseifBlock);
                            elseIfBlocks.Add((elseifCond, elseifBlock));
                        }
                        else if (l.Equals("else", StringComparison.OrdinalIgnoreCase))
                        {
                            ParseStatements(lines, elseBlock);
                        }
                        else if (l.Equals("endif", StringComparison.OrdinalIgnoreCase))
                        {
                            done = true;
                            break;
                        }
                        else break;
                    }
                    output.Add(new MpIf(condition, thenBlock, elseIfBlocks, elseBlock));
                }
                else
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (parts.Count > 0) output.Add(new MpCommand(parts[0], parts.Skip(1).ToList()));
                }
            }
        }
    }
    #endregion

    #region Variables
    public static class VariableSubstitutor
    {
        public static string Resolve(string text, MobExecutionContext ctx)
        {
            return text.Replace("$n", ctx.Target?.Name ?? "")
                       .Replace("$r", ctx.Victim?.Name ?? "")
                       .Replace("$i", ctx.Self?.Name ?? "")
                       .Replace("$t", ctx.Secondary?.Name ?? "")
                       .Replace("$I", ctx.Self?.Name.ToUpper() ?? "")
                       .Replace("$N", ctx.Target?.Name.ToUpper() ?? "")
                       .Replace("$R", ctx.Victim?.Name.ToUpper() ?? "")
                       .Replace("$T", ctx.Secondary?.Name.ToUpper() ?? "");
        }
    }
#endregion

#region Expression Engine with Dictionary
public class ExpressionEngine
{
    private readonly Dictionary<string, Func<MobExecutionContext, string[], bool>> _functions;
    private readonly Random _rand = new Random();

    public ExpressionEngine()
    {
        _functions = new Dictionary<string, Func<MobExecutionContext, string[], bool>>(StringComparer.OrdinalIgnoreCase)
        {
            ["rand"] = (ctx, args) => args.Length > 0 && int.TryParse(args[0], out int p) && _rand.Next(100) < p,
            ["ispc"] = (ctx, args) => ctx.Target != null && !ctx.Target.IsNpc,
            ["isnpc"] = (ctx, args) => ctx.Target != null && ctx.Target.IsNpc,
            ["mobinroom"] = (ctx, args) =>
            {
                if (args.Length < 2) return false;
                string targetName = args[1].Trim('\'');
                int count = ctx.Self.CurrentRoom.Mobs.Count(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase));
                return int.TryParse(args[0], out int expected) && count == expected;
            }
        };
    }

    public bool Evaluate(MobExecutionContext ctx, string expr)
    {
        expr = expr.Trim();

        // Boolean NOT
        if (expr.StartsWith("!"))
        {
            return !Evaluate(ctx, expr.Substring(1));
        }

        // Boolean AND/OR
        int andIndex = expr.IndexOf("&&", StringComparison.Ordinal);
        if (andIndex >= 0)
        {
            var left = expr.Substring(0, andIndex);
            var right = expr.Substring(andIndex + 2);
            return Evaluate(ctx, left) && Evaluate(ctx, right);
        }
        int orIndex = expr.IndexOf("||", StringComparison.Ordinal);
        if (orIndex >= 0)
        {
            var left = expr.Substring(0, orIndex);
            var right = expr.Substring(orIndex + 2);
            return Evaluate(ctx, left) || Evaluate(ctx, right);
        }

        // Numeric comparisons
        string[] numericOps = new[] { ">=", "<=", "==", "!=", "<", ">" };
        foreach (var op in numericOps)
        {
            int idx = expr.IndexOf(op, StringComparison.Ordinal);
            if (idx >= 0)
            {
                string left = expr.Substring(0, idx).Trim();
                string right = expr.Substring(idx + op.Length).Trim();
                if (double.TryParse(ResolveValue(left, ctx), out double lVal) && double.TryParse(ResolveValue(right, ctx), out double rVal))
                {
                    return op switch
                    {
                        ">" => lVal > rVal,
                        "<" => lVal < rVal,
                        ">=" => lVal >= rVal,
                        "<=" => lVal <= rVal,
                        "==" => lVal == rVal,
                        "!=" => lVal != rVal,
                        _ => false
                    };
                }
                // fallback to string equality if not numeric
                return ResolveValue(left, ctx).Equals(ResolveValue(right, ctx), StringComparison.OrdinalIgnoreCase);
            }
        }

        // Function call: name(args)
        var match = Regex.Match(expr, @"(\w+)\((.*)\)");
        if (match.Success)
        {
            string name = match.Groups[1].Value;
            string[] args = match.Groups[2].Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
            if (_functions.TryGetValue(name, out var func)) return func(ctx, args);
        }

        // fallback: treat as literal boolean
        return bool.TryParse(expr, out bool bVal) && bVal;
    }

    private string ResolveValue(string s, MobExecutionContext ctx)
    {
        return s.Replace("$n", ctx.Target?.Name ?? "")
                .Replace("$i", ctx.Self?.Name ?? "")
                .Replace("$r", ctx.Victim?.Name ?? "")
                .Replace("$t", ctx.Secondary?.Name ?? "");
    }

    public void RegisterFunction(string name, Func<MobExecutionContext, string[], bool> func) => _functions[name] = func;
}
#endregion

#region Commands
public interface IMobCommand { void Execute(MobExecutionContext ctx, List<string> args); }
    public class MobExecutionContext { public Mob Self { get; set; } = null!; public Mob? Target { get; set; } public Mob? Victim { get; set; } public Mob? Secondary { get; set; } }
    public class CommandRegistry
    {
        private readonly Dictionary<string, IMobCommand> _commands = new();
        public void Register(string name, IMobCommand cmd) => _commands[name.ToLower()] = cmd;
        public bool TryGet(string name, out IMobCommand cmd) => _commands.TryGetValue(name.ToLower(), out cmd);
    }

    public static class SmaugCommands
    {
        public static void RegisterAll(CommandRegistry registry)
        {
            registry.Register("say", new SayCommand());
            registry.Register("snicker", new SimpleActionCommand("snickers"));
            registry.Register("chuckle", new SimpleActionCommand("chuckles"));
            registry.Register("mpslay", new MpSlayCommand());
            registry.Register("mpdamage", new MpDamageCommand());
            registry.Register("mpe", new MpeCommand());
        }

        private class SimpleActionCommand : IMobCommand { private readonly string _action; public SimpleActionCommand(string a) => _action = a; public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Act(_action); }
        private class SayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Say(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); }
        private class MpSlayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null) return; Console.WriteLine($"{ctx.Self.Name} slays {ctx.Target.Name}!"); ctx.Target.HitPoints = 0; } }
        private class MpDamageCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null || args.Count < 1) return; if (int.TryParse(args[0], out int dmg)) ctx.Target.ReceiveDamage(dmg); } }
        private class MpeCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { ctx.Self.Emote(VariableSubstitutor.Resolve(string.Join(' ', args), ctx)); } }
    }
    #endregion

    #region Trigger Dispatcher
    public static class ActPatternMatcher
    {
        public static bool Match(string actText, SmaugTriggerHeader header, MobExecutionContext ctx)
        {
            if (header.ActPattern == null) return false;
            string pattern = Regex.Escape(header.ActPattern).Replace("\\*", ".*");
            bool match = Regex.IsMatch(actText, pattern, RegexOptions.IgnoreCase);
            if (!match) return false;
            if (header.ActFlag == 'p' && ctx.Target != null && !actText.Contains(ctx.Target.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if (header.ActFlag == 'r' && ctx.Self != null && !actText.Contains(ctx.Self.Name, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
    }

    public class MobProgExecutor
    {
        private readonly CommandRegistry _registry;
        private readonly ExpressionEngine _engine = new ExpressionEngine();
        public MobProgExecutor(CommandRegistry registry) => _registry = registry;
        public void ExecuteBlock(SmaugMobProgBlock block, MobExecutionContext ctx) => ExecuteStatements(block.Statements, ctx);
        private void ExecuteStatements(List<MpStatement> statements, MobExecutionContext ctx)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case MpCommand cmd:
                        if (_registry.TryGet(cmd.CommandName, out var command)) command.Execute(ctx, cmd.Args);
                        else Console.WriteLine($"Unknown command: {cmd.CommandName}");
                        break;
                    case MpIf ifStmt:
                        if (_engine.Evaluate(ctx, ifStmt.Condition)) ExecuteStatements(ifStmt.ThenBlock, ctx);
                        else
                        {
                            bool executed = false;
                            foreach (var (e, blk) in ifStmt.ElseIfBlocks)
                                if (_engine.Evaluate(ctx, e)) { ExecuteStatements(blk, ctx); executed = true; break; }
                            if (!executed && ifStmt.ElseBlock.Count > 0) ExecuteStatements(ifStmt.ElseBlock, ctx);
                        }
                        break;
                }
            }
        }
    }

    public class TriggerDispatcher
    {
        private readonly List<SmaugMobProgBlock> _mobProgs = new();
        private readonly MobProgExecutor _executor;
        public TriggerDispatcher(MobProgExecutor executor) => _executor = executor;
        public void Register(SmaugMobProgBlock block) => _mobProgs.Add(block);
        public void Dispatch(string triggerType, string actText, MobExecutionContext ctx)
        {
            foreach (var prog in _mobProgs)
            {
                if (prog.Header.Type.ToString().Equals(triggerType, StringComparison.OrdinalIgnoreCase))
                {
                    if (prog.Header.Type == SmaugTriggerType.Act && !ActPatternMatcher.Match(actText, prog.Header, ctx)) continue;
                    _executor.ExecuteBlock(prog, ctx);
                }
            }
        }
    }
    #endregion
*/
/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

    #region World Simulation
    public class Room
    {
        public int Vnum { get; set; }
        public List<Mob> Mobs { get; set; } = new();
        public List<Player> Players { get; set; } = new();

        public int CountMob(string name) => Mobs.Count(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public class Mob
    {
        public string Name { get; set; } = "";
        public bool IsNpc { get; set; } = true;
        public int Level { get; set; } = 1;
        public Room CurrentRoom { get; set; } = null!;
        public int HitPoints { get; set; } = 100;

        public void ReceiveDamage(int amount)
        {
            HitPoints -= amount;
            Console.WriteLine($"{Name} takes {amount} damage. HP={HitPoints}");
            if (HitPoints <= 0) Console.WriteLine($"{Name} has died!");
        }

        public void Say(string msg) => Console.WriteLine($"{Name} says: {msg}");
        public void Act(string msg) => Console.WriteLine($"{Name} acts: {msg}");
        public void Emote(string msg) => Console.WriteLine($"{Name} emotes: {msg}");
    }

    public class Player : Mob
    {
        public Player(string name)
        {
            Name = name;
            IsNpc = false;
        }
    }
    #endregion

    #region MobProg AST
    public enum SmaugTriggerType { Act, Fight, Rand, Greet, AllGreet }

    public record SmaugTriggerHeader
    {
        public SmaugTriggerType Type { get; init; }
        public char? ActFlag { get; init; }
        public string? ActPattern { get; init; }
    }

    public abstract record MpStatement;
    public record MpCommand(string CommandName, List<string> Args) : MpStatement;
    public record MpIf(string Condition, List<MpStatement> ThenBlock,
                        List<(string, List<MpStatement>)> ElseIfBlocks,
                        List<MpStatement> ElseBlock) : MpStatement;

    public record SmaugMobProgBlock
    {
        public SmaugTriggerHeader Header { get; init; }
        public List<MpStatement> Statements { get; init; } = new();
    }
    #endregion

    #region Parser
    public static class SmaugTriggerHeaderParser
    {
        public static SmaugTriggerHeader Parse(string line)
        {
            line = line.Trim('>', '~').Trim();
            var parts = line.Split(' ', 3);
            SmaugTriggerType type = parts[0].ToLower() switch
            {
                "act_prog" => SmaugTriggerType.Act,
                "fight_prog" => SmaugTriggerType.Fight,
                "rand_prog" => SmaugTriggerType.Rand,
                "greet_prog" => SmaugTriggerType.Greet,
                "all_greet_prog" => SmaugTriggerType.AllGreet,
                _ => throw new Exception($"Unknown trigger type {parts[0]}")
            };

            char? flag = null;
            string? pattern = null;
            if (parts.Length > 1)
            {
                if (parts[1].Length == 1 && type == SmaugTriggerType.Act)
                {
                    flag = parts[1][0];
                    pattern = parts.Length > 2 ? parts[2] : null;
                }
                else
                    pattern = string.Join(' ', parts.Skip(1));
            }

            return new SmaugTriggerHeader { Type = type, ActFlag = flag, ActPattern = pattern };
        }
    }

    public static class SmaugMobProgParser
    {
        public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
        {
            var enumerator = lines.GetEnumerator();
            if (!enumerator.MoveNext()) throw new Exception("Empty block");
            var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);

            var statements = new List<MpStatement>();
            ParseStatements(enumerator, statements);
            return new SmaugMobProgBlock { Header = header, Statements = statements };
        }

        private static void ParseStatements(IEnumerator<string> lines, List<MpStatement> output)
        {
            while (lines.MoveNext())
            {
                var line = lines.Current.Trim();
                if (line == "~") return;
                if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                {
                    string condition = line.Substring(3).Trim();
                    var thenBlock = new List<MpStatement>();
                    var elseIfBlocks = new List<(string, List<MpStatement>)>();
                    var elseBlock = new List<MpStatement>();
                    ParseStatements(lines, thenBlock);

                    bool done = false;
                    while (!done && lines.Current != null)
                    {
                        var l = lines.Current.Trim();
                        if (l.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
                        {
                            string elseifCond = l.Substring(7).Trim();
                            var elseifBlock = new List<MpStatement>();
                            ParseStatements(lines, elseifBlock);
                            elseIfBlocks.Add((elseifCond, elseifBlock));
                        }
                        else if (l.Equals("else", StringComparison.OrdinalIgnoreCase))
                        {
                            ParseStatements(lines, elseBlock);
                        }
                        else if (l.Equals("endif", StringComparison.OrdinalIgnoreCase))
                        {
                            done = true;
                            break;
                        }
                        else break;
                    }
                    output.Add(new MpIf(condition, thenBlock, elseIfBlocks, elseBlock));
                }
                else if (line.Equals("else", StringComparison.OrdinalIgnoreCase) ||
                         line.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
                         line.Equals("endif", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                else
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (parts.Count > 0) output.Add(new MpCommand(parts[0], parts.Skip(1).ToList()));
                }
            }
        }
    }
    #endregion

    #region Variables
    public static class VariableSubstitutor
    {
        public static string Resolve(string text, MobExecutionContext ctx)
        {
            return text.Replace("$n", ctx.Target?.Name ?? "")
                       .Replace("$r", ctx.Victim?.Name ?? "")
                       .Replace("$i", ctx.Self?.Name ?? "")
                       .Replace("$t", ctx.Secondary?.Name ?? "")
                       .Replace("$I", ctx.Self?.Name.ToUpper() ?? "")
                       .Replace("$N", ctx.Target?.Name.ToUpper() ?? "")
                       .Replace("$R", ctx.Victim?.Name.ToUpper() ?? "")
                       .Replace("$T", ctx.Secondary?.Name.ToUpper() ?? "");
        }
    }
    #endregion

    #region Expression Engine
    public class EvaluationContext
    {
        public MobExecutionContext MobCtx { get; set; } = null!;
        private static Random _rand = new Random();

        public bool EvalBool(string expr)
        {
            expr = expr.Trim();
            // Simple equality check
            if (expr.Contains("=="))
            {
                var parts = expr.Split("==", 2).Select(p => p.Trim()).ToArray();
                var left = ResolveValue(parts[0]);
                var right = ResolveValue(parts[1]);
                return left.Equals(right, StringComparison.OrdinalIgnoreCase);
            }
            // Random percent: rand(33)
            if (expr.StartsWith("rand(", StringComparison.OrdinalIgnoreCase))
            {
                var num = expr.Substring(5, expr.Length - 6);
                if (int.TryParse(num, out int p)) return _rand.Next(100) < p;
            }
            // Function calls
            if (expr.StartsWith("ispc(", StringComparison.OrdinalIgnoreCase))
            {
                var arg = expr.Substring(5, expr.Length - 6);
                return MobCtx.Target != null && !MobCtx.Target.IsNpc;
            }
            if (expr.StartsWith("isnpc(", StringComparison.OrdinalIgnoreCase))
            {
                var arg = expr.Substring(6, expr.Length - 7);
                return MobCtx.Target != null && MobCtx.Target.IsNpc;
            }
            if (expr.StartsWith("mobinroom(", StringComparison.OrdinalIgnoreCase))
            {
                var args = expr.Substring(10, expr.Length - 11).Split(',', StringSplitOptions.RemoveEmptyEntries);
                int vnum = int.Parse(args[0].Trim());
                Room room = MobCtx.Self.CurrentRoom;
                return room.CountMob(args[1].Trim().Trim('\'')) == vnum;
            }
            return false;
        }

        private string ResolveValue(string s)
        {
            return s switch
            {
                "$n" => MobCtx.Target?.Name ?? "",
                "$i" => MobCtx.Self?.Name ?? "",
                _ => s
            };
        }
    }
    #endregion

    #region Commands
    public interface IMobCommand { void Execute(MobExecutionContext ctx, List<string> args); }

    public class MobExecutionContext
    {
        public Mob Self { get; set; } = null!;
        public Mob? Target { get; set; }
        public Mob? Victim { get; set; }
        public Mob? Secondary { get; set; }
    }

    public class CommandRegistry
    {
        private readonly Dictionary<string, IMobCommand> _commands = new();
        public void Register(string name, IMobCommand cmd) => _commands[name.ToLower()] = cmd;
        public bool TryGet(string name, out IMobCommand cmd) => _commands.TryGetValue(name.ToLower(), out cmd);
    }

    public static class SmaugCommands
    {
        public static void RegisterAll(CommandRegistry registry)
        {
            registry.Register("say", new SayCommand());
            registry.Register("snicker", new SimpleActionCommand("snickers"));
            registry.Register("chuckle", new SimpleActionCommand("chuckles"));
            registry.Register("mpslay", new MpSlayCommand());
            registry.Register("mpdamage", new MpDamageCommand());
            registry.Register("mpe", new MpeCommand());
        }

        private class SimpleActionCommand : IMobCommand { private readonly string _action; public SimpleActionCommand(string a) => _action = a; public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Act(_action); }
        private class SayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Say(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); }
        private class MpSlayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null) return; Console.WriteLine($"{ctx.Self.Name} slays {ctx.Target.Name}!"); ctx.Target.HitPoints = 0; } }
        private class MpDamageCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null || args.Count < 1) return; if (int.TryParse(args[0], out var dmg)) ctx.Target.ReceiveDamage(dmg); } }
        private class MpeCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { ctx.Self.Emote(VariableSubstitutor.Resolve(string.Join(' ', args), ctx)); } }
    }
    #endregion

    #region Trigger Dispatcher
    public static class ActPatternMatcher
    {
        public static bool Match(string actText, SmaugTriggerHeader header, MobExecutionContext ctx)
        {
            if (header.ActPattern == null) return false;
            string pattern = Regex.Escape(header.ActPattern).Replace("\\*", ".*");
            bool match = Regex.IsMatch(actText, pattern, RegexOptions.IgnoreCase);
            if (!match) return false;
            if (header.ActFlag == 'p' && ctx.Target != null && !actText.Contains(ctx.Target.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if (header.ActFlag == 'r' && ctx.Self != null && !actText.Contains(ctx.Self.Name, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }
    }

    public class MobProgExecutor
    {
        private readonly CommandRegistry _registry;
        private readonly EvaluationContext _evalCtx = new EvaluationContext();
        public MobProgExecutor(CommandRegistry registry) => _registry = registry;

        public void ExecuteBlock(SmaugMobProgBlock block, MobExecutionContext ctx)
        {
            _evalCtx.MobCtx = ctx;
            ExecuteStatements(block.Statements, ctx);
        }

        private void ExecuteStatements(List<MpStatement> statements, MobExecutionContext ctx)
        {
            foreach (var stmt in statements)
            {
                switch (stmt)
                {
                    case MpCommand cmd:
                        if (_registry.TryGet(cmd.CommandName, out var command))
                            command.Execute(ctx, cmd.Args);
                        else Console.WriteLine($"Unknown command: {cmd.CommandName}");
                        break;
                    case MpIf ifStmt:
                        if (_evalCtx.EvalBool(ifStmt.Condition))
                            ExecuteStatements(ifStmt.ThenBlock, ctx);
                        else
                        {
                            bool executed = false;
                            foreach (var (cond, blk) in ifStmt.ElseIfBlocks)
                            {
                                if (_evalCtx.EvalBool(cond)) { ExecuteStatements(blk, ctx); executed = true; break; }
                            }
                            if (!executed && ifStmt.ElseBlock.Count > 0) ExecuteStatements(ifStmt.ElseBlock, ctx);
                        }
                        break;
                }
            }
        }
    }

    public class TriggerDispatcher
    {
        private readonly List<SmaugMobProgBlock> _mobProgs = new();
        private readonly MobProgExecutor _executor;
        public TriggerDispatcher(MobProgExecutor executor) => _executor = executor;

        public void Register(SmaugMobProgBlock block) => _mobProgs.Add(block);

        public void Dispatch(string triggerType, string actText, MobExecutionContext ctx)
        {
            foreach (var prog in _mobProgs)
            {
                if (prog.Header.Type.ToString().Equals(triggerType, StringComparison.OrdinalIgnoreCase))
                {
                    if (prog.Header.Type == SmaugTriggerType.Act && !ActPatternMatcher.Match(actText, prog.Header, ctx)) continue;
                    _executor.ExecuteBlock(prog, ctx);
                }
            }
        }
    }
    #endregion

    #region Example
    class Program
    {
        static void Main()
        {
            var room = new Room { Vnum = 1 };
            var mob = new Mob { Name = "Frupy", CurrentRoom = room };
            var player = new Player("Adventurer") { CurrentRoom = room };
            room.Mobs.Add(mob); room.Players.Add(player);

            var registry = new CommandRegistry();
            SmaugCommands.RegisterAll(registry);

            var executor = new MobProgExecutor(registry);
            var dispatcher = new TriggerDispatcher(executor);

            var blockLines = new List<string>
        {
            ">act_prog p bows before you.~",
            "snicker",
            "mpe $n While $n is foolishly grovelling, $I raises her sceptre...",
            "if ispc($n)",
            "say Hello $n!",
            "else",
            "say I only talk to NPCs.",
            "endif",
            "mpslay $n",
            "~"
        };

            var block = SmaugMobProgParser.ParseBlock(blockLines);
            dispatcher.Register(block);

            var ctx = new MobExecutionContext { Self = mob, Target = player };
            dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
        }
    }
    #endregion
}
*/

/*
#region World Simulation
public class Room
{
    public int Vnum { get; set; }
    public List<Mob> Mobs { get; set; } = new();
    public List<Player> Players { get; set; } = new();

    public int CountMob(string name) => Mobs.Count(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

public class Mob
{
    public string Name { get; set; } = "";
    public bool IsNpc { get; set; } = true;
    public int Level { get; set; } = 1;
    public Room CurrentRoom { get; set; } = null!;
    public int HitPoints { get; set; } = 100;

    public void ReceiveDamage(int amount)
    {
        HitPoints -= amount;
        Console.WriteLine($"{Name} takes {amount} damage. HP={HitPoints}");
        if (HitPoints <= 0) Console.WriteLine($"{Name} has died!");
    }

    public void Say(string msg) => Console.WriteLine($"{Name} says: {msg}");
    public void Act(string msg) => Console.WriteLine($"{Name} acts: {msg}");
    public void Emote(string msg) => Console.WriteLine($"{Name} emotes: {msg}");
}

public class Player : Mob
{
    public Player(string name)
    {
        Name = name;
        IsNpc = false;
    }
}
#endregion

#region AST and MobProg
public enum SmaugTriggerType { Act, Fight, Rand, Greet, AllGreet }

public record SmaugTriggerHeader
{
    public SmaugTriggerType Type { get; init; }
    public char? ActFlag { get; init; }
    public string? ActPattern { get; init; }
    public int? ProgNumber { get; init; }
}

public abstract record MpStatement;
public record MpCommand(string CommandName, List<string> Args) : MpStatement;
public record MpIf(MpExpr Condition, List<MpStatement> ThenBlock,
                    List<(MpExpr, List<MpStatement>)> ElseIfBlocks,
                    List<MpStatement> ElseBlock) : MpStatement;

public abstract record MpExpr;
public record MpRawExpr(string RawText) : MpExpr;

public record SmaugMobProgBlock
{
    public SmaugTriggerHeader Header { get; init; }
    public List<MpStatement> Statements { get; init; } = new();
}
#endregion

#region Parser
public static class SmaugTriggerHeaderParser
{
    public static SmaugTriggerHeader Parse(string line)
    {
        // Example: >act_prog p bows before you.~
        line = line.Trim('>', '~').Trim();
        var parts = line.Split(' ', 3);
        var typeStr = parts[0].ToLower();
        SmaugTriggerType type = typeStr switch
        {
            "act_prog" => SmaugTriggerType.Act,
            "fight_prog" => SmaugTriggerType.Fight,
            "rand_prog" => SmaugTriggerType.Rand,
            "greet_prog" => SmaugTriggerType.Greet,
            "all_greet_prog" => SmaugTriggerType.AllGreet,
            _ => throw new Exception($"Unknown trigger type {typeStr}")
        };

        char? flag = null;
        string? pattern = null;
        if (parts.Length > 1)
        {
            if (parts[1].Length == 1 && type == SmaugTriggerType.Act)
            {
                flag = parts[1][0];
                pattern = parts.Length > 2 ? parts[2] : null;
            }
            else
                pattern = parts[1] + (parts.Length > 2 ? " " + parts[2] : "");
        }

        return new SmaugTriggerHeader { Type = type, ActFlag = flag, ActPattern = pattern };
    }
}

public static class SmaugMobProgParser
{
    public static SmaugMobProgBlock ParseBlock(IEnumerable<string> lines)
    {
        var enumerator = lines.GetEnumerator();
        if (!enumerator.MoveNext()) throw new Exception("Empty block");
        var header = SmaugTriggerHeaderParser.Parse(enumerator.Current);

        var statements = new List<MpStatement>();
        ParseStatements(enumerator, statements);
        return new SmaugMobProgBlock { Header = header, Statements = statements };
    }

    private static void ParseStatements(IEnumerator<string> lines, List<MpStatement> output)
    {
        while (lines.MoveNext())
        {
            var line = lines.Current.Trim();
            if (line == "~") return;
            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
            {
                var condition = line.Substring(3).Trim();
                var thenBlock = new List<MpStatement>();
                var elseIfBlocks = new List<(MpExpr, List<MpStatement>)>();
                var elseBlock = new List<MpStatement>();

                ParseStatements(lines, thenBlock);

                bool done = false;
                while (!done && lines.Current != null)
                {
                    var l = lines.Current.Trim();
                    if (l.StartsWith("elseif ", StringComparison.OrdinalIgnoreCase))
                    {
                        var condText = l.Substring(7).Trim();
                        var elseifBlock = new List<MpStatement>();
                        ParseStatements(lines, elseifBlock);
                        elseIfBlocks.Add((new MpRawExpr(condText), elseifBlock));
                    }
                    else if (l.Equals("else", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseStatements(lines, elseBlock);
                    }
                    else if (l.Equals("endif", StringComparison.OrdinalIgnoreCase))
                    {
                        done = true;
                        break;
                    }
                    else break;
                }

                output.Add(new MpIf(new MpRawExpr(condition), thenBlock, elseIfBlocks, elseBlock));
            }
            else if (line.Equals("else", StringComparison.OrdinalIgnoreCase) ||
                     line.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
                     line.Equals("endif", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            else
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (parts.Count > 0) output.Add(new MpCommand(parts[0], parts.Skip(1).ToList()));
            }
        }
    }
}
#endregion

#region Variable Substitution
public static class VariableSubstitutor
{
    public static string Resolve(string text, MobExecutionContext ctx)
    {
        return text.Replace("$n", ctx.Target?.Name ?? "")
                   .Replace("$r", ctx.Victim?.Name ?? "")
                   .Replace("$i", ctx.Self?.Name ?? "")
                   .Replace("$t", ctx.Secondary?.Name ?? "")
                   .Replace("$I", ctx.Self?.Name.ToUpper() ?? "")
                   .Replace("$N", ctx.Target?.Name.ToUpper() ?? "")
                   .Replace("$R", ctx.Victim?.Name.ToUpper() ?? "")
                   .Replace("$T", ctx.Secondary?.Name.ToUpper() ?? "");
    }
}
#endregion

#region Command System
public interface IMobCommand { void Execute(MobExecutionContext ctx, List<string> args); }

public class MobExecutionContext
{
    public Mob Self { get; set; }
    public Mob? Target { get; set; }
    public Mob? Victim { get; set; }
    public Mob? Secondary { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
    public void Say(string message) => Self.Say(message);
}

public class CommandRegistry
{
    private readonly Dictionary<string, IMobCommand> _commands = new();
    public void Register(string name, IMobCommand cmd) => _commands[name.ToLower()] = cmd;
    public bool TryGet(string name, out IMobCommand cmd) => _commands.TryGetValue(name.ToLower(), out cmd);
}

public static class SmaugCommands
{
    public static void RegisterAll(CommandRegistry registry)
    {
        registry.Register("say", new SayCommand());
        registry.Register("snicker", new SimpleActionCommand("snickers"));
        registry.Register("chuckle", new SimpleActionCommand("chuckles"));
        registry.Register("mpslay", new MpSlayCommand());
        registry.Register("mpdamage", new MpDamageCommand());
        registry.Register("mpe", new MpeCommand());
        registry.Register("mpforce", new MpForceCommand());
        registry.Register("mprestore", new MpRestoreCommand());
        registry.Register("c", new MpCastCommand());
    }

    private class SimpleActionCommand : IMobCommand { private readonly string _action; public SimpleActionCommand(string action) => _action = action; public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Act(_action); }
    private class SayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) => ctx.Self.Say(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); }
    private class MpSlayCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null) return; Console.WriteLine($"{ctx.Self.Name} slays {ctx.Target.Name}!"); ctx.Target.HitPoints = 0; } }
    private class MpDamageCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (ctx.Target == null || args.Count < 1) return; if (int.TryParse(args[0], out var dmg)) ctx.Target.ReceiveDamage(dmg); } }
    private class MpeCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { ctx.Self.Emote(VariableSubstitutor.Resolve(string.Join(" ", args), ctx)); } }
    private class MpForceCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { if (args.Count < 2) return; string targetName = VariableSubstitutor.Resolve(args[0], ctx); string command = string.Join(' ', args.GetRange(1, args.Count - 1)); Mob? target = ctx.Self.CurrentRoom.Mobs.FirstOrDefault(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) ?? ctx.Self.CurrentRoom.Players.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)); if (target != null) Console.WriteLine($"{ctx.Self.Name} forces {target.Name} to: {command}"); } }
    private class MpRestoreCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { string targetName = args.Count > 0 ? VariableSubstitutor.Resolve(args[0], ctx) : ""; int amount = args.Count > 1 && int.TryParse(args[1], out var a) ? a : 100; Mob? target = ctx.Self.CurrentRoom.Mobs.FirstOrDefault(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) ?? ctx.Self.CurrentRoom.Players.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)); if (target != null) { target.HitPoints += amount; Console.WriteLine($"{ctx.Self.Name} restores {amount} HP to {target.Name} (HP={target.HitPoints})"); } } }
    private class MpCastCommand : IMobCommand { public void Execute(MobExecutionContext ctx, List<string> args) { string spellName = args.Count > 0 ? args[0] : "unknown spell"; string targetName = args.Count > 1 ? VariableSubstitutor.Resolve(args[1], ctx) : ""; Mob? target = ctx.Self.CurrentRoom.Mobs.FirstOrDefault(m => m.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)) ?? ctx.Self.CurrentRoom.Players.FirstOrDefault(p => p.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase)); if (target != null) Console.WriteLine($"{ctx.Self.Name} casts {spellName} on {target.Name}!"); } }
}
#endregion

#region Trigger Dispatcher
public static class ActPatternMatcher
{
    public static bool Match(string actText, SmaugTriggerHeader header, MobExecutionContext ctx)
    {
        if (header.ActPattern == null) return false;
        string pattern = Regex.Escape(header.ActPattern).Replace("\\*", ".*");
        bool match = Regex.IsMatch(actText, pattern, RegexOptions.IgnoreCase);
        if (!match) return false;
        if (header.ActFlag == 'p' && ctx.Target != null && !actText.Contains(ctx.Target.Name, StringComparison.OrdinalIgnoreCase)) return false;
        if (header.ActFlag == 'r' && ctx.Self != null && !actText.Contains(ctx.Self.Name, StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}

public class MobProgExecutor
{
    private readonly CommandRegistry _registry;
    public MobProgExecutor(CommandRegistry registry) => _registry = registry;
    public void ExecuteBlock(SmaugMobProgBlock block, MobExecutionContext ctx)
    {
        ExecuteStatements(block.Statements, ctx);
    }
    private void ExecuteStatements(List<MpStatement> statements, MobExecutionContext ctx)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case MpCommand cmd:
                    if (_registry.TryGet(cmd.CommandName, out var command))
                        command.Execute(ctx, cmd.Args);
                    else Console.WriteLine($"Unknown command: {cmd.CommandName}");
                    break;
                case MpIf ifStmt:
                    bool cond = true; // TODO: integrate real expression evaluation
                    if (cond) ExecuteStatements(ifStmt.ThenBlock, ctx);
                    else
                    {
                        bool executed = false;
                        foreach (var (e, b) in ifStmt.ElseIfBlocks)
                        {
                            if (cond) { ExecuteStatements(b, ctx); executed = true; break; }
                        }
                        if (!executed && ifStmt.ElseBlock.Count > 0) ExecuteStatements(ifStmt.ElseBlock, ctx);
                    }
                    break;
            }
        }
    }
}

public class TriggerDispatcher
{
    private readonly List<SmaugMobProgBlock> _mobProgs = new();
    private readonly MobProgExecutor _executor;
    public TriggerDispatcher(MobProgExecutor executor) => _executor = executor;
    public void Register(SmaugMobProgBlock block) => _mobProgs.Add(block);
    public void Dispatch(string triggerType, string actText, MobExecutionContext ctx)
    {
        foreach (var prog in _mobProgs)
        {
            if (prog.Header.Type.ToString().Equals(triggerType, StringComparison.OrdinalIgnoreCase))
            {
                if (prog.Header.Type == SmaugTriggerType.Act && !ActPatternMatcher.Match(actText, prog.Header, ctx))
                    continue;
                _executor.ExecuteBlock(prog, ctx);
            }
        }
    }
}
#endregion
*/
