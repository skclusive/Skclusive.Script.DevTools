using System.Collections.Generic;

namespace Skclusive.Script.DevTools.Redux
{
    public interface IComputedState<S> where S : class
    {
        S State { get; }
    }

    public interface INextLiftedState<S> where S : class
    {
        IList<IComputedState<S>> ComputedStates { get;  }
    }

    public interface IReduxPayload<S> where S : class
    {
        string Type { get; }

        int ActionId { get; }

        int Index { get; }

        INextLiftedState<S> NextLiftedState { get; }
    }

    public interface IReduxMessage<S> where S : class
    {
        string Type { get; }

        IReduxPayload<S> Payload { get; }

        int Id { get; }

        string Source { get; }

        S State { get; }
    }

    public interface IReduxToggleState<T, S> where T : class where S : class
    {
        IDictionary<int, T> ActionsById { get; }

        IList<S> ComputedStates { get; }

        int CurrentStateIndex { get; }

        int NextActionId { get; }

        IList<int> SkippedActionIds { get; }

        IList<int> StaggedActionIds { get; }
    }
}