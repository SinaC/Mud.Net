using Mud.Common.Attributes;

namespace Mud.Server.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class CommandAttribute(string name, params string[] categories) : ExportAttribute // every command will be exported without ContractType
{
    public const int DefaultPriority = 500;
    public const string DefaultCategory = "";

    public string Name { get; } = name.ToLowerInvariant();
    public string[] Categories { get; set; } = categories == null || categories.Length == 0
            ? [DefaultCategory]
            : categories;
    public int Priority { get; set; } = DefaultPriority;
    public bool Hidden { get; set; } = false;
    public bool NoShortcut { get; set; } = false;
    public bool AddCommandInParameters { get; set; } = false;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DynamicCommandAttribute : ExportAttribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class ItemCommandAttribute(string name, params string[] categories) : ActorCommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class RoomCommandAttribute(string name, params string[] categories) : ActorCommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class CharacterCommandAttribute(string name, params string[] categories) : ActorCommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class NonPlayableCharacterCommandAttribute(string name, params string[] categories) : CharacterCommandAttribute(name, categories) // Cannot have master/leader
{
}

[AttributeUsage(AttributeTargets.Class)]
public class PlayableCharacterCommandAttribute(string name, params string[] categories) : CharacterCommandAttribute(name, categories) // Must be impersonated
{
}

[AttributeUsage(AttributeTargets.Class)]
public class PlayerCommandAttribute(string name, params string[] categories) : ActorCommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class AdminCommandAttribute(string name, params string[] categories) : PlayerCommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class ActorCommandAttribute(string name, params string[] categories) : CommandAttribute(name, categories)
{
}

[AttributeUsage(AttributeTargets.Class)]
public class SyntaxAttribute(params string[] syntax) : Attribute
{
    public string[] Syntax { get; } = syntax;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AliasAttribute(string alias) : Attribute
{
    public string Alias { get; } = alias;
}
