using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mud.Server.GameAction
{
    // TODO: not anymore a static class
    // TODO: find a way to store multiple command with the same name (delete exists for player and for admin, skills exists for character and admin)
    // TODO: find a way to create the Trie with the correct command in case of duplicate name
    public class GameActionManager : IGameActionManager
    {
        private Dictionary<string, IGameActionInfo> _gameActions;

        public GameActionManager(IAssemblyHelper assemblyHelper)
        {
            _gameActions = new Dictionary<string, IGameActionInfo>();
            // Get commands
            Type iGameActionType = typeof(IGameAction);
            foreach (var gameActionType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
                 .Where(t => t.IsClass && !t.IsAbstract && iGameActionType.IsAssignableFrom(t))))
            {
                var commandAndSyntaxAttributes = GetCommandAndSyntaxAttributes(gameActionType);
                foreach (var commandAttribute in commandAndSyntaxAttributes.commandAttributes)
                {
                    IGameActionInfo gameActionInfo = CreateGameActionInfo(gameActionType, commandAttribute, commandAndSyntaxAttributes.syntaxCommandAttribute);
                    if (_gameActions.ContainsKey(gameActionInfo.Name))
                        Log.Default.WriteLine(LogLevels.Error, "Duplicate game action {0}", gameActionInfo.Name);
                    else
                        _gameActions.Add(gameActionInfo.Name, gameActionInfo);
                }
            }
        }

        #region IGameActionManager

        public IEnumerable<IGameActionInfo> GameActions => _gameActions.Values;

        public IGameActionInfo this[string name]
        {
            get 
            {
                if (!_gameActions.TryGetValue(name, out var gameActionInfo))
                    return null;
                return gameActionInfo;
            }
        }

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
            // TODO
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
                .GroupBy(x => x.Key) // TODO: remove this
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
                   (x, commandAttribute) => new TrieEntry<ICommandExecutionInfo>(commandAttribute.Name, new CommandMethodInfo(x.methodInfo, commandAttribute, x.attributes.syntaxCommandAttribute)));
        }

        private static IEnumerable<TrieEntry<ICommandExecutionInfo>> GetCommandsFromAssembly(Assembly assembly, Type type)
        {
            Type iGameActionType = typeof(IGameAction);
            Type commandAttributeType = typeof(CommandAttribute);

            //foreach(var group in assembly.GetTypes().Where(t => !t.IsAbstract && iGameActionType.IsAssignableFrom(t) && t.GetCustomAttributes(commandAttributeType, false).Any()).GroupBy(x => x.Name).OrderBy(x => x.Key))
            //    PolymorphismSimulator(type, group);

            //return assembly.GetTypes()
            //    .Where(t => !t.IsAbstract && iGameActionType.IsAssignableFrom(t) && t.GetCustomAttributes(commandAttributeType, false).Any())
            //    // TODO check if x inherits from GameActionBase<Type,>
            //    //.Where(t => t.GetGenericParameterConstraints().Any(c => t.IsAssignableFrom(c)))
            //    .Select(t => new { executionType = t, attributes = GetCommandAndSyntaxAttributes(t) })
            //    .SelectMany(x => x.attributes.commandAttributes,
            //       (x, commandAttribute) => new TrieEntry<ICommandExecutionInfo>(commandAttribute.Name, CreateGameActionInfoStatic(x.executionType, commandAttribute, x.attributes.syntaxCommandAttribute)));
            return  assembly.GetTypes()
                .Where(t => !t.IsAbstract && iGameActionType.IsAssignableFrom(t) && t.GetCustomAttributes(commandAttributeType, false).Any())
                .GroupBy(t => t.Name)
                .OrderBy(g => g.Key)
                .Select(g => PolymorphismSimulator(type, g))
                .Where(x => x != null)
                .Select(t => new { executionType = t, attributes = GetCommandAndSyntaxAttributes(t) })
                .SelectMany(x => x.attributes.commandAttributes,
                   (x, commandAttribute) => new TrieEntry<ICommandExecutionInfo>(commandAttribute.Name, CreateGameActionInfoStatic(x.executionType, commandAttribute, x.attributes.syntaxCommandAttribute)));
        }

        private static (IEnumerable<CommandAttribute> commandAttributes, SyntaxAttribute syntaxCommandAttribute) GetCommandAndSyntaxAttributes(MethodInfo methodInfo)
        {
            IEnumerable<CommandAttribute> commandAttributes = methodInfo.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Distinct(new CommandAttributeEqualityComparer());
            SyntaxAttribute syntaxCommandAttribute = methodInfo.GetCustomAttribute(typeof(SyntaxAttribute)) as SyntaxAttribute ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;

            return (commandAttributes, syntaxCommandAttribute);
        }

        private static (IEnumerable<CommandAttribute> commandAttributes, SyntaxAttribute syntaxCommandAttribute) GetCommandAndSyntaxAttributes(Type type)
        {
            IEnumerable<CommandAttribute> commandAttributes = type.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Distinct(new CommandAttributeEqualityComparer());
            SyntaxAttribute syntaxCommandAttribute = type.GetCustomAttribute(typeof(SyntaxAttribute)) as SyntaxAttribute ?? CommandExecutionInfo.DefaultSyntaxCommandAttribute;

            return (commandAttributes, syntaxCommandAttribute);
        }

        private static Type PolymorphismSimulator(Type actorType, IGrouping<string, Type> typeByNames)
        {
            Type[] actorTypeImplementedInterfaces = actorType.GetInterfaces().Reverse().ToArray(); // interfaces are sorted from most to less nested -> reverse to get top level interface
            Type bestType = null;
            int bestLevel = int.MaxValue;
            foreach (Type t in typeByNames)
            {
                bool doneWithThisType = false;
                Type baseType = t.BaseType;
                while (baseType != null)
                {
                    if (baseType.GenericTypeArguments?.Length > 0)
                    {
                        for (int i = 0; i < actorTypeImplementedInterfaces.Length; i++)
                            if (actorTypeImplementedInterfaces[i] == baseType.GenericTypeArguments[0])
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
    }
}
