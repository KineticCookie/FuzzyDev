using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace FDL.Extensions
{
    public interface IExtension
    {
        void Do();
    }

    public abstract class Extension : MarshalByRefObject, IExtension
    {
        public abstract void Do();
    }

    public static class ExtensionsKernel
    {
        private static Dictionary<Extension, AppDomain> Modules = new Dictionary<Extension, AppDomain>();

        public static Extension Load(string path)
        {
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException("Incorrect path to assembly file", path);
            string domainName = System.IO.Path.GetFileNameWithoutExtension(path);
            AppDomainSetup domainSetup = AppDomain.CurrentDomain.SetupInformation;
            AppDomain domainModule = AppDomain.CreateDomain(domainName, null, domainSetup);

            Type loaderType = typeof(ProxyLoader);
            ProxyLoader loader = (ProxyLoader)domainModule.CreateInstanceAndUnwrap(loaderType.Assembly.FullName, loaderType.FullName);
            Extension module = loader.InjectAssembly(path);

            Modules.Add(module, domainModule);
            return module;
        }

        public static void Unload(Extension extension)
        {
            AppDomain domain;
            Modules.TryGetValue(extension, out domain);
            if (ReferenceEquals(domain, null))
                throw new NullReferenceException("Unknown module");
            Modules.Remove(extension);
            AppDomain.Unload(domain);
        }
    }

    public class ProxyLoader : MarshalByRefObject
    {
        private Assembly ModuleAssembly;

        public Extension InjectAssembly(string assemblyPath)
        {
            try
            {
                ModuleAssembly = Assembly.LoadFile(assemblyPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            string moduleName = GetModuleClassName();
            var moduleHandler = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(ModuleAssembly.FullName, moduleName) as Extension;
            return moduleHandler;
        }

        private string GetModuleClassName()
        {
            var asmTypes = ModuleAssembly.GetExportedTypes();
            foreach (var itemType in asmTypes)
            {
                if (itemType.GetInterface(nameof(IExtension)) != null)
                {
                    return itemType.FullName;
                }
            }
            return null;
        }
    }
}
