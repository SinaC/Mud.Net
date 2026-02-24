//using Mud.Domain;
//using Mud.POC.Entities;
//using Mud.Server.Commands.Character.Item;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;

//namespace Mud.POC.MobProgram.BasedMudMobProgram;

//public abstract class MobProgNode { }

//public class MobProgBlock : MobProgNode
//{
//    public List<MobProgNode> Statements { get; } = new();
//}

//public class IfNode : MobProgNode
//{
//    public ConditionNode Condition { get; set; }
//    public MobProgBlock TrueBlock { get; set; } = new();
//    public MobProgBlock FalseBlock { get; set; } = new();
//}

//public class CommandNode : MobProgNode
//{
//    public string Command { get; set; }
//    public List<string> Arguments { get; set; } = new();
//}

//public class BreakNode : MobProgNode { }

//#region Condition Tree

//public abstract class ConditionNode { }

//public class BinaryCondition : ConditionNode
//{
//    public ConditionNode Left { get; set; }
//    public string Operator { get; set; } // and / or
//    public ConditionNode Right { get; set; }
//}

//public class ComparisonCondition : ConditionNode
//{
//    public string Left { get; set; }
//    public string Operator { get; set; } // >= <= > < ==
//    public string Right { get; set; }
//}

//public class FunctionCondition : ConditionNode
//{
//    public string Name { get; set; }
//    public List<string> Arguments { get; set; } = new();
//}

//public class MobProgParser
//{
//    private readonly string[] _lines;
//    private int _index = 0;

//    public MobProgParser(string script)
//    {
//        _lines = script
//            .Split('\n')
//            .Select(l => l.Trim())
//            .Where(l => l.Length > 0)
//            .ToArray();
//    }

//    private IfNode ParseIf()
//    {
//        var ifNode = new IfNode();

//        var conditionLines = new List<string>();

//        // First IF line
//        conditionLines.Add(_lines[_index].Substring(3).Trim());
//        _index++;

//        // Collect AND/OR continuation lines
//        while (_index < _lines.Length &&
//               (_lines[_index].StartsWith("and ") ||
//                _lines[_index].StartsWith("or ")))
//        {
//            conditionLines.Add(_lines[_index]);
//            _index++;
//        }

//        ifNode.Condition = ParseConditionChain(conditionLines);

//        ifNode.TrueBlock = ParseBlock();

//        if (_index < _lines.Length && _lines[_index] == "else")
//        {
//            _index++;
//            ifNode.FalseBlock = ParseBlock();
//        }

//        if (_index < _lines.Length && _lines[_index] == "endif")
//        {
//            _index++;
//        }
//        else
//        {
//            throw new Exception("Missing endif.");
//        }

//        return ifNode;
//    }

//    private ConditionNode ParseConditionChain(List<string> lines)
//    {
//        ConditionNode current = ParseSingleCondition(lines[0]);

//        for (int i = 1; i < lines.Count; i++)
//        {
//            var line = lines[i];
//            var op = line.StartsWith("and ") ? "and" : "or";
//            var conditionText = line.Substring(op.Length).Trim();

//            var right = ParseSingleCondition(conditionText);

//            current = new BinaryCondition
//            {
//                Left = current,
//                Operator = op,
//                Right = right
//            };
//        }

//        return current;
//    }

//    private ConditionNode ParseSingleCondition(string text)
//    {
//        var tokens = Tokenize(text);

//        // Comparison operator case
//        if (tokens.Count == 3 &&
//            (tokens[1] == ">" ||
//             tokens[1] == "<" ||
//             tokens[1] == ">=" ||
//             tokens[1] == "<=" ||
//             tokens[1] == "=="))
//        {
//            return new ComparisonCondition
//            {
//                Left = tokens[0],
//                Operator = tokens[1],
//                Right = tokens[2]
//            };
//        }

//        // Otherwise treat as function condition
//        return new FunctionCondition
//        {
//            Name = tokens[0],
//            Arguments = tokens.Skip(1).ToList()
//        };
//    }

