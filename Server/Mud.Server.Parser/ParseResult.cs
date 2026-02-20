using Mud.Server.Parser.Interfaces;

namespace Mud.Server.Parser;

public class ParseResult : IParseResult
{
    public required string Command { get; set; } = default!;
    public required string RawParameters { get; set; } = default!;
    public required ICommandParameter[] Parameters { get; set; } = default!;
    public required bool ForceOutOfGame { get; set; } = default!;
}
