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
        private static int currentSlot = 0;
        private static int currentRow = 0;
        private static int currentCpuInRow = 0;

        private static void Main(string[] args)
        {
            const string FILE_NAME = @"Samples/dc.in";

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
                                   .Select(x => new UnavailableCell(x[0], x[1]))
                                   .ToList();

                var servers = (from i in Enumerable.Range(0, serversCount)
                              select reader.ExtractValues<int>().ToArray())
                              .Select(x => new Server(IndexGenerator.GetIndex(), x[0], x[1]))
                              .ToList();

                int nbServerByGroup = serversCount / poolCount;

                int c = 0;
                IndexGenerator.SetIndex(0);
                foreach (var server in servers.OrderByDescending(x => x.Score))
                {
                    FindServerPlace(server, unuvailable, slotsCount);

                    if (currentRow >= rowsCount) // No more rows available
                    {
                        server.Unused = true;
                        c ++;
                        continue;
                    }


                    server.Row = currentRow;
                    server.Slot = currentSlot;

                    currentSlot += server.Size;
                    currentCpuInRow += server.Capacity;

                }

                foreach (var server in servers.OrderBy(x => x.Index))
                {
                    int group = IndexGenerator.GetIndex() % poolCount;

                    if (server.Unused)
                        Console.WriteLine("x");
                    else
                        Console.WriteLine("{0} {1} {2}", server.Row, server.Slot, group);
                }

                reader.Close();
                inputStream.Close();
            }
        }

        private static void FindServerPlace(Server server, List<UnavailableCell> unAvailable, int slotsCount)
        {
            bool hasError = false;

            var hasUnavailableCell = (
                from u in unAvailable
                where u.Row == currentRow && currentSlot <= u.Slot && u.Slot < currentSlot + server.Size
                select u)
                .FirstOrDefault();

            if (hasUnavailableCell != null) // Can not put a server because of unvavailability
            {
                currentSlot = hasUnavailableCell.Slot + 1;
                hasError = true;
            }

            if (currentSlot + server.Size > slotsCount) // Slot limit reached
            {
                currentSlot = 0;
                currentCpuInRow = 0;
                currentRow++;
                hasError = true;
            }

            if (hasError) // Could not place a server we check on next line or, next to unvavailable cell.
                FindServerPlace(server, unAvailable, slotsCount);
        }
    }
}
