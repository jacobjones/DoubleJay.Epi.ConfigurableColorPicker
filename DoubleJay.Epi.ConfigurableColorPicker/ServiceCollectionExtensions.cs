using System;
using System.Linq;
using DoubleJay.Epi.ConfigurableColorPicker.Manager;
using DoubleJay.Epi.ConfigurableColorPicker.Manager.Caching;
using EPiServer.Shell.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace DoubleJay.Epi.ConfigurableColorPicker
{
    public static class ServiceCollectionExtensions
    {
        private const string ModuleName = "DoubleJay.Epi.ConfigurableColorPicker";
        
        public static IServiceCollection AddConfigurableColorPicker(this IServiceCollection services)
        {
            services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals(ModuleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new ModuleDetails {Name = ModuleName});
                    }
                });

            services.AddSingleton<IColorPaletteManager, ColorPaletteManagerCachingProxy>();

            return services;
        }
    }
}
