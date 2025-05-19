namespace magicdl.Components.Classes
{
    public class MTGCard
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string CMC { get; set; } = string.Empty;
        public List<string> SetApperances { get; set; } = new List<string>();
        public DateOnly? ReleaseDate { get; set; } = null;
        public List<string> Color { get; set; } = new List<string>();
        public List<string> Type { get; set; } = new List<string>();
        public string? Oracle_Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }
    }
}
