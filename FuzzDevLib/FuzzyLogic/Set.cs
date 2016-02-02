using FDL.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using FDL.FuzzyLogic.Membership;

namespace FDL.FuzzyLogic
{
    public class Variable
    {
        public readonly UniversalSet UniversalSet;

        public Variable(string name, UniversalSet universalSet)
        {
            Name = name;
            UniversalSet = universalSet;
        }

        public string Name { get; private set; }
        public double Value { get; set; }
    }

    public class UniversalSet : IEnumerable<double>
    {
        public readonly double[] Domain;
        public readonly string Name;
        public List<Set> Sets = new List<Set>();

        public UniversalSet(string name, double[] domain)
        {
            Domain = domain;
            Name = name;
        }

        public UniversalSet(string name, double beginInterval, double endInterval, double stepInterval)
        {
            Name = name;
            var domainList = new List<double>();
            for (double i = beginInterval; i <= endInterval; i += stepInterval)
                domainList.Add(i);

            Domain = domainList.ToArray();
        }

        public IEnumerator<double> GetEnumerator() => ((IEnumerable<double>)Domain).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<double>)Domain).GetEnumerator();
    }

    public class Set
    {
        public static readonly Set Empty = new Set("Empty set");
        public readonly string Name;
        public MembershipDelegate MembershipFunction;
        public Dictionary<double, double> Values;

        /// <summary>
        ///     Create fuzzy set in universe
        /// </summary>
        /// <param name="name">Name of set</param>
        /// <param name="membershipFunc">Membership function</param>
        /// <param name="universe">Universal set</param>
        public Set(string name, MembershipDelegate membershipFunc, UniversalSet universe)
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
        public Set(string name, Dictionary<double, double> values)
        {
            Name = name;
            Values = values;
        }

        private Set(string name) : this(name, new Dictionary<double, double>())
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
                        : Helper.Math.FindNearest(element, Values.Keys);
                }
                return membership;
            }
        }

        private double AddElement(double element)
        {
            var membership = MembershipFunction(element);
            if (membership > 0)
                Values.Add(element, membership);
            return membership;
        }

        public override string ToString() => Name;
    }

}
