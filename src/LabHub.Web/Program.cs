using LabHub.Web;

var builder = WebApplication.CreateBuilder(args);
Startup.ConfigureServices(builder);

var app = builder.Build();
Startup.ConfigurePipeline(app);
Startup.LoadInitialContent(app);

app.Run();

/// <summary>Маркер для интеграционных тестов.</summary>
public partial class Program;
