namespace server.Models
{
    public class ListItem
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; } = 1;
        public bool Checked { get; set; } = false;
    }
}