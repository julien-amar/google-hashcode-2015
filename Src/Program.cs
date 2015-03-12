using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QualificationTask.Model;
using TrialRound.Extentions;

namespace QualificationTask
{
    class Program
    {
        private static void Main(string[] args)
        {
            const string FILE_NAME = @"Samples/sample.in";

            StringBuilder sbOut = new StringBuilder();

            using (var inputStream = File.OpenRead(FILE_NAME))
            {
                var reader = new StreamReader(inputStream);

                // parse first line to get Matrix Size
                var inputInt = reader.ExtractValues<int>().ToArray();

                var rowsCount = inputInt[0];
                var slotsCount = inputInt[1];
                var slotUnavailableCount = inputInt[2];
                var poolCount = inputInt[3];
                var serversCount = inputInt[4];

                var unuvailable = (from i in Enumerable.Range(0, slotUnavailableCount)
                                   select reader.ExtractValues<int>().ToArray())
                                   .ToList();

                var servers = (from i in Enumerable.Range(0, serversCount)
                              select reader.ExtractValues<int>().ToArray())
                              .Select(x => new Server(IndexGenerator.GetIndex(), x[0], x[1]))
                              .ToList();

                int currentSlot = 0;
                int currentRow = 0;
                int currentCpuInRow = 0;
                int cpu = 0;

                int nbServerByGroup = serversCount / poolCount;

                foreach (var server in servers)
                {
                    if (currentSlot + server.Slots > slotsCount) // Slot limit reached
                    {
                        currentSlot = 0;
                        currentCpuInRow = 0;
                        currentRow++;
                    }

                    if (currentRow >= rowsCount)
                    {
                        Console.WriteLine("x");
                        continue;
                    }

                    // filling current slot
                    Console.WriteLine("{0} {1} {2}", currentRow, currentSlot, server.Index / nbServerByGroup);

                    currentSlot += server.Slots;
                    currentCpuInRow += server.Capacity;
                }

                reader.Close();
                inputStream.Close();
            }
        }

    }
}
