using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Social;
using System.Reflection;

namespace Mud.Server.GameAction;

[Export(typeof(IGameActionManager)), Shared]
public class GameActionManager : IGameActionManager
{
    private ILogger<GameActionManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private ICommandParser CommandParser { get; }
    private IGuardGenerator GuardGenerator { get; }
    private IAbilityManager AbilityManager { get; }

    private List<IGameActionInfo> _dynamicGameActionInfos;
    private List<IGameActionInfo> _staticGameActionInfos;
    private readonly Dictionary<Type, IGameActionInfo> _staticGameActionInfosByExecutionType;
    private readonly Dictionary<Type, IReadOnlyTrie<IGameActionInfo>> _gameActionInfosTrieByActorType;

    public GameActionManager(ILogger<GameActionManager> logger, IServiceProvider serviceProvider, IAssemblyHelper assemblyHelper, ICommandParser commandParser, IGuardGenerator guardGenerator, IAbilityManager abilityManager, ISocialManager socialManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        CommandParser = commandParser;
        GuardGenerator = guardGenerator;
        AbilityManager = abilityManager;

        _gameActionInfosTrieByActorType = []; // will be filled when a call to GetGameActions will be called

        // static game actions
        _staticGameActionInfos = [];
        var iGameActionType = typeof(IGameAction);
        foreach (var gameActionType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(iGameActionType))))
        {
            var dynamicCommandAttribute = gameActionType.GetCustomAttribute<DynamicCommandAttribute>();
            if (dynamicCommandAttribute == null)
            {
                var (commandAttribute, syntaxAttribute, aliasAttributes, helpAttribute) = GetGameActionAttributes(gameActionType);
                var gai = CreateGameActionInfo(gameActionType, commandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
                _staticGameActionInfos.Add(gai);
            }
        }
        Logger.LogDebug("GameActionManager: {count} static commands found", _staticGameActionInfos.Count);
        _staticGameActionInfosByExecutionType = _staticGameActionInfos.ToDictionary(x => x.CommandExecutionType);
        // dynamic game actions
        _dynamicGameActionInfos = [];
        if (socialManager != null)
        {
            var socialGameActionInfos = socialManager.GetGameActions();
            _dynamicGameActionInfos.AddRange(socialGameActionInfos);
        }
        Logger.LogDebug("GameActionManager: {count} dynamic commands generated", _dynamicGameActionInfos.Count);
    }

    #region IGameActionManager

    public IEnumerable<IGameActionInfo> GameActions => _staticGameActionInfos;

    public string? Execute<TActor>(IGameActionInfo gameActionInfo, TActor actor, string commandLine, string command, params ICommandParameter[] parameters)
        where TActor: IActor
    {
        var gameActionInstance = ServiceProvider.GetRequiredService(gameActionInfo.CommandExecutionType);
        if (gameActionInstance is not IGameAction gameAction)
        {
            Logger.LogError("GameAction {name} cannot be created or is not {type}.", gameActionInfo.Name, typeof(IGameAction).FullName ?? "???");
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
        if (!_staticGameActionInfosByExecutionType.TryGetValue(gameActionType, out var gameActionInfo))
        {
            Logger.LogError("GameAction type {type} not found in GameActionManager.", gameActionType.FullName ?? "???");
            return "Something goes wrong.";
        }
        var command = gameActionInfo.Name;
        var parameters = commandLine == null
            ? Enumerable.Empty<ICommandParameter>().ToArray()
            : CommandParser.SplitParameters(commandLine).Select(CommandParser.ParseParameter).ToArray();
        return Execute(gameActionInfo, actor, commandLine!, command, parameters);
    }

    public IReadOnlyTrie<IGameActionInfo> GetGameActions<TActor>()
        where TActor : IActor
    {
        var actorType = typeof(TActor);

        if (_gameActionInfosTrieByActorType.TryGetValue(actorType, out var readonlyTrie))
            return readonlyTrie;
        var gameActions = GetGameActionsByActorType<TActor>();
        var trie = new Trie<IGameActionInfo>(gameActions);
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

        // static game action infos
        var staticGameActionInfos = _staticGameActionInfos
            .GroupBy(t => t.Name)
            .OrderBy(g => g.Key)
            .Select(g => PolymorphismSimulator(actorType, iActorType, actorTypeSortedImplementedInterfaces, g.Key, g, x => x.CommandExecutionType)).ToArray();
        // one entry using CommandAttribute.Name and one entry by AliasAttribute.Alias
        var staticEntries = staticGameActionInfos.Where(x => x != null).SelectMany(x => x!.Names, (gameActionInfo, name) => new TrieEntry<IGameActionInfo>(name.ToLowerInvariant(), gameActionInfo!));

        // dynamic game action infos
        var dynamicGameActionInfos = _dynamicGameActionInfos
            .GroupBy(t => t.Name)
            .OrderBy(g => g.Key)
            .Select(g => PolymorphismSimulator(actorType, iActorType, actorTypeSortedImplementedInterfaces, g.Key, g, x => x.CommandExecutionType)).ToArray();
        // remove dynamic game actions with same name as an existing static game action
        var dynamicEntries = new List<TrieEntry<IGameActionInfo>>();
        foreach (var dynamicGameInfoAction in dynamicGameActionInfos.Where(x => x != null))
        {
            // search if a static game action info with the same exists
            var existingStaticGameActionInfoWithSameNameOrAlias = staticGameActionInfos.FirstOrDefault(x => x is not null && StringCompareHelpers.AnyStringEquals(x.Names, dynamicGameInfoAction!.Name));
            if (existingStaticGameActionInfoWithSameNameOrAlias == null)
                dynamicEntries.Add(new TrieEntry<IGameActionInfo>(dynamicGameInfoAction!.Name.ToLowerInvariant(), dynamicGameInfoAction!));
            else
                Logger.LogError("GameActionManager: dynamic command {dynamicName} conflicts with static command {staticName} -> keeps static one", dynamicGameInfoAction!.Name, existingStaticGameActionInfoWithSameNameOrAlias.Name);
        }

        //
        return staticEntries.Concat(dynamicEntries);
    }

    private IGameActionInfo CreateGameActionInfo(Type type, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
    {
        switch (commandAttribute)
        {
            case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
            {
                var characterGuards = GuardGenerator.GenerateCharacterGuards(type);
                return new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, characterGuards);
            }
            case CharacterCommandAttribute characterCommandAttribute:
            {
                var characterGuards = GuardGenerator.GenerateCharacterGuards(type);

                var abilityDefinition = AbilityManager[type];
                if (abilityDefinition != null)
                {
                    if (abilityDefinition.Type == AbilityTypes.Skill)
                        return new SkillGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, abilityDefinition, characterGuards);
                    else
                        Logger.LogError("GameActionManager.CreateGameActionInfo: ability definition {ability} for type {type} is not a skill.", abilityDefinition.Name, type.FullName ?? "???");
                }
                return new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, characterGuards);
            }
            case AdminCommandAttribute adminCommandAttribute:
            {
                var playerGuards = GuardGenerator.GeneratePlayerGuards(type);
                var adminGuards = GuardGenerator.GenerateAdminGuards(type);
                return new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, playerGuards, adminGuards);
            }
            case PlayerCommandAttribute playerCommandAttribute:
            {
                var playerGuards = GuardGenerator.GeneratePlayerGuards(type);
                return new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, playerGuards);
            }
            case ItemCommandAttribute itemCommandAttribute:
                return new ItemGameActionInfo(type, itemCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            case RoomCommandAttribute roomCommandAttribute:
                return new RoomGameActionInfo(type, roomCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
            case ActorCommandAttribute actorCommandAttribute:
                return new ActorGameActionInfo(type, actorCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute);
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
                        Logger.LogError("Type {type} doesn't any generic type argument of type {actorType}.", type.FullName ?? "???", baseActorType.FullName ?? "???");
                }

                if (doneWithThisType)
                    break;
                type = type.BaseType;
            }
        }
        Logger.LogTrace("{actorName}: {name} => {best}", actorType.Name, name, (best == default ? "???" : selectorFunc(best).FullName));
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
