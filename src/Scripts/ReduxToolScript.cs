using Microsoft.AspNetCore.Components.Rendering;
using Skclusive.Core.Component;

namespace Skclusive.Script.DevTools.Redux
{
    public class ReduxToolScript : ScriptBase
    {
        public override string GetScript()
        {
            return @"!function(e){""use strict"";class c{static connect(e,t){try{const n=Math.random().toString(36).substr(2),s=window.__REDUX_DEVTOOLS_EXTENSION__;if(s){const i=s.connect({name:t}),o=i.subscribe(c.callback(n));return c.cache[n]={id:n,target:e,name:t,unsubscribe:o,devTools:i},n}}catch(e){console.error(e)}return-1}static callback(e){return function(t){const{target:n}=c.cache[e];n.invokeMethodAsync(""OnMessageAsync"",JSON.stringify(t))}}static send(e,t,n){const s=c.cache[e];if(s){const e=JSON.parse(n),c=s.devTools;""initial""===t?c.init(e):c.send(JSON.parse(t),e)}}static dispose(e){const t=c.cache[e];t&&t.unsubscribe(),delete c.cache[e]}}var t,n,s;s={},(n=""cache"")in(t=c)?Object.defineProperty(t,n,{value:s,enumerable:!0,configurable:!0,writable:!0}):t[n]=s,window.Skclusive={...window.Skclusive,Script:{...(window.Skclusive||{}).Script,DevTools:{ReduxTool:c}}},e.ReduxTool=c}({});";
        }
    }
}
