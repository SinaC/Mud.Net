using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Mud.POC.MobProgram.EnhancedSmaugMobProgram;

#region Context Models

public class Character
{
    public string Name { get; set; } = "Bob";
    public int Level { get; set; } = 10;
    public int Gold { get; set; } = 100;
    public int HP { get; set; } = 50;
    public int MaxHP { get; set; } = 100;
    public string Alignment { get; set; } = "neutral";
    public string Race { get; set; } = "human";
    public string Class { get; set; } = "warrior";
    public string Clan { get; set; } = "none";
    public bool IsNpc { get; set; } = false;
}

public class MobContext
{
    public Character Self { get; set; } = new Character { Name = "Mob", IsNpc = true };
    public Character Target { get; set; } = new Character();
    public int RoomId { get; set; } = 1000;
    public int MobCount { get; set; } = 2;
    public int PlayerCount { get; set; } = 1;
    public bool IsFighting { get; set; } = false;
}

#endregion

#region Lexer

public enum TokenType
{
    Identifier, Number, String, Operator,
    And, Or,
    If, Else, EndIf, Break,
    LeftParen, RightParen
}

public record Token(TokenType Type, string Value);

public class Lexer
{
    public static List<List<Token>> Tokenize(string script)
    {
        var result = new List<List<Token>>();
        var lines = script.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("*")) continue;

            result.Add(TokenizeLine(line));
        }

        return result;
    }

    private static List<Token> TokenizeLine(string line)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < line.Length)
        {
            if (char.IsWhiteSpace(line[i])) { i++; continue; }

            if (char.IsDigit(line[i]))
            {
                int start = i;
                while (i < line.Length && char.IsDigit(line[i])) i++;
                tokens.Add(new Token(TokenType.Number, line[start..i]));
                continue;
            }

            if (line[i] == '\'')
            {
                i++;
                int start = i;
                while (i < line.Length && line[i] != '\'') i++;
                tokens.Add(new Token(TokenType.String, line[start..i]));
                i++;
                continue;
            }

            if (line[i] == '(') { tokens.Add(new Token(TokenType.LeftParen, "(")); i++; continue; }
            if (line[i] == ')') { tokens.Add(new Token(TokenType.RightParen, ")")); i++; continue; }

            if ("<>=!".Contains(line[i]))
            {
                int start = i++;
                if (i < line.Length && line[i] == '=') i++;
                tokens.Add(new Token(TokenType.Operator, line[start..i]));
                continue;
            }

            int idStart = i;
            while (i < line.Length && !char.IsWhiteSpace(line[i]))
                i++;

            string word = line[idStart..i].ToLower();

            tokens.Add(word switch
            {
                "if" => new Token(TokenType.If, word),
                "else" => new Token(TokenType.Else, word),
                "endif" => new Token(TokenType.EndIf, word),
                "and" => new Token(TokenType.And, word),
                "or" => new Token(TokenType.Or, word),
                "break" => new Token(TokenType.Break, word),
                _ => new Token(TokenType.Identifier, word)
            });
        }

        return tokens;
    }
}

#endregion

#region AST

public abstract class Expr { }
public class LiteralExpr : Expr { public object Value; }
public class IdentifierExpr : Expr { public string Name; }
public class BinaryExpr : Expr { public Expr Left; public string Op; public Expr Right; }

public abstract class Statement { }
public class CommandStatement : Statement { public string Command; }
public class BreakStatement : Statement { }
public class IfStatement : Statement
{
    public Expr Condition;
    public List<Statement> TrueBranch = new();
    public List<Statement> FalseBranch = new();
}

#endregion

#region Parser

public class Parser
{
    private List<List<Token>> _lines;
    private int _index;

    public List<Statement> Parse(string script)
    {
        _lines = Lexer.Tokenize(script);
        _index = 0;
        return ParseBlock();
    }

