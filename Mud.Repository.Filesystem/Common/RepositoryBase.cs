﻿using AutoMapper;
using Mud.Container;
using Mud.Settings;
using System.IO;
using System.Xml.Serialization;

namespace Mud.Repository.Filesystem.Common
{
    public abstract class RepositoryBase
    {
        protected IMapper Mapper => DependencyContainer.Current.GetInstance<IMapper>();
        protected ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();

        protected T Load<T>(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (FileStream file = File.OpenRead(filename))
            {
                return (T)deserializer.Deserialize(file);
            }
        }

        protected void Save<T>(T data, string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            using (FileStream file = File.Create(filename))
            {
                serializer.Serialize(file, data);
            }
        }
    }
}
