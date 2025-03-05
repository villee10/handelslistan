namespace server.Models
{
    public class shopping_list
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OwnerId { get; set; }
    }
}