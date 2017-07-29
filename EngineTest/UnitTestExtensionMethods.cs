using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EngineTest
{
    internal static class UnitTestExtensionMethods
    {
        internal static object GetPrivateField(this object obj, string fieldNAme)
        {
            PrivateObject po = new PrivateObject(obj);
            return po.GetField(fieldNAme);
        }
    }
}
