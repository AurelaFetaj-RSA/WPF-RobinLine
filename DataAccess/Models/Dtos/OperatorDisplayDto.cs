namespace WPF_RobinLine.DataAccess.Models.Dtos
{
    public class OperatorDisplayDto
    {
        public int? Id { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        //public DateTime? HiredDate { get; set; }

        private DateTime? _hiredDate;
        public DateTime? HiredDate
        {
            get => _hiredDate?.Date; 
            set => _hiredDate = value;
        }
    }
}
