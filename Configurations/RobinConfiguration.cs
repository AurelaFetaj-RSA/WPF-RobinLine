using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_RobinLine.Configurations
{
    public class RobinConfiguration
    {
        // Robot Parameters
        public bool R1Inclusion { get; set; } = false;
        public bool R2Inclusion { get; set; } = false;

        // Oven Parameters
        public bool Oven1Inclusion { get; set; } = false;
        public bool Oven2Inclusion { get; set; } = false;
        public int BeltSpeed { get; set; } = 2;

        // Oven 1 Settings
        public int Oven1TempSetpoint { get; set; } = 0;
        public int Oven1FanPercentage { get; set; } = 0;
        public int Oven1LampsPercentage { get; set; } = 0;

        // Oven 2 Settings
        public int Oven2TempSetpoint { get; set; } = 0;
        public int Oven2FanPercentage { get; set; } = 0;
        public int Oven2LampsPercentage { get; set; } = 0;
    }
}
