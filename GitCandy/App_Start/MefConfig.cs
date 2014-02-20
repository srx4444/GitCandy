using GitCandy.Base;
using GitCandy.Schedules;
using System;
using System.Composition.Convention;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Linq;

namespace GitCandy
{
    public static class MefConfig
    {
        public static void RegisterMef()
        {
            PreLoad();

            var builder = RegisterExports();
            var resolver = DependencyResolver.Current;
            var newResolver = new MefDependencyResolver(builder, resolver);
            DependencyResolver.SetResolver(newResolver);
        }

        private static AttributedModelProvider RegisterExports()
        {
            var builder = new ConventionBuilder();
            //builder.ForType<learner>().Export<learner>();
            //builder.ForTypesMatching
            //    (x => x.GetProperty("SourceMaterial") != null).Export<exam>();

            builder.ForTypesDerivedFrom<IJob>().Export<IJob>();

            return builder;
        }

        private static void PreLoad()
        {
            var binFolder = HttpContext.Current != null
                ? HttpRuntime.BinDirectory
                : AppDomain.CurrentDomain.BaseDirectory;

            foreach (var file in Directory.GetFiles(binFolder, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(file);
                    if (name.ProcessorArchitecture == ProcessorArchitecture.None ||
                        name.ProcessorArchitecture != ProcessorArchitecture.MSIL &&
                        typeof(GitCandyApplication).Assembly.GetName().ProcessorArchitecture != name.ProcessorArchitecture)
                        continue;

                    if (AppDomain.CurrentDomain.GetAssemblies().All(ass =>
                        !AssemblyName.ReferenceMatchesDefinition(name, ass.GetName())))
                    {
                        Assembly.Load(name);
                    }
                }
                catch { }
            }
        }
    }
}