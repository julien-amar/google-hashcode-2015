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
        private static int currentRow = 0;
        private static int[] currentSlots;

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

                IndexGenerator.SetIndex(0);

                currentSlots = new int[rowsCount];

                Group[] groups = (
                    from g in Enumerable.Range(0, poolCount)
                    select new Group())
                    .ToArray();

                foreach (var server in servers)
                {
                    var group = groups.OrderBy(x => x.TotalCapacity).First();

                    server.Group = group;

                    group.Servers.Add(server);
                }

                var test1 = groups.Min(x => x.TotalCapacity);
                var test2 = groups.Max(x => x.TotalCapacity);

                foreach (var group in groups)
                {
                    var server = group.Servers
                        .Where(x => x.Unused)
                        .OrderByDescending(x => x.Score)
                        .First();

                    FindServerPlace(server, unuvailable, slotsCount);

                    server.Row = currentRow;
                    server.Slot = currentSlots[currentRow];
                    server.Unused = false;

                    currentSlots[currentRow] += server.Size;

                    currentRow = (currentRow + 1) % rowsCount;
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
                where u.Row == currentSlots[currentRow] && currentSlots[currentRow] <= u.Slot && u.Slot < currentSlots[currentRow] + server.Size
                select u)
                .FirstOrDefault();

            if (hasUnavailableCell != null) // Can not put a server because of unvavailability
            {
                currentSlots[currentRow] = hasUnavailableCell.Slot + 1;
                hasError = true;
            }


            if (currentSlots[currentRow] + server.Size > slotsCount) // Slot limit reached
            {
                currentRow++;
                hasError = true;
            }

            if (hasError) // Could not place a server we check on next line or, next to unvavailable cell.
                FindServerPlace(server, unAvailable, slotsCount);
        }
    }
}
