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

        private object _node { set; get; }

        private IStateTreeNode Node => _node.GetStateTree();

        private IDictionary<int, IActionContext> ActionContexts { set; get; } = new Dictionary<int, IActionContext>();

        private Action ChangesMadeSetter { set; get; }

        private StateTreeConnectOptions Options { set; get; }

        private S InitialState { set; get; }

        private bool ApplyingSnapshot { set; get; }

        private List<(StateTreeToolAction action, S snapshot)> Buffered = new List<(StateTreeToolAction action, S snapshot)>();

        private IDisposable BufferDisposable { set; get; }

        public StateTreeTool(IReduxTool<StateTreeToolAction, S> reduxTool)
        {
            ReduxTool = reduxTool;
        }

        public void Configure(object node, StateTreeConnectOptions options = null)
        {
            _node = node;

            Options = options ?? new StateTreeConnectOptions { LogArgsNearName = true, LogChildActions = false, LogIdempotentActionSteps = true };

            ReduxTool.OnStart += OnStart;

            ReduxTool.OnReset += OnReset;

            ReduxTool.OnCommit += OnCommit;

            ReduxTool.OnState += OnState;

            InitialState = Node.GetSnapshot<S>();

            BufferDisposable = Node.OnAction((call) =>
            {
                var action = new StateTreeToolAction
                {
                    Type = $"{call.Path}/{call.Name}",

                    Args = call.Arguments.ToList()
                };

                var snapshot =  Node.GetSnapshot<S>();

                Buffered.Add((action, snapshot));
            }, true);
        }

        public async Task ConnectAsync()
        {
            await ReduxTool.ConnectAsync(Node.GetType().Name);
        }

        private async Task FlushBuffered()
        {
            foreach (var buffer in Buffered)
            {
                await ReduxTool.SendAsync(buffer.action, buffer.snapshot);
            }

            BufferDisposable.Dispose();

            Buffered.Clear();

            Buffered = null;
        }

        private void OnStart(object sender, EventArgs e)
        {
           _ = OnStartAsync();
        }

        private async Task OnStartAsync()
        {
            await ReduxTool.InitAsync(InitialState);

            await FlushBuffered();

            Node.OnAction((call) =>
            {
                if (ApplyingSnapshot)
                    return;

                var action = new StateTreeToolAction
                {
                    Type = $"{call.Path}/{call.Name}",

                    Args = call.Arguments.ToList()
                };

                _ = ReduxTool.SendAsync(action, Node.GetSnapshot<S>());
            }, true);
        }

        private void OnState(object sender, IReduxMessage<S> message)
        {
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

        private void OnCommit(object sender, EventArgs e)
        {
            _ = ReduxTool.InitAsync(Node.GetSnapshot<S>());
        }

        private void OnReset(object sender, EventArgs e)
        {
            Node.ApplySnapshot(InitialState);

            _ = ReduxTool.InitAsync(InitialState);
        }

        private void ApplySnapshot(S state)
        {
            ApplyingSnapshot = true;

            Node.ApplySnapshot(state);

            ApplyingSnapshot = false;
        }

        public async ValueTask DisposeAsync()
        {
            if (ReduxTool == null)
                return;

            ReduxTool.OnStart -= OnStart;

            ReduxTool.OnReset -= OnReset;

            ReduxTool.OnCommit -= OnCommit;

            ReduxTool.OnState -= OnState;

            await ReduxTool.DisposeAsync();

            ReduxTool = null;
        }
    }
}
