namespace WPF_RobinLine.Models
{
    public class ProductionSummary
    {
        public DateTime ProductionDate { get; set; }
        public string ProductionHourRange { get; set; }
        public int ItemsProduced { get; set; }
        public int DefectiveItems { get; set; }
        public string ShiftName { get; set; }
    }
}
