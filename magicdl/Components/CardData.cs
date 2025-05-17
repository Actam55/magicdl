// File: magicdl.Components.Data.CardData.cs

using System.Collections.Generic;
using magicdl.Components.Classes;

namespace magicdl.Components
{
    public static class CardData
    {
        public static List<MTGCard> GetDummyCards()
        {
            return new List<MTGCard>
            {
                new MTGCard
                {
                    Name = "Lightning Bolt",
                    ImageUrl = "https://example.com/bolt.jpg",
                    SetApperances = new List<string> {"Alpha", "Beta", "Revised"},
                    Rarity = "Common",
                    Type = new List<string> {"Instant"},
                    Color = new List<string> {"Red" },
                    CMC = "1"
                },
                new MTGCard
                {
                    Name = "Black Lotus",
                    ImageUrl = "https://example.com/lotus.jpg",
                    SetApperances = new List<string> {"Alpha"},
                    Rarity = "Rare",
                    Type = new List<string> {"Artifact"},
                    Color = new List<string> {"Colorless"},
                    CMC = "0"
                },
                new MTGCard
                {
                    Name = "Counterspell",
                    ImageUrl = "https://example.com/counter.jpg",
                    SetApperances = new List<string> {"Revised"},
                    Rarity = "Uncommon",
                    Type = new List<string> {"Instant" },
                    Color = new List < string > { "Blue" },
                    CMC = "2"
                }
                // Add more as needed
            };
        }
    }
}
