var builder = DistributedApplication.CreateBuilder(args);

// Credenciales fijas de desarrollo (admin/admin) para conectarse también desde DataGrip.
var pgUser = builder.AddParameter("postgres-user", "admin");
var pgPassword = builder.AddParameter("postgres-password", "admin", secret: true);

var postgres = builder.AddPostgres("postgres", pgUser, pgPassword)
    .WithImageTag("17.6")
    .WithDataVolume()
    .WithHostPort(5432);

var db = postgres.AddDatabase("gestionequipodb");

var api = builder.AddProject<Projects.GestionEquipo_Api>("api")
    .WithReference(db)
    .WithHttpEndpoint(port: 5080, name: "http")
    .WaitFor(db);

builder.AddJavaScriptApp("ui", "../../teamhub-ui", "start")
    .WithReference(api)
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
