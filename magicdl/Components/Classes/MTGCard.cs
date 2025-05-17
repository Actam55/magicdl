namespace magicdl.Components.Classes
{
    public class MTGCard
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string CMC { get; set; } = string.Empty;
        public List<string> SetApperances { get; set; } = new List<string>();
        public List<string> Color { get; set; } = new List<string>();
        public List<string> Type { get; set; } = new List<string>();
    }
}
