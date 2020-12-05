using Skclusive.Core.Component;
using Skclusive.Script.DevTools.Redux;

namespace Skclusive.Script.DevTools
{
    public class DevToolsScriptProvider : ScriptTypeProvider
    {
        public DevToolsScriptProvider() : base(priority: 1400, typeof(ReduxToolScript))
        {
        }
    }
}
