namespace VKSaver.Core.Services.Interfaces
{
    public interface IServiceResolver
    {
        T Resolve<T>() where T: class;
        T Resolve<T>(string name) where T: class;
    }
}
