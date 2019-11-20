using Microsoft.Extensions.DependencyInjection;
using Skclusive.Script.DevTools.Redux;
using Skclusive.Script.DevTools.StateTree;

namespace Skclusive.Script.DevTools
{
    public static class DevToolsExtension
    {
        public static void AddDevTools(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IReduxTool<,>), typeof(ReduxTool<,>));

            services.AddSingleton(typeof(IStateTreeTool<>), typeof(StateTreeTool<>));
        }
    }
}
