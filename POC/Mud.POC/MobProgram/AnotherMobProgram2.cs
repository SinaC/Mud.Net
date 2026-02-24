using Mud.POC.MobProgram.AnotherMobProgram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Mud.POC.MobProgram.AnotherMobProgram2;

public class Mob
{
    public string Name { get; set; }
    public bool IsPc { get; set; }
    public int Level { get; set; }
    public int Alignment { get; set; }
    public bool IsAffected(string affect)
    {
        return false;
    }
}

public class MobContext
{
    public Mob Self { get; set; }
    public Mob Triggerer { get; set; }
    public Mob Secondary { get; set; }
    public Mob Target { get; set; }
    public Dictionary<string, object> Variables { get; set; } = new();
}

// =========================
// AST NODES
// =========================

public abstract class Node { }

public class CommandNode : Node
{
    public string Command { get; set; }
}

public class IfNode : Node
{
    public BoolExpr Condition { get; set; }
    public List<Node> TrueBranch { get; set; } = new();
    public List<Node> FalseBranch { get; set; } = new();
}

public class BreakNode : Node { }

// =========================
// BOOLEAN EXPRESSION TREE
// =========================

public abstract class BoolExpr
{
    public abstract bool Evaluate(MobProgramEvaluator evaluator);
}

public class AndExpr : BoolExpr
{
    public BoolExpr Left { get; }
    public BoolExpr Right { get; }

    public AndExpr(BoolExpr left, BoolExpr right)
    {
        Left = left;
        Right = right;
    }

    public override bool Evaluate(MobProgramEvaluator evaluator)
    {
        if (!Left.Evaluate(evaluator))
            return false;

        return Right.Evaluate(evaluator);
    }
}

public class OrExpr : BoolExpr
{
    public BoolExpr Left { get; }
    public BoolExpr Right { get; }

    public OrExpr(BoolExpr left, BoolExpr right)
    {
        Left = left;
        Right = right;
    }

    public override bool Evaluate(MobProgramEvaluator evaluator)
    {
        if (Left.Evaluate(evaluator))
            return true;

        return Right.Evaluate(evaluator);
    }
}

public class ConditionExpr : BoolExpr
{
    public string Raw { get; }

    public ConditionExpr(string raw)
    {
        Raw = raw;
    }

    public override bool Evaluate(MobProgramEvaluator evaluator)
    {
        return evaluator.EvaluateSimpleCondition(Raw);
    }
}

// =========================
// PARSER
// =========================

public class MobProgramParser
{
    private readonly string[] _lines;
    private int _index;

    public MobProgramParser(string program)
    {
        _lines = program.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
    }

