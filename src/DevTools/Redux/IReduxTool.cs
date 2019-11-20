using System;
using System.Threading.Tasks;

namespace Skclusive.Script.DevTools.Redux
{
    public enum ReduxStatus
    {
        Idle,

        Requested,

        Connected,

        Disposed
    }

    public interface IReduxTool<T, S> : IDisposable where T : class where S : class
    {
        ReduxStatus Status { get; }

        event EventHandler OnStart;

        event EventHandler OnReset;

        event EventHandler OnCommit;

        event EventHandler<string> OnMessage;

        event EventHandler<IReduxMessage<S>> OnState;

        event EventHandler<IReduxMessage<IReduxToggleState<T, S>>> OnToggle;

        Task ConnectAsync(string name);

        Task InitAsync(S state);

        Task SendAsync(T action, S state);
    }
}