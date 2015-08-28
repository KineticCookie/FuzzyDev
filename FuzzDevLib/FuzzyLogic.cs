using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FDL.Utility;

namespace FDL.FuzzyLogic
{

    #region Membership

    public interface IMembershipFunction
    {
        double Function(double x);
    }

    public class TriangularMF : IMembershipFunction
    {
        public readonly double A;
        public readonly double B;
        public readonly double C;

        public TriangularMF(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
        }

        public double Function(double x)
        {
            if (A <= x && x <= B)
                return 1 - (B - x)/(B - A);
            if (B <= x && x <= C)
                return 1 - (x - B)/(C - B);
            return 0;
        }
    }

    public class TrapezoidalMF : IMembershipFunction
    {
        public readonly double A;
        public readonly double B;
        public readonly double C;
        public readonly double D;

        public TrapezoidalMF(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }

        public double Function(double x)
        {
            if (A <= x && x <= B)
                return 1 - (B - x)/(B - A);
            if (B <= x && x <= C)
                return 1;
            if (C <= x && x <= D)
                return 1 - (x - C)/(D - C);
            return 0;
        }
    }

    #endregion Membership

    #region Set

    public class FuzzyVariable
    {
        public readonly FuzzyUniversalSet UniversalSet;

        public FuzzyVariable(string name, FuzzyUniversalSet fuzzyUniversalSet)
        {
            Name = name;
            UniversalSet = fuzzyUniversalSet;
        }

        public string Name { get; private set; }
        public double Value { get; set; }
    }

    public class FuzzyUniversalSet : IEnumerable
    {
        public readonly double[] Domain;
        public readonly string Name;
        public List<FuzzySet> Sets = new List<FuzzySet>();

        public FuzzyUniversalSet(string name, double[] domain)
        {
            Domain = domain;
            Name = name;
        }

        public FuzzyUniversalSet(string name, double beginInterval, double endInterval, double stepInterval)
        {
            Name = name;
            var domainList = new List<double>();
            for (double i = beginInterval; i <= endInterval; i += stepInterval)
                domainList.Add(i);

            Domain = domainList.ToArray();
        }

        IEnumerator IEnumerable.GetEnumerator() => Domain.GetEnumerator();
    }

    public class FuzzySet
    {
        public static readonly FuzzySet Empty = new FuzzySet("Empty set");
        public readonly string Name;
        public IMembershipFunction MembershipFunction;
        public Dictionary<double, double> Values;

        /// <summary>
        ///     Create fuzzy set in universe
        /// </summary>
        /// <param name="name">Name of set</param>
        /// <param name="membershipFunc">Membership function</param>
        /// <param name="universe">Universal set</param>
        public FuzzySet(string name, IMembershipFunction membershipFunc, FuzzyUniversalSet universe)
        {
            Name = name;
            Values = new Dictionary<double, double>();
            MembershipFunction = membershipFunc;

            foreach (double element in universe)
            {
                AddElement(element);
            }

            universe.Sets.Add(this);
        }

        /// <summary>
        ///     Create independent fuzzy set
        /// </summary>
        /// <param name="name">Name of set</param>
        /// <param name="values">Element - membership pair dictionary</param>
        public FuzzySet(string name, Dictionary<double, double> values)
        {
            Name = name;
            Values = values;
        }

        private FuzzySet(string name) : this(name, new Dictionary<double, double>())
        {
        }

        /// <summary>
        ///     Get the element's membership value
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public double this[double element]
        {
            get
            {
                if (Values.Count == 0) // Empty set
                    return 0;

                double membership;
                if (!Values.TryGetValue(element, out membership))
                {
                    membership = MembershipFunction != null
                        ? AddElement(element)
                        : MathHelper.FindNearest(element, Values.Keys);
                }
                return membership;
            }
        }

        private double AddElement(double element)
        {
            var membership = MembershipFunction.Function(element);
            if (membership > 0)
                Values.Add(element, membership);
            return membership;
        }
    }

    #endregion Set

    #region Operators

    #region SetOps

    public interface IFuzzySetOperators
    {
        FuzzySet Union(FuzzySet left, FuzzySet right);
        FuzzySet Intersect(FuzzySet left, FuzzySet right);
    }

    public class MinMaxFuzzySetOperators : IFuzzySetOperators
    {
        public FuzzySet Union(FuzzySet left, FuzzySet right)
        {
            var resultDic = new Dictionary<double, double>();
            foreach (var item in left.Values)
            {
                var rightMembership = right[item.Key];
                resultDic.Add(item.Key, Math.Max(item.Value, rightMembership));
            }
            foreach (var item in right.Values)
            {
                if (resultDic.ContainsKey(item.Key))
                    continue;

                var leftMembership = left[item.Key];
                resultDic.Add(item.Key, Math.Max(item.Value, leftMembership));
            }
            var name = string.Concat(left.Name, " UNION ", right.Name);
            return new FuzzySet(name, resultDic);
        }

        public FuzzySet Intersect(FuzzySet left, FuzzySet right)
        {
            var values = new Dictionary<double, double>();
            foreach (var item in left.Values)
            {
                var rightMembership = right[item.Key];
                if (rightMembership > 0)
                {
                    values.Add(item.Key, Math.Min(item.Value, rightMembership));
                }
            }
            var name = string.Concat(left.Name, " INTERSECT ", right.Name);
            return new FuzzySet(name, values);
        }
    }

    #endregion SetOps

    #region LogicOps

    public interface IFuzzyExpression
    {
        double Evaluate(Dictionary<string, double> values);
    }

    public class FuzzyAnd : IFuzzyExpression
    {
        private readonly IFuzzyExpression _left;
        private readonly IFuzzyExpression _right;

        public FuzzyAnd(IFuzzyExpression left, IFuzzyExpression right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(Dictionary<string, double> values)
        {
            var leftResult = _left.Evaluate(values);
            var rightResult = _right.Evaluate(values);
            return Math.Min(leftResult, rightResult);
        }
    }

    public class FuzzyOr : IFuzzyExpression
    {
        private readonly IFuzzyExpression _left;
        private readonly IFuzzyExpression _right;

        public FuzzyOr(IFuzzyExpression left, IFuzzyExpression right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(Dictionary<string, double> values)
        {
            var leftResult = _left.Evaluate(values);
            var rightResult = _right.Evaluate(values);
            return Math.Max(leftResult, rightResult);
        }
    }

    public class FuzzyIs : IFuzzyExpression
    {
        private readonly string _left;
        private readonly FuzzySet _right;

        public FuzzyIs(string left, FuzzySet right)
        {
            _left = left;
            _right = right;
        }

        public double Evaluate(Dictionary<string, double> values)
        {
            double value;
            return values.TryGetValue(_left, out value) ? _right[value] : double.NaN;
        }
    }

    public class FuzzyNot : IFuzzyExpression
    {
        private readonly IFuzzyExpression _operand;

        public FuzzyNot(IFuzzyExpression operand)
        {
            _operand = operand;
        }

        public double Evaluate(Dictionary<string, double> values)
        {
            return 1 - _operand.Evaluate(values);
        }
    }

    #endregion LogicOps

    #endregion Operators

    #region Rules

    public class FuzzyRule
    {
        public readonly IFuzzyExpression Condition;
        public readonly string Name;
        public readonly FuzzySet Then;

        public FuzzyRule(string name, IFuzzyExpression condition, FuzzySet then)
        {
            Name = name;
            Then = then;
            Condition = condition;
        }

        public FuzzySet Evaluate(Dictionary<string, double> values)
        {
            var result = Condition.Evaluate(values);
            var newDic = Then.Values.Where(item => item.Value <= result)
                .ToDictionary(item => item.Key, item => item.Value);
            return new FuzzySet(string.Concat(Name, " result set"), newDic);
        }
    }

    public class FuzzyRuleSet
    {
        public List<FuzzyRule> Rules;

        public FuzzyRuleSet(params FuzzyRule[] rules)
        {
            Rules = rules.ToList();
        }

        public FuzzySet Evaluate(Dictionary<string, double> values)
        {
            var ops = new MinMaxFuzzySetOperators();

            var rulesResults = Rules.Select(fuzzyRule => fuzzyRule.Evaluate(values));

            return rulesResults.Aggregate(FuzzySet.Empty, (current, result) => ops.Union(current, result));
        }
    }

    #endregion Rules

    #region Defuzzification

    public interface ISetDefuzzificator
    {
        double Defuzzificate(FuzzySet fuzzySet);
    }

    public class DefuzzCentreOfMass : ISetDefuzzificator
    {
        public double Defuzzificate(FuzzySet fuzzySet)
        {
            double firstSum = 0;
            double secondSum = 0;

            foreach (var value in fuzzySet.Values)
            {
                firstSum += value.Key*value.Value;
                secondSum += value.Value;
            }

            return firstSum/secondSum;
        }
    }

    #endregion Defuzzification

    #region Logic Inference Machine

    public class FuzzyInferenceMachine
    {
        private readonly Dictionary<string, double> _inputs;
        public ISetDefuzzificator Defuzzificator;
        public FuzzyRuleSet RuleSet;
        public List<FuzzyUniversalSet> Universes;

        public FuzzyInferenceMachine(IEnumerable<string> inputs, IEnumerable<FuzzyUniversalSet> universes,
            FuzzyRuleSet ruleSet, ISetDefuzzificator defuzzificator)
        {
            _inputs = new Dictionary<string, double>();
            foreach (var input in inputs)
            {
                _inputs.Add(input, double.NaN);
            }
            Universes = new List<FuzzyUniversalSet>(universes);
            RuleSet = ruleSet;
            Defuzzificator = defuzzificator;
        }

        public double Decision { get; private set; }

        public double this[string inputName]
        {
            get
            {
                double value;
                _inputs.TryGetValue(inputName, out value);
                return value;
            }
            set
            {
                if (_inputs.ContainsKey(inputName))
                {
                    _inputs[inputName] = value;
                    MakeDecision();
                }
            }
        }

        private void MakeDecision()
        {
            var answer = RuleSet.Evaluate(_inputs);
            Decision = Defuzzificator.Defuzzificate(answer);
        }
    }

    #endregion Logic Inference Machine
}