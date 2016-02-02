using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDL.FuzzyLogic.Membership
{
    public delegate double MembershipDelegate(double x);

    public static class MembershipFactory
    {
        public static MembershipDelegate MakeTriangular(double a, double b, double c)
        {
            return new MembershipDelegate((x) =>
            {
                if (a <= x && x <= b)
                    return 1 - (b - x) / (b - a);
                else if (b <= x && x <= c)
                    return 1 - (x - b) / (c - b);
                else
                    return 0;
            });
        }

        public static MembershipDelegate MakeTrapezoidal(double a, double b, double c, double d)
        {
            return new MembershipDelegate((x) =>
            {
                if (a <= x && x <= b)
                    return 1 - (b - x) / (b - a);
                else if (b <= x && x <= c)
                    return 1;
                else if (c <= x && x <= d)
                    return 1 - (x - c) / (d - c);
                else
                    return 0;
            });
        }
    }

}
