using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Common.Logging;
using Common.Logging.Log4Net;
using log4net;
using log4net.Core;
using ScriptCs.Contracts;
using LogManager = log4net.LogManager;

namespace ScriptCs.Hosting.WebApi
{
    internal class CodeConfigurableLog4NetLogger : Log4NetLogger
    {
        protected internal CodeConfigurableLog4NetLogger(ILoggerWrapper log)
            : base(log)
        {
        }
    }

    public class WebApiConfigurationBuilder
    {
        private HttpConfiguration _configuration;
        private readonly string _scriptsPath;
        private IList<Func<string, ScriptClass>> _typeStrategies; 

        public WebApiConfigurationBuilder(HttpConfiguration configuration, string webBin)
        {
            _configuration = configuration;
            _scriptsPath = Path.Combine(webBin, "Scripts");
            _typeStrategies = new List<Func<string, ScriptClass>>();
        }

        public WebApiConfigurationBuilder AddTypeResolutionStrategy(Func<string,ScriptClass> strategy)
        {
            _typeStrategies.Add(strategy);
            return this;
        }

        public HttpConfiguration Build()
        {
            var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        var commonLogger = new CodeConfigurableLog4NetLogger(logger);

            IList<Func<string, ScriptClass>> typeStrategies = new List<Func<string, ScriptClass>>(_typeStrategies);
            var services = new ScriptServicesBuilder(new ScriptConsole(), commonLogger).
                FilePreProcessor<WebApiFilePreProcessor>().Build();

            var preProcessor = (WebApiFilePreProcessor) services.FilePreProcessor;
            typeStrategies.Add(ControllerStategy);
            preProcessor.SetClassStrategies(typeStrategies);
            preProcessor.LoadSharedCode(Path.Combine(_scriptsPath, "Shared"));
            ProcessScripts(services);
            return _configuration;
        }

        private void ProcessScripts(ScriptServices services)
        {
            IList<Type> controllers = new List<Type>();
            var packs = services.ScriptPackResolver.GetPacks().Union(new List<IScriptPack>() { new WebApiScriptHack() });
            services.Executor.Initialize(services.AssemblyResolver.GetAssemblyPaths(_scriptsPath, _scriptsPath), packs);
            var scripts = services.FileSystem.EnumerateFiles(_scriptsPath, "*.csx", SearchOption.TopDirectoryOnly);
            foreach (var script in scripts)
            {
                var result = services.Executor.Execute(script);
                var resultType = result.ReturnValue as Type;
                if (resultType != null)
                {
                    if (resultType.IsSubclassOf(typeof(ApiController)))
                    {
                        controllers.Add(resultType);
                    }
                }
            }

            var controllerResolver = new AssemblyControllerTypeResolver(controllers);
            _configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerResolver);
        }

        private ScriptClass ControllerStategy(string name)
        {
            if (name.ToLower().EndsWith("controller.csx"))
            {
                return new ScriptClass
                    {
                        BaseType = "ApiController",
                        ClassName = Path.GetFileNameWithoutExtension(name)
                    };
            }
            return null;
        }
    }
}
