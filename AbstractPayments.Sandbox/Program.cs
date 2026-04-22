var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var api = app.MapGroup("/v1/api/payments");

api.MapPost("/", () =>
{
  return Results.Ok(new { Status = "Processing" });
});

api.MapGet("/{id}", (string id) =>
{
  return Results.Ok(new { Id = id, Status = "Completed" });
});

app.Run();
