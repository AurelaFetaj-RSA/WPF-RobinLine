namespace WPF_RobinLine.DataAccess.Models.Dtos
{
    public class ShiftDisplayDto
    {
        public int? Id { get; set; }
        public int No { get; set; }
        public string Description { get; set; }
        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        public int? Target { get; set; }
    }
}
