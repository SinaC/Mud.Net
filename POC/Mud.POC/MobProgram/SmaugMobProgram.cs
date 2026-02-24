using Mud.POC.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mud.POC.MobProgram.SmaugMobProgram;

using System;
using System.Collections.Generic;

public abstract class Node { }

public class ProgramNode : Node
{
    public string TriggerType { get; set; }
    public string TriggerArgument { get; set; }
    public List<Node> Body { get; set; } = new();
}

public class CommandNode : Node
{
    public string Command { get; set; }
    public CommandNode(string cmd) => Command = cmd;
}

public class IfNode : Node
{
    public string Condition { get; set; }
    public List<Node> TrueBranch { get; set; } = new();
    public List<(string condition, List<Node> body)> ElseIfBranches { get; set; } = new();
    public List<Node> FalseBranch { get; set; } = new();
}

public class MobProgParser
{
    private readonly List<string> _lines;
    private int _pos = 0;

    public MobProgParser(string script)
    {
        _lines = script
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();
    }

    private string Current => _pos < _lines.Count ? _lines[_pos] : null;
    private void Advance() => _pos++;

    public ProgramNode Parse()
    {
        if (!Current.StartsWith(">"))
            throw new Exception("Script must start with trigger");

        var header = Current.Substring(1).Split(' ', 2);
        var program = new ProgramNode
        {
            TriggerType = header[0],
            TriggerArgument = header.Length > 1 ? header[1] : ""
        };

        Advance();
        program.Body = ParseBlock();

        return program;
    }

    private List<Node> ParseBlock()
    {
        var nodes = new List<Node>();

        while (Current != null && Current != "~")
        {
            if (Current.StartsWith("if "))
                nodes.Add(ParseIf());
            else
            {
                nodes.Add(new CommandNode(Current));
                Advance();
            }
        }

        return nodes;
    }

    private IfNode ParseIf()
    {
        var ifNode = new IfNode();
        ifNode.Condition = Current.Substring(3).Trim();
        Advance();

        while (Current != null)
        {
            if (Current.StartsWith("elseif "))
            {
                Advance();
                break;
            }
            if (Current.StartsWith("else"))
            {
                Advance();
                break;
            }
            if (Current.StartsWith("endif"))
            {
                Advance();
                return ifNode;
            }

            ifNode.TrueBranch.Add(Current.StartsWith("if ")
                ? ParseIf()
                : new CommandNode(Current));

            if (!Current.StartsWith("if "))
                Advance();
        }

        while (Current != null && Current.StartsWith("elseif "))
        {
            var condition = Current.Substring(7).Trim();
            Advance();

            var branch = new List<Node>();

            while (Current != null &&
                   !Current.StartsWith("elseif ") &&
                   !Current.StartsWith("else") &&
                   !Current.StartsWith("endif"))
            {
                branch.Add(Current.StartsWith("if ")
                    ? ParseIf()
                    : new CommandNode(Current));

                if (!Current.StartsWith("if "))
                    Advance();
            }

            ifNode.ElseIfBranches.Add((condition, branch));
        }

        if (Current != null && Current.StartsWith("else"))
        {
            Advance();

            while (Current != null && !Current.StartsWith("endif"))
            {
                ifNode.FalseBranch.Add(Current.StartsWith("if ")
                    ? ParseIf()
                    : new CommandNode(Current));

                if (!Current.StartsWith("if "))
                    Advance();
            }
        }

        if (Current != null && Current.StartsWith("endif"))
            Advance();

        return ifNode;
    }
}

public class MobContext
{
    public string MobName { get; set; }
    public string PlayerName { get; set; }

    public string Expand(string text)
    {
        if (PlayerName != null)
            text = text.Replace("$n", PlayerName);

        text = text.Replace("$i", MobName);

        return text;
    }
}


public class MobProgInterpreter
{
    private readonly ProgramNode _program;

    public MobProgInterpreter(ProgramNode program)
    {
        _program = program;
    }

    public void Execute(MobContext context)
    {
        ExecuteBlock(_program.Body, context);
    }

    private void ExecuteBlock(List<Node> nodes, MobContext ctx)
    {
        foreach (var node in nodes)
            ExecuteNode(node, ctx);
    }

    private void ExecuteNode(Node node, MobContext ctx)
    {
        switch (node)
        {
            case CommandNode cmd:
                ExecuteCommand(cmd.Command, ctx);
                break;

            case IfNode ifNode:
                ExecuteIf(ifNode, ctx);
                break;
        }
    }

    private void ExecuteIf(IfNode node, MobContext ctx)
    {
        if (EvaluateCondition(node.Condition, ctx))
        {
            ExecuteBlock(node.TrueBranch, ctx);
            return;
        }

        foreach (var (condition, body) in node.ElseIfBranches)
        {
            if (EvaluateCondition(condition, ctx))
            {
                ExecuteBlock(body, ctx);
                return;
            }
        }

        ExecuteBlock(node.FalseBranch, ctx);
    }

    private bool EvaluateCondition(string condition, MobContext ctx)
    {
        condition = condition.Trim();

        if (condition.StartsWith("ispc("))
            return ctx.PlayerName != null;

        if (condition.StartsWith("isnpc("))
            return ctx.PlayerName == null;

        return false;
    }

    private void ExecuteCommand(string command, MobContext ctx)
    {
        var expanded = ctx.Expand(command);

        if (expanded.StartsWith("say "))
        {
            Console.WriteLine($"{ctx.MobName} says: {expanded.Substring(4)}");
        }
        else
        {
            Console.WriteLine($"{ctx.MobName} executes: {expanded}");
        }
    }
}