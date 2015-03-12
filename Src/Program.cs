using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private static bool[] fullRows;
        private static Color[] groupColors ;
        private static Group[] groups;

        private static void ExportMatrixtoBitmap(Server[,] matrix, int nbInstruction)
        {
            var outputFile = nbInstruction.ToString("000000000") + ".bmp";

            matrix.ToBitmap(
                outputFile,
                ColorExport,
                false);
        }

        private static Color ColorExport(Server cell)
        {
            var index = groups.ToList().IndexOf(cell.Group);

            return groupColors[index];

        }

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

                Server[,] datacenter = new Server[rowsCount, slotsCount];
                 groupColors = new Color[poolCount];

                for (int i = 0; i < poolCount; i++)
                {
                    groupColors[i] = Color.FromKnownColor((KnownColor)i);
                }

                int nbServerByGroup = serversCount / poolCount;

                IndexGenerator.SetIndex(0);

                currentSlots = new int[rowsCount];
                fullRows = new bool[rowsCount];

                groups = (
                    from g in Enumerable.Range(0, poolCount)
                    select new Group())
                    .ToArray();

                foreach (var server in servers)
                {
                    var group = groups.OrderBy(x => x.TotalCapacity).First();

                    server.Group = group;

                    group.Servers.Add(server);
                }

                while (servers.Any(x => x.State == Server.StateEnum.NeedAnalyze))
                {
                    foreach (var group in groups)
                    {
                        if (group.Servers.Count(x => x.State == Server.StateEnum.NeedAnalyze) == 0)
                            continue;

                        var server = group.Servers
                            .Where(x => x.State == Server.StateEnum.NeedAnalyze)
                            .OrderByDescending(x => x.Score)
                            .First();

                        fullRows[currentRow] = currentSlots[currentRow] == rowsCount;
 
                        FindServerPlace(server, unuvailable, slotsCount, rowsCount);

                        currentRow = (currentRow + 1) % rowsCount;
                    }
                }

                foreach (var server in servers.OrderBy(x => x.Index))
                {
                    var group = groups.ToList().IndexOf(server.Group);

                    if (server.State == Server.StateEnum.Unused)
                        Console.WriteLine("x");
                    else
                        Console.WriteLine("{0} {1} {2}", server.Row, server.Slot, group);
                }

                reader.Close();
                inputStream.Close();
            }
        }

        private static bool FindServerPlace(Server server, List<UnavailableCell> unAvailable, int slotsCount, int rowsCount)
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

            if (hasError && currentRow >= rowsCount)
            {
                server.State = Server.StateEnum.Unused;
                return false;
            }
            
            if (hasError)
                return FindServerPlace(server, unAvailable, slotsCount, rowsCount);

            // Could not place a server we check on next line or, next to unvavailable cell.
            server.Row = currentRow;
            server.Slot = currentSlots[currentRow];
            server.State = Server.StateEnum.Used;

            currentSlots[currentRow] += server.Size;
            return true;
        }
    }
}
