using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PR24_2017_PZ3.Model
{
    public class SwitchEntity : PowerEntity
    {
        private string status;

        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }
    }
}
