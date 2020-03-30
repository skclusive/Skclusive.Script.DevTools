using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skclusive.Script.DevTools.Redux;
using Skclusive.Script.DevTools.StateTree;

namespace Skclusive.Script.DevTools
{
    public static class DevToolsExtension
    {
        public static void TryAddDevToolsServices(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(IReduxTool<,>), typeof(ReduxTool<,>));

            services.TryAddSingleton(typeof(IStateTreeTool<>), typeof(StateTreeTool<>));
        }
    }
}
