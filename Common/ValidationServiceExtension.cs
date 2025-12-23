using FluentValidation;
using System.Reflection;

namespace Profakt.Intranet.Common
{
    public static class ValidationServiceExtensions
    {
        public static IServiceCollection AddRequestValidation(this IServiceCollection services, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            return services;
        }
    }
}
