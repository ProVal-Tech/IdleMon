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
        public string WriteIdleTime() {
            string idleJson = JsonSerializer.Serialize(new IdleInfo());
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            File.WriteAllText($"{userPath}/idletime.json", idleJson);
            return idleJson;
        }

        public async static Task Uninstall() {
            _ = await Cli.Wrap("powershell")
            .WithArguments(new[] { "-c", $"Get-Service {SERVICE_NAME}* | Stop-Service -PassThru | % {{ sc.exe delete $_.Name }}" })
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
            _ = await Cli.Wrap("sc")
                .WithArguments(new[] { "delete", SERVICE_NAME })
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
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
