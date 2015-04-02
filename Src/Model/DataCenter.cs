using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QualificationTask.Extensions;

namespace QualificationTask.Model
{
    public class DataCenter
    {
        public int RowsCount { get; private set; }
        public int SlotsCount { get; private set; }
        public int SlotUnavailableCount { get; private set; }
        public int PoolCount { get; private set; }
        public int ServersCount { get; private set; }
        public List<UnavailableCell> Unavailable { get; private set; }
        public List<Server> Servers { get; private set; }
        public List<Group> Groups { get; private set; }

        private int currentRow = 0;
        private int[] currentSlots;

        public void Load(string inputFile)
        {
            using (var inputStream = File.OpenRead(inputFile))
            {
                var reader = new StreamReader(inputStream);

                var inputInt = reader.ExtractValues<int>().ToArray();

                var indexGenerator = new IndexGenerator();

                RowsCount = inputInt[0];
                SlotsCount = inputInt[1];
                SlotUnavailableCount = inputInt[2];
                PoolCount = inputInt[3];
                ServersCount = inputInt[4];

                Unavailable = (
                        from i in Enumerable.Range(0, inputInt[2])
                        select reader.ExtractValues<int>().ToArray()
                    )
                    .Select(x => new UnavailableCell(x[0], x[1]))
                    .ToList();

                Servers = (
                        from i in Enumerable.Range(0, inputInt[4])
                        select reader.ExtractValues<int>().ToArray()
                    )
                    .Select(x => new Server(indexGenerator.GetIndex(), x[0], x[1]))
                    .ToList();

                reader.Close();
                inputStream.Close();
            }
        }

        public string Process()
        {
            SplitServerInGroups();

            PlaceServers();

            return GenerateOutput();
        }

        public void Dump()
        {
            for (int y = 0; y < RowsCount; ++y)
            {
                for (int x = 0; x < SlotsCount; ++x)
                {
                    if (Unavailable.Any(u => u.Row == y && u.Slot == x))
                        Console.Write("x");
                    else if (Servers.Any(s => s.Row == y && s.Slot == x && s.State == Server.StateEnum.Used))
                    {
                        var server = Servers.Single(s => s.Row == y && s.Slot == x && s.State == Server.StateEnum.Used);

                        if (server.Size == 1)
                            Console.Write("O");
                        else
                        {
                            Console.Write("[");

                            for (int s = 1; s < server.Size - 1; ++s)
                                Console.Write("=");

                            Console.Write("]");
                        }

                        x += server.Size - 1;
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        private string GenerateOutput()
        {
            StringBuilder output = new StringBuilder();

            foreach (var server in Servers.OrderBy(x => x.Index))
            {
                var group = Groups.IndexOf(server.Group);

                if (server.State == Server.StateEnum.Unused)
                    output.AppendLine("x");
                else
                    output.AppendFormat("{0} {1} {2}{3}", server.Row, server.Slot, group, Environment.NewLine);
            }

            return output.ToString();
        }

        private void SplitServerInGroups()
        {
            var indexGenerator = new IndexGenerator();

            Groups = (
                from g in Enumerable.Range(0, PoolCount)
                select new Group(indexGenerator.GetIndex()))
                .ToList();

            foreach (var server in Servers.OrderByDescending(x => x.Score))
            {
                var group = Groups.OrderBy(x => x.TotalCapacity).First();

                server.Group = @group;

                @group.Servers.Add(server);
            }
        }

        private void PlaceServers()
        {
            currentSlots = new int[RowsCount];

            for (int row = 0; row < RowsCount; ++row)
            {
                while (Servers.Any(x => x.State == Server.StateEnum.NeedAnalyze))
                {
                    var reste = Servers.Count(x => x.State == Server.StateEnum.NeedAnalyze);

                    currentRow = row;

                    var lessRepresentedGroup =
                        Groups
                        .Where(x => x.Servers.Any(s => s.State == Server.StateEnum.NeedAnalyze))
                        //.OrderBy(x => x.GetUsedCapacity(row))
                        .OrderBy(x => x.UsedCapacity)
                        .ThenBy(x => x.Servers.Where(s => s.State == Server.StateEnum.NeedAnalyze).Max(y => y.Score))
                        .ThenBy(x => x.Servers.Where(s => s.State == Server.StateEnum.NeedAnalyze).Min(y => y.Size))
                        .FirstOrDefault();

                    if (lessRepresentedGroup == null)
                    {
                        break;
                    }

                    var server = lessRepresentedGroup.Servers
                        .Where(x => x.State == Server.StateEnum.NeedAnalyze)
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => x.Size)
                        .First();

                    var canPutInRow = FindServerPlace(server);

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

                foreach (var server in Servers)
                    if (server.State == Server.StateEnum.Ignore)
                        server.State = Server.StateEnum.NeedAnalyze;
            }

            foreach (var server in Servers)
                if (server.State == Server.StateEnum.NeedAnalyze)
                    server.State = Server.StateEnum.Unused;
        }

        private bool FindServerPlace(Server server)
        {
            var hasUnavailableCell = (
                from u in Unavailable
                where u.Row == currentRow && currentSlots[currentRow] == u.Slot
                select u)
                .FirstOrDefault();

            if (hasUnavailableCell != null) // Can not put a server because of unvavailability
            {
                currentSlots[currentRow] = hasUnavailableCell.Slot + 1;

                return FindServerPlace(server);
            }

            if (currentSlots[currentRow] + server.Size > SlotsCount) // Slot limit reached
            {
                return false;
            }

            // Could not place a server we check on next line or, next to unvavailable cell.
            return true;
        }

    }
}
