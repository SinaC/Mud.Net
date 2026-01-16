using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Mud.POC.Serialization
{
    //https://nikiforovall.blog/dotnet/aspnetcore/2024/04/06/openapi-polymorphism.html
    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        private ILookup<Type, DerivedTypeDefinition> DerivedTypeDefinitionsByBaseType { get; }

        public PolymorphicTypeResolver()
        {
            var derivedTypeDefinitions = new List<DerivedTypeDefinition>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    var attribute = type.GetCustomAttribute<PolymorphismAttribute>();
                    if (attribute != null)
                        derivedTypeDefinitions.Add(new DerivedTypeDefinition { BaseType = attribute.BaseType, DerivedType = type, Discriminator = attribute.Discriminator ?? type.Name });
                }
            }

            DerivedTypeDefinitionsByBaseType = derivedTypeDefinitions.ToLookup(x => x.BaseType);
        }

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            var derivedTypeDefinitions = DerivedTypeDefinitionsByBaseType[jsonTypeInfo.Type]?.ToArray();
            if (derivedTypeDefinitions?.Length > 0)
            {
                var derivedTypes = derivedTypeDefinitions.Select(x => new JsonDerivedType(x.DerivedType, x.Discriminator)).ToList();
                var polymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$discriminator",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                };
                foreach (var derivedTypeDefinition in derivedTypeDefinitions)
                    polymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedTypeDefinition.DerivedType, derivedTypeDefinition.Discriminator));
                jsonTypeInfo.PolymorphismOptions = polymorphismOptions;
            }

            return jsonTypeInfo;
        }

        private class DerivedTypeDefinition
        {
            public required Type BaseType { get; set; }
            public required Type DerivedType { get; set; }
            public required string Discriminator { get; set; }
        }
    }
}
