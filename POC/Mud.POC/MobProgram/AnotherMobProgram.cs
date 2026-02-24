using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.MobProgram.AnotherMobProgram;

// Mob and Context Classes
public class Mob
{
    public string Name { get; set; }
    public bool IsPc { get; set; }
    public int Level { get; set; }
    public int Alignment { get; set; }
}

public class MobContext
{
    public Mob Self { get; set; }      // the mob executing the program
    public Mob Target { get; set; }    // $n
    public Mob Secondary { get; set; } // $t
    public Mob Remembered { get; set; } // $q
    public Dictionary<string, object> Variables { get; set; } = new();
}

// AST Nodes
public abstract class Node { }

public class CommandNode : Node
{
    public string Command { get; set; }
}

public class IfNode : Node
{
    public string Condition { get; set; }
    public List<Node> TrueBranch { get; set; } = new();
    public List<Node> FalseBranch { get; set; } = new();
}

public class BreakNode : Node
{
}

public class MobProgramParser
{
    private readonly string[] _lines;
    private int _index;

    public MobProgramParser(string program)
    {
        _lines = program.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        _index = 0;
    }

    public List<Node> Parse()
    {
        var nodes = new List<Node>();
        while (_index < _lines.Length)
        {
            var line = _lines[_index].Trim();
            _index++;

            if (string.IsNullOrEmpty(line)) // empty line
                continue;

            if (line.StartsWith("*")) // comment
                continue;

            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                nodes.Add(ParseIf(line.Substring(3).Trim()));
            else if (line.Equals("break", StringComparison.OrdinalIgnoreCase))
                nodes.Add(new BreakNode());
            else
                nodes.Add(new CommandNode { Command = line });
        }
        return nodes;
    }

    private IfNode ParseIf(string firstLineCondition)
    {
        var conditionLines = new List<string> { firstLineCondition };
        var ifNode = new IfNode();

        // Collect multi-line conditions starting with "or" or "and"
        while (_index < _lines.Length)
        {
            var line = _lines[_index].Trim();
            if (!line.StartsWith("or ") && !line.StartsWith("and ")) break;
            conditionLines.Add(line);
            _index++;
        }

        ifNode.Condition = string.Join(" ", conditionLines);

        // Parse true/false branches
        var currentBranch = ifNode.TrueBranch;
        while (_index < _lines.Length)
        {
            var line = _lines[_index].Trim();
            _index++;

            if (string.IsNullOrWhiteSpace(line)) // empty line
                continue;

            if (line.StartsWith("*")) // comment
                continue;

            if (line.Equals("endif", StringComparison.OrdinalIgnoreCase))
                break;
            else if (line.Equals("else", StringComparison.OrdinalIgnoreCase))
                currentBranch = ifNode.FalseBranch;
            else if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                currentBranch.Add(ParseIf(line.Substring(3).Trim()));
            else if (line.Equals("break", StringComparison.OrdinalIgnoreCase))
                currentBranch.Add(new BreakNode());
            else
                currentBranch.Add(new CommandNode { Command = line });
        }

        return ifNode;
    }
}

public class MobProgramEvaluatorWithVariables
{
    private readonly MobContext _context;
    private readonly Dictionary<string, int> _variables;
    private readonly System.Random _rand = new();
    private bool _shouldBreak = false;

    public MobProgramEvaluatorWithVariables(MobContext context, Dictionary<string, int> variables = null)
    {
        _context = context;
        _variables = variables ?? new Dictionary<string, int>();
    }

    public void Evaluate(List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            if (_shouldBreak)
                return;

            switch (node)
            {
                case CommandNode cmd:
                    ExecuteCommand(cmd.Command);
                    break;

                case IfNode ifNode:
                    if (EvaluateCondition(ifNode.Condition))
                        Evaluate(ifNode.TrueBranch);
                    else
                        Evaluate(ifNode.FalseBranch);
                    break;

                case BreakNode:
                    _shouldBreak = true;
                    return;
            }
        }
    }

    // =========================
    // BOOLEAN CONDITION ENGINE
    // =========================

    //private bool EvaluateCondition(string condition)
    //{
    //    // Split by 'or' first
    //    var orParts = condition.Split(new[] { " or " }, StringSplitOptions.None);
    //    foreach (var orPart in orParts)
    //    {
    //        var andParts = orPart.Split(new[] { " and " }, StringSplitOptions.None);
    //        bool andResult = true;
    //        foreach (var andPart in andParts)
    //        {
    //            if (!EvaluateSimpleCondition(andPart.Trim()))
    //            {
    //                andResult = false;
    //                break;
    //            }
    //        }
    //        if (andResult) return true;
    //    }
    //    return false;
    //}
    private bool EvaluateCondition(string condition)
    {
        var tokens = TokenizeCondition(condition);

        bool? currentResult = null;
        string pendingOperator = null;

        foreach (var token in tokens)
        {
            if (token == "and")
            {
                if (currentResult != null && currentResult == false)
                {
                    Console.WriteLine("SHORT-CIRCUIT (AND)");
                    return false;
                }
                pendingOperator = token;
                continue;
            }
            else if (token == "or")
            {
                if (currentResult != null && currentResult == true)
                {
                    Console.WriteLine("SHORT-CIRCUIT (OR)");
                    return true;
                }
                pendingOperator = token;
                continue;
            }

            bool value = EvaluateSimpleCondition(token);

            if (currentResult == null)
            {
                currentResult = value;
                continue;
            }

            if (pendingOperator == "and")
            {
                currentResult = currentResult.Value && value;
            }
            else if (pendingOperator == "or")
            {
                currentResult = currentResult.Value || value;
            }
        }

        return currentResult ?? false;
    }

    private List<string> TokenizeCondition(string condition)
    {
        var tokens = new List<string>();
        var parts = condition.Split(' ');

        var current = new List<string>();

        foreach (var part in parts)
        {
            if (part == "and" || part == "or")
            {
                if (current.Count > 0)
                {
                    tokens.Add(string.Join(" ", current));
                    current.Clear();
                }

                tokens.Add(part);
            }
            else
            {
                current.Add(part);
            }
        }

        if (current.Count > 0)
            tokens.Add(string.Join(" ", current));

        return tokens;
    }

    // =========================
    // SIMPLE CONDITIONS
    // =========================

    private bool EvaluateSimpleCondition(string cond)
    {
        cond = cond.Trim();

        // rand <number>
        if (cond.StartsWith("rand "))
        {
            var parts = cond.Split(' ');
            if (int.TryParse(parts[1], out int chance))
                return _rand.Next(100) < chance;
        }

        // isnpc $var
        else if (cond.StartsWith("isnpc "))
        {
            var parts = cond.Split(' ');
            var mob = ResolveMobVariable(parts[1]);
            if (mob is null)
                return false;
            return !mob.IsPc;
        }
        else if (cond.StartsWith("objhere "))
        {
            Console.WriteLine($"COND objhere check: {cond}");
            return true; // stub
        }
        else if (cond.StartsWith("has "))
        {
            Console.WriteLine($"COND has check: {cond}");
            return true; // stub
        }
        else if (cond.StartsWith("carries "))
        {
            Console.WriteLine($"COND carries check: {cond}");
            return false; // stub
        }
        else if (cond.StartsWith("ispc "))
        {
            var parts = cond.Split(' ');
            var mob = ResolveMobVariable(parts[1]);
            if (mob is null)
                return false;
            return mob.IsPc;
        }
        else if (cond.StartsWith("isgood "))
        {
            string varName = cond.Substring(7).Trim();
            var mob = ResolveMobVariable(varName);

            bool result = mob != null && mob.Alignment >= 450;
            Console.WriteLine($"COND isgood {varName} => {result}");
            return result;
        }
        // TODO: generic string comparison
        // Generic integer comparison
        else
        {
            var tokens = cond.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 4)
            {
                // Example:
                // level $n <= 5
                if (IsComparisonOperator(tokens[2]))
                {
                    string leftExpr = $"{tokens[0]} {tokens[1]}";
                    string op = tokens[2];
                    string rightExpr = tokens[3];

                    return EvaluateIntegerComparison(leftExpr, op, rightExpr);
                }
            }
            else if (tokens.Length == 3)
            {
                // Example:
                // mobs > 1
                if (IsComparisonOperator(tokens[1]))
                {
                    return EvaluateIntegerComparison(tokens[0], tokens[1], tokens[2]);
                }
            }
        }

        Console.WriteLine($"Unknown condition: {cond}");

        return false; // default false
    }

    private bool EvaluateIntegerComparison(string leftExpr, string op, string rightExpr)
    {
        int left = ResolveIntegerValue(leftExpr);
        int right = ResolveIntegerValue(rightExpr);

        bool result = op switch
        {
            "<" => left < right,
            ">" => left > right,
            "<=" => left <= right,
            ">=" => left >= right,
            "==" => left == right,
            "!=" => left != right,
            _ => false
        };

        Console.WriteLine($"COND {leftExpr} {op} {rightExpr} => {result} ({left} {op} {right})");
        return result;
    }

    private int ResolveIntegerValue(string expr)
    {
        expr = expr.Trim();

        // Direct number
        if (int.TryParse(expr, out int literal))
            return literal;

        // level $n
        if (expr.StartsWith("level ", StringComparison.OrdinalIgnoreCase)) //if (expr.StartsWith("level "))
        {
            string varName = expr.Substring(6).Trim();
            var mob = ResolveMobVariable(varName);
            return mob?.Level ?? 0;
        }

        // mobs
        if (expr.Equals("mobs", StringComparison.OrdinalIgnoreCase))
        {
            return 2; // stub room mob count
        }

        // hour
        if (expr.Equals("hour", StringComparison.OrdinalIgnoreCase))
        {
            return _variables["hour"];
        }

        Console.WriteLine($"Unknown integer expression: {expr}");
        return 0;
    }

    private bool IsComparisonOperator(string op)
    {
        return op is "<" or ">" or "<=" or ">=" or "==" or "!=";
    }

    // =========================
    // COMMAND EXECUTION
    // =========================

    private void ExecuteCommand(string command)
    {
        command = ReplaceVariables(command);
        var parts = command.Split(' ', 2);
        var cmd = parts[0].ToLower();
        var arg = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "echo":
                Console.WriteLine(arg);
                break;

            case "damage":
                Console.WriteLine(arg);
                break;

            default:
                Console.WriteLine($"Unknown command: {command}");
                break;
        }
    }

    // =========================
    // VARIABLE RESOLUTION
    // =========================

    private Mob ResolveMobVariable(string name)
    {
        return name switch
        {
            "$i" => _context.Self,
            "$n" => _context.Target,
            "$t" => _context.Secondary,
            _ => null
            //_ => _mobVars.ContainsKey(name) ? _mobVars[name] as Mob : null
        };
    }

    private string ReplaceVariables(string command)
    {
        if (_context.Self != null)
            command = command.Replace("$i", _context.Self.Name);
        if (_context.Target != null)
            command = command.Replace("$n", _context.Target.Name);
        if (_context.Secondary != null)
            command = command.Replace("$t", _context.Secondary.Name);

        foreach (var kv in _context.Variables)
        {
            command = command.Replace(kv.Key, kv.Value.ToString());
        }

        //foreach (var kv in _mobVars)
        //{
        //    if (kv.Value is Mob mob)
        //        command = command.Replace(kv.Key, mob.Name);
        //}

        return command;
    }
}