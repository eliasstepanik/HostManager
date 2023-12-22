using AutoProxy.Server.Services;
var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
/*builder.Services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase(databaseName: "DataContext"))*/


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<DockerService>();
app.MapGrpcService<ProxmoxService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client");

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();