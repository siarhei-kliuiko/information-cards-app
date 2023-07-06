using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace InformationCardsServer
{
    public class InformationCard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] ImageData { get; set; }
    }

    public class InformationCardsStorage
    {
        private static readonly JSchema storageJsonSchema = new JSchemaGenerator().Generate(typeof(List<InformationCard>));
        private static readonly string storageFilePath = Path.Combine(AppContext.BaseDirectory, "storage.json");

        private static bool TryReadStorageFile(out string storageReadResult)
        {
            storageReadResult = null;
            string jsonText = null;
            if (File.Exists(storageFilePath))
            {
                try
                {
                    jsonText = File.ReadAllText(storageFilePath);
                }
                catch (Exception ex)
                {
                    storageReadResult = ex.Message;
                }

                if (jsonText != null)
                {
                    if (jsonText != String.Empty)
                    {
                        JArray cards = null;
                        try
                        {
                            cards = JArray.Parse(jsonText);
                        }
                        catch
                        {
                            storageReadResult = "Storage file is corrupted";
                        }

                        if (cards != null)
                        {
                            if (cards.IsValid(storageJsonSchema, out IList<string> errors))
                            {
                                storageReadResult = jsonText;
                                return true;
                            }
                            else
                            {
                                storageReadResult = string.Join(Environment.NewLine, errors);
                            }
                        }
                    }
                    else
                    {
                        storageReadResult = "[]";
                        return true;
                    }
                }
            }
            else
            {
                storageReadResult = "Storage file not found";
            }

            return false;
        }

        private static void RewriteStorageFile(IEnumerable<InformationCard> cardsToWrite)
        {
            File.WriteAllText(storageFilePath, JsonConvert.SerializeObject(cardsToWrite));
        }

        public static IResult GetCards()
        {
            string storageReadResult;
            if (!TryReadStorageFile(out storageReadResult))
            {
                return Results.NotFound(storageReadResult);
            }
            else
            {
                return Results.Ok(storageReadResult);
            }
        }

        public static IResult CreateCard(InformationCard card)
        {
            string storageReadResult;
            if (!TryReadStorageFile(out storageReadResult))
            {
                return Results.NotFound(storageReadResult);
            }

            List<InformationCard> cards;
            cards = JsonConvert.DeserializeObject<List<InformationCard>>(storageReadResult);
            card.Id = cards.Count == 0 ? 0 : cards.Max(card => card.Id) + 1;

            cards.Add(card);
            RewriteStorageFile(cards);

            return Results.Ok();
        }

        public static IResult UpdateCard(InformationCard editedCard)
        {
            string storageReadResult;
            if (!TryReadStorageFile(out storageReadResult))
            {
                return Results.NotFound(storageReadResult);
            }

            var cards = JsonConvert.DeserializeObject<List<InformationCard>>(storageReadResult);
            var cardToEdit = cards.SingleOrDefault(card => card.Id == editedCard.Id);
            if (cardToEdit != null)
            {
                cardToEdit.Name = editedCard.Name;
                cardToEdit.ImageData = editedCard.ImageData;
                RewriteStorageFile(cards);
                return Results.Ok();
            }
            else
            {
                return Results.NotFound("Information card is not found");
            }
        }

        public static IResult RemoveCard(int id)
        {
            string storageReadResult;
            if (!TryReadStorageFile(out storageReadResult))
            {
                return Results.NotFound(storageReadResult);
            }

            var cards = JsonConvert.DeserializeObject<List<InformationCard>>(storageReadResult);
            RewriteStorageFile(cards.Where(card => card.Id != id));
            return Results.Ok();
        }
    }
}
