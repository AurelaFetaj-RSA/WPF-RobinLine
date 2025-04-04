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
        public bool R1Inclusion { get; set; } = true;
        public bool R2Inclusion { get; set; } = true;

        // Oven Parameters
        public bool Oven1Inclusion { get; set; } = true;
        public bool Oven2Inclusion { get; set; } = true;
        public int BeltSpeed { get; set; } = 100;

        // Oven 1 Settings
        public int Oven1TempSetpoint { get; set; } = 150;
        public int Oven1FanPercentage { get; set; } = 80;
        public int Oven1LampsPercentage { get; set; } = 60;

        // Oven 2 Settings
        public int Oven2TempSetpoint { get; set; } = 180;
        public int Oven2FanPercentage { get; set; } = 70;
        public int Oven2LampsPercentage { get; set; } = 50;
    }
}
