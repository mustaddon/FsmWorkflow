using System;
using System.Collections.Generic;
using System.Text;

namespace RandomSolutions
{
    public interface IFsmWorkflowFactory
    {
        IStateMachine<TState, TEvent> Create<T, TState, TEvent>(T obj);
    }
}
