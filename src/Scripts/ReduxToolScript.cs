using Microsoft.AspNetCore.Components.Rendering;
using Skclusive.Core.Component;

namespace Skclusive.Script.DevTools.Redux
{
    public class ReduxToolScript : ScriptBase
    {
        public override string GetScript()
        {
            return @"!function(){""use strict"";const n={};window.Skclusive={...window.Skclusive,Script:{...(window.Skclusive||{}).Script,DevTools:{Redux:{connect:function(i,o){try{const s=Math.random().toString(36).substr(2),t=window.__REDUX_DEVTOOLS_EXTENSION__;if(t){const c=t.connect({name:o}),e=c.subscribe(function(n){return function(n){i(n)};function i(i){n.invokeMethodAsync(""OnMessageAsync"",JSON.stringify(i))}}(i));return n[s]={id:s,target:i,name:o,unsubscribe:e,devTools:c},s}}catch(n){console.error(n)}return-1},send:function(i,o,s){const t=n[i];if(t){const n=JSON.parse(s),i=t.devTools;""initial""===o?i.init(n):i.send(JSON.parse(o),n)}},dispose:function(i){const o=n[i];o&&(o.unsubscribe(),n[i]=void 0)}}}}}}();";
        }
    }
}
