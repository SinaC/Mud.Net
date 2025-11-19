using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.GameAction;

public class CommandParser : ICommandParser
{
    private static readonly ICommandParameter[] _noParameters = Enumerable.Empty<ICommandParameter>().ToArray();

    private static readonly CommandParameter EmptyCommandParameter = new(string.Empty, string.Empty, false);

    private ILogger<CommandParser> Logger { get; }

    public CommandParser(ILogger<CommandParser> logger)
    {
        Logger = logger;
    }

    public ICommandParameter[] NoParameters
        => _noParameters;

    public bool ExtractCommandAndParameters(string input, out string command, out ICommandParameter[] parameters)
    {
        return ExtractCommandAndParameters(null, input, out command, out parameters, out _);
    }

    public bool ExtractCommandAndParameters(Func<bool, IReadOnlyDictionary<string,string>?>? aliasesFunc, string? input, out string command, out ICommandParameter[] parameters, out bool forceOutOfGame)
    {
        Logger.LogTrace("Extracting command and parameters [{0}]", input ?? "(none)");

        // No command ?
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.LogWarning("Empty command");
            command = default!;
            parameters = default!;
            forceOutOfGame = false;
            return false;
        }

        // Split into command and remaining tokens
        var extractedCommandInfo = ExtractCommandAndTokens(input);

        command = extractedCommandInfo.command;
        IEnumerable<string> tokens = extractedCommandInfo.tokens;
        // Check if forcing OutOfGame
        if (command.StartsWith('/'))
        {
            forceOutOfGame = true;
            command = command[1..]; // remove '/'
        }
        else
            forceOutOfGame = false;

        // Substitute by alias if found
        var aliases = aliasesFunc?.Invoke(forceOutOfGame);
        if (aliases != null)
        {
            if (aliases.TryGetValue(command, out var alias))
            {
                Logger.LogDebug("Alias found : {0} -> {1}", command, alias);
                // Extract command and raw parameters
                var aliasExtractedCommandInfo = ExtractCommandAndTokens(alias);
                tokens = aliasExtractedCommandInfo.tokens;
            }
        }

        // Parse parameter
        parameters = tokens.Select(ParseParameter).ToArray();

        if (parameters.Any(x => x == CommandParameter.InvalidCommandParameter))
        {
            Logger.LogWarning("Invalid command parameters");
            return false;
        }

        return true;
    }

    private (string command, IEnumerable<string> tokens) ExtractCommandAndTokens(string commandLine)
    {
        Logger.LogTrace("Extracting command [{0}]", commandLine);

        // handle special case of ' command (alias for say)
        var startsWithSimpleQuote = commandLine.StartsWith('\'');
        // Split
        string[] tokens = startsWithSimpleQuote
            ? "\'".Yield().Concat(SplitParameters(commandLine[1..])).ToArray()
            : SplitParameters(commandLine).ToArray();
        // First token is the command
        string command = tokens[0];

        return (command, tokens.Skip(1)); // return command and remaining tokens
    }

    public IEnumerable<string> SplitParameters(string parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters))
            yield break;
        var sb = new StringBuilder();
        bool inQuote = false;
        foreach (char c in parameters)
        {
            if ((c == '"' || c == '\'') && !inQuote)
            {
                inQuote = true;
                continue;
            }
            if (c != '"' && c != '\'' && !(char.IsWhiteSpace(c) && !inQuote))
            {
                sb.Append(c);
                continue;
            }
            if (sb.Length > 0)
            {
                var result = sb.ToString();
                sb.Clear();
                inQuote = false;
                yield return result;
            }
        }
        if (sb.Length > 0)
            yield return sb.ToString();
    }

    public ICommandParameter ParseParameter(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return EmptyCommandParameter;
        int dotIndex = parameter.IndexOf('.');
        if (dotIndex < 0)
        {
            bool isAll = string.Equals(parameter, "all", StringComparison.InvariantCultureIgnoreCase);
            return isAll
                    ? new CommandParameter(parameter, string.Empty, true)
                    : new CommandParameter(parameter, parameter, 1);
        }
        if (dotIndex == 0)
            return CommandParameter.InvalidCommandParameter; // only . is invalid
        string countAsString = parameter[..dotIndex];
        string value = parameter[(dotIndex + 1)..];
        bool isCountAll = string.Equals(countAsString, "all", StringComparison.InvariantCultureIgnoreCase);
        if (isCountAll)
            return new CommandParameter(parameter, value, true);
        if (!int.TryParse(countAsString, out int count)) // string.string is not splitted
            return new CommandParameter(parameter, value, 1);
        if (count <= 0 || string.IsNullOrWhiteSpace(value)) // negative count or empty value is invalid
            return CommandParameter.InvalidCommandParameter;
        return new CommandParameter(parameter, value, count);
    }

    public string JoinParameters(IEnumerable<ICommandParameter> parameters)
    {
        var commandParameters = parameters as ICommandParameter[] ?? parameters.ToArray();
        if (commandParameters.Length == 0)
            return string.Empty;

        string joined = string.Join(" ", commandParameters.Select(x => x.RawValue));
        return joined;
    }
}
