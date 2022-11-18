using System;
using System.Linq;
using DoubleJay.Epi.ConfigurableColorPicker;
using DoubleJay.Epi.ConfigurableColorPicker.Manager;
using DoubleJay.Epi.ConfigurableColorPicker.Manager.Caching;
using EPiServer.Shell.Modules;

// ReSharper disable once CheckNamespace -> Intended.
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDoubleJayColorPicker(this IServiceCollection services)
        {
            var moduleName = typeof(PropertyPaletteColor).Namespace ?? "DoubleJay.Epi.ConfigurableColorPicker";

            return services.AddSingleton<IColorPaletteManager, ColorPaletteManagerCachingProxy>()
                .Configure<ProtectedModuleOptions>(options =>
                {
                    if (!options.Items.Any(moduleDetails => moduleDetails.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase)))
                        options.Items.Add(new ModuleDetails { Name = moduleName });
                });
        }
    }
}
