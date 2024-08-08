using System.Net.Http.Headers;
using System.Text.Json;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using OfficeOpenXml;

namespace DevOps
{
    public class DevOps
    {
        public string projectUrl { get; set; } = $"_apis/projects?api-version=6.0";

        public required Projects Projects { get; set; }

        public int CellValue { get; set; }

        public required ExcelWorksheet Worksheet { get; set; }

        public Api api { get; set; } = new Api("Organization", "PersonalAccessToken");

        public async Task Invoke()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            Worksheet = package.Workbook.Worksheets.Add("Repository_AppSvcId");

            // Add the headers
            Worksheet.Cells[1, 1].Value = "Project";
            Worksheet.Cells[1, 2].Value = "Repository";
            Worksheet.Cells[1, 3].Value = "DisplayName";
            Worksheet.Cells[1, 4].Value = "Technology";
            Worksheet.Cells[1, 5].Value = "Version";

            CellValue = 2;

            await GetProjects();
            await GetRepository();

            // Save the file in the current directory
            var folderPath = Directory.GetCurrentDirectory();
            var fileName = "Output.xlsx";
            var fullPath = Path.Combine(folderPath, fileName);

            // Ensure the directory exists
            Directory.CreateDirectory(folderPath);
            var file = new FileInfo(fullPath);
            package.SaveAs(file);
        }

        public async Task GetProjects()
        {
            var api = new Api("organization", "PersonalAccessToken");
            // Invoke the Api.Invoke method to get the project details
            var response = await api.Invoke(projectUrl);
            var projects = JsonSerializer.Deserialize<Projects>(response);
        }

        public async Task GetRepository()
        {
            foreach (var project in Projects.value)
            {
                // Get the repository details
                string repositoryUrl = $"{project.id}/_apis/git/repositories?api-version=6.0";

                var repoResponse = await api.Invoke(repositoryUrl);
                var repo = JsonSerializer.Deserialize<Respositories>(repoResponse);

                foreach (var repository in repo.value)
                {
                    string filePath = "/asset.yml";
                    string reposUrl = $"{project.id}/_apis/git/repositories/{repository.id}/items?path={filePath}&includeContent=true&api-version=6.0";
                    var content = await api.Invoke(reposUrl);
                    //
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance) // Use camel case
                            .Build();

                        var items = deserializer.Deserialize<List<Config>>(content);

                        foreach (var item in items)
                        {
                            // Add the data
                            Worksheet.Cells[CellValue, 1].Value = project.name;
                            Worksheet.Cells[CellValue, 2].Value = repository.name;
                            Worksheet.Cells[CellValue, 3].Value = item.displayName;
                            Worksheet.Cells[CellValue, 4].Value = item.technology;
                            Worksheet.Cells[CellValue, 5].Value = item.version;

                            CellValue++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve file content. Status code: " + repository.name);
                    }
                }
            }
        }

    }
}


public class Api(string org, string pat)
{
    // Generic method to make a GET request
    public async Task<string> Invoke(string url)
    {
        string baseUrl = $"https://dev.azure.com/{org}";

        using HttpClient client = new();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}")));

        HttpResponseMessage response = await client.GetAsync($"{baseUrl}/{url}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return string.Empty;
        }
    }
}


public class Projects
{
    public int count { get; set; }
    public Project[] value { get; set; }
}

public class Project
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string state { get; set; }
    public int revision { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
}


public class Respositories
{
    public Repository[] value { get; set; }
    public int count { get; set; }
}

public class Repository
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public Project project { get; set; }
    public string defaultBranch { get; set; }
    public int size { get; set; }
    public string remoteUrl { get; set; }
    public string sshUrl { get; set; }
    public string webUrl { get; set; }
    public bool isDisabled { get; set; }
    public bool isInMaintenance { get; set; }
}

public class Config
{
    public MetaData metaData { get; set; }
    public string displayName { get; set; }
    public string technology { get; set; }
    public int version { get; set; }

}

public class MetaData
{
    public string displayName { get; set; }
    public string technology { get; set; }
    public int version { get; set; }
}


