namespace RandomSolutions
{
    public interface IFsmWorkflowFactory
    {
        IStateMachine<TState, TEvent> Create<T, TState, TEvent>(T obj);
    }
}
