namespace Mud.Server.Parser.Interfaces;

public interface IParser
{
    ICommandParameter[] NoParameters { get; }

    bool ExtractCommandAndParameters(string input, out string command, out ICommandParameter[] parameters);
    bool ExtractCommandAndParameters(Func<bool, IReadOnlyDictionary<string, string>?>? aliasesFunc, string? input, out string command, out ICommandParameter[] parameters, out bool forceOutOfGame);

    IEnumerable<string> SplitParameters(string parameters);
    ICommandParameter ParseParameter(string parameter);
    string JoinParameters(IEnumerable<ICommandParameter> parameters);
}
