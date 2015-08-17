using System;
using System.Collections.Generic;
using System.Linq;

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

    public class FuzzyUniversalSet
    {
        public List<FuzzySet> Sets;
        public string Name { get; private set; }
    }

    public class FuzzySet
    {
        public static FuzzySet Empty = new FuzzySet("Empty set");
        public readonly string Name;
        public Dictionary<double, double> Values;

        public FuzzySet(string name, IMembershipFunction membershipFunc, double beginInterval, double endInterval,
            double step)
        {
            Name = name;
            Values = new Dictionary<double, double>();

            for (var value = beginInterval; value <= endInterval; value += step)
            {
                var membership = membershipFunc.Function(value);
                if (membership > 0)
                    Values.Add(value, membership);
            }
        }

        public FuzzySet(string name, Dictionary<double, double> values)
        {
            Name = name;
            Values = values;
        }

        public FuzzySet(string name) : this(name, new Dictionary<double, double>())
        {
        }

        public double this[double value]
        {
            get
            {
                double membership;
                Values.TryGetValue(value, out membership);
                return membership;
            }
        }
    }

    #endregion Set

    #region Operator

    public interface IFuzzySetOperators
    {
        FuzzySet Union(FuzzySet left, FuzzySet right);
        FuzzySet Intersect(FuzzySet left, FuzzySet right);
    }

    public interface IFuzzyLogicOperators
    {
        double And(double left, double right);
        double Or(double left, double right);
        double Not(double input);
    }

    public class ZadehLogicOperators : IFuzzyLogicOperators
    {
        public double And(double left, double right)
        {
            return Math.Min(left, right);
        }

        public double Not(double input)
        {
            return 1 - input;
        }

        public double Or(double left, double right)
        {
            return Math.Max(left, right);
        }
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

    #endregion Operator

    #region Rule

    public class FuzzyRule
    {
        public readonly string Name;
        public double Condition;
        public FuzzySet Then;

        public FuzzyRule(string name, double condition, FuzzySet then)
        {
            Name = name;
            Then = then;
            Condition = condition;
        }

        public FuzzySet Evaluate()
        {
            var newDic = Then.Values.Where(item => item.Value <= Condition)
                .ToDictionary(item => item.Key, item => item.Value);
            return new FuzzySet("Rule", newDic);
        }
    }

    public class FuzzyRuleSet
    {
        public List<FuzzyRule> Rules;

        public FuzzyRuleSet(params FuzzyRule[] rules)
        {
            Rules = rules.ToList();
        }

        public FuzzySet Evaluate()
        {
            var ops = new MinMaxFuzzySetOperators();

            var rulesResults = Rules.Select(fuzzyRule => fuzzyRule.Evaluate());

            return rulesResults.Aggregate(FuzzySet.Empty, (current, result) => ops.Union(current, result));
        }
    }

    #endregion Rule

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
}