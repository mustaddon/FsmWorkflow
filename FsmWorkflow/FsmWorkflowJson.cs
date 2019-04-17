using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RandomSolutions
{
    public class FsmWorkflowJson<TObj, TState, TEvent> : IFsmWorkflowBuilder<TObj, TState, TEvent>
    {
        protected readonly string _json;
        protected readonly IEnumerable<Assembly> _refs;
        static string _assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        public FsmWorkflowJson(string json, IEnumerable<Assembly> refs = null)
        {
            _json = json;
            _refs = (refs ?? new Assembly[0]).Concat(new[] {
                Assembly.GetEntryAssembly(),
                Assembly.GetCallingAssembly(),
                Assembly.GetExecutingAssembly(),
                typeof(TObj).Assembly,
                typeof(TState).Assembly,
                typeof(TEvent).Assembly,
            });
        }

        public virtual IStateMachine<TState, TEvent> Build(TObj obj)
        {
            return new FsmBuilder<TState, TEvent>(BuildModel(obj)).Build();
        }

        protected virtual FsmModel<TState, TEvent> BuildModel(TObj obj)
        {
            return DeserializeModel(CreateArgs(obj));
        }

        protected virtual object CreateArgs(TObj obj)
        {
            return new
            {
                _obj = obj,
                _this = this,
            };
        }

        protected FsmModel<TState, TEvent> DeserializeModel(object args)
        {
            var jsonModel = JsonConvert.DeserializeObject<JsonFsmModel>(_json);

            var model = new FsmModel<TState, TEvent>();

            if (jsonModel.Start != null)
                model.Start = CSharpEval.Execute<TState>(jsonModel.Start, args, _refs);

            if (jsonModel.OnJump != null)
                model.OnJump = x => CSharpEval.Execute(jsonModel.OnJump, _concatArgs(args, new { _args = x }), _refs);

            if (jsonModel.OnReset != null)
                model.OnReset = x => CSharpEval.Execute(jsonModel.OnReset, _concatArgs(args, new { _args = x }), _refs);

            if (jsonModel.OnTrigger != null)
                model.OnTrigger = x => CSharpEval.Execute(jsonModel.OnTrigger, _concatArgs(args, new { _args = x }), _refs);

            if (jsonModel.OnFire != null)
                model.OnFire = x => CSharpEval.Execute(jsonModel.OnFire, _concatArgs(args, new { _args = x }), _refs);

            if (jsonModel.OnError != null)
                model.OnError = x => CSharpEval.Execute(jsonModel.OnError, _concatArgs(args, new { _args = x }), _refs);

            if (jsonModel.States != null)
                foreach (var sKvp in jsonModel.States)
                {
                    var sModel = new FsmStateModel<TState, TEvent>();

                    if (sKvp.Value.Enable != null)
                        sModel.Enable = x => CSharpEval.Execute<bool>(sKvp.Value.Enable, _concatArgs(args, new { _args = x }), _refs);

                    if (sKvp.Value.OnEnter != null)
                        sModel.OnEnter = x => CSharpEval.Execute(sKvp.Value.OnEnter, _concatArgs(args, new { _args = x }), _refs);

                    if (sKvp.Value.OnExit != null)
                        sModel.OnExit = x => CSharpEval.Execute(sKvp.Value.OnExit, _concatArgs(args, new { _args = x }), _refs);

                    if (sKvp.Value.Events != null)
                        foreach (var eKvp in sKvp.Value.Events)
                        {
                            var eModel = new FsmEventModel<TState, TEvent>();

                            if (eKvp.Value.Enable != null)
                                eModel.Enable = x => CSharpEval.Execute<bool>(eKvp.Value.Enable, _concatArgs(args, new { _args = x }), _refs);

                            if (eKvp.Value.Execute != null)
                                eModel.Execute = x => CSharpEval.Execute<object>(eKvp.Value.Execute, _concatArgs(args, new { _args = x }), _refs);

                            if (eKvp.Value.JumpTo != null)
                                eModel.JumpTo = x => CSharpEval.Execute<TState>(eKvp.Value.JumpTo, _concatArgs(args, new { _args = x }), _refs);

                            sModel.Events.Add(_convertTo<TEvent>(eKvp.Key), eModel);
                        }

                    model.States.Add(_convertTo<TState>(sKvp.Key), sModel);
                }

            return model;
        }


        static Dictionary<string, Tuple<Type, object>> _concatArgs(params object[] args)
        {
            var result = new Dictionary<string, Tuple<Type, object>>();

            foreach (var x in args)
                foreach (var kvp in _getArgs(x))
                    if (result.ContainsKey(kvp.Key))
                        result[kvp.Key] = kvp.Value;
                    else
                        result.Add(kvp.Key, kvp.Value);

            return result;
        }

        static Dictionary<string, Tuple<Type, object>> _getArgs(object args)
        {
            return args.GetType().GetProperties().Where(x => x.CanRead)
                .ToDictionary(x => x.Name, x =>
                {
                    var val = x.GetValue(args);
                    return new Tuple<Type, object>(val?.GetType() ?? x.PropertyType, val);
                });
        }

        static bool _isEnum(Type type)
        {
            return type.IsEnum || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments().FirstOrDefault()?.IsEnum == true);
        }

        static T _convertTo<T>(string val)
        {
            var type = typeof(T);

            if (_isEnum(type))
                return (T)Enum.Parse(type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() : type, val, true);

            return (T)TypeDescriptor.GetConverter(type).ConvertFrom(val);
        }
    }
}
