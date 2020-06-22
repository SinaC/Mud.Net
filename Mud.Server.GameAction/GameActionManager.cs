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
        private readonly Dictionary<Type, IGameActionInfo> _gameActionInfos;
        private readonly Dictionary<Type, IReadOnlyTrie<IGameActionInfo>> _gameActionInfosTrieByActorType;

        public GameActionManager(IAssemblyHelper assemblyHelper)
        {
            _gameActionInfosTrieByActorType = new Dictionary<Type, IReadOnlyTrie<IGameActionInfo>>(); // will be filled when a call to GetGameActions will be called

            Type iGameActionType = typeof(IGameAction);

            // Gather all game action
            _gameActionInfos = assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && iGameActionType.IsAssignableFrom(t)))
                .Select(t => new { executionType = t, attributes = GetGameActionAttributes(t) })
                .ToDictionary(typeAndAttributes => typeAndAttributes.executionType, typeAndAttributes => CreateGameActionInfo(typeAndAttributes.executionType, typeAndAttributes.attributes.commandAttribute, typeAndAttributes.attributes.syntaxAttribute, typeAndAttributes.attributes.aliasAttributes));
        }

        #region IGameActionManager

        public IEnumerable<IGameActionInfo> GameActions => _gameActionInfos.Values;

        public string Execute<TActor>(IGameActionInfo gameActionInfo, TActor actor, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor: IActor
        {
            if (DependencyContainer.Current.GetRegistration(gameActionInfo.CommandExecutionType, false) == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} not found in DependencyContainer.", gameActionInfo.Name);
                return "Something goes wrong.";
            }
            IGameAction gameAction = DependencyContainer.Current.GetInstance(gameActionInfo.CommandExecutionType) as IGameAction;
            if (gameAction == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} cannot be instantiated or is not {1}.", gameActionInfo.Name, typeof(IGameAction).FullName);
                return "Something goes wrong.";
            }
            string commandLine = command + " " + rawParameters;
            IActionInput actionInput = new ActionInput(gameActionInfo, actor, commandLine, command, rawParameters, parameters);
            string guardsResult = gameAction.Guards(actionInput);
            if (guardsResult != null)
                return guardsResult;
            gameAction.Execute(actionInput);
            return null;
        }

        public string Execute<TGameAction, TActor>(TActor actor, string command, string rawParameters, params ICommandParameter[] parameters)
            where TActor : IActor
        {
            Type gameActionType = typeof(TGameAction);
            if (!_gameActionInfos.TryGetValue(gameActionType, out var gameActionInfo))
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction type {0} not found in GameActionManager.", gameActionType.FullName);
                return "Something goes wrong.";
            }
            return Execute(gameActionInfo, actor, command, rawParameters, parameters);
        }

        public IReadOnlyTrie<IGameActionInfo> GetGameActions<TActor>()
            where TActor : IActor
        {
            Type actorType = typeof(TActor);

            if (_gameActionInfosTrieByActorType.TryGetValue(actorType, out var readonlyTrie))
                return readonlyTrie;
            var gameActions = GetGameActionsByActorType<TActor>();
            Trie<IGameActionInfo> trie = new Trie<IGameActionInfo>(gameActions);
            _gameActionInfosTrieByActorType.Add(actorType, trie);
            return trie;
        }

        #endregion

        private IEnumerable<TrieEntry<IGameActionInfo>> GetGameActionsByActorType<TActor>()
            where TActor : IActor
        {
            Type iGameActionType = typeof(IGameAction);
            Type commandAttributeType = typeof(CommandAttribute);

            Type actorType = typeof(TActor);
            Type[] actorTypeSortedImplementedInterfaces = GetSortedImplementedInterfaces(actorType);

            var gameActionInfos = _gameActionInfos
                .Values
                .GroupBy(t => t.Name)
                .OrderBy(g => g.Key)
                .Select(g => PolymorphismSimulator(actorType, actorTypeSortedImplementedInterfaces, g.Key, g, x => x.CommandExecutionType))
                .Where(x => x != null);
            // return one entry using CommandAttribute.Name and one entry by AliasAttribute.Alias
            foreach (var gameActionInfo in gameActionInfos)
            {
                // return command
                yield return new TrieEntry<IGameActionInfo>(gameActionInfo.Name, gameActionInfo);
                // return aliases
                foreach (string alias in gameActionInfo.Aliases)
                    yield return new TrieEntry<IGameActionInfo>(alias, gameActionInfo);
            }
        }

        private IGameActionInfo CreateGameActionInfo(Type type, CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
        {
            switch (commandAttribute)
            {
                case AdminCommandAttribute adminCommandAttribute:
                    return new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes);
                case PlayerCommandAttribute playerCommandAttribute:
                    return new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes);
                case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                    return new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes);
                case CharacterCommandAttribute characterCommandAttribute:
                    return new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes);
                default:
                    return new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes);
            }
        }

        private (CommandAttribute commandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes) GetGameActionAttributes(Type type)
        {
            CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? GameActionInfo.DefaultSyntaxCommandAttribute;
            IEnumerable<AliasAttribute> aliasAttributes = type.GetCustomAttributes<AliasAttribute>();

            return (commandAttribute, syntaxAttribute, aliasAttributes);
        }

        private Type[] GetSortedImplementedInterfaces(Type actorType)
        {
            return actorType.GetInterfaces()
                .Select(i => new { implementedInterface = i, level = GetNestedLevel(i) })
                .OrderByDescending(x => x.level)
                .Select(x => x.implementedInterface)
                .ToArray();
        }

        private T PolymorphismSimulator<T>(Type actorType, Type[] actorTypeSortedImplementedInterfaces, string name, IEnumerable<T> collection, Func<T,Type> selectorFunc)
            where T: class
        {
            T best = default;
            int bestLevel = int.MaxValue;
            foreach (T t in collection) // foreach type on the same command name, search the one with the highest level of class nesting
            {
                bool doneWithThisType = false;
                Type baseType = selectorFunc(t).BaseType;
                while (baseType != null)
                {
                    if (baseType.GenericTypeArguments?.Length > 0)
                    {
                        for (int i = 0; i < actorTypeSortedImplementedInterfaces.Length; i++)
                            if (actorTypeSortedImplementedInterfaces[i] == baseType.GenericTypeArguments[0])
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

                    if (doneWithThisType)
                        break;
                    baseType = baseType.BaseType;
                }
            }
            Debug.Print(actorType.Name + ": " + name + " => " + (best == default ? "???" : selectorFunc(best).FullName));
            return best;
        }

        private int GetNestedLevel(Type type)
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
