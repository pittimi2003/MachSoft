var builder = DistributedApplication.CreateBuilder(args);
var environmentName = builder.Configuration.GetSection("KubernetesValues")["KubernetesEnvironment"] ?? "dev";
var apiService = builder.AddProject<Projects.Mss_WorkForce_Code_ApiService>($"apiservice-{environmentName}");

builder.AddProject<Projects.Mss_WorkForce_Code_Web>($"webfrontend-{environmentName}")
	.WithExternalHttpEndpoints()
	.WithReference(apiService);

builder.AddProject<Projects.Mss_WorkForce_Code_Simulator>($"mss-workforce-code-simulator-{environmentName}", launchProfileName: null)
    .WithHttpsEndpoint(
        name: "simulator-https",
        port: 6001,
        isProxied: false)
    .WithEnvironment("ASPNETCORE_URLS", "https://0.0.0.0:6001");

builder.AddProject<Projects.Mss_WorkForce_Code_Simulator>($"mss-workforce-code-simulator-job-{environmentName}", launchProfileName: null)
    .WithHttpsEndpoint(
        name: "simulator-job-https",
        port: 6002,
        isProxied: false)
    .WithEnvironment("ASPNETCORE_URLS", "https://0.0.0.0:6002");

builder.AddProject<Projects.Mss_WorkForce_Code_DataBaseManager>($"mss-workforce-code-databasemanager-{environmentName}");

builder.AddProject<Projects.Mss_WorkForce_Code_WFMConnector>($"mss-workforce-code-wfmconnector-{environmentName}");

builder.AddProject<Projects.Mss_WorkForce_Code_WMSSimulator>($"mss-workforce-code-wmssimulator-{environmentName}");

builder.AddProject<Projects.Mss_WorkForce_Code_WMSSimulatorWeb>($"mss-workforce-code-wmssimulatorweb-{environmentName}");

builder.AddProject<Projects.Mss_WorkForce_Code_HubSignalR>($"mss-workforce-code-signalrhub-{environmentName}");

builder.Build().Run();


