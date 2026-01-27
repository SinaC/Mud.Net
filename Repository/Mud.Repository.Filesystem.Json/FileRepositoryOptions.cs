namespace Mud.Repository.Filesystem.Json
{
    public class FileRepositoryOptions
    {
        public const string SectionName = "FileRepository.Settings";

        public required string AccountPath { get; init; }
        public required string AvatarPath { get; init; }
        public required string SocialPath { get; init; }
    }
}
