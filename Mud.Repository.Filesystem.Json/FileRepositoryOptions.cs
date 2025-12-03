namespace Mud.Repository.Filesystem.Json
{
    public class FileRepositoryOptions
    {
        public const string SectionName = "FileRepository.Settings";

        public required string PlayerPath { get; init; }
        public required string AdminPath { get; init; }
        public required string LoginFilename { get; init; }
    }
}
