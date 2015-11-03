using FDL.FuzzyLogic.Inference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDL.FuzzyLogic.Rules
{
    #region Expressions

    public interface IExpression
    {
        double Evaluate(InferenceContext context);
        string ToText();
    }

    public class And : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public And(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(InferenceContext context)
        {
            var leftResult = _left.Evaluate(context);
            var rightResult = _right.Evaluate(context);
            return context.Options.LogicOps.And(leftResult, rightResult);
        }

        public string ToText()
        {
            return $"({_left.ToText()} AND {_right.ToText()})";
        }
    }

    public class Or : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public Or(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(InferenceContext context)
        {
            var leftResult = _left.Evaluate(context);
            var rightResult = _right.Evaluate(context);
            return context.Options.LogicOps.Or(leftResult, rightResult);
        }

        public string ToText()
        {
            return $"({_left.ToText()} OR {_right.ToText()})";
        }
    }

    public class Is : IExpression
    {
        private readonly string _left;
        private readonly Set _right;

        public Is(string left, Set right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(InferenceContext context)
        {
            double value = context[_left];
            return value == double.NaN ? double.NaN : _right[value];
        }

        public string ToText()
        {
            return $"{_left} IS {_right}";
        }
    }

    public class Not : IExpression
    {
        private readonly IExpression _operand;

        public Not(IExpression operand)
        {
            _operand = operand;
        }

        public double Evaluate(InferenceContext context)
        {
            return context.Options.LogicOps.Not(_operand.Evaluate(context));
        }

        public string ToText()
        {
            return $"NOT {_operand}";
        }
    }

    #endregion Expressions

    #region Rules

    public class Rule
    {
        public readonly IExpression Condition;
        public readonly string Name;
        public readonly Set Then;

        public Rule(string name, IExpression condition, Set then)
        {
            Name = name;
            Then = then;
            Condition = condition;
        }

        public Set Evaluate(InferenceContext context)
        {
            var result = Condition.Evaluate(context);
            var newDic = Then.Values.Where(item => item.Value <= result).ToDictionary(item => item.Key, item => item.Value);
            return new Set(string.Concat(Name, " result set"), newDic);
        }

        public Task<Set> EvaluateAsync(InferenceContext context)
        {
            return Task.Run(() =>
            {
                var result = Condition.Evaluate(context);
                var newDic = Then.Values.Where(item => item.Value <= result).ToDictionary(item => item.Key, item => item.Value);
                return new Set(string.Concat(Name, " result set"), newDic);
            });
        }

        public override string ToString() => $"{Name}: IF {Condition.ToText()} THEN {Then}";
    }

    public class RuleSet
    {
        public List<Rule> Rules;

        public RuleSet(params Rule[] rules)
        {
            Rules = rules.ToList();
        }

        public Set Evaluate(InferenceContext context)
        {
            var resultSet = Set.Empty;
            Parallel.ForEach(Rules, rule =>
            {
                var result = rule.Evaluate(context);
                context.Options.SetOps.Union(resultSet, result);
            });
            return resultSet;
        }

        public Task<Set> EvaluateAsync(InferenceContext context)
        {
            return Task.Run(() =>
            {
                var resultSet = Set.Empty;
                Parallel.ForEach(Rules, rule =>
                {
                    var result = rule.Evaluate(context);
                    context.Options.SetOps.Union(resultSet, result);
                });
                return resultSet;
            });
        }

        public void Add(Rule rule)
        {
            if (ReferenceEquals(rule, null)) return;
            Rules.Add(rule);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var fuzzyRule in Rules)
            {
                sb.AppendLine(fuzzyRule.ToString());
            }
            return sb.ToString();
        }
    }

    #endregion Rules
}
