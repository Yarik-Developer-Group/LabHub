using LabHub.Web;

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureServices(builder);

var app = builder.Build();
Startup.ConfigurePipeline(app);
Startup.LoadInitialContent(app);

app.Run();

public partial class Program;
