﻿using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.Server.Command
{
    // TODO: not anymore a static class and implement ICommandManager
    // TODO: add interface with IEnumerable<ICommandInfo> GameActions and CreateInstance
    public class CommandManager
    {
        public CommandManager(IAssemblyHelper assemblyHelper)
        {
            Dictionary<string, CommandInfo> commandInfos = new Dictionary<string, CommandInfo>();

            // Get commands and register them in IOC
            Type iGameActionType = typeof(IGameAction);
            foreach (var gameActionType in assemblyHelper.AllReferencedAssemblies.SelectMany(a => a.GetTypes()
                 .Where(t => t.IsClass && !t.IsAbstract && iGameActionType.IsAssignableFrom(t))))
            {
                CommandInfo commandInfo = CommandInfo.Create(gameActionType);
                if (commandInfos.ContainsKey(commandInfo.Name))
                    Log.Default.WriteLine(LogLevels.Error, "Duplicate command {0}", commandInfo.Name);
                else
                {
                    commandInfos.Add(commandInfo.Name, commandInfo);
                    DependencyContainer.Current.Register(gameActionType); // !! TODO: skills will be registered twice
                }
            }
        }

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
                   (x, commandAttribute) => new TrieEntry<CommandExecutionInfo>(commandAttribute.Name, new CommandInfo(x.executionType, commandAttribute, x.attributes.syntaxCommandAttribute)));
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
