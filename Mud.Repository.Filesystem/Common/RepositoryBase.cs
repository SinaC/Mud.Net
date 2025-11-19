using AutoMapper;
using Microsoft.Extensions.Logging;
using Mud.Settings.Interfaces;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Common;

public abstract class RepositoryBase
{
    protected ILogger<RepositoryBase> Logger { get; }
    protected IMapper Mapper { get; }
    protected ISettings Settings { get; }

    protected RepositoryBase(ILogger<RepositoryBase> logger, IMapper mapper, ISettings settings)
    {
        Logger = logger;
        Mapper = mapper;
        Settings = settings;
    }

    protected T Load<T>(string filename)
    {
        XmlSerializer deserializer = new (typeof(T));
        using FileStream file = File.OpenRead(filename);
        return (T)deserializer.Deserialize(file)!;
    }

    protected void Save<T>(T data, string filename)
    {
        XmlSerializer serializer = new (typeof(T));
        Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? throw new InvalidOperationException());
        using FileStream file = File.Create(filename);
        serializer.Serialize(file, data);
    }
}
