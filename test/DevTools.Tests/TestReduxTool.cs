using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Skclusive.Script.DevTools.Redux;
using Xunit;
using Newtonsoft.Json;
using System.Linq;

namespace Skclusive.Script.DevTools.Tests
{
    internal class TestAction
    {
        public string Type { set; get; }
    }

    internal class TestState
    {
        public string Value { set; get; }

        public int Count { set; get; }
    }

    public class TestReduxTool
    {
        [Fact]
        public async Task TestConnectAsync()
        {
            MockJSRuntime mockJsRuntime = new MockJSRuntime();

            ReduxTool<TestAction, TestState> reduxTool = new ReduxTool<TestAction, TestState>(mockJsRuntime, Enumerable.Empty<JsonConverter>());

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.connect", (args) =>
            {
                Assert.Equal(2, args.Length);

                Assert.Equal(reduxTool, (args[0] as DotNetObjectReference<ReduxTool<TestAction, TestState>>).Value);

                Assert.Equal("Skclusive", args[1]);

                return 1;
            });

            Assert.Equal(ReduxStatus.Idle, reduxTool.Status);

            await reduxTool.ConnectAsync("Skclusive");

            Assert.Equal(ReduxStatus.Requested, reduxTool.Status);

            await Assert.ThrowsAsync<Exception>(() => reduxTool.ConnectAsync("Skclusive"));
        }

        [Fact]
        public async Task TestConnectedAsync()
        {
            MockJSRuntime mockJsRuntime = new MockJSRuntime();

            ReduxTool<TestAction, TestState> reduxTool = new ReduxTool<TestAction, TestState>(mockJsRuntime, Enumerable.Empty<JsonConverter>());

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.connect", (args) =>
            {
                return 1;
            });

            await reduxTool.ConnectAsync("Skclusive");

            await reduxTool.OnMessageAsync(@"
            {
               ""type"": ""START""
            }
            ");

            Assert.Equal(ReduxStatus.Connected, reduxTool.Status);
        }

        [Fact]
        public async Task TestInitAsync()
        {
            MockJSRuntime mockJsRuntime = new MockJSRuntime();

            ReduxTool<TestAction, TestState> reduxTool = new ReduxTool<TestAction, TestState>(mockJsRuntime, Enumerable.Empty<JsonConverter>());

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.connect", (args) =>
            {
                return 1;
            });

            var initialState = new TestState
            {
                Value = "Initial 1",

                Count = 1
            };

            await Assert.ThrowsAsync<Exception>(() => reduxTool.InitAsync(initialState));

            await reduxTool.ConnectAsync("Skclusive");

            await Assert.ThrowsAsync<Exception>(() => reduxTool.InitAsync(initialState));

            await reduxTool.OnMessageAsync(@"
            {
               ""type"": ""START""
            }
            ");

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.send", (args) =>
            {
                Assert.Equal(3, args.Length);

                Assert.Equal(1, args[0]);

                Assert.Equal("initial", args[1]);

                Assert.Equal(@"{""value"":""Initial 1"",""count"":1}", args[2]);

                return null;
            });

            await reduxTool.InitAsync(initialState);
        }


        [Fact]
        public async Task TestSendAsync()
        {
            MockJSRuntime mockJsRuntime = new MockJSRuntime();

            ReduxTool<TestAction, TestState> reduxTool = new ReduxTool<TestAction, TestState>(mockJsRuntime, Enumerable.Empty<JsonConverter>());

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.connect", (args) =>
            {
                return 1;
            });

            await reduxTool.ConnectAsync("Skclusive");

            var initialState = new TestState
            {
                Value = "Initial 1",

                Count = 1
            };

            var addTest = new TestAction { Type = "Test_Add" };

            var newState = new TestState { Value = "New Value 2", Count = 2 };

            await Assert.ThrowsAsync<Exception>(() => reduxTool.SendAsync(addTest, newState));

            await reduxTool.OnMessageAsync(@"
            {
               ""type"": ""START""
            }
            ");

            await reduxTool.InitAsync(initialState);

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.send", (args) =>
            {
                Assert.Equal(3, args.Length);

                Assert.Equal(1, args[0]);

                Assert.Equal(@"{""type"":""Test_Add""}", args[1]);

                Assert.Equal(@"{""value"":""New Value 2"",""count"":2}", args[2]);

                return null;
            });

            await reduxTool.SendAsync(addTest, newState);
        }

        [Fact]
        public async Task TestMessageAsync()
        {
            MockJSRuntime mockJsRuntime = new MockJSRuntime();

            ReduxTool<TestAction, TestState> reduxTool = new ReduxTool<TestAction, TestState>(mockJsRuntime, Enumerable.Empty<JsonConverter>());

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.connect", (args) =>
            {
                return 1;
            });

            await reduxTool.ConnectAsync("Skclusive");

            var initialState = new TestState
            {
                Value = "Initial 1",

                Count = 1
            };

            var addTest = new TestAction { Type = "Test_Add" };

            var newState = new TestState { Value = "New Value 2", Count = 2 };

            await Assert.ThrowsAsync<Exception>(() => reduxTool.SendAsync(addTest, newState));

            await reduxTool.OnMessageAsync(@"
            {
               ""type"": ""START""
            }
            ");

            await reduxTool.InitAsync(initialState);

            mockJsRuntime.AddAction("Skclusive.Script.DevTools.Redux.send", (args) =>
            {
                Assert.Equal(3, args.Length);

                Assert.Equal(1, args[0]);

                Assert.Equal(@"{""type"":""Test_Add""}", args[1]);

                Assert.Equal(@"{""value"":""New Value 2"",""count"":2}", args[2]);

                return null;
            });

            await reduxTool.SendAsync(addTest, newState);

            void OnState(object sender, IReduxMessage<TestState> e)
            {
                Assert.Equal("Old Value 1", e.State.Value);

                Assert.Equal(1, e.State.Count);
            }

            reduxTool.OnState += OnState;

            await reduxTool.OnMessageAsync(@"
            {
                  ""type"": ""DISPATCH"",
                  ""payload"": {
                    ""type"": ""JUMP_TO_ACTION"",
                    ""actionId"": 1
                  },
                  ""state"": ""{\""value\"":\""Old Value 1\"",\""count\"":1}"",
                  ""id"": ""1"",
                  ""source"": ""@devtools-extension""
                }
            ");

            reduxTool.OnState -= OnState;
        }
    }
}
