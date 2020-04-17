using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.Repository.Tests.Mock;
using Mud.Settings;

namespace Mud.Repository.Tests
{
    [TestClass]
    public abstract class MappingTestsBase
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            DependencyContainer.Current.Register<ISettings, MockSettings>(SimpleInjector.Lifestyle.Singleton);
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;
                cfg.AddProfile<Mongo.AutoMapperProfile>();
                cfg.AddProfile<Filesystem.AutoMapperProfile>();
            });
            DependencyContainer.Current.RegisterInstance(mapperConfiguration.CreateMapper());
        }
    }
}
