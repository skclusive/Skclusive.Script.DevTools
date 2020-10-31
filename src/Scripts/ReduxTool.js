// @ts-check

function generateId() {
  return Math.random().toString(36).substr(2);
}

export class ReduxTool {
  static cache = {};

  static connect(target, name) {
    try {
      const id = generateId();
      // @ts-ignore
      const extension = window.__REDUX_DEVTOOLS_EXTENSION__;
      if (extension) {
        const devTools = extension.connect({ name: name });
        const unsubscribe = devTools.subscribe(ReduxTool.callback(id));
        ReduxTool.cache[id] = {
          id,
          target,
          name,
          unsubscribe,
          devTools,
        };
        return id;
      }
    } catch (ex) {
      console.error(ex);
    }
    return -1;
  }

  static callback(id) {
    return function callback(message) {
      const { target } = ReduxTool.cache[id];
      target.invokeMethodAsync("OnMessageAsync", JSON.stringify(message));
    };
  }

  static send(id, action, state) {
    const record = ReduxTool.cache[id];
    if (record) {
      const json = JSON.parse(state);
      const devTools = record.devTools;
      if (action === "initial") {
        devTools.init(json);
      } else {
        devTools.send(JSON.parse(action), json);
      }
    }
  }

  static dispose(id) {
    const record = ReduxTool.cache[id];
    if (record) {
      record.unsubscribe();
    }
    delete ReduxTool.cache[id];
  }
}

// @ts-ignore
window.Skclusive = {
  // @ts-ignore
  ...window.Skclusive,
  Script: {
    // @ts-ignore
    ...(window.Skclusive || {}).Script,
    DevTools: {
      ReduxTool,
    },
  },
};
