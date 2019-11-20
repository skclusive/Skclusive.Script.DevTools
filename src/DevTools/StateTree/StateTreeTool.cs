using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Skclusive.Mobx.StateTree;
using Skclusive.Script.DevTools.Redux;

namespace Skclusive.Script.DevTools.StateTree
{
    public class StateTreeTool<S> : IStateTreeTool<S> where S : class
    {
        public static string GetTypeName(IStateTreeNode node)
        {
            return node.GetType().Name ?? "(UnnamedType)";
        }

        public static List<string> GetTargetTypePath(IStateTreeNode node)
        {
            var names = new List<string>();
            IStateTreeNode current = node;
            while (current != null)
            {
                names.Insert(0, GetTypeName(current));
                current = current.HasParent() ? current.GetParent<IStateTreeNode>() : null;
            }
            return names;
        }

        private IReduxTool<StateTreeToolAction, S> ReduxTool { get; set; }

        private int HandlingMonitorAction { set; get; }

        private object _node {set; get;}

        private IStateTreeNode Node  => _node.GetStateTree();

        private IDictionary<int, IActionContext> ActionContexts { set; get; } = new Dictionary<int, IActionContext>();

        private Action ChangesMadeSetter { set; get; }

        private StateTreeConnectOptions Options { set; get; }

        private S InitialState { set; get; }

        private bool ApplyingSnapshot { set; get; }

        public StateTreeTool(IReduxTool<StateTreeToolAction, S> reduxTool)
        {
            ReduxTool = reduxTool;
        }

        public Task ConnectAsync(object node)
        {
            return ConnectAsync(node, new StateTreeConnectOptions { LogArgsNearName = true, LogChildActions = false, LogIdempotentActionSteps = true });
        }

        public async Task ConnectAsync(object node, StateTreeConnectOptions options)
        {
            _node = node;

            Options = options;

            ReduxTool.OnStart += OnStart;

            ReduxTool.OnReset += OnReset;

            ReduxTool.OnCommit += OnCommit;

            ReduxTool.OnState += OnState;

            await ReduxTool.ConnectAsync(Node.GetType().Name);

            InitialState = Node.GetSnapshot<S>();

            Node.OnAction((call) =>
            {
                if (ApplyingSnapshot)
                    return;

                var action = new StateTreeToolAction
                {
                    Type = $"{call.Path}/{call.Name}",

                    Args = call.Arguments.ToList()
                };

                ReduxTool.SendAsync(action, Node.GetSnapshot<S>());
            }, true);
        }

        private void OnStart(object sender, EventArgs e)
        {
            _ = ReduxTool.InitAsync(InitialState);
        }

        private void OnState(object sender, IReduxMessage<S> message)
        {
            try
            {
                HandlingMonitorAction++;

                switch (message.Payload.Type)
                {
                    case "ROLLBACK":
                    {
                       _ = ReduxTool.InitAsync(message.State);
                       break;
                    }
                    case "JUMP_TO_STATE":
                    case "JUMP_TO_ACTION":
                    {
                        ApplySnapshot(message.State);

                        break;
                    }
                    case "IMPORT_STATE":
                    {
                        var nextLiftedState = message.Payload.NextLiftedState;

                        var computedStates = nextLiftedState.ComputedStates;

                        ApplySnapshot(computedStates[computedStates.Count - 1].State);

                        _ = ReduxTool.InitAsync(message.State);
                        break;
                    }
                }
            }
            finally
            {
                HandlingMonitorAction--;
            }
        }

        private void OnCommit(object sender, EventArgs e)
        {
            try
            {
                HandlingMonitorAction++;

                _ = ReduxTool.InitAsync(Node.GetSnapshot<S>());
            }
            finally
            {
                HandlingMonitorAction--;
            }
        }

        private void OnReset(object sender, EventArgs e)
        {
            try
            {
                HandlingMonitorAction++;

                Node.ApplySnapshot(InitialState);

                _ = ReduxTool.InitAsync(InitialState);
            }
            finally
            {
                HandlingMonitorAction--;
            }
        }

        private void ApplySnapshot(S state)
        {
            ApplyingSnapshot = true;

            Node.ApplySnapshot(state);

            ApplyingSnapshot = false;
        }

        public void Dispose()
        {
            if (ReduxTool == null)
            throw new Exception("ReduxTool is not available or disposed");

            ReduxTool.OnStart -= OnStart;

            ReduxTool.OnReset -= OnReset;

            ReduxTool.OnCommit -= OnCommit;

            ReduxTool.OnState -= OnState;

            ReduxTool.Dispose();

            ReduxTool = null;
        }
    }
}