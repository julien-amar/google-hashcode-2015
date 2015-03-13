using System;
using System.Collections.Generic;
using System.Linq;
using QualificationTask.Model;

namespace QualificationTask
{
    public class DataModel
    {
        public int RowsCount { get; set; }
        public int SlotsCount { get; set; }
        public int SlotUnavailableCount { get; set; }
        public int PoolCount { get; set; }
        public int ServersCount { get; set; }
        public List<UnavailableCell> Unavailable { get; set; }
        public List<Server> Servers { get; set; }

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

                        Console.Write("[");

                        for (int s = 1; s < server.Size - 1; ++s)
                            Console.Write("=");

                        Console.Write("]");

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
    }
}