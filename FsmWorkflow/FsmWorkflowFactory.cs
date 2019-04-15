using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RandomSolutions
{
    public class FsmWorkflowFactory : IFsmWorkflowFactory
    {
        public FsmWorkflowFactory(IEnumerable<IFsmWorkflowBuilder> builders)
        {
            _builders = (builders ?? throw new ArgumentNullException(nameof(builders)))
                .GroupBy(x => _getKey(x))
                .ToDictionary(g => g.Key, g =>
                {
                    var workflow = g.First();
                    return new Tuple<IFsmWorkflowBuilder, Lazy<MethodInfo>>(workflow,
                        new Lazy<MethodInfo>(() => workflow.GetType().GetMethods().First(x => x.Name == _buildMethodName)));
                });
        }

        public IStateMachine<TState, TEvent> Create<T, TState, TEvent>(T obj)
        {
            var types = new[] { typeof(T), typeof(TState), typeof(TEvent) };
            var key = _getKey(types);

            if (!_builders.ContainsKey(key))
                throw new Exception(string.Format(_builderNotFound, string.Join(",", types.Select(x => x.Name))));

            var builder = _builders[key];
            return builder.Item2.Value.Invoke(builder.Item1, new object[] { obj }) as IStateMachine<TState, TEvent>;
        }

        readonly Dictionary<string, Tuple<IFsmWorkflowBuilder, Lazy<MethodInfo>>> _builders;

        static string _getKey(IFsmWorkflowBuilder workflow)
        {
            var i = workflow.GetType().GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IFsmWorkflowBuilder<,,>));

            return _getKey(i.GenericTypeArguments);
        }

        static string _getKey(params Type[] types) => string.Join("|", types.Select(x => x.FullName));

        const string _buildMethodName = nameof(IFsmWorkflowBuilder<object, object, object>.Build);
        const string _builderNotFound = "FsmWorkflowBuilder<{0}> not found";
    }
}
