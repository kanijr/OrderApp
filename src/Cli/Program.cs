using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;

var information = new
{
  Application = "OrderApp",
  Student = "Демчишин Тарас",
  Group = "ФЕІ-36",

  OSDescription = RuntimeInformation.OSDescription,
  Environment = Environment.OSVersion.ToString(),
  Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
  DotNetVersion = Environment.Version.ToString(),
  Runtime = RuntimeInformation.FrameworkDescription,
  AppDirectory = AppContext.BaseDirectory,
  CurrentDirectory = Environment.CurrentDirectory,

  SubjectArea = "Замовлення",
  Entities = "клієнт, товар, замовлення, рядок замовлення"
};

if (args.Contains("--json"))
{
  var options = new JsonSerializerOptions
  {
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  Console.WriteLine(JsonSerializer.Serialize(information, options));
}
else
{
  Console.WriteLine($"{information.Application} – практикум з крос-платформного програмування");
  Console.WriteLine($"Студент: {information.Student}, група {information.Group}");
  Console.WriteLine(new string('-', 52));

  Console.WriteLine($"ОС (OSDescription) : {information.OSDescription}");
  Console.WriteLine($"ОС (Environment) : {information.Environment}");
  Console.WriteLine($"Архітектура процесу : {information.Architecture}");
  Console.WriteLine($"Версія .NET (CLR) : {information.DotNetVersion}");
  Console.WriteLine($"Runtime : {information.Runtime}");
  Console.WriteLine($"Каталог застосунку : {information.AppDirectory}");
  Console.WriteLine($"Поточний каталог : {information.CurrentDirectory}");

  Console.WriteLine(new string('-', 52));

  Console.WriteLine($"Предметна область: {information.SubjectArea} ({information.Entities})");
}