using VContainer;

namespace Provider
{
    internal static class ObjectResolverExtensions
    {
        public static RegistrationBuilder RegisterPlainEntryPoint<T>(this IContainerBuilder builder,
            Lifetime lifetime = Lifetime.Singleton)
        {
            builder.RegisterBuildCallback(objectResolver => objectResolver.Resolve<T>());
            return builder.Register<T>(lifetime);
        }
    }
}