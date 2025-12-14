using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddContainer("sqlserver", image: "mcr.microsoft.com/mssql/server:2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "${SQL_SA_PASSWORD:-Your_password123}")
    .WithEndpoint(port: 1433, name: "tcp");

var rabbit = builder.AddContainer("rabbitmq", image: "rabbitmq:3.13-management")
    .WithEndpoint(port: 5672, name: "amqp")
    .WithEndpoint(port: 15672, name: "management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "${RABBITMQ_USERNAME:-admin}")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "${RABBITMQ_PASSWORD:-rabbit@123}");

var mongo = builder.AddContainer("mongodb", image: "mongo:7.0")
    .WithEndpoint(port: 27017, name: "mongodb");

var otel = builder.AddContainer("otel-collector", image: "otel/opentelemetry-collector:0.116.0")
    .WithBindMount("./infra/otel-collector-config.yaml", "/etc/otelcol/config.yaml")
    .WithEndpoint(port: 4317, name: "otlp");

var gateway = builder.AddProject("gateway", "../Gateway/Gateway.csproj").WithExternalHttpEndpoints();
var ui = builder.AddProject("ui", "../Ui.BlazorServer/Ui.BlazorServer.csproj");
var elsa = builder.AddProject("elsa", "../Elsa.Server/Elsa.Server.csproj");

var arc = builder.AddProject("arc", "../../services/arc.api/ARC.Api.csproj");
var dar = builder.AddProject("dar", "../../services/dar.api/DAR.Api.csproj");
var dba = builder.AddProject("dba", "../../services/dba.api/DBA.Api.csproj");
var bud = builder.AddProject("bud", "../../services/bud.api/BUD.Api.csproj");
var ctr = builder.AddProject("ctr", "../../services/ctr.api/CTR.Api.csproj");
var cos = builder.AddProject("cos", "../../services/cos.api/COS.Api.csproj");
var rap = builder.AddProject("rap", "../../services/rap.api/RAP.Api.csproj");
var rde = builder.AddProject("rde", "../../services/rde.api/RDE.Api.csproj");
var rep = builder.AddProject("rep", "../../services/rep.api/REP.Api.csproj");
var rre = builder.AddProject("rre", "../../services/rre.api/RRE.Api.csproj");
var rst = builder.AddProject("rst", "../../services/rst.api/RST.Api.csproj");

var otlpEndpoint = otel.GetEndpoint("otlp");
var rabbitEndpoint = rabbit.GetEndpoint("amqp");
var mongoConnectionString = $"mongodb://{mongo.Resource.Name}:27017";

var services = new[] { arc, dar, dba, bud, ctr, cos, rap, rde, rep, rre, rst, gateway, ui, elsa };

foreach (var service in services)
{
    service.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", otlpEndpoint);
    service.WithEnvironment("RABBITMQ__HOST", rabbitEndpoint);
    service.WithEnvironment("MONGODB__CONNECTIONSTRING", mongoConnectionString);
}

gateway.WithReference(ui).WithReference(elsa);
ui.WithReference(gateway);

builder.Build().Run();
