using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skclusive.Mobx.StateTree;

namespace Skclusive.Script.DevTools.StateTree
{
    public class StateTreeToolAction
    {
        public string Type { set; get; }

        public List<object> Args { set; get; }
    }

    public class StateTreeConnectOptions
    {
        public bool LogIdempotentActionSteps { set; get; }

        public bool LogChildActions { set; get; }

        public bool LogArgsNearName { set; get; }
    }

    public interface IStateTreeTool<S> : IDisposable where S : class
    {
        Task ConnectAsync(object node);

        Task ConnectAsync(object node, StateTreeConnectOptions options);

        // Task ConnectAsync(IStateTreeNode node);

        // Task ConnectAsync(IStateTreeNode node, StateTreeConnectOptions options);
    }
}