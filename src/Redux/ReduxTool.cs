using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Skclusive.Core.Component;

namespace Skclusive.Script.DevTools.Redux
{
    public class ReduxTool<T, S> : IReduxTool<T, S> where T : class where S : class
    {
        public ReduxTool(IScriptService scriptService, IEnumerable<JsonConverter> converters)
        {
            ScriptService = scriptService;

            SerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

                // TypeNameHandling = TypeNameHandling.All
            };
            foreach(var converter in converters)
            {
                SerializerSettings.Converters.Add(converter);
            }
        }

        private object Id;

        public ReduxStatus Status { get; private set; } = ReduxStatus.Idle;

        private IScriptService ScriptService { get; }

        public event EventHandler OnStart;

        public event EventHandler OnReset;

        public event EventHandler OnCommit;

        public event EventHandler<string> OnMessage;

        public event EventHandler<IReduxMessage<S>> OnState;

        public event EventHandler<IReduxMessage<IReduxToggleState<T, S>>> OnToggle;

        private readonly static EventArgs EVENT_ARGS = new EventArgs();

        private JsonSerializerSettings SerializerSettings { get; }

        [JSInvokable]
        public Task OnMessageAsync(string json)
        {
            if (Status == ReduxStatus.Requested || Status == ReduxStatus.Connected)
            {
                HandleMessage(json);
            }

            return Task.CompletedTask;
        }

        private void HandleMessage(string json)
        {
            IReduxMessage<string> message = Deserialize<ReduxMessage<string>>(json);

            if (message.Type == "START")
            {
                Status = ReduxStatus.Connected;

                var onStart = OnStart;

                onStart?.Invoke(this, EVENT_ARGS);

                return;
            }

            if (Status != ReduxStatus.Connected)
            {
                // ignoring messages if not connected
                return;
            }

            var onPush = OnMessage;

            onPush?.Invoke(this, json);

            switch (message.Payload?.Type)
            {
                case "RESET":
                {
                    var onReset = OnReset;

                    onReset?.Invoke(this, EVENT_ARGS);

                    break;
                }
                case "COMMIT":
                {
                    var onCommit = OnCommit;

                    onCommit?.Invoke(this, EVENT_ARGS);

                    break;
                }
                case "IMPORT_STATE":
                case "ROLLBACK":
                case "JUMP_TO_STATE":
                case "JUMP_TO_ACTION":
                {
                    var computedStates = message.Payload.NextLiftedState?.ComputedStates.Select(s => new ComputedState<S> { State = Deserialize<S>(s.State) }).ToList();

                    var state = Deserialize<S>(message.State);

                    var onState = OnState;

                    onState?.Invoke(this, new ReduxMessage<S>
                    {
                        Payload = message.Payload != null ? new ReduxPayload<S>
                        {
                            ActionId = message.Payload.ActionId,

                            Type = message.Payload.Type,

                            Index = message.Payload.Index,

                            NextLiftedState = new NextLiftedState<S>
                            {
                                ComputedStates = computedStates
                            }
                        } : null,

                        Id = message.Id,

                        Type = message.Type,

                        Source = message.Source,

                        State = state
                    });

                    break;
                }
                case "TOGGLE_ACTION":
                {
                    var state = Deserialize<ReduxToggleState<T, S>>(message.State);

                    var onToggle = OnToggle;

                    onToggle?.Invoke(this, new ReduxMessage<IReduxToggleState<T, S>>
                    {
                        Payload = message.Payload != null ? new ReduxPayload<IReduxToggleState<T, S>>
                        {
                            ActionId = message.Payload.ActionId,

                            Type = message.Payload.Type,

                            Index = message.Payload.Index
                        } : null,

                        Id = message.Id,

                        Type = message.Type,

                        Source = message.Source,

                        State = new ReduxToggleState<T, S>
                        {
                            SkippedActionIds = state.SkippedActionIds,

                            StaggedActionIds = state.StaggedActionIds,

                            CurrentStateIndex = state.CurrentStateIndex,

                            NextActionId = state.NextActionId,

                            ActionsById = new Dictionary<int, T>(state.ActionsById),

                            ComputedStates = new List<S>(state.ComputedStates)
                        }
                    });

                    break;
                }
            }
        }

        public async Task ConnectAsync(string name)
        {
            if (Status != ReduxStatus.Idle)
            {
                throw new Exception("Can not connect as the tool is not idle");
            }

            Status = ReduxStatus.Requested;

            Id = await ScriptService.InvokeAsync<object>("Skclusive.Script.DevTools.Redux.connect", DotNetObjectReference.Create(this), name);
        }

        private async Task SendAsync(string action, string state)
        {
            await ScriptService.InvokeVoidAsync("Skclusive.Script.DevTools.Redux.send", Id, action, state);
        }

        private string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

        private X Deserialize<X>(string json)
        {
            return JsonConvert.DeserializeObject<X>(json, SerializerSettings);
        }

        public Task SendAsync(T action, S state)
        {
            ThrowIfNotConnected();

            return SendAsync(Serialize(action), Serialize(state));
        }

        public Task InitAsync(S state)
        {
            ThrowIfNotConnected();

            return SendAsync("initial", Serialize(state));
        }

        private void ThrowIfNotConnected()
        {
            if (Status != ReduxStatus.Connected)
            {
                throw new Exception("Can not perform the action as the tool is not connected");
            }
        }

        public async ValueTask DisposeAsync()
        {
            Status = ReduxStatus.Disposed;

            if (Status == ReduxStatus.Connected)
            {
                await ScriptService.InvokeVoidAsync("Skclusive.Script.DevTools.Redux.dispose", Id);
            }
        }
    }
}
