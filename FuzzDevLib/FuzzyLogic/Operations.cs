using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDL.FuzzyLogic.Operations
{

    #region SetOps

    public interface ISetOperators
    {
        Set Union(Set left, Set right);
        Set Intersect(Set left, Set right);
    }

    public class MinMaxSetOperators : ISetOperators
    {
        public Set Union(Set left, Set right)
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
            return new Set(name, resultDic);
        }

        public Set Intersect(Set left, Set right)
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
            return new Set(name, values);
        }
    }

    #endregion SetOps

    #region LogicOps

    public interface ILogicOperators
    {
        double And(double left, double right);
        double Or(double left, double right);
        double Not(double operand);
    }

    public class ZadehOperators : ILogicOperators
    {
        public double And(double left, double right)
        {
            return Math.Min(left, right);
        }

        public double Not(double operand)
        {
            return 1 - operand;
        }

        public double Or(double left, double right)
        {
            return Math.Max(left, right);
        }
    }

    #endregion LogicOps
    #region Defuzzification

    public delegate double DefuzzificatorDelegate(Set set);

    public static class DefuzzificatorFactory
    {
        public static DefuzzificatorDelegate MakeCenterOfMass()
        {
            return new DefuzzificatorDelegate((set) =>
            {
                double firstSum = 0;
                double secondSum = 0;

                foreach (var value in set.Values)
                {
                    firstSum += value.Key * value.Value;
                    secondSum += value.Value;
                }

                return firstSum / secondSum;
            });
        }
    }

    #endregion Defuzzification
}
