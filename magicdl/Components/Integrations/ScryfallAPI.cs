using BlazorBootstrap;
using magicdl.Components.Classes;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace magicdl.Components.Data
{
    public class ScryfallAPI
    {
        private List<MTGCard> cardList = new List<MTGCard>();
        private List<MTGCard> guessHistory = new List<MTGCard>();
        private MTGCard currentGuess = new MTGCard();
        private MTGCard correctCard = new MTGCard();
        private string currentGuessString = string.Empty;
        private HttpResponseMessage response = new HttpResponseMessage();
        private string json = string.Empty;
        private int cardsFound;
        private List<string> keywords = new List<string>();
        private bool disabled = false;
        private bool showSuccessAlert = false;
        private bool hintGotten = false;

        public enum MatchLevel
        {
            None,
            Partial,
            Full
        }

        private async Task<AutoCompleteDataProviderResult<MTGCard>> CardListProvider(AutoCompleteDataProviderRequest<MTGCard> request)
        {
            string filterText = request.Filter.Value;
            Console.WriteLine($"User typed: {filterText}");

            var filteredCardList = cardList
                .Where(card => card.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(card =>
                {
                    var name = card.Name;

                    if (name.Equals(filterText, StringComparison.OrdinalIgnoreCase))
                        return 0;
                    else if (name.StartsWith(filterText, StringComparison.OrdinalIgnoreCase))
                        return 1;
                    else if (name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                        return 2;
                    else
                        return 3;
                })
                .ThenBy(card =>
                {
                    // Secondary sort by index of match, so 'Bolt' appears before 'Firebolt' if search is 'bolt'
                    int index = card.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase);
                    return index == -1 ? int.MaxValue : index;
                })
                .ThenBy(card => card.Name) // Final fallback alphabetical
                .ToList();

            return await Task.FromResult(request.ApplyTo(filteredCardList));
        }

        private void OnAutoCompleteChanged(MTGCard card)
        {
            if (card is null || string.IsNullOrEmpty(card.Name))
            {
                // Display message or toast
                Console.WriteLine("Card not found.");
                return;
            }

            currentGuess = card;
            guessHistory.Insert(0, currentGuess);
            cardList.Remove(currentGuess);
            currentGuessString = string.Empty;
            if (currentGuess.Name == correctCard.Name)
            {
                disabled = true;
                showSuccessAlert = true;
            }
        }

        public MatchLevel GetMatchLevel(List<string> guessList, List<string> correctList)
        {
            if (guessList == null || correctList == null || guessList.Count == 0)
                return MatchLevel.None;

            bool exactMatch = guessList.All(item => correctList.Contains(item))
                   && correctList.All(item => guessList.Contains(item));
            bool anyMatch = guessList.Any(item => correctList.Contains(item));

            if (exactMatch)
                return MatchLevel.Full;
            else if (anyMatch)
                return MatchLevel.Partial;
            else
                return MatchLevel.None;
        }

        public string MatchLevelToCssClass(MatchLevel level) => level switch
        {
            MatchLevel.Full => "bg-success",
            MatchLevel.Partial => "bg-warning",
            _ => "bg-danger"
        };

        public void CardGuessChanged(string value)
        {
            currentGuess = SetCard(value);
            if (string.IsNullOrEmpty(currentGuess.Name))
            {
                //Give a message that the card was not found.
                return;
            }

            guessHistory.Insert(0, currentGuess);
            currentGuessString = string.Empty;

            //StateHasChanged();
        }

        public MTGCard SetCard(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new MTGCard();
            }

            foreach (MTGCard card in cardList)
            {
                if (value.Equals(card.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return card;
                }
            }
            return new MTGCard();
        }

        public async Task<(List<MTGCard>, MTGCard)> GetCardListAsync()
        {
            guessHistory = new List<MTGCard>();
            cardList = new List<MTGCard>();
            Random rnd = new Random();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Magicdl/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd(("application/json;q=0.9,*/*;q=0.8"));
            response = await client.GetAsync("https://api.scryfall.com/cards/search?order=edhrec&dir=asc&q=id%3Awubrg+-t=land+prefer%3Aoldest&page=1");
            json = await response.Content.ReadAsStringAsync();

            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;
                foreach (JsonElement cardElement in root.GetProperty("data").EnumerateArray())
                {
                    MTGCard card = new MTGCard();
                    card.Name = cardElement.GetProperty("name").GetString();
                    card.CMC = cardElement.GetProperty("cmc").GetDouble();
                    string typeLine = cardElement.GetProperty("type_line").GetString();
                    string beforeDash = typeLine.Split('—')[0].Trim();
                    card.Type = beforeDash.Split(' ').ToList();
                    card.Color = cardElement.GetProperty("color_identity").EnumerateArray().Select(c => c.GetString()).ToList();
                    if (card.Color.Count == 0)
                    {
                        card.Color.Add("Colorless");
                    }
                    string releaseDateStr = cardElement.GetProperty("released_at").GetString();
                    if (DateOnly.TryParse(releaseDateStr, out DateOnly parsedDate))
                    {
                        card.ReleaseDate = parsedDate;
                    }
                    else
                    {
                        card.ReleaseDate = null;
                    }
                    if (cardElement.TryGetProperty("keywords", out JsonElement keywordsElement))
                    {
                        List<string?> keywords = keywordsElement.EnumerateArray().Select(k => k.GetString()).Where(k => !string.IsNullOrEmpty(k)).ToList();

                        card.Keywords = keywords.Any() ? keywords : new List<string> { "None" };
                    }
                    else
                    {
                        card.Keywords = new List<string> { "None" };
                    }


                    card.Oracle_Text = cardElement.GetProperty("oracle_text").GetString();

                    if (cardElement.TryGetProperty("image_uris", out JsonElement imageUris))
                    {
                        if (imageUris.TryGetProperty("art_crop", out JsonElement imageUrl))
                        {
                            card.ImageUrl = imageUrl.GetString();
                        }
                    }
                    cardList.Add(card);
                }
            }
            int randomIndex = rnd.Next(0, cardList.Count);
            correctCard = cardList[randomIndex];
            hintGotten = false;
            return (cardList, correctCard);
        }
    }
}
