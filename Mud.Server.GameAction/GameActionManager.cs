using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mud.Server.GameAction
{
    // TODO: not anymore a static class
    public class GameActionManager : IGameActionManager
    {
        private readonly ILookup<string, IGameActionInfo> _gameActions;

        public GameActionManager(IAssemblyHelper assemblyHelper)
        {
            Type iGameActionType = typeof(IGameAction);
            _gameActions = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iGameActionType.IsAssignableFrom(t)))
                .Select(t => new { gameActionType = t, commandAndSyntaxAttributes = GetCommandAndSyntaxAttributes(t) })
                .SelectMany(x => x.commandAndSyntaxAttributes.commandAttributes, (gameActionTypeAndAttributes, commandAttribute) => CreateGameActionInfo(gameActionTypeAndAttributes.gameActionType, commandAttribute, gameActionTypeAndAttributes.commandAndSyntaxAttributes.syntaxAttribute))
                .ToLookup(x => x.Name);
        }

        #region IGameActionManager

        public IEnumerable<IGameActionInfo> GameActions => _gameActions.SelectMany(x => x);

        public IGameAction CreateInstance(IGameActionInfo gameActionInfo)
        {
            if (DependencyContainer.Current.GetRegistration(gameActionInfo.CommandExecutionType, false) == null)
            {     
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} not found in DependencyContainer.", gameActionInfo.Name);
                return default;
            }
            IGameAction instance = DependencyContainer.Current.GetInstance(gameActionInfo.CommandExecutionType) as IGameAction;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} cannot be instantiated or is not {1}.", gameActionInfo.Name, typeof(IGameAction).Name);
                return default;
            }
            return instance;
        }

        public IActionInput CreateActionInput<TActor>(IGameActionInfo gameActionInfo, TActor actor, string commandLine, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor: IActor
        {
            ActionInput actionInput = new ActionInput(gameActionInfo, actor, string.Empty/*TODO*/, command, rawParameters, parameters);
            return actionInput;
        }

        #endregion

        private IGameActionInfo CreateGameActionInfo(Type type, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
        {
            switch (commandAttribute)
            {
                case AdminCommandAttribute adminCommandAttribute:
                    return new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute);
                case PlayerCommandAttribute playerCommandAttribute:
                    return new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute);
                case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                    return new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute);
                case CharacterCommandAttribute characterCommandAttribute:
                    return new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute);
                default:
                    return new GameActionInfo(type, commandAttribute, syntaxAttribute);
            }
        }

        private static GameActionInfo CreateGameActionInfoStatic(Type type, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute)
        {
            switch (commandAttribute)
            {
                case AdminCommandAttribute adminCommandAttribute:
                    return new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute);
                case PlayerCommandAttribute playerCommandAttribute:
                    return new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute);
                case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                    return new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute);
                case CharacterCommandAttribute characterCommandAttribute:
                    return new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute);
                default:
                    return new GameActionInfo(type, commandAttribute, syntaxAttribute);
            }
        }

        public static IReadOnlyTrie<ICommandExecutionInfo> GetCommands(Type type)
        {
            var commands = GetCommandsFromType(type).Union(GetCommandsFromAssembly(type.Assembly, type))
                .GroupBy(x => x.Key) // TODO: remove this when GetCommandsFromType will be deleted
                .Select(x => x.First());
            Trie<ICommandExecutionInfo> trie = new Trie<ICommandExecutionInfo>(commands);
            return trie;
        }

        private static IEnumerable<TrieEntry<ICommandExecutionInfo>> GetCommandsFromType(Type type)
        {
            Type commandAttributeType = typeof(CommandAttribute);
            return type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
               .Where(x => x.GetCustomAttributes(commandAttributeType, false).Any())
               .Select(x => new { methodInfo = x, attributes = GetCommandAndSyntaxAttributes(x) })
               .SelectMany(x => x.attributes.commandAttributes,
                   (x, commandAttribute) => new TrieEntry<ICommandExecutionInfo>(commandAttribute.Name, new CommandMethodInfo(x.methodInfo, commandAttribute, x.attributes.syntaxAttribute)));
        }

        private static IEnumerable<TrieEntry<ICommandExecutionInfo>> GetCommandsFromAssembly(Assembly assembly, Type actorType)
        {
            Type iGameActionType = typeof(IGameAction);
            Type commandAttributeType = typeof(CommandAttribute);

            Type[] actorTypeSortedImplementedInterfaces = actorType.GetInterfaces()
                .Select(i => new { implementedInterface = i, level = GetNestedLevel(i) })
                .OrderByDescending(x => x.level)
                .Select(x => x.implementedInterface)
                .ToArray();

            return assembly.GetTypes()
                .Where(t => !t.IsAbstract && iGameActionType.IsAssignableFrom(t) && t.GetCustomAttributes(commandAttributeType, false).Any())
                .GroupBy(t => t.Name)
                .OrderBy(g => g.Key)
                .Select(g => PolymorphismSimulator(actorType, actorTypeSortedImplementedInterfaces, g))
                .Where(x => x != null)
                .Select(t => new { executionType = t, attributes = GetCommandAndSyntaxAttributes(t) })
                .SelectMany(x => x.attributes.commandAttributes,
                   (x, commandAttribute) => new TrieEntry<ICommandExecutionInfo>(commandAttribute.Name, CreateGameActionInfoStatic(x.executionType, commandAttribute, x.attributes.syntaxAttribute)));
        }

        private static (IEnumerable<CommandAttribute> commandAttributes, SyntaxAttribute syntaxAttribute) GetCommandAndSyntaxAttributes(MethodInfo methodInfo)
        {
            IEnumerable<CommandAttribute> commandAttributes = methodInfo.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Distinct(new CommandAttributeEqualityComparer());
            SyntaxAttribute syntaxCommandAttribute = methodInfo.GetCustomAttribute(typeof(SyntaxAttribute)) as SyntaxAttribute ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;

            return (commandAttributes, syntaxCommandAttribute);
        }

        private static (IEnumerable<CommandAttribute> commandAttributes, SyntaxAttribute syntaxAttribute) GetCommandAndSyntaxAttributes(Type type)
        {
            IEnumerable<CommandAttribute> commandAttributes = type.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Distinct(new CommandAttributeEqualityComparer());
            SyntaxAttribute syntaxCommandAttribute = type.GetCustomAttribute(typeof(SyntaxAttribute)) as SyntaxAttribute ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;

            return (commandAttributes, syntaxCommandAttribute);
        }

        private static Type PolymorphismSimulator(Type actorType, Type[] actorTypeSortedImplementedInterfaces, IGrouping<string, Type> typeByNames)
        {
            Type bestType = null;
            int bestLevel = int.MaxValue;
            foreach (Type t in typeByNames) // foreach type on the same command name, search the one with the highest level of class nesting
            {
                bool doneWithThisType = false;
                Type baseType = t.BaseType;
                while (baseType != null)
                {
                    if (baseType.GenericTypeArguments?.Length > 0)
                    {
                        for (int i = 0; i < actorTypeSortedImplementedInterfaces.Length; i++)
                            if (actorTypeSortedImplementedInterfaces[i] == baseType.GenericTypeArguments[0])
                            {
                                if (i < bestLevel)
                                {
                                    bestType = t;
                                    bestLevel = i;
                                }
                                doneWithThisType = true;
                                break;
                            }
                    }

                    if (doneWithThisType)
                        break;
                    baseType = baseType.BaseType;
                }
            }
            Debug.Print(actorType.Name + ": "+ typeByNames.Key + " => " + bestType?.FullName ?? "???");
            return bestType;
        }

        private static int GetNestedLevel(Type type)
        {
            if (type == null)
                return 0;

            int maxLevel = 1 + GetNestedLevel(type.BaseType);

            foreach (Type implementedInterface in type.GetInterfaces())
            {
                int implementedInterfaceLevel = 1 + GetNestedLevel(implementedInterface);
                if (implementedInterfaceLevel > maxLevel)
                    maxLevel = implementedInterfaceLevel;
            }

            return maxLevel;
        }
    }
}
