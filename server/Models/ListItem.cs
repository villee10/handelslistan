namespace server.Models // Samma namespace som User
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