using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Principal;
using CliWrap;

namespace IdleMon {
    public sealed class IdleService {
        public static string SERVICE_NAME { get; } = "IdleMon";
        public string IDLE_FILE { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/idletime.json";
        public string WriteIdleFile() {
            string idleJson = JsonSerializer.Serialize(new IdleInfo());
            File.WriteAllText(IDLE_FILE, idleJson);
            return idleJson;
        }

        public void RemoveIdleFile() {
            try {
                File.Delete(IDLE_FILE);
            } finally { }
        }

        public static string GetIdleReport() {
            string usersPath = $@"{Environment.GetEnvironmentVariable("HOMEDRIVE") ?? @"C:"}\Users";
            List<IdleInfo?> idleFiles = Directory.GetDirectories(usersPath, "*", enumerationOptions: new EnumerationOptions() { IgnoreInaccessible = true })
                .SelectMany(d => Directory.GetFiles(d, "idletime.json", enumerationOptions: new EnumerationOptions() { IgnoreInaccessible = true }))
                .Select(f => JsonSerializer.Deserialize<IdleInfo>(File.ReadAllText(f)))
                .ToList();
            return JsonSerializer.Serialize(idleFiles);
        }

        public async static Task Uninstall() {
            _ = await Cli.Wrap("powershell")
            .WithArguments(new[] { "-c", $"Get-Service {SERVICE_NAME}* | Stop-Service -PassThru | % {{ sc.exe delete $_.Name }}" })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
            CommandResult result = await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", SERVICE_NAME })
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            /*
             * Exit code 0 indicates successful removal.
             * Exit code 1060 indicates that no service exists with that name.
             */
            switch (result.ExitCode) {
                case 0:
                    Console.WriteLine("Successfully uninstalled IdleMon service.");
                    break;
                case 1060:
                    break;
                default:
                    throw new Exception("Failed to uninstall IdleMon service.");
            }
        }
    }

    public sealed class IdleInfo {
        public string Username { get; } = Environment.UserName;
        public string Domain { get; } = Environment.UserDomainName;
        public TimeSpan IdleTime { get; } = UserInput.IdleTime;
        public bool IsAdmin { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        public bool IsDomainAdmin { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole("Domain Admins");
        public bool IsEnterpriseAdmin { get; } = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole("Enterprise Admins");
    }
}