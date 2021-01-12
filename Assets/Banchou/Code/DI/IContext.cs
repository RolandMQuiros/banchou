namespace Banchou.DependencyInjection {
    public interface IContext {
        DiContainer InstallBindings(DiContainer container);
    }
}