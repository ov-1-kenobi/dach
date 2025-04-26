using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
//     .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options => {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.Events ??= new OpenIdConnectEvents();
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api/file"))//TODO:KO; need to move auth and login pieces to auth controller so this works out.. either errors work OR 'sign in' currently
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.HandleResponse();
            }
            return Task.CompletedTask;
        };
    })//builder.Configuration.GetSection("AzureAdB2C"))
    .EnableTokenAcquisitionToCallDownstreamApi() // Enables token acquisition for downstream APIs
    .AddInMemoryTokenCaches(); // Caches tokens in memory for reuse


builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null;});

if (environmentName == "Development")
{
    builder.Services.AddSingleton(new BlobServiceClient("UseDevelopmentStorage=true"));
    builder.Services.AddSingleton(new TableServiceClient("UseDevelopmentStorage=true"));
    //builder.Services.AddSignalR();
}
else
{
    var storageConnectionString = configuration["StorageAccountConnectionString"];
    builder.Services.AddSingleton(new BlobServiceClient(storageConnectionString));
    builder.Services.AddSingleton(new TableServiceClient(storageConnectionString));

    //var signalRConnectionString = configuration["AzureSignalRConnectionString"];
    //builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
}
builder.Services.AddCors(options => {
    options.AddPolicy("AllowSpecificOrigin",
        builder => {
            builder.WithOrigins("https://localhost:7140") // Replace with your allowed origin
                   .AllowAnyHeader()
                   .AllowAnyMethod(); // Allow credentials if needed
        });
});
var app = builder.Build();

// Middleware to handle authentication and authorization
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403 || context.Response.StatusCode == 302)
    {
        context.Response.Headers.Append("HX-Retarget", "#error-message");
        context.Response.Headers.Append("HX-Reswap", "innerHTML");
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST");
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

/** setup to use static/htmx content from wwwroot **/
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
