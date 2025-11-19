using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.DataStructures.Trie;
using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Diagnostics;
using System.Reflection;

namespace Mud.Server.GameAction;

public class GameActionManager : IGameActionManager
{
    private ILogger<GameActionManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private ICommandParser CommandParser { get; }

    private readonly Dictionary<Type, IGameActionInfo> _gameActionInfosByExecutionType;
    private readonly Dictionary<Type, IReadOnlyTrie<IGameActionInfo>> _gameActionInfosTrieByActorType;

    public GameActionManager(ILogger<GameActionManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper, ICommandParser commandParser)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        CommandParser = commandParser;

        _gameActionInfosTrieByActorType = []; // will be filled when a call to GetGameActions will be called

        Type iGameActionType = typeof(IGameAction);

        // Gather all game action
        _gameActionInfosByExecutionType = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iGameActionType.IsAssignableFrom(t)))
            .Select(t => new { executionType = t, attributes = GetGameActionAttributes(t) })
            .ToDictionary(typeAndAttributes => typeAndAttributes.executionType, typeAndAttributes => CreateGameActionInfo(typeAndAttributes.executionType, typeAndAttributes.attributes.commandAttribute, typeAndAttributes.attributes.syntaxAttribute, typeAndAttributes.attributes.aliasAttributes, typeAndAttributes.attributes.helpAttribute));
    }

    #region IGameActionManager

    public IEnumerable<IGameActionInfo> GameActions => _gameActionInfosByExecutionType.Values;

    public string? Execute<TActor>(IGameActionInfo gameActionInfo, TActor actor, string commandLine, string command, params ICommandParameter[] parameters)
        where TActor: IActor
    {
        var gameActionInstance = ServiceProvider.GetService(gameActionInfo.CommandExecutionType);
        if (gameActionInstance == null)
        {
            Logger.LogError("GameAction {0} not found in DependencyContainer.", gameActionInfo.Name);
            return "Something goes wrong.";
        }
        if (gameActionInstance is not IGameAction gameAction)
        {
            Logger.LogError("GameAction {0} cannot be created or is not {1}.", gameActionInfo.Name, typeof(IGameAction).FullName ?? "???");
            return "Something goes wrong.";
        }
        var actionInput = new ActionInput(gameActionInfo, actor, commandLine, command, parameters);
        var guardsResult = gameAction.Guards(actionInput);
        if (guardsResult != null)
            return guardsResult;
        gameAction.Execute(actionInput);
        return null;
    }

    public string? Execute<TGameAction, TActor>(TActor actor, string? commandLine)
        where TActor : IActor
    {
        var gameActionType = typeof(TGameAction);
        if (!_gameActionInfosByExecutionType.TryGetValue(gameActionType, out var gameActionInfo))
        {
            Logger.LogError("GameAction type {0} not found in GameActionManager.", gameActionType.FullName ?? "???");
            return "Something goes wrong.";
        }
        string command = gameActionInfo.Name;
        var parameters = commandLine == null
            ? Enumerable.Empty<ICommandParameter>().ToArray()
            : CommandParser.SplitParameters(commandLine).Select(CommandParser.ParseParameter).ToArray();
        return Execute(gameActionInfo, actor, commandLine!, command, parameters);
    }

    public string? Execute<TActor>(TActor actor, string command, params ICommandParameter[] parameters)
        where TActor : IActor
    {
        var gameActionInfo = _gameActionInfosByExecutionType.Values.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, command));
        if (gameActionInfo == null)
        {
            Logger.LogError("GameAction matching name {0} not found in GameActionManager.", command);
            return "Something goes wrong.";
        }
        string commandLine = command + CommandParser.JoinParameters(parameters);
        return Execute(gameActionInfo, actor, commandLine, command, parameters);
    }

    public IReadOnlyTrie<IGameActionInfo> GetGameActions<TActor>()
        where TActor : IActor
    {
        var actorType = typeof(TActor);

        if (_gameActionInfosTrieByActorType.TryGetValue(actorType, out var readonlyTrie))
            return readonlyTrie;
        var gameActions = GetGameActionsByActorType<TActor>();
        Trie<IGameActionInfo> trie = new(gameActions);
        _gameActionInfosTrieByActorType.Add(actorType, trie);
        return trie;
    }

    #endregion

    private IEnumerable<TrieEntry<IGameActionInfo>> GetGameActionsByActorType<TActor>()
        where TActor : IActor
    {
        var iActorType = typeof(IActor);
        var actorType = typeof(TActor);
        var actorTypeSortedImplementedInterfaces = GetSortedImplementedInterfaces(actorType);

        var gameActionInfos = _gameActionInfosByExecutionType
            .Values
            .GroupBy(t => t.Name)
            .OrderBy(g => g.Key)
            .Select(g => PolymorphismSimulator(actorType, iActorType, actorTypeSortedImplementedInterfaces, g.Key, g, x => x.CommandExecutionType))
            .Where(x => x != null);
        // return one entry using CommandAttribute.Name and one entry by AliasAttribute.Alias
        return gameActionInfos.SelectMany(x => x!.Names, (gameActionInfo, name) => new TrieEntry<IGameActionInfo>(name, gameActionInfo!));
    }

    private IGameActionInfo CreateGameActionInfo(Type type, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
    {
        switch (commandAttribute)
        {
            case AdminCommandAttribute adminCommandAttribute:
                return new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            case PlayerCommandAttribute playerCommandAttribute:
                return new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                return new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            case CharacterCommandAttribute characterCommandAttribute:
                return new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            default:
                Logger.LogWarning("GameActionManager.CreateGameActionInfo: default game action info for type {0}", type.FullName ?? "???");
                return new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
        }
    }

    private static (CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute) GetGameActionAttributes(Type type)
    {
        var commandAttribute = type.GetCustomAttribute<CommandAttribute>()!;
        var syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? GameActionInfo.DefaultSyntaxCommandAttribute;
        var aliasAttributes = type.GetCustomAttributes<AliasAttribute>();
        var helpAttribute = type.GetCustomAttribute<HelpAttribute>();

        return (commandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
    }

    private static Type[] GetSortedImplementedInterfaces(Type actorType)
    {
        return actorType.GetInterfaces()
            .Select(i => new { implementedInterface = i, level = GetNestedLevel(i) })
            .OrderByDescending(x => x.level)
            .Select(x => x.implementedInterface)
            .ToArray();
    }

    // search among collection, which type has in its inheritance an interface matching actorType, if multiple type are eligible, choose the one with highest level of nesting
    private T? PolymorphismSimulator<T>(Type actorType, Type baseActorType, Type[] actorTypeSortedImplementedInterfaces, string name, IEnumerable<T> collection, Func<T,Type> selectorFunc)
        where T: class
    {
        T? best = default;
        int bestLevel = int.MaxValue;
        foreach (T t in collection) // foreach type on the same command name, search the one with the highest level of class nesting
        {
            bool doneWithThisType = false;
            var type = selectorFunc(t);
            while (type != null)
            {
                if (type.GenericTypeArguments?.Length > 0)
                {
                    var genericTypeArgumentToCheck = type.GenericTypeArguments.FirstOrDefault(baseActorType.IsAssignableFrom);
                    if (genericTypeArgumentToCheck != null)
                    {
                        int maxToTest = Math.Min(bestLevel, actorTypeSortedImplementedInterfaces.Length);
                        for (int i = 0; i < maxToTest; i++)
                            if (actorTypeSortedImplementedInterfaces[i] == genericTypeArgumentToCheck)
                            {
                                if (i < bestLevel)
                                {
                                    best = t;
                                    bestLevel = i;
                                }

                                doneWithThisType = true;
                                break;
                            }
                    }
                    else
                        Logger.LogError("Type {0} doesn't any generic type argument of type {1}.", type.FullName ?? "???", baseActorType.FullName ?? "???");
                }

                if (doneWithThisType)
                    break;
                type = type.BaseType;
            }
        }
        Debug.Print(actorType.Name + ": " + name + " => " + (best == default ? "???" : selectorFunc(best).FullName));
        return best;
    }

    private static int GetNestedLevel(Type? type)
    {
        if (type == null)
            return 0;

        int maxLevel = 1 + GetNestedLevel(type.BaseType);

        foreach (var implementedInterface in type.GetInterfaces())
        {
            int implementedInterfaceLevel = 1 + GetNestedLevel(implementedInterface);
            if (implementedInterfaceLevel > maxLevel)
                maxLevel = implementedInterfaceLevel;
        }

        return maxLevel;
    }
}