    private List<Statement> ParseBlock()
    {
        var statements = new List<Statement>();

        while (_index < _lines.Count)
        {
            var tokens = _lines[_index];

            if (tokens[0].Type == TokenType.EndIf)
            {
                _index++;
                break;
            }

            if (tokens[0].Type == TokenType.Else)
            {
                break;
            }

            if (tokens[0].Type == TokenType.If)
            {
                _index++;
                var expr = new ExpressionParser().Parse(tokens.Skip(1).ToList());
                var ifStmt = new IfStatement { Condition = expr };

                ifStmt.TrueBranch = ParseBlock();

                if (_index < _lines.Count &&
                    _lines[_index][0].Type == TokenType.Else)
                {
                    _index++;
                    ifStmt.FalseBranch = ParseBlock();
                }

                statements.Add(ifStmt);
                continue;
            }

            if (tokens[0].Type == TokenType.Break)
            {
                statements.Add(new BreakStatement());
                _index++;
                continue;
            }

            statements.Add(new CommandStatement
            {
                Command = string.Join(" ", tokens.Select(t => t.Value))
            });

            _index++;
        }

        return statements;
    }
}

#endregion

#region Expression Parser (Precedence)

public class ExpressionParser
{
    private List<Token> _tokens;
    private int _pos;

    public Expr Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _pos = 0;
        return ParseOr();
    }

    private Expr ParseOr()
    {
        var expr = ParseAnd();
        while (Match(TokenType.Or))
            expr = new BinaryExpr { Left = expr, Op = "or", Right = ParseAnd() };
        return expr;
    }

    private Expr ParseAnd()
    {
        var expr = ParseComparison();
        while (Match(TokenType.And))
            expr = new BinaryExpr { Left = expr, Op = "and", Right = ParseComparison() };
        return expr;
    }

    private Expr ParseComparison()
    {
        var left = ParsePrimary();
        if (Match(TokenType.Operator))
            return new BinaryExpr
            {
                Left = left,
                Op = Previous().Value,
                Right = ParsePrimary()
            };
        return left;
    }

    private Expr ParsePrimary()
    {
        if (Match(TokenType.Number))
            return new LiteralExpr { Value = int.Parse(Previous().Value) };

        if (Match(TokenType.String))
            return new LiteralExpr { Value = Previous().Value };

        if (Match(TokenType.Identifier))
            return new IdentifierExpr { Name = Previous().Value };

        if (Match(TokenType.LeftParen))
        {
            var expr = ParseOr();
            Consume(TokenType.RightParen);
            return expr;
        }

        throw new Exception("Invalid expression");
    }

    private bool Match(TokenType type)
    {
        if (_pos < _tokens.Count && _tokens[_pos].Type == type)
        {
            _pos++;
            return true;
        }
        return false;
    }

    private Token Previous() => _tokens[_pos - 1];
    private void Consume(TokenType type)
    {
        if (!Match(type))
            throw new Exception($"Expected {type}");
    }
}

#endregion

#region Interpreter

public class Interpreter
{
    private readonly ConcurrentDictionary<string, Func<MobContext, object>> _conditions
        = new();

    private readonly ConcurrentDictionary<string, Action<MobContext, string>> _commands
        = new();

    private static readonly ThreadLocal<System.Random> _rng =
        new(() => new System.Random());

    private const int MaxInstructions = 10000;
    private int _instructionCount;

    public Interpreter()
    {
        RegisterConditions();
        RegisterCommands();
    }

    public void Execute(List<Statement> statements, MobContext ctx)
    {
        _instructionCount = 0;
        ExecuteBlock(statements, ctx);
    }

    private bool ExecuteBlock(List<Statement> statements, MobContext ctx)
    {
        foreach (var stmt in statements)
        {
            if (_instructionCount++ > MaxInstructions)
                throw new Exception("Execution limit exceeded");

            switch (stmt)
            {
                case CommandStatement cmd:
                    ExecuteCommand(ctx, cmd.Command);
                    break;

                case BreakStatement:
                    return true;

                case IfStatement ifs:
                    bool result = Evaluate(ifs.Condition, ctx);
                    var branch = result ? ifs.TrueBranch : ifs.FalseBranch;
                    if (ExecuteBlock(branch, ctx))
                        return true;
                    break;
            }
        }
        return false;
    }

