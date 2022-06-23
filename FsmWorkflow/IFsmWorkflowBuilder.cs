namespace RandomSolutions
{
    public interface IFsmWorkflowBuilder<T, TState, TEvent> : IFsmWorkflowBuilder
    {
        IStateMachine<TState, TEvent> Build(T obj);
    }

    public interface IFsmWorkflowBuilder { }
}
