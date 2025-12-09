using Mud.Common.Attributes;
using Mud.Domain;

namespace Mud.Server.GameAction;

[AttributeUsage(AttributeTargets.Class)]
public abstract class CommandAttribute : ExportAttribute // every command will be exported without ContractType
{
    public const int DefaultPriority = 500;
    public const string DefaultCategory = "";

    public string Name { get; }
    public int Priority { get; set; } // Lower value means higher priority
    public bool Hidden { get; set; } // Not displayed in command list
    public bool NoShortcut { get; set; } // Command must be fully typed
    public bool AddCommandInParameters { get; set; } // Command must be added in parameter list
    public string[] Categories { get; set; }

    public CommandAttribute(string name, params string[] categories)
    {
        Name = name.ToLowerInvariant();
        Priority = DefaultPriority;
        Hidden = false;
        Categories = categories == null || categories.Length == 0
            ? [DefaultCategory]
            : categories;
        NoShortcut = false;
        AddCommandInParameters = false;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ItemCommandAttribute : ActorCommandAttribute
{
    public ItemCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class RoomCommandAttribute : ActorCommandAttribute
{
    public RoomCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}


[AttributeUsage(AttributeTargets.Class)]
public class CharacterCommandAttribute : ActorCommandAttribute
{
    public Positions MinPosition { get; set; }
    public bool NotInCombat { get; set; }

    public CharacterCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
        MinPosition = Positions.Sleeping;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PlayableCharacterCommandAttribute : CharacterCommandAttribute // Must be impersonated
{
    public PlayableCharacterCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class PlayerCommandAttribute : ActorCommandAttribute
{
    public bool MustBeImpersonated { get; set; }
    public bool CannotBeImpersonated { get; set; }

    public PlayerCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class AdminCommandAttribute : PlayerCommandAttribute
{
    public AdminLevels MinLevel { get; set; }

    public AdminCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ActorCommandAttribute : CommandAttribute
{
    public ActorCommandAttribute(string name, params string[] categories)
        : base(name, categories)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class SyntaxAttribute : Attribute
{
    public string[] Syntax { get; }

    public SyntaxAttribute(params string[] syntax)
    {
        Syntax = syntax;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AliasAttribute : Attribute
{
    public string Alias { get; }

    public AliasAttribute(string alias)
    {
        Alias = alias;
    }
}
