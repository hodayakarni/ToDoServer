using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using TodoApi;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();//swagger
//cors
builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.WithOrigins("http://localhost:3000")
        .AllowAnyHeader() 
        .AllowAnyMethod();
}));
   
builder.Services.AddSingleton<ToDoDbContext>();//הזרקה


//swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
    });
});

var app = builder.Build();

app.UseCors("MyPolicy");//cors

app.UseSwagger();//swagger
//swagger
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.MapGet("/items", (ToDoDbContext context) =>
{
    return context.Items.ToList();
});
app.MapPost("/items", async(ToDoDbContext context, Item item)=>{
    context.Add(item);
    await context.SaveChangesAsync();
    return item;
});
app.MapPut("/items/{id}", async(ToDoDbContext context, [FromBody]Item item, int id)=>{
    var updateItem = await context.Items.FindAsync(id);
    if(updateItem is null) return Results.NotFound();
    updateItem.IsComplete = item.IsComplete;
    await context.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/items/{id}", async(ToDoDbContext context, int id)=>{
    var deleteItem = await context.Items.FindAsync(id);
    if(deleteItem is null) return Results.NotFound();
    context.Items.Remove(deleteItem);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
