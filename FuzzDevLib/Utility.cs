using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FDL.Utility
{
    #region Helpers

    public static class DictionaryHelper
    {
        public static Dictionary<T, double> ToDictionaryWithZeros<T>(IEnumerable<T> enumerable)
        {
            if (ReferenceEquals(enumerable, null))
                throw new ArgumentNullException(nameof(enumerable));

            var result = new Dictionary<T, double>();
            foreach (var item in enumerable)
                result.Add(item, 0);
            return result;
        }

        public static Dictionary<T, double> ToDictionaryWithRandom<T>(IEnumerable<T> enumerable)
        {
            if (ReferenceEquals(enumerable, null))
                throw new ArgumentNullException(nameof(enumerable));

            var result = new Dictionary<T, double>();
            foreach (var item in enumerable)
                result.Add(item, RandomHelper.NextDouble);
            return result;
        }
    }

    public static class RandomHelper
    {
        private static Random rnd = new Random(Guid.NewGuid().GetHashCode());
        private static int count = 0;
        public static double NextDouble
        {
            get
            {
                if (++count == 42)
                {
                    count = 0;
                    rnd = new Random(Guid.NewGuid().GetHashCode());
                }
                return rnd.NextDouble() - 0.5;
            }
        }
    }

    #endregion Helpers

    #region Exceptions

    public class RuntimeException_NotImplemented : NotImplementedException
    {
        private static string GetCallerMethodFullname()
        {
            var st = new System.Diagnostics.StackTrace();
            var stackFrame = st.GetFrame(2);
            var methodBase = stackFrame.GetMethod();
            var classInfo = methodBase.ReflectedType;

            string methodTypeName = string.Empty;
            var methodInfo = methodBase as System.Reflection.MethodInfo;
            if (methodInfo != null)
                methodTypeName = methodInfo.ReturnType.Name;

            var paramsArray = methodBase.GetParameters();
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < paramsArray.Length; i++)
            {
                sb.Append(paramsArray[i].ParameterType.Name);
                if (i != paramsArray.Length - 1)
                    sb.Append(", ");
            }
            string methodParamsTypeName = sb.ToString();

            string methodName = methodBase.Name;
            string className = classInfo.Name;
            string namespaceName = classInfo.Namespace;
            string caller = string.Format("{3} {0}.{1}.{2} ({4})", namespaceName, className, methodName, methodTypeName, methodParamsTypeName);
            return caller;
        }

        public RuntimeException_NotImplemented() : this(GetCallerMethodFullname()) { }

        private RuntimeException_NotImplemented(string caller) : base(string.Format("{0} is not implemented", caller)) { }
    }

    #endregion Exceptions

    #region Log

    public abstract class Logger
    {
        public string FormatString { get; set; }
        
        public Logger() : this("[{0:dd/MM/yyyy hh:mm:ss}] {1}: {2}") { }

        public Logger(string formatString) { FormatString = formatString; }

        public abstract void Log(string msg);
        public abstract void Warning(string msg);
        public abstract void Error(string msg);
    }

    public class ConsoleLogger : Logger
    {
        private ConsoleLogger():base() { }
        static ConsoleLogger() { }

        public static ConsoleLogger This = new ConsoleLogger();

        public override void Log(string msg)
        {
            Console.WriteLine(string.Format(FormatString, DateTime.Now.ToString(), nameof(Log), msg));
        }

        public override void Warning(string msg)
        {
            throw new NotImplementedException();
        }

        public override void Error(string msg)
        {
            throw new NotImplementedException();
        }
    }

    public class FileLogger : Logger
    {
        public override void Error(string msg)
        {
            throw new NotImplementedException();
        }

        public override void Log(string msg)
        {
            throw new NotImplementedException();
        }

        public override void Warning(string msg)
        {
            throw new NotImplementedException();
        }
    }

    #endregion Log

    #region Console

    public static class ConsoleExt
    {
        public static void WriteTextSlow(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < text.Length; i++)
            {
                Console.Write(text[i]);
                Thread.Sleep(rnd.Next(10, 100));
            }
            Console.WriteLine();
        }

        public static void WriteSlow<T>(IEnumerable<T> input)
        {
            if (ReferenceEquals(input, null))
                throw new ArgumentNullException(nameof(input));

            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (var item in input)
            {
                Console.Write(item + " ");
                Thread.Sleep(rnd.Next(10, 100));
            }
            Console.WriteLine();
        }
    }

    #endregion Console
}