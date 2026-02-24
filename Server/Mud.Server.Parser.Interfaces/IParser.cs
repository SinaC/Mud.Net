namespace Mud.Server.Parser.Interfaces;

public interface IParser
{
    ICommandParameter[] NoParameters { get; }

    IParseResult? Parse(string input);
    IParseResult? Parse(Func<bool, IReadOnlyDictionary<string, string>?>? aliasesFunc, string? input);

    ICommandParameter ParseParameter(string parameter);
    string JoinParameters(IEnumerable<ICommandParameter> parameters);
}
