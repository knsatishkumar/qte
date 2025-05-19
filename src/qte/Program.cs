using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


string yourGitHubAppName = "qte";
string githubCopilotCompletionsUrl = 
    "https://api.githubcopilot.com/chat/completions";

app.MapGet("/", () => "Hello World!");

app.MapPost("/agent", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken,
    [FromBody] Request userRequest) =>
{
    var octokitClient =
        new GitHubClient(
            new Octokit.ProductHeaderValue(yourGitHubAppName))
        {
            Credentials = new Credentials(githubToken)
        };
    var user = await octokitClient.User.Current();

    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content =
                "Start every response with the user's name, " +
                $"which is @{user.Login}"
    });
    userRequest.Messages.Insert(0, new Message
    {
        Role = "system",
        Content =
            "You are an expert Quality and Test Engineering Expert and a helpful assistant that replies to " +
            "user messages with high quality and test engineering knowledge.And Recommendations using organization " +
            " approved tools are Playwright, Selenium, Karate, Reqnroll, Cucumber, Blazemeter and best practices. "
    });
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", githubToken);
    userRequest.Stream = true;

    var copilotLLMResponse = await httpClient.PostAsJsonAsync(
    githubCopilotCompletionsUrl, userRequest);
    
    var responseStream = 
    await copilotLLMResponse.Content.ReadAsStreamAsync();
    return Results.Stream(responseStream, "application/json");
});



app.Run();

