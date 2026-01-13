using AutoBogus;
using Moq;
using Mud.Common;
using Mud.Domain.SerializationData.Avatar;
using Mud.Repository.Filesystem.Json.Resolvers;
using Mud.Server.Affects.Character;
using Mud.Server.Item.SerializationData;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mud.Repository.Tests
{
    [TestClass]
    public abstract class MappingTestsBase
    {
        protected IServiceProvider _serviceProvider = default!;
        protected JsonSerializerOptions _options = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            _serviceProvider = serviceProviderMock.Object;

            var assemblyHelperMock = new Mock<IAssemblyHelper>();
            assemblyHelperMock.SetupGet(x => x.AllReferencedAssemblies).Returns([typeof(AffectDataBase).Assembly, typeof(ItemCorpseData).Assembly, typeof(CharacterAttributeAffectData).Assembly]);

            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = new PolymorphicTypeResolver(assemblyHelperMock.Object)
            };
            _options.Converters.Add(new JsonStringEnumConverter());

            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });
        }
    }
}
