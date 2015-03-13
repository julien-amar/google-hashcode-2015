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
        private static int currentRow = 0;
        private static int[] currentSlots;
        private static bool[] fullRows;

        private static DataModel LoadDate()
        {
            var sample = ConfigurationManager.AppSettings["Sample"];

            using (var inputStream = File.OpenRead(sample))
            {
                var reader = new StreamReader(inputStream);

                var inputInt = reader.ExtractValues<int>().ToArray();

                var indexGenerator = new IndexGenerator();

                var data = new DataModel()
                {
                    RowsCount = inputInt[0],
                    SlotsCount = inputInt[1],
                    SlotUnavailableCount = inputInt[2],
                    PoolCount = inputInt[3],
                    ServersCount = inputInt[4],

                    Unavailable = (
                        from i in Enumerable.Range(0,inputInt[2])
                        select reader.ExtractValues<int>().ToArray()
                    )
                    .Select(x => new UnavailableCell(x[0],x[1]))
                    .ToList(),

                    Servers = (
                        from i in Enumerable.Range(0,inputInt[4])
                        select reader.ExtractValues<int>().ToArray()
                    )
                    .Select(x => new Server(indexGenerator.GetIndex(), x[0], x[1]))
                    .ToList()
                };

                reader.Close();
                inputStream.Close();

                return data;
            }
        }

        private static Group[] SplitServerInGroups(DataModel data)
        {
            var indexGenerator = new IndexGenerator();

            Group[] groups = (
                from g in Enumerable.Range(0, data.PoolCount)
                select new Group(indexGenerator.GetIndex()))
                .ToArray();

            foreach (var server in data.Servers)
            {
                var group = groups.OrderBy(x => x.TotalCapacity).First();

                server.Group = @group;

                @group.Servers.Add(server);
            }
            return groups;
        }

        private static void Main(string[] args)
        {
            var data = LoadDate();

            currentSlots = new int[data.RowsCount];

            var groups = SplitServerInGroups(data);

            foreach (var group in groups)
                group.Dump();

            PlaceServers(data, groups);

            foreach (var server in data.Servers.OrderBy(x => x.Index))
            {
                var group = groups.ToList().IndexOf(server.Group);

                if (server.State == Server.StateEnum.Unused)
                    Console.WriteLine("x");
                else
                    Console.WriteLine("{0} {1} {2}", server.Row, server.Slot, group);
            }

            data.Dump();
        }

        private static void PlaceServers(DataModel data, Group[] groups)
        {
            while (data.Servers.Any(x => x.State == Server.StateEnum.NeedAnalyze))
            {
                foreach (var group in groups)
                {
                    if (@group.Servers.Count(x => x.State == Server.StateEnum.NeedAnalyze) == 0)
                        continue;

                    var server = @group.Servers
                        .Where(x => x.State == Server.StateEnum.NeedAnalyze)
                        .OrderByDescending(x => x.Score)
                        .First();

                    int t = currentRow;

                    FindServerPlace(server, data);

                    currentRow = t;

                    currentRow = (currentRow + 1)%data.RowsCount;
                }
            }
        }

        private static bool FindServerPlace(Server server, DataModel data)
        {
            bool hasError = false;

            var hasUnavailableCell = (
                from u in data.Unavailable
                where u.Row == currentRow && currentSlots[currentRow] <= u.Slot && u.Slot < currentSlots[currentRow] + server.Size
                select u)
                .FirstOrDefault();

            if (hasUnavailableCell != null) // Can not put a server because of unvavailability
            {
                currentSlots[currentRow] = hasUnavailableCell.Slot + 1;
                hasError = true;
            }

            if (currentSlots[currentRow] + server.Size > data.SlotsCount) // Slot limit reached
            {
                currentRow++;
                hasError = true;
            }

            if (hasError && currentRow >= data.RowsCount)
            {
                server.State = Server.StateEnum.Unused;
                return false;
            }
            
            if (hasError)
                return FindServerPlace(server, data);

            // Could not place a server we check on next line or, next to unvavailable cell.
            server.Row = currentRow;
            server.Slot = currentSlots[currentRow];
            server.State = Server.StateEnum.Used;

            currentSlots[currentRow] += server.Size;
            return true;
        }

    
    }
}
