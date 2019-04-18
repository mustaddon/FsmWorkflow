# FsmWorkflow [![NuGet version](https://badge.fury.io/nu/RandomSolutions.FsmWorkflow.svg)](http://badge.fury.io/nu/RandomSolutions.FsmWorkflow)
Finite-state machine (FSM) based workflow

## Example

*Code based workflow declaration*
```C#
public enum State { S1, S2, S3 }
public enum Action { A1, A2, A3 }

public class SampleObject
{
    public string Title { get; set; }
    public State State { get; set; }
}

public class SampleWorkflow : IFsmWorkflowBuilder<SampleObject, State, Action>
{
    public IStateMachine<State, Action> Build(SampleObject obj)
    {
        return new FsmBuilder<State, Action>(obj.State)
            .OnJump(x =>
            {
                obj.State = x.Fsm.Current;
                Console.WriteLine($"State changed to {obj.State} from {x.PrevState}");
            })
            .State(State.S1)
                .On(Action.A1).Execute(x => { /* some operations */ return obj.Title; })
                .On(Action.A2).JumpTo(State.S2)
            .State(State.S2)
                .On(Action.A3).Enable(x => /* some conditions */ true).JumpTo(State.S3)
            .State(State.S3)
                .OnEnter(x => Console.WriteLine($"Enter to final state"))
            .Build();
    }
}
```

```C#
var sampleObject = new SampleObject { Title = "Test", State = State.S1  };
var workflowBuilder = new SampleWorkflow();
var workflow = workflowBuilder.Build(sampleObject);

workflow.Trigger(Action.A2);
workflow.Trigger(Action.A3);


// Console output:
// State change to S2 from S1
// State change to S3 from S2
// Enter to final state
```


*JSON based workflow declaration*
```json
{
  "start": "_obj.State",
  "onJump": "_obj.State = _args.Fsm.Current; Console.WriteLine($\"State changed to {_obj.State} from {_args.PrevState}\")",

  "states": {
    "S1": {
      "events": {
        "A1": { "execute": "/* some operations */ return _obj.Title" },
        "A2": { "jumpTo": "ConsoleApp.State.S2" }
      }
    },
    "S2": {
      "events": {
        "A3": { "jumpTo": "ConsoleApp.State.S3" }
      }
    },
    "S3": {
      "onEnter": "Console.WriteLine(\"Enter to final state\")"
    }
  }
}
```

```C#
var sampleObject = new SampleObject { Title = "Test", State = State.S1  };
var workflowBuilder = new FsmWorkflowJson<SampleObject,State,Action>(File.ReadAllText(@"SampleWorkflow.json"));
var workflow = workflowBuilder.Build(sampleObject);

workflow.Trigger(Action.A2);
workflow.Trigger(Action.A3);


// Console output:
// State change to S2 from S1
// State change to S3 from S2
// Enter to final state
```