    private bool Evaluate(Expr expr, MobContext ctx)
    {
        switch (expr)
        {
            case LiteralExpr lit:
                return Convert.ToBoolean(lit.Value);

            case IdentifierExpr id:
                return Convert.ToBoolean(GetValue(id.Name, ctx));

            case BinaryExpr bin:
                if (bin.Op == "and")
                    return Evaluate(bin.Left, ctx) && Evaluate(bin.Right, ctx);
                if (bin.Op == "or")
                    return Evaluate(bin.Left, ctx) || Evaluate(bin.Right, ctx);

                var left = Convert.ToInt32(GetExprValue(bin.Left, ctx));
                var right = Convert.ToInt32(GetExprValue(bin.Right, ctx));

                return bin.Op switch
                {
                    "==" => left == right,
                    "!=" => left != right,
                    ">" => left > right,
                    ">=" => left >= right,
                    "<" => left < right,
                    "<=" => left <= right,
                    _ => false
                };
        }
        return false;
    }

    private object GetExprValue(Expr expr, MobContext ctx)
    {
        return expr switch
        {
            LiteralExpr lit => lit.Value,
            IdentifierExpr id => GetValue(id.Name, ctx),
            _ => Evaluate(expr, ctx)
        };
    }

    private object GetValue(string name, MobContext ctx)
    {
        if (name == "rand") return _rng.Value.Next(100);
        if (_conditions.TryGetValue(name, out var func))
            return func(ctx);
        return 0;
    }

    private void ExecuteCommand(MobContext ctx, string cmd)
    {
        var parts = cmd.Split(' ');
        if (_commands.TryGetValue(parts[0], out var action))
            action(ctx, cmd);
        else
            Console.WriteLine("EXEC: " + cmd);
    }

    private void RegisterCommands()
    {
        _commands["mob"] = (ctx, cmd) => Console.WriteLine("MOB CMD: " + cmd);
        _commands["echo"] = (ctx, cmd) => Console.WriteLine("ECHO: " + cmd[5..]);
    }

    private void RegisterConditions()
    {
        _conditions["level"] = ctx => ctx.Target.Level;
        _conditions["gold"] = ctx => ctx.Target.Gold;
        _conditions["hp"] = ctx => ctx.Target.HP;
        _conditions["maxhp"] = ctx => ctx.Target.MaxHP;
        _conditions["hour"] = ctx => DateTime.Now.Hour;
        _conditions["mobs"] = ctx => ctx.MobCount;
        _conditions["players"] = ctx => ctx.PlayerCount;
        _conditions["room"] = ctx => ctx.RoomId;
        _conditions["inroom"] = ctx => ctx.RoomId;
        _conditions["hitprcnt"] = ctx => ctx.Target.HP * 100 / ctx.Target.MaxHP;
        _conditions["alignmentvalue"] = ctx => ctx.Target.Alignment == "good" ? 1000 :
                                               ctx.Target.Alignment == "evil" ? -1000 : 0;
        _conditions["ispc"] = ctx => !ctx.Target.IsNpc;
        _conditions["isnpc"] = ctx => ctx.Target.IsNpc;
        _conditions["isgood"] = ctx => ctx.Target.Alignment == "good";
        _conditions["isevil"] = ctx => ctx.Target.Alignment == "evil";
        _conditions["isneutral"] = ctx => ctx.Target.Alignment == "neutral";
        _conditions["fighting"] = ctx => ctx.IsFighting;
        _conditions["charhere"] = ctx => true;
        _conditions["objhere"] = ctx => true;
        _conditions["objexists"] = ctx => true;
        _conditions["affected"] = ctx => false;
        _conditions["act"] = ctx => false;
        _conditions["name"] = ctx => ctx.Target.Name;
        _conditions["race"] = ctx => ctx.Target.Race;
        _conditions["class"] = ctx => ctx.Target.Class;
        _conditions["clan"] = ctx => ctx.Target.Clan;
        _conditions["clanlevel"] = ctx => 1;
        _conditions["position"] = ctx => 0;
    }
}

#endregion