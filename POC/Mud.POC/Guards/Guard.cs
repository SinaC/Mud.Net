namespace Mud.POC.Guards;

//
public interface IActor
{
}

public interface ICharacter : IActor
{
    int Level { get; }
}
//

public interface IGuard<TActor>
    where TActor : IActor
{
    string? Guards(TActor actor, string[] parameters);
}

public class ArgumentNeededGuard : IGuard<IActor>, IGuard<ICharacter>
{
    public string? Guards(IActor actor, string[] parameters)
    {
        if (parameters.Length == 0)
            return "Argument needed.";
        return null;
    }

    public string? Guards(ICharacter actor, string[] parameters)
        => Guards(actor as IActor, parameters);
}

public class LevelGuard : IGuard<ICharacter>
{
    private readonly int _requiredLevel;

    public LevelGuard(int requiredLevel)
    {
        _requiredLevel = requiredLevel;
    }

    public string? Guards(ICharacter actor, string[] parameters)
    {
        if (actor.Level < _requiredLevel)
            return $"You need to be at least level {_requiredLevel} to do that.";
        return null;
    }
}

public abstract class BaseCommand<TActor>
    where TActor : IActor
{
    public abstract IGuard<TActor>[] Guards { get; }

    public virtual string? CheckGuards(TActor actor, string[] parameters)
    {
        foreach (var guard in Guards)
        {
            var result = guard.Guards(actor, parameters);
            if (result != null)
                return result;
        }
        return null;
    }
}

public class TestCharacterCommand : BaseCommand<ICharacter>
{
    public override IGuard<ICharacter>[] Guards =>
    [
        new ArgumentNeededGuard(),
        new LevelGuard(5)
    ];

    public override string? CheckGuards(ICharacter actor, string[] parameters)
    {
        return base.CheckGuards(actor, parameters);
    }
}

public class TestActorCommand : BaseCommand<IActor>
{
    public override IGuard<IActor>[] Guards =>
    [
        new ArgumentNeededGuard()
    ];
}