using System;
using System.Configuration;
using System.IO;
using System.Linq;
using QualificationTask.Extensions;
using QualificationTask.Model;

namespace QualificationTask
{
    class Program
    {
        private static void Main(string[] args)
        {
            var inputFile = ConfigurationManager.AppSettings["Sample"];
            var dataCenter = new DataCenter();
            
            dataCenter.Load(inputFile);

            var output = dataCenter.Process();

            File.WriteAllText(@"output.txt", output);
        }
    }
}
