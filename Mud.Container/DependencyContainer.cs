namespace Mud.Container;

public class DependencyContainer
{
    private static readonly Lazy<SimpleInjector.Container> Lazy = new(() => new SimpleInjector.Container());

    public static SimpleInjector.Container Current => _manual ?? Lazy.Value;

    // TODO: remove this when every class will have ctor with used interface
    private static SimpleInjector.Container _manual = default!;
    public static void SetManualContainer(SimpleInjector.Container container)
    {
        _manual = container;
    }
}
