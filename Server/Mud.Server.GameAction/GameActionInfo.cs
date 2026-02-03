using Mud.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class GameActionInfo : IGameActionInfo
{
    public static readonly SyntaxAttribute DefaultSyntaxCommandAttribute = new("[cmd]");

    public string Name { get; }
    public int Priority { get; }
    public bool Hidden { get; }
    public bool NoShortcut { get; }
    public bool AddCommandInParameters { get; }
    public string[] Categories { get; }

    public string[] Syntax { get; }

    public string[] Aliases { get; }

    public string? Help { get; }

    public Type CommandExecutionType { get; }

    public IEnumerable<string> Names => Name.Yield().Concat(Aliases ?? Enumerable.Empty<string>());


    public GameActionInfo(Type commandExecutionType, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
    {
        CommandExecutionType = commandExecutionType;

        Name = commandAttribute.Name;
        Priority = commandAttribute.Priority;
        Hidden = commandAttribute.Hidden;
        NoShortcut = commandAttribute.NoShortcut;
        AddCommandInParameters = commandAttribute.AddCommandInParameters;
        Categories = commandAttribute.Categories;

        Syntax = syntaxAttribute.Syntax;

        Aliases = aliasAttributes?.Select(x => x.Alias).ToArray() ?? [];

        Help = helpAttribute?.Help;
    }
}
