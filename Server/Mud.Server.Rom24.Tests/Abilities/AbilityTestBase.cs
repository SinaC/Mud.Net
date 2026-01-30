using Microsoft.Extensions.Logging;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Rom24.Spells;
using System.Reflection;

namespace Mud.Server.Rom24.Tests.Abilities;

[TestClass]
public abstract class AbilityTestBase
{
    protected IServiceProvider _serviceProvider = default!;
    protected Mock<IGuardGenerator> _guardGeneratorMock = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        _serviceProvider = serviceProviderMock.Object;
        _guardGeneratorMock = new Mock<IGuardGenerator>();

        RegisterAdditionalDependencies(serviceProviderMock);
    }

    protected virtual void RegisterAdditionalDependencies(Mock<IServiceProvider> serviceProviderMock)
    {
    }

    protected static ICommandParameter[] BuildParameters(string parameters)
    {
        var parser = new CommandParser(new Mock<ILogger<CommandParser>>().Object);
        return parser.SplitParameters(parameters).Select(parser.ParseParameter).ToArray();
    }

    protected IActionInput BuildActionInput<TGameAction>(IActor actor, string commandLine)
        where TGameAction:IGameAction
    {
        Type type = typeof(TGameAction);
        CommandAttribute commandAttribute = type.GetCustomAttribute<CommandAttribute>()!;
        SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? GameActionInfo.DefaultSyntaxCommandAttribute;
        IEnumerable<AliasAttribute> aliasAttributes = type.GetCustomAttributes<AliasAttribute>();

        var guardGenerator = _guardGeneratorMock.Object;
        IGameActionInfo gameActionInfo;
        switch (commandAttribute)
        {
            case PlayableCharacterCommandAttribute playableCharacterCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    var characterGuards = guardGenerator.GenerateCharacterGuards(type);
                    gameActionInfo = new PlayableCharacterGameActionInfo(type, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards, characterGuards);
                    break;
                }
            case CharacterCommandAttribute characterCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    var characterGuards = guardGenerator.GenerateCharacterGuards(type);
                    if (type.IsAssignableTo(typeof(ISkill)))
                    {
                        var skillDefinition = new AbilityDefinition(type, characterGuards);
                        gameActionInfo = new SkillGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, null, skillDefinition, actorGuards, characterGuards);
                    }
                    else
                        gameActionInfo = new CharacterGameActionInfo(type, characterCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards, characterGuards);
                    break;
                }
            case AdminCommandAttribute adminCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    var playerGuards = guardGenerator.GeneratePlayerGuards(type);
                    var adminGuards = guardGenerator.GenerateAdminGuards(type);
                    gameActionInfo = new AdminGameActionInfo(type, adminCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards, playerGuards, adminGuards);
                    break;
                }
            case PlayerCommandAttribute playerCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    var playerGuards = guardGenerator.GeneratePlayerGuards(type);
                    gameActionInfo = new PlayerGameActionInfo(type, playerCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards, playerGuards);
                    break;
                }
            case ItemCommandAttribute itemCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    gameActionInfo = new ItemGameActionInfo(type, itemCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards);
                    break;
                }
            case RoomCommandAttribute roomCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    gameActionInfo = new RoomGameActionInfo(type, roomCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards);
                    break;
                }
            case ActorCommandAttribute actorCommandAttribute:
                {
                    var actorGuards = guardGenerator.GenerateActorGuards(type);
                    gameActionInfo = new ActorGameActionInfo(type, actorCommandAttribute, syntaxAttribute, aliasAttributes, null, actorGuards);
                    break;
                }
            default:
                gameActionInfo = new GameActionInfo(type, commandAttribute, syntaxAttribute, aliasAttributes, null);
                break;
        }

        new CommandParser(new Mock<ILogger<CommandParser>>().Object).ExtractCommandAndParameters(commandLine, out var command, out var parameters);
        return new ActionInput(gameActionInfo, actor, commandLine, command, parameters);
    }

    protected static IAbilityLearned BuildAbilityLearned(string name)
    {
        var abilityDefinitionMock = new Mock<IAbilityDefinition>();
        abilityDefinitionMock.SetupGet(x => x.Name).Returns(name);
        var mock = new Mock<IAbilityLearned>();
        mock.SetupGet(x => x.AbilityUsage).Returns(new AbilityUsage(name, 1, [new AbilityResourceCost(ResourceKinds.Mana, 50, CostAmountOperators.Fixed)], 1, 100, abilityDefinitionMock.Object));
        mock.SetupGet(x => x.Name).Returns(name);
        mock.Setup(x => x.HasCost).Returns(true);
        return mock.Object;
    }

    public class AssemblyHelper : IAssemblyHelper
    {
        public IEnumerable<Assembly> AllReferencedAssemblies => [typeof(Server.Server).Assembly, typeof(AcidBlast).Assembly];
    }
}
