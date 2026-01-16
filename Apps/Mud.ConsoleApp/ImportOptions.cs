namespace Mud.Server.ConsoleApp;

public class ImportOptions
{
    public const string SectionName = "Import.Settings";

    public required string Importer { get; init; }
    public required string Path { get; init; }
    public required string[] Areas { get; init; }
    public required string[] Lists { get; init; }
}
