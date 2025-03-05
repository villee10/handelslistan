namespace server.Models
{
    public class list_item
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public bool Checked { get; set; } = false;
    }
}