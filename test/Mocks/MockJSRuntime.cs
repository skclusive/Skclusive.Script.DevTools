using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Skclusive.Script.DevTools.Tests
{
    public class MockJSRuntime : IJSRuntime
    {
        private IDictionary<string, Func<object[], object>> Actions = new Dictionary<string, Func<object[], object>>();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[] args)
        {
            return InvokeAsync<TValue>(identifier, default(CancellationToken), args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
        {
            var action = Actions.ContainsKey(identifier) ? Actions[identifier] : null;

            return new ValueTask<TValue>((TValue)action?.Invoke(args));
        }

        public void AddAction(string identifier, Func<object[], object> action)
        {
            Actions.Add(identifier, action);
        }
    }
}
