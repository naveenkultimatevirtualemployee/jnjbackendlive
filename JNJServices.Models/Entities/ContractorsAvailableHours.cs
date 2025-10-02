using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JNJServices.Models.Entities
{
    public class ContractorsAvailableHours
    {
        public int AvailableDayNum { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

    }
}
