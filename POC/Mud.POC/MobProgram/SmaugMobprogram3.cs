using System;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.MobProgram.SmaugMobprogram3;

using System;
using System.Collections.Generic;
using System.Linq;

    #region WORLD MODEL

    public abstract class Character
    {
        public string Name { get; set; }
        public bool IsNpc { get; protected set; }
        public Room Room { get; set; }
    }

public class Player : Character
    {
        public Player(string name)
        {
            Name = name;
            IsNpc = false;
        }
    }

public class Mob : Character
    {
        public Mob(string name)
        {
            Name = name;
            IsNpc = true;
        }
    }

public class Room
    {
        public List<Character> Characters { get; } = new();
        public void Enter(Character ch)
        {
            Characters.Add(ch);
            ch.Room = this;
        }
    }

#endregion

#region LEXER

public enum TokenType { Identifier, Number, LParen, RParen, DollarVar, NewLine, Tilde, GreaterThan, EOF }

public record Token(TokenType Type, string Value);

public class Lexer
    {
        private readonly string _input;
        private int _pos;

        public Lexer(string input) => _input = input.Replace("\r", "");

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _input.Length)
            {
                char c = _input[_pos];

                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n')
                        tokens.Add(new Token(TokenType.NewLine, "\n"));
                    _pos++;
                    continue;
                }

                if (char.IsLetter(c))
                {
                    int start = _pos;
                    while (_pos < _input.Length &&
                           (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
                        _pos++;

                    tokens.Add(new Token(TokenType.Identifier, _input[start.._pos]));
                    continue;
                }

                if (char.IsDigit(c))
                {
                    int start = _pos;
                    while (_pos < _input.Length && char.IsDigit(_input[_pos]))
                        _pos++;

                    tokens.Add(new Token(TokenType.Number, _input[start.._pos]));
                    continue;
                }

                if (c == '$')
                {
                    int start = _pos++;
                    while (_pos < _input.Length && char.IsLetter(_input[_pos]))
                        _pos++;

                    tokens.Add(new Token(TokenType.DollarVar, _input[start.._pos]));
                    continue;
                }

                switch (c)
                {
                    case '(':
                        tokens.Add(new Token(TokenType.LParen, "("));
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RParen, ")"));
                        break;
                    case '~':
                        tokens.Add(new Token(TokenType.Tilde, "~"));
                        break;
                    case '>':
                        tokens.Add(new Token(TokenType.GreaterThan, ">"));
                        break;
                }

                _pos++;
            }

            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }
    }

#endregion

#region AST

public abstract record Node;

public record MobProgram(string Trigger, string Arg, List<Node> Statements) : Node;

public record CommandNode(string Command, List<string> Args) : Node;

public record IfBranch(Condition Condition, List<Node> Statements);

public record IfNode(List<IfBranch> Branches, List<Node> ElseBranch) : Node;

public abstract record Condition;

public record IsPcCondition(string Target) : Condition;
public record IsNpcCondition(string Target) : Condition;
public record RandCondition(int Percent) : Condition;

#endregion

#region PARSER

public class Parser
    {
        private List<Token> _tokens;
        private int _pos;

        public List<MobProgram> Parse(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;

            var programs = new List<MobProgram>();

            while (!Match(TokenType.EOF))
            {
                if (Match(TokenType.GreaterThan))
                {
                    Advance();
                    programs.Add(ParseProgram());
                }
                else
                    Advance();
            }

            return programs;
        }

        private MobProgram ParseProgram()
        {
            string trigger = Consume(TokenType.Identifier).Value;
            string arg = Consume(TokenType.Number).Value;

            var statements = new List<Node>();

            while (!Match(TokenType.Tilde))
            {
                if (Match(TokenType.Identifier, "if"))
                    statements.Add(ParseIf());
                else if (Match(TokenType.NewLine))
                    Advance();
                else
                    statements.Add(ParseCommand());
            }

            Consume(TokenType.Tilde);
            return new MobProgram(trigger, arg, statements);
        }

        private IfNode ParseIf()
        {
            Consume(TokenType.Identifier); // if
            var branches = new List<IfBranch>();

            var cond = ParseCondition();
            var stmts = ParseBlock();
            branches.Add(new IfBranch(cond, stmts));

            while (Match(TokenType.Identifier, "elseif"))
            {
                Advance();
                var elseifCond = ParseCondition();
                var elseifStmts = ParseBlock();
                branches.Add(new IfBranch(elseifCond, elseifStmts));
            }

            List<Node> elseBranch = new();

            if (Match(TokenType.Identifier, "else"))
            {
                Advance();
                elseBranch = ParseBlock();
            }

            Consume(TokenType.Identifier); // endif

            return new IfNode(branches, elseBranch);
        }

        private List<Node> ParseBlock()
        {
            var list = new List<Node>();

            while (!Match(TokenType.Identifier, "elseif") &&
                   !Match(TokenType.Identifier, "else") &&
                   !Match(TokenType.Identifier, "endif"))
            {
                if (Match(TokenType.Identifier, "if"))
                    list.Add(ParseIf());
                else if (Match(TokenType.NewLine))
                    Advance();
                else
                    list.Add(ParseCommand());
            }

            return list;
        }

        private Condition ParseCondition()
        {
            if (Match(TokenType.Identifier, "ispc"))
            {
                Advance();
                Consume(TokenType.LParen);
                string target = Consume(TokenType.DollarVar).Value;
                Consume(TokenType.RParen);
                return new IsPcCondition(target);
            }

            if (Match(TokenType.Identifier, "isnpc"))
            {
                Advance();
                Consume(TokenType.LParen);
                string target = Consume(TokenType.DollarVar).Value;
                Consume(TokenType.RParen);
                return new IsNpcCondition(target);
            }

            if (Match(TokenType.Identifier, "rand"))
            {
                Advance();
                Consume(TokenType.LParen);
                int percent = int.Parse(Consume(TokenType.Number).Value);
                Consume(TokenType.RParen);
                return new RandCondition(percent);
            }

            throw new Exception("Unknown condition");
        }

        private CommandNode ParseCommand()
        {
            string cmd = Consume(TokenType.Identifier).Value;
            var args = new List<string>();

            while (!Match(TokenType.NewLine) &&
                   !Match(TokenType.Identifier, "endif") &&
                   !Match(TokenType.Identifier, "else") &&
                   !Match(TokenType.Identifier, "elseif") &&
                   !Match(TokenType.Tilde))
            {
                args.Add(Advance().Value);
            }

            return new CommandNode(cmd, args);
        }

        private bool Match(TokenType type, string value = null)
        {
            if (_pos >= _tokens.Count) return false;
            if (_tokens[_pos].Type != type) return false;
            if (value != null && _tokens[_pos].Value != value) return false;
            return true;
        }

        private Token Consume(TokenType type)
        {
            if (!Match(type)) throw new Exception("Unexpected token");
            return Advance();
        }

        private Token Advance() => _tokens[_pos++];
    }

#endregion

#region EVALUATOR

public class ExecutionContext
    {
        public Random Random = new();
        public Mob Self;
        public Character Actor;
        public Character Victim;
        public Character Target;
    }

public class Evaluator
    {
        public void Execute(MobProgram prog, ExecutionContext ctx)
        {
            ExecuteStatements(prog.Statements, ctx);
        }

        private void ExecuteStatements(List<Node> nodes, ExecutionContext ctx)
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case CommandNode cmd:
                        ExecuteCommand(cmd, ctx);
                        break;

                    case IfNode ifNode:
                        ExecuteIf(ifNode, ctx);
                        break;
                }
            }
        }

        private void ExecuteIf(IfNode ifNode, ExecutionContext ctx)
        {
            foreach (var branch in ifNode.Branches)
            {
                if (Evaluate(branch.Condition, ctx))
                {
                    ExecuteStatements(branch.Statements, ctx);
                    return;
                }
            }

            ExecuteStatements(ifNode.ElseBranch, ctx);
        }

        private bool Evaluate(Condition cond, ExecutionContext ctx)
        {
            return cond switch
            {
                IsPcCondition pc => Resolve(pc.Target, ctx)?.IsNpc == false,
                IsNpcCondition npc => Resolve(npc.Target, ctx)?.IsNpc == true,
                RandCondition r => ctx.Random.Next(100) < r.Percent,
                _ => false
            };
        }

        private Character Resolve(string token, ExecutionContext ctx)
        {
            return token switch
            {
                "$n" => ctx.Actor,
                "$i" => ctx.Self,
                "$r" => ctx.Victim,
                "$t" => ctx.Target,
                _ => null
            };
        }

        private string Expand(string text, ExecutionContext ctx)
        {
            return text
                .Replace("$n", ctx.Actor?.Name)
                .Replace("$i", ctx.Self?.Name)
                .Replace("$I", ctx.Self?.Name?.ToUpper());
        }

        private void ExecuteCommand(CommandNode cmd, ExecutionContext ctx)
        {
            var args = cmd.Args.Select(a => Expand(a, ctx));
            if (cmd.Command == "say")
                Console.WriteLine($"{ctx.Self.Name} says: {string.Join(" ", args)}");
        }
    }

    #endregion