using System;
using System.Collections.Generic;
using System.Text;

namespace RandomSolutions
{
    public interface IFsmWorkflowBuilder<T, TState, TEvent> : IFsmWorkflowBuilder
    {
        IStateMachine<TState, TEvent> Build(T obj);
    }

    public interface IFsmWorkflowBuilder { }
}
