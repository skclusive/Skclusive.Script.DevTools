using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skclusive.Script.DevTools.Redux;
using Skclusive.Script.DevTools.StateTree;
using Skclusive.Core.Component;
using Skclusive.Text.Json;

namespace Skclusive.Script.DevTools
{
    public static class DevToolsExtension
    {
        public static void TryAddDevToolsServices(this IServiceCollection services, ICoreConfig config)
        {
            services.TryAddCoreServices(config);

            services.TryAddJsonServices();

            services.TryAddScoped(typeof(IReduxTool<,>), typeof(ReduxTool<,>));

            services.TryAddScoped(typeof(IStateTreeTool<>), typeof(StateTreeTool<>));

            services.TryAddScriptTypeProvider<DevToolsScriptProvider>();
        }
    }
}
