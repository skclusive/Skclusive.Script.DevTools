using System;
using System.Collections.Generic;

namespace Skclusive.Script.DevTools.StateTree
{
    public interface IActionContext
    {
        IActionContext Parent { set; get; }

        string Name { set; get; }

        string TargetTypePath { set; get; }

        int Id { set; get; }

        bool RunningAsync { set; get; }

        bool Errored { set; get; }

        bool ErrorReported { set; get; }

        int Step { set; get; }

        List<object> CallArgs { set; get; }

        Action ChangesMadeSetter { set; get; }
    }
}


