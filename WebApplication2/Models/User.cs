namespace Trainacc.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Port { get; set; }
        public required string NumberType { get; set; }
    }
}
