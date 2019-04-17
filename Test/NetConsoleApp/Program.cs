using RandomSolutions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var someObject = new SomeObject { Title = "some title", State = State.S1, };
            var json = File.ReadAllText(Path.GetFullPath(@"../../../ConsoleApp/test.json"));

            //var workflowBuilder = new SomeWorkflowBuilder();
            //var workflowBuilder = new FsmWorkflowJson<SomeObject, State, Action>(json);
            var workflowBuilder = new SomeWorkflowJson(json);

            var workflowFactory = new FsmWorkflowFactory(new[] { workflowBuilder });
            var workflow = workflowFactory.Create<SomeObject, State, Action>(someObject);

            foreach (var action in new[] { Action.A1, Action.A2, Action.A1, Action.A3, Action.A1 })
            {
                Console.WriteLine($"{workflow.Current} actions: {string.Join(", ", workflow.GetEvents())}");
                Console.WriteLine($"Result: {workflow.Trigger(action)}\n");
            }
        }
    }



    public enum State { S1, S2, S3 }
    public enum Action { A1, A2, A3 }

    public class SomeObject
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
                    .On(Action.A1).Execute(x => { Console.WriteLine($"Execute {x.Fsm.Current}>{x.Event}"); return obj.Title; })
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


    public class SomeWorkflowJson : FsmWorkflowJson<SomeObject, State, Action>
    {
        public SomeWorkflowJson(string json)
            : base(json)
        {

        }

        public void Test() => Console.WriteLine("test");

        public void CustomWrite(FsmEnterArgs<State, Action> x)
            => Console.WriteLine($"Enter state {x.Fsm.Current} from {x.PrevState}");

        public void CustomWrite(FsmExitArgs<State, Action> x)
            => Console.WriteLine($"Exit state {x.Fsm.Current} to {x.NextState}");
    }
}
