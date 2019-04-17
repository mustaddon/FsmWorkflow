using System;
using System.Collections.Generic;
using System.Text;

namespace RandomSolutions
{
    class JsonFsmModel
    {
        public string Start { get; set; }
        public string OnJump { get; set; }
        public string OnTrigger { get; set; }
        public string OnFire { get; set; }
        public string OnError { get; set; }
        public string OnReset { get; set; }
        public Dictionary<string, JsonFsmState> States { get; set; }
    }

    class JsonFsmState
    {
        public string Enable { get; set; }
        public string OnEnter { get; set; }
        public string OnExit { get; set; }
        public Dictionary<string, JsonFsmEvent> Events { get; set; }
    }

    class JsonFsmEvent
    {
        public string Enable { get; set; }
        public string Execute { get; set; }
        public string JumpTo { get; set; }
    }
}
