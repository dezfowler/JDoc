using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JDoc.Specs
{
    public class Class1
    {
        public static void RunSpec()
        {
            var runner = TechTalk.SpecFlow.TestRunnerManager.Instance.CreateTestRunner(Assembly.GetExecutingAssembly(), true);

            // TODO: What are binding assemblies?
            // Ans: binding is the steps types

            //runner.InitializeTestRunner(bindingAssemblies);

            
        }
    }
}