    public List<Node> Parse()
    {
        var nodes = new List<Node>();

        while (_index < _lines.Length)
        {
            var line = _lines[_index++].Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*"))
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

    private IfNode ParseIf(string firstConditionLine)
    {
        var conditionLines = new List<string> { firstConditionLine };

        while (_index < _lines.Length)
        {
            var peek = _lines[_index].Trim();
            if (!peek.StartsWith("or ") && !peek.StartsWith("and "))
                break;

            conditionLines.Add(peek);
            _index++;
        }

        string fullCondition = string.Join(" ", conditionLines);

        var ifNode = new IfNode
        {
            Condition = ParseConditionExpression(fullCondition)
        };

        var currentBranch = ifNode.TrueBranch;

        while (_index < _lines.Length)
        {
            var line = _lines[_index++].Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*"))
                continue;

            if (line.Equals("endif", StringComparison.OrdinalIgnoreCase))
                break;

            if (line.Equals("else", StringComparison.OrdinalIgnoreCase))
            {
                currentBranch = ifNode.FalseBranch;
                continue;
            }

            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
            {
                currentBranch.Add(ParseIf(line.Substring(3).Trim()));
                continue;
            }

            if (line.Equals("break", StringComparison.OrdinalIgnoreCase))
            {
                currentBranch.Add(new BreakNode());
                continue;
            }

            currentBranch.Add(new CommandNode { Command = line });
        }

        return ifNode;
    }

    // ===== Boolean Expression Parsing =====

    private BoolExpr ParseConditionExpression(string condition)
    {
        var tokens = condition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int index = 0;
        return ParseOr(tokens, ref index);
    }

    private BoolExpr ParseOr(string[] tokens, ref int index)
    {
        var left = ParseAnd(tokens, ref index);

        while (index < tokens.Length &&
               tokens[index].Equals("or", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            var right = ParseAnd(tokens, ref index);
            left = new OrExpr(left, right);
        }

        return left;
    }

    private BoolExpr ParseAnd(string[] tokens, ref int index)
    {
        var left = ParsePrimary(tokens, ref index);

        while (index < tokens.Length &&
               tokens[index].Equals("and", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            var right = ParsePrimary(tokens, ref index);
            left = new AndExpr(left, right);
        }

        return left;
    }

    private BoolExpr ParsePrimary(string[] tokens, ref int index)
    {
        var parts = new List<string>();

        while (index < tokens.Length &&
               !tokens[index].Equals("and", StringComparison.OrdinalIgnoreCase) &&
               !tokens[index].Equals("or", StringComparison.OrdinalIgnoreCase))
        {
            parts.Add(tokens[index]);
            index++;
        }

        return new ConditionExpr(string.Join(" ", parts));
    }
}

// =========================
// EVALUATOR
// =========================

public class MobProgramEvaluator
{
    private readonly MobContext _context;
    private readonly Dictionary<string, int> _variables;
    private bool _shouldBreak = false;
    private readonly System.Random _rand = new();

    public MobProgramEvaluator(MobContext context, Dictionary<string, int> variables = null)
    {
        _context = context;
        _variables = variables ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        RegisterConditions();
        RegisterFunctions();
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
                    if (ifNode.Condition.Evaluate(this))
                        Evaluate(ifNode.TrueBranch);
                    else
                        Evaluate(ifNode.FalseBranch);
                    break;

                case BreakNode:
                    Console.WriteLine("BREAK");
                    _shouldBreak = true;
                    return;
            }
        }
    }

    // =========================
    // SIMPLE CONDITION ENGINE
    // =========================

    private bool IsInRange(int? value,  int min, int max)
        => value > min && value < max;

    private Dictionary<string, Func<MobContext, string, bool>> _singleArgumentConditions = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Func<MobContext, string, string, bool>> _twoArgumentsConditions = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Func<MobContext, int>> _parameterLessIntegerFunctions = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Func<MobContext, string, int>> _oneParameterIntegerFunctions = new(StringComparer.OrdinalIgnoreCase);

    private void RegisterConditions()
    {
        // single argument
        _singleArgumentConditions["rand"] = (ctx, param) => _rand.Next(100) < ResolveIntegerValue(param);
        _singleArgumentConditions["mobhere"] = (ctx, param) =>
        {
            // TODO
            if (int.TryParse(param, out int vnum))
                Console.WriteLine($"mobhere vnum {vnum}");
            else
                Console.WriteLine($"mobhere name {param}");
            return false;
        };
        _singleArgumentConditions["objhere"] = (ctx, param) =>
        {
            // TODO
            if (int.TryParse(param, out int vnum))
                Console.WriteLine($"objhere vnum {vnum}");
            else
                Console.WriteLine($"objhere name {param}");
            return true;
        };
        _singleArgumentConditions["mobexists"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["isnpc"] = (ctx, param) => ResolveMobVariable(param)?.IsPc == false;
        _singleArgumentConditions["ispc"] = (ctx, param) => ResolveMobVariable(param)?.IsPc == true;
        _singleArgumentConditions["isgood"] = (ctx, param) => ResolveMobVariable(param)?.Alignment >= 350;
        _singleArgumentConditions["isneutral"] = (ctx, param) => IsInRange(ResolveMobVariable(param)?.Alignment, -350, 350);
        _singleArgumentConditions["isevil"] = (ctx, param) => ResolveMobVariable(param)?.Alignment <= -350;
        _singleArgumentConditions["isimmort"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["ischarm"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["isfollow"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["isactive"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["isdelay"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["isvisible"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["hastarget"] = (ctx, param) => false; // TODO
        _singleArgumentConditions["istarget"] = (ctx, param) => false; // TODO

        // two arguments
        // character
        _twoArgumentsConditions["affected"] = (ctx, ch, affect) => ResolveMobVariable(ch)?.IsAffected(affect) == true;
        _twoArgumentsConditions["act"] = (ctx, ch, act) => false; // TODO
        _twoArgumentsConditions["off"] = (ctx, ch, off) => false; // TODO
        _twoArgumentsConditions["imm"] = (ctx, ch, imm) => false; // TODO
        _twoArgumentsConditions["carries"] = (ctx, ch, itemName) => false; // TODO
        _twoArgumentsConditions["wears"] = (ctx, ch, itemName) => false; // TODO
        _twoArgumentsConditions["has"] = (ctx, ch, itemType) => true; // TODO
        _twoArgumentsConditions["uses"] = (ctx, ch, itemType) => false; // TODO
        _twoArgumentsConditions["name"] = (ctx, ch, name) => ResolveMobVariable(ch)?.Name == name;
        _twoArgumentsConditions["pos"] = (ctx, ch, position) => false; // TODO
        _twoArgumentsConditions["clan"] = (ctx, ch, clanName) => false; // TODO
        _twoArgumentsConditions["race"] = (ctx, ch, raceName) => false; // TODO
        _twoArgumentsConditions["class"] = (ctx, ch, className) => false; // TODO
        // item
        _twoArgumentsConditions["objtype"] = (ctx, item, itemType) => false; // TODO
    }

    private void RegisterFunctions()
    {
        _parameterLessIntegerFunctions["rand"] = (ctx) => _rand.Next(100);
        _parameterLessIntegerFunctions["people"] = (ctx) => 2; // TODO
        _parameterLessIntegerFunctions["players"] = (ctx) => 1; // TODO
        _parameterLessIntegerFunctions["mobs"] = (ctx) => 2; // TODO
        _parameterLessIntegerFunctions["clones"] = (ctx) => 0; // TODO
        _parameterLessIntegerFunctions["order"] = (ctx) => 0; // TODO
        _parameterLessIntegerFunctions["hour"] = (ctx) => DateTime.Now.Hour; // TODO

        // character
        _oneParameterIntegerFunctions["vnum"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["hpcnt"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["room"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["sex"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["level"] = (ctx, ch) => ResolveMobVariable(ch)?.Level ?? 0;
        _oneParameterIntegerFunctions["align"] = (ctx, ch) => ResolveMobVariable(ch)?.Alignment ?? 0;
        _oneParameterIntegerFunctions["money"] = (ctx, ch) => 0; // TODO
        // item
        _oneParameterIntegerFunctions["objval0"] = (ctx, ch) => 0; // TODO: not available for the moment in ItemBlueprintBase
        _oneParameterIntegerFunctions["objval1"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["objval2"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["objval3"] = (ctx, ch) => 0; // TODO
        _oneParameterIntegerFunctions["objval4"] = (ctx, ch) => 0; // TODO
    }

    public bool EvaluateSimpleCondition(string cond)
    {
        var tokens = cond.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
            return false;

        Console.WriteLine($"EVALUATE SIMPLE CONDITION: {string.Join('|', tokens)}");

        if (tokens.Length == 2)
        {
            if (_singleArgumentConditions.TryGetValue(tokens[0], out var evaluateFunc))
                return evaluateFunc(_context, tokens[1]);
        }
        else if (tokens.Length == 3)
        {
            // try to evaluate as integer expression
            if (IsComparisonOperator(tokens[1]))
            {
                var leftExpr = tokens[0];
                var op = tokens[1];
                var rightExpr = tokens[2];

                return EvaluateIntegerComparison(leftExpr, op, rightExpr);
            }

            // try to evaluate as mob expression
            if (_twoArgumentsConditions.TryGetValue(tokens[0], out var evaluateFunc))
                return evaluateFunc(_context, tokens[1], tokens[2]);
        }
        else if (tokens.Length == 4)
        {
            if (IsComparisonOperator(tokens[2]))
            {
                var leftExpr = $"{tokens[0]} {tokens[1]}";
                var op = tokens[2];
                var rightExpr = tokens[3];

                return EvaluateIntegerComparison(leftExpr, op, rightExpr);
            }
        }

        Console.WriteLine($"UNKNOWN SIMPLE CONDITION: {string.Join('|', tokens)}");

        return false;
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

        // direct number
        if (int.TryParse(expr, out int literal))
            return literal;

        var tokens = expr.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // parameter-less function
        if (tokens.Length == 1)
        { 
            if (_parameterLessIntegerFunctions.TryGetValue(expr, out var function))
                return function(_context);
        }

        // 1-parameter function
        if (tokens.Length == 2)
        {
            if (_oneParameterIntegerFunctions.TryGetValue(tokens[0], out var function))
                return function(_context, tokens[1]);
        }

        Console.WriteLine($"UNKNOWN INTEGER EXPRESSION: {expr}");
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
        Console.WriteLine($"EXECUTE: {command}");
    }

    // =========================
    // VARIABLE RESOLUTION
    // =========================

    private Mob ResolveMobVariable(string name)
    {
        return name switch
        {
            "$i" => _context.Self,
            "$n" => _context.Triggerer,
            "$t" => _context.Secondary,
            "$q" => _context.Target,
            _ => null
        };
    }

    private string ReplaceVariables(string command)
    {
        if (_context.Self != null)
            command = command.Replace("$i", _context.Self.Name);
        if (_context.Triggerer != null)
            command = command.Replace("$n", _context.Triggerer.Name);
        if (_context.Secondary != null)
            command = command.Replace("$t", _context.Secondary.Name);

        return command;
    }
}