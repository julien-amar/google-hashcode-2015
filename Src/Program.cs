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

            //foreach (var group in groups)
                //group.Dump();

            PlaceServers(data, groups);

            foreach (var server in data.Servers.OrderBy(x => x.Index))
            {
                var group = groups.ToList().IndexOf(server.Group);

                if (server.State == Server.StateEnum.Unused)
                    Console.WriteLine("x");
                else
                    Console.WriteLine("{0} {1} {2}", server.Row, server.Slot, group);
            }

            /*
            data.Dump();

            var serverbyrow = data.Servers.Where(s=>s.State == Server.StateEnum.Used).GroupBy(s => s.Row);

            foreach (var servers in serverbyrow.OrderBy(x => x.Key))
            {
                Console.WriteLine("row : " + servers.Key);

                var bygroups = servers.GroupBy(x => x.Group);

                foreach (var g in bygroups.OrderBy(x => x.Key.Index))
                {
                    Console.WriteLine("group #" + g.Key.Index + " total capacity " + g.Key.GetUsedCapacity(servers.Key));
                }
            }

            foreach (var group in groups)
                group.Dump();
            */
        }

        private static void PlaceServers(DataModel data, Group[] groups)
        {
                for (int row = 0; row < data.RowsCount; ++row )
                {
                    //Console.WriteLine("Analyse row #" + row);

                    while (data.Servers.Any(x => x.State == Server.StateEnum.NeedAnalyze))
                    {
                        var reste = data.Servers.Count(x => x.State == Server.StateEnum.NeedAnalyze);

                        currentRow = row;

                        var lessRepresentedGroup =
                            groups
                            .Where(x => x.Servers.Any(s => s.State == Server.StateEnum.NeedAnalyze))
                            .OrderBy(x => x.GetUsedCapacity(row))
                            .ThenBy(x => x.Servers.Where(s => s.State == Server.StateEnum.NeedAnalyze).Max(y => y.Score))
                            .FirstOrDefault();

                        if (lessRepresentedGroup == null)
                        {
                            break;
                        }

                        var server = lessRepresentedGroup.Servers
                            .Where(x => x.State == Server.StateEnum.NeedAnalyze)
                            .OrderByDescending(x => x.Score)
                            .First();

                        var canPutInRow = FindServerPlace(server, data);

                        if (canPutInRow)
                        {
                            server.Row = currentRow;
                            server.Slot = currentSlots[currentRow];
                            server.State = Server.StateEnum.Used;

                            currentSlots[currentRow] += server.Size;
                        }
                        else
                        {
                            server.State = Server.StateEnum.Ignore;
                        }
                    }

                    foreach (var server in data.Servers)
                        if (server.State == Server.StateEnum.Ignore)
                            server.State = Server.StateEnum.NeedAnalyze;
                }

            foreach (var server in data.Servers)
                    if (server.State == Server.StateEnum.NeedAnalyze)
                        server.State = Server.StateEnum.Unused;
        }

        private static bool FindServerPlace(Server server, DataModel data)
        {
            var hasUnavailableCell = (
                from u in data.Unavailable
                where u.Row == currentRow && currentSlots[currentRow] <= u.Slot && u.Slot < currentSlots[currentRow] + server.Size
                select u)
                .FirstOrDefault();

            if (hasUnavailableCell != null) // Can not put a server because of unvavailability
            {
                currentSlots[currentRow] = hasUnavailableCell.Slot + 1;

                return FindServerPlace(server, data);
            }

            if (currentSlots[currentRow] + server.Size > data.SlotsCount) // Slot limit reached
            {
                return false;
            }

            // Could not place a server we check on next line or, next to unvavailable cell.
            return true;
        }
    }
}
