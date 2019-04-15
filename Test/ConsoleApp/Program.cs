using RandomSolutions;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = new SomeObject
            {
                Title = "test",
                State = State.S1,
            };

            var workflowBuilder = new SomeWorkflowBuilder();
            var workflowFactory = new FsmWorkflowFactory(new[] { workflowBuilder });
            var workflow = workflowFactory.Create<SomeObject, State, Action>(obj);

            foreach (var action in new[] { Action.A1, Action.A2, Action.A1, Action.A3, Action.A1 })
            {
                Console.WriteLine($"{workflow.Current}: {string.Join(", ", workflow.GetEvents())}");
                Console.WriteLine($"Result: {workflow.Trigger(action)}\n");
            }
        }
    }



    enum State { S1, S2, S3 }
    enum Action { A1, A2, A3 }

    class SomeObject
    {
        public string Title { get; set; }
        public State State { get; set; }
    }

    class SomeWorkflowBuilder : IFsmWorkflowBuilder<SomeObject, State, Action>
    {
        public IStateMachine<State, Action> Build(SomeObject obj)
        {
            var fsm = new FsmBuilder<State, Action>(obj.State)
                .OnJump(x => obj.State = x.Fsm.Current)
                .OnReset(x => obj.State = x.Fsm.Current)
                .State(State.S1)
                    .OnEnter(_consoleWrite)
                    .OnExit(_consoleWrite)
                    .On(Action.A1).Execute(x => { Console.WriteLine($"Execute {x.Fsm.Current}>{x.Event}"); return "some data"; })
                    .On(Action.A2).JumpTo(State.S2)
                    .On(Action.A3).JumpTo(State.S3)
                .State(State.S2)
                    .OnEnter(_consoleWrite)
                    .OnExit(_consoleWrite)
                    .On(Action.A1).JumpTo(State.S1)
                .State(State.S3)
                    .OnEnter(_consoleWrite)
                    .OnExit(_consoleWrite)
                .Build();

            return fsm;
        }
        
        static void _consoleWrite(FsmEnterArgs<State, Action> x)
            => Console.WriteLine($"Enter state {x.Fsm.Current} from {x.PrevState}");

        static void _consoleWrite(FsmExitArgs<State, Action> x)
            => Console.WriteLine($"Exit state {x.Fsm.Current} to {x.NextState}");
    }
}
