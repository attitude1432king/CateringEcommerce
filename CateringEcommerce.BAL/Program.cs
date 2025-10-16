using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.BAL.DatabaseHelper;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.Run();