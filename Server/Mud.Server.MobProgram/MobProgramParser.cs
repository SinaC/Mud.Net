using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.MobProgram.Interfaces;

namespace Mud.Server.MobProgram;

[Export(typeof(IMobProgramParser)), Shared]
public class MobProgramParser : IMobProgramParser
{
    private class ParsingContext
    {
        public required string[] Lines { get; set; }
        public int Index { get; set; }
    }

    public IEnumerable<INode> Parse(string program)
    {
        var nodes = new List<INode>();

        var ctx = new ParsingContext
        {
            Lines = program.Split(["\r\n", "\n"], StringSplitOptions.None),
            Index = 0
        };
        while (ctx.Index < ctx.Lines.Length)
        {
            var line = ctx.Lines[ctx.Index++].Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('*'))
                continue;

            if (line.StartsWith("if ", StringComparison.OrdinalIgnoreCase))
                nodes.Add(ParseIf(line[3..].Trim(), ctx));
            else if (line.Equals("break", StringComparison.OrdinalIgnoreCase))
                nodes.Add(new BreakNode());
            else
                nodes.Add(new CommandNode { Command = line });
        }

        return nodes;
    }

    private IfNode ParseIf(string firstConditionLine, ParsingContext ctx)
    {
        var conditionLines = new List<string> { firstConditionLine };

        while (ctx.Index < ctx.Lines.Length)
        {
            var peek = ctx.Lines[ctx.Index].Trim();
            if (!peek.StartsWith("or ") && !peek.StartsWith("and "))
                break;

            conditionLines.Add(peek);
            ctx.Index++;
        }

        var fullCondition = string.Join(" ", conditionLines);

        var condition = ParseConditionExpression(fullCondition);
        var ifNode = new IfNode { Condition = condition };

        var currentBranch = ifNode.TrueBranch;

        while (ctx.Index < ctx.Lines.Length)
        {
            var line = ctx.Lines[ctx.Index++].Trim();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('*'))
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
                currentBranch.Add(ParseIf(line[3..].Trim(), ctx));
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
        var tokens = condition.Tokenize(true).ToArray();
        var index = 0;
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
            left = new OrExpr { Left = left, Right = right };
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
            left = new AndExpr { Left = left, Right = right };
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

        return new ConditionExpr { Raw = string.Join(" ", parts) };
    }
}