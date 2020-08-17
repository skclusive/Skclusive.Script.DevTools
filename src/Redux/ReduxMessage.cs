using System.Collections.Generic;
using System.Linq;

namespace Skclusive.Script.DevTools.Redux
{
    public class ComputedState<S> : IComputedState<S> where S : class
    {
       internal S State { get; set; }

        S IComputedState<S>.State => State;
    }

    public class NextLiftedState<S> : INextLiftedState<S> where S : class
    {
        internal List<ComputedState<S>> ComputedStates { get; set; }

        IList<IComputedState<S>> INextLiftedState<S>.ComputedStates => ComputedStates.Select(
        s => new ComputedState<S>
        {
            State = s.State
        } as IComputedState<S>).ToList();
    }

    public class ReduxPayload<S> : IReduxPayload<S> where S : class
    {
       public string Type { get; set; }

       public int ActionId { get; set; }

       public int Index { get; set; }

       public NextLiftedState<S> NextLiftedState { set; get; }

       INextLiftedState<S> IReduxPayload<S>.NextLiftedState => NextLiftedState;
    }

    public class ReduxMessage<S> : IReduxMessage<S> where S : class
    {
        public string Type { get; set; }

        public int Id { get; set; }

        public string Source { get; set; }

        public ReduxPayload<S> Payload { get; set; }

        public S State { get; set; }

        IReduxPayload<S> IReduxMessage<S>.Payload => Payload;
    }

    public class ReduxToggleState<T, S> : IReduxToggleState<T, S> where T : class where S : class
    {
        public Dictionary<int, T> ActionsById { set; get; }

        public IList<S> ComputedStates { set; get; }

        public int CurrentStateIndex { set; get; }

        public int NextActionId { set; get; }

        public IList<int> SkippedActionIds { set; get; }

        public IList<int> StaggedActionIds { set; get; }

        IDictionary<int, T> IReduxToggleState<T, S>.ActionsById => ActionsById;

        IList<S> IReduxToggleState<T, S>.ComputedStates => ComputedStates;

        int IReduxToggleState<T, S>.CurrentStateIndex => CurrentStateIndex;

        int IReduxToggleState<T, S>.NextActionId => NextActionId;

        IList<int> IReduxToggleState<T, S>.SkippedActionIds => SkippedActionIds;

        IList<int> IReduxToggleState<T, S>.StaggedActionIds => StaggedActionIds;
    }
}