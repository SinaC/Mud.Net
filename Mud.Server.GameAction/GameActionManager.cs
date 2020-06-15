using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.GameAction
{
    // TODO: not anymore a static class
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
                IGameActionInfo commandInfo = GameActionInfo.Create(gameActionType);
                if (_gameActions.ContainsKey(commandInfo.Name))
                    Log.Default.WriteLine(LogLevels.Error, "Duplicate game action {0}", commandInfo.Name);
                else
                    _gameActions.Add(commandInfo.Name, commandInfo);
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


        public IGameAction CreateInstance(string name)
        {
            IGameActionInfo gameActionInfo = this[name];
            if (gameActionInfo == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} doesn't exist.", name);
                return default;
            }
            if (DependencyContainer.Current.GetRegistration(gameActionInfo.CommandExecutionType, false) == null)
            {     
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} not found in DependencyContainer.", name);
                return default;
            }
            IGameAction instance = DependencyContainer.Current.GetInstance(gameActionInfo.CommandExecutionType) as IGameAction;
            if (instance == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameAction {0} cannot be instantiated or is not {1}.", name, typeof(IGameAction).Name);
                return default;
            }
            return instance;
        }

        #endregion

        public static IReadOnlyTrie<CommandExecutionInfo> GetCommands(Type type)
        {
            var commands = GetCommandsFromType(type).Union(GetCommandsFromAssembly(type.Assembly));
            Trie<CommandExecutionInfo> trie = new Trie<CommandExecutionInfo>(commands);
            return trie;
        }

        private static IEnumerable<TrieEntry<CommandExecutionInfo>> GetCommandsFromType(Type type)
        {
            Type commandAttributeType = typeof(CommandAttribute);
            return type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
               .Where(x => x.GetCustomAttributes(commandAttributeType, false).Any())
               .Select(x => new { methodInfo = x, attributes = GetCommandAndSyntaxAttributes(x) })
               .SelectMany(x => x.attributes.commandAttributes,
                   (x, commandAttribute) => new TrieEntry<CommandExecutionInfo>(commandAttribute.Name, new CommandMethodInfo(x.methodInfo, commandAttribute, x.attributes.syntaxCommandAttribute)));
        }

        private static IEnumerable<TrieEntry<CommandExecutionInfo>> GetCommandsFromAssembly(Assembly assembly)
        {
            Type commandAttributeType = typeof(CommandAttribute);
            return assembly.GetTypes()
                .Where(t => t.GetCustomAttributes(commandAttributeType, false).Any())
                // TODO check if x inherits from CommandBase<Type>
                //.Where(t => t.GetGenericParameterConstraints().Any(c => t.IsAssignableFrom(c)))
                .Select(x => new { executionType = x, attributes = GetCommandAndSyntaxAttributes(x) })
                .SelectMany(x => x.attributes.commandAttributes,
                   (x, commandAttribute) => new TrieEntry<CommandExecutionInfo>(commandAttribute.Name, new GameActionInfo(x.executionType, commandAttribute, x.attributes.syntaxCommandAttribute)));
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
    }
}
