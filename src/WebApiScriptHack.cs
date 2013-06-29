using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCs.Contracts;

namespace ScriptCs.Hosting.WebApi
{
    public class WebApiScriptHack : IScriptPack
    {
        public void Initialize(IScriptPackSession session)
        {
            session.AddReference(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "System.Web.Http.dll"));
            session.AddReference("System.Net.Http");
            session.ImportNamespace("System.Web.Http");
            session.ImportNamespace("System.Net.Http");
        }

        public IScriptPackContext GetContext()
        {
            return null;
        }

        public void Terminate()
        {
        }
    }
}