//    ////////////////////////////
//    public class MobProgExecutionContext
//    {
//        public Character Actor { get; set; }      // $n
//        public Character Target { get; set; }     // $q
//        public Character Random { get; set; }     // $r
//        public Character Mob { get; set; }        // $i
//        public Room Room { get; set; }
//        public GameWorld World { get; set; }

//        public bool BreakRequested { get; set; }
//    }

//    ///////////////////////////
//    private string ExpandVariables(string input, MobProgExecutionContext ctx)
//    {
//        return input
//            .Replace("$i", ctx.Mob?.Name ?? "")
//            .Replace("$n", ctx.Actor?.Name ?? "")
//            .Replace("$q", ctx.Target?.Name ?? "")
//            .Replace("$r", ctx.Random?.Name ?? "")
//            .Replace("$l", ctx.Room?.Name ?? "")
//            .Replace("$I", ctx.Mob?.ShortDescription ?? "");
//    }

//    //////////////////////////
//    public bool EvaluateCondition(ConditionNode node, MobProgExecutionContext ctx)
//    {
//        switch (node)
//        {
//            case BinaryCondition b:
//                if (b.Operator == "and")
//                    return EvaluateCondition(b.Left, ctx) &&
//                           EvaluateCondition(b.Right, ctx);

//                if (b.Operator == "or")
//                    return EvaluateCondition(b.Left, ctx) ||
//                           EvaluateCondition(b.Right, ctx);

//                break;

//            case ComparisonCondition c:
//                return EvaluateComparison(c, ctx);

//            case FunctionCondition f:
//                return EvaluateFunction(f, ctx);
//        }

//        return false;
//    }

//    //////////////////////////////
//    private bool EvaluateComparison(ComparisonCondition c, MobProgExecutionContext ctx)
//    {
//        int left = ResolveNumericValue(c.Left, ctx);
//        int right = ResolveNumericValue(c.Right, ctx);

//        return c.Operator switch
//        {
//            ">" => left > right,
//            "<" => left < right,
//            ">=" => left >= right,
//            "<=" => left <= right,
//            "==" => left == right,
//            _ => false
//        };
//    }

//    //    ResolveNumericValue() must understand:
//    //hour
//    //mobs
//    //constants
//    ///////////////////////////

//    private bool EvaluateFunction(FunctionCondition f, MobProgExecutionContext ctx)
//    {
//        switch (f.Name)
//        {
//            case "ispc":
//                return ResolveCharacter(f.Arguments[0], ctx)?.IsPlayer == true;

//            case "has":
//                return HasItem(ctx.Mob, f.Arguments[1]);

//            case "carries":
//                var ch = ResolveCharacter(f.Arguments[0], ctx);
//                return HasItem(ch, f.Arguments[1]);

//            case "objhere":
//                return ctx.Room.HasObject(int.Parse(f.Arguments[0]));

//            case "mobs":
//                return ctx.Room.MobCount;

//            default:
//                return false;
//        }
//    }
//    //To emulate SMAUG exactly, you’ll need to port all mobprog condition types from mud_prog.c

//    /////////////////////////////
//    public void ExecuteBlock(MobProgBlock block, MobProgExecutionContext ctx)
//    {
//        foreach (var stmt in block.Statements)
//        {
//            if (ctx.BreakRequested)
//                return;

//            switch (stmt)
//            {
//                case IfNode ifNode:
//                    if (EvaluateCondition(ifNode.Condition, ctx))
//                        ExecuteBlock(ifNode.TrueBlock, ctx);
//                    else
//                        ExecuteBlock(ifNode.FalseBlock, ctx);
//                    break;

//                case CommandNode cmd:
//                    ExecuteCommand(cmd, ctx);
//                    break;

//                case BreakNode:
//                    ctx.BreakRequested = true;
//                    return;
//            }
//        }
//    }
//    ///////////////////////////////////
//    private void ExecuteCommand(CommandNode cmd, MobProgExecutionContext ctx)
//    {
//        var args = cmd.Arguments
//                      .Select(a => ExpandVariables(a, ctx))
//                      .ToList();

//        switch (cmd.Command)
//        {
//            case "say":
//                ctx.Mob.Say(string.Join(" ", args));
//                break;

//            case "mob echo":
//                ctx.Room.Broadcast(string.Join(" ", args));
//                break;

//            case "mob goto":
//            case "mob go":
//                ctx.Mob.MoveTo(int.Parse(args[0]));
//                break;

//            case "mob oload":
//                ctx.Room.LoadObject(int.Parse(args[0]));
//                break;

//            case "mob remember":
//                ctx.Mob.Remember(ctx.Target);
//                break;

//            case "mob forget":
//                ctx.Mob.Forget();
//                break;

//            case "give":
//                GiveItem(args, ctx);
//                break;

//            case "room echo":
//                ctx.Room.Broadcast(string.Join(" ", args));
//                break;

//            default:
//                Console.WriteLine($"Unknown command: {cmd.Command}");
//                break;
//        }
//    }
//}

//using static System.Runtime.InteropServices.JavaScript.JSType;

//To be 100 % accurate, you’ll need to mirror SMAUG’s mpcmd.c implementation.
/////////////////////////////////////////////

//public class MobProgVM
//{
//    public void Execute(MobProgram program, MobProgExecutionContext ctx)
//    {
//        ctx.BreakRequested = false;
//        ExecuteBlock(program.Script, ctx);
//    }
//}


///////////////////////////////////////////
//Instead of:

//ctx.Mob.MoveTo(roomId);

//Use an adapter:

//public interface ISmaugCompatibilityAdapter
//{
//    void MobGoto(Character mob, int roomVnum);
//    void MobEcho(Room room, string text);
//    void MobRemember(Character mob, Character target);
//    void MobForget(Character mob);
//    bool IsPc(Character ch);
//    int GetHour();
//}

//Your VM calls the adapter.

//Your modern engine implements the adapter.

//This isolates legacy behavior completely.
//////////////////////////////////
//Break Behavior

//break stops the current program immediately.

//Not just the block.

//Your VM must stop execution entirely:

//case BreakNode:
//    ctx.BreakRequested = true;
//    return;

//    And the top - level loop must respect it.

//Condition Resolution Rules

//SMAUG resolves identifiers like:

//    hour

//    mobs

//ispc

//isnpc

//carries

//has

//objhere

//You must match the exact truth behavior, including edge cases.

//For example:

//if (IS_NPC(victim))

//        means null targets evaluate false, not crash.

//So your evaluator must always null - check.

////////
//Variable Expansion Must Match SMAUG

//SMAUG supports:

//    Variable Meaning
//$i mob
//$n actor
//$q target
//$r random character
//$l room name
//$I mob short desc
//$N actor short desc
//$Q target short desc

//And they are expanded before command execution.

//Do NOT expand at parse time.
////////////
//SMAUG processes mobprogs in this order:

//    Check trigger type

//    Check trigger argument match

//    Execute first matching program

//    Stop unless flagged for multiple

//    So your trigger system must:

//foreach (var prog in mob.Programs)
//        {
//            if (TriggerMatches(prog, eventContext))
//            {
//                vm.Execute(prog, ctx);
//                if (!prog.AllowMultiple)
//                    break;
//            }
//        }

//    Order matters.

////////////////////
//Object and Character Resolution Rules

//SMAUG resolves targets loosely.

//For example:

//give NASCAR $q

//Search order:

//    Mob inventory

//Equipment

//Room contents(sometimes)

//Case - insensitive matching

//Your compatibility layer must replicate this fuzzy matching logic.

//Otherwise old area files break.

//////////////
//Numeric Evaluation Rules

//When resolving numeric values:

//    Unknown identifier = 0

//Missing target = 0

//Null comparisons = false

//Never throw exceptions.

//SMAUG silently fails.

////////////////////
//Do NOT:

//Make it strongly typed

//Make it strict

//Throw errors

//“Improve” logic

//Old mobprogs rely on sloppy behavior.

//You must emulate the sloppiness.

/////////////////////////