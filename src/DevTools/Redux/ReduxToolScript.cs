using Microsoft.AspNetCore.Components.Rendering;
using Skclusive.Core.Component;

namespace Skclusive.Script.DevTools.Redux
{
    public class ReduxToolScript : StaticComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "script");
            builder.AddContent(1,
            #region DevTools.js
           @"
           (function () {
            'use strict';

            function generateId() {
              return Math.random()
                .toString(36)
                .substr(2);
            }

            const connectionPool = {};

            function connect(target, name) {
              try {
                const id = generateId();
                const extension = window.__REDUX_DEVTOOLS_EXTENSION__;
                if (extension) {
                  const devTools = extension.connect({ name: name });
                  const unsubscribe = devTools.subscribe(subscriper(target));
                  connectionPool[id] = { id, target, name, unsubscribe, devTools };
                  return id;
                }
              } catch (ex) {
                console.error(ex);
              }
              return -1;
            }

            function subscriper(target) {
              return function callback(message) {
                  onMessage(message);
              };

              function onMessage(state) {
                target.invokeMethodAsync(
                  'OnMessageAsync',
                  JSON.stringify(state)
                );
              }
            }

            function send(id, action, state) {
              const record = connectionPool[id];
              if (record) {
                const json = JSON.parse(state);
                const devTools = record.devTools;
                if (action === 'initial') {
                  devTools.init(json);
                } else {
                  devTools.send(JSON.parse(action), json);
                }
              }
            }

            function dispose(id) {
              const record = connectionPool[id];
              if (record) {
                record.unsubscribe();
                connectionPool[id] = undefined;
              }
            }

            window.Skclusive = {
              ...window.Skclusive,
              Script: {
                ...((window.Skclusive || {}).Script),
                DevTools: {
                  Redux: {
                    connect,
                    send,
                    dispose
                  }
                }
              }
            };

          }());
           "
            #endregion
            );
            builder.CloseElement();
        }
    }
}
