using InformationCardsServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/cards", () => InformationCardsStorage.GetCards());
app.MapPost("/cards", (InformationCard card) => InformationCardsStorage.CreateCard(card));
app.MapPut("/cards", (InformationCard card) => InformationCardsStorage.UpdateCard(card));
app.MapDelete("/cards/{id}", (int id) => InformationCardsStorage.RemoveCard(id));

app.Run("https://localhost:7271");