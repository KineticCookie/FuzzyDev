using FDL.FuzzyLogic.Operations;
using FDL.FuzzyLogic.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDL.FuzzyLogic.Inference
{
    public class InferenceContext
    {
        private Dictionary<string, double> _values;
        public InferenceEvaluationOptions Options { get; private set; }

        public InferenceContext(IEnumerable<string> values, InferenceEvaluationOptions options)
        {
            _values = new Dictionary<string, double>();
            foreach (var input in values)
            {
                _values.Add(input, double.NaN);
            }
            Options = options;
        }

        public double this[string element]
        {
            get
            {
                double value;
                return _values.TryGetValue(element, out value) ? value : double.NaN;
            }
            set
            {
                if (_values.ContainsKey(element))
                {
                    _values[element] = value;
                }
            }
        }
    }

    public class InferenceEvaluationOptions
    {
        public DefuzzificatorDelegate Defuzzificator { get; private set; }
        public ISetOperators SetOps { get; private set; }
        public ILogicOperators LogicOps { get; private set; }

        public InferenceEvaluationOptions(DefuzzificatorDelegate defuzz, ISetOperators setOps, ILogicOperators logicOps)
        {
            Defuzzificator = defuzz;
            SetOps = setOps;
            LogicOps = logicOps;
        }
    }

    public class InferenceMachine
    {
        public RuleSet RuleSet { get; private set; }
        public List<UniversalSet> Universes { get; private set; }
        public InferenceContext Context { get; private set; }
        public double Decision { get; private set; }

        public InferenceMachine(InferenceContext context, IEnumerable<UniversalSet> universes,
            RuleSet ruleSet)
        {
            Universes = new List<UniversalSet>(universes);
            RuleSet = ruleSet;
            Context = context;
        }

        public double this[string inputName]
        {
            get
            {
                return Context[inputName];
            }
            set
            {
                Context[inputName] = value;
                var answer = RuleSet.Evaluate(Context);
                Decision = Context.Options.Defuzzificator(answer);
            }
        }

    }

}
