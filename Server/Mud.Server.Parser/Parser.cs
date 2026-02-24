using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Parser;

[Export(typeof(IParser)), Shared]
public partial class Parser : IParser
{
    private static readonly ICommandParameter[] _noParameters = Enumerable.Empty<ICommandParameter>().ToArray();
    private static readonly ICommandParameter EmptyCommandParameter = new CommandParameter(string.Empty, string.Empty, false, false);

    private ILogger<Parser> Logger { get; }

    public Parser(ILogger<Parser> logger)
    {
        Logger = logger;
    }

    public ICommandParameter[] NoParameters
        => _noParameters;

    public IParseResult? Parse(string input)
        => Parse(null, input);

    public IParseResult? Parse(Func<bool, IReadOnlyDictionary<string,string>?>? aliasesFunc, string? input)
    {
        Logger.LogTrace("Extracting command and parameters [{input}]", input ?? "(none)");

        // No command ?
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.LogWarning("Empty command");
            return null;
        }

        // Split into command and remaining tokens
        var extractedCommandInfo = ExtractCommandAndTokens(input);

        var forceOutOfGame = false;
        var command = extractedCommandInfo.command;
        var rawParameters = extractedCommandInfo.rawParameters;
        IEnumerable<string> tokens = extractedCommandInfo.tokens;
        // Check if forcing OutOfGame
        if (command.StartsWith('/'))
        {
            forceOutOfGame = true;
            command = command[1..]; // remove '/'
        }

        // Substitute by alias if found
        var aliases = aliasesFunc?.Invoke(forceOutOfGame);
        if (aliases != null)
        {
            if (aliases.TryGetValue(command, out var alias))
            {
                Logger.LogDebug("Alias found : {command} -> {alias}", command, alias);
                // Extract command and raw parameters
                var aliasExtractedCommandInfo = ExtractCommandAndTokens(alias);
                command = aliasExtractedCommandInfo.command;
                rawParameters = aliasExtractedCommandInfo.rawParameters;
                tokens = aliasExtractedCommandInfo.tokens;
            }
        }

        // Parse parameter
        var parameters = tokens.Select(ParseParameter).ToArray();

        if (parameters.Any(x => x == CommandParameter.InvalidCommandParameter))
        {
            Logger.LogWarning("Invalid command parameters");
            return null;
        }

        return new ParseResult
        {
            Command = command,
            RawParameters = rawParameters,
            Parameters = parameters,
            ForceOutOfGame = forceOutOfGame
        };
    }

    private (string command, string rawParameters, IEnumerable<string> tokens) ExtractCommandAndTokens(string commandLine)
    {
        Logger.LogTrace("Extracting command [{command}]", commandLine);

        var (command, parameters) = SplitCommandAndParameters(commandLine);
        var tokens = parameters.Tokenize(false).ToArray();

        return (command, parameters, tokens);
    }

    private static char[] SpecialCommands { get; } = [',', '.', '\'', ':', ';', '?'];
    private static (string command, string parameters) SplitCommandAndParameters(string commandLine)
    {
        var candidate = commandLine.Trim();

        var index = -1;
        for (var i = 0; i < candidate.Length; i++)
        {
            var c = candidate[i];
            if (SpecialCommands.Contains(c) || char.IsWhiteSpace(c))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            return (commandLine, string.Empty);
        else if (index == 0)
            return (commandLine[0].ToString(), commandLine.Substring(1).Trim());
        else
            return (commandLine.Substring(0, index), commandLine.Substring(index + 1).Trim());
    }

    public ICommandParameter ParseParameter(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return EmptyCommandParameter;
        int dotIndex = parameter.IndexOf('.');
        if (dotIndex < 0)
        {
            bool isAll = StringCompareHelpers.StringEquals(parameter, "all");
            return isAll
                    ? new CommandParameter(parameter, parameter, true, true) // all
                    : new CommandParameter(parameter, parameter, 1);
        }
        if (dotIndex == 0)
            return CommandParameter.InvalidCommandParameter; // only . is invalid
        string countAsString = parameter[..dotIndex];
        string value = parameter[(dotIndex + 1)..];
        bool isCountAll = StringCompareHelpers.StringEquals(countAsString, "all");
        if (isCountAll)
            return new CommandParameter(parameter, value, true, false); // all.xxx
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
