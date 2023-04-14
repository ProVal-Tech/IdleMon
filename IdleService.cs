using System.Text.Json;
using System.Security.Principal;
using CliWrap;
using System.DirectoryServices.AccountManagement;

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
            List<IdleInfo?> idleFiles = Directory.GetDirectories(usersPath, "*", new EnumerationOptions() { IgnoreInaccessible = true })
                .SelectMany(d => Directory.GetFiles(d, "idletime.json", new EnumerationOptions() { IgnoreInaccessible = true }))
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
        public string Username { get; set; }
        public string Domain { get; set; }
        public TimeSpan IdleTime { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDomainAdmin { get; set; }
        public bool IsEnterpriseAdmin { get; set; }

        public IdleInfo() {
            Username = Environment.UserName;
            Domain = Environment.UserDomainName;
            IdleTime = UserInput.IdleTime;
            IsAdmin = false;
            IsDomainAdmin = false;
            IsEnterpriseAdmin = false;

            using PrincipalContext machineContext = new(ContextType.Machine);
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();
            IsAdmin = IsMember(machineContext, "Administrators", currentUser.User);
            try {
                using PrincipalContext domainContext = new(ContextType.Domain);
                IsDomainAdmin = IsMember(domainContext, "Domain Admins", currentUser.User);
                IsEnterpriseAdmin = IsMember(domainContext, "Enterprise Admins", currentUser.User);
                if (IsDomainAdmin || IsEnterpriseAdmin) {
                    IsAdmin = true;
                    return;
                }
            } catch { }
        }

        private static bool IsMember(PrincipalContext context, string groupName, SecurityIdentifier? userSid) {
            try {
                GroupPrincipal? targetGroup = new(context) { SamAccountName = groupName };
                PrincipalSearcher pSearcher = new(targetGroup);
                targetGroup = pSearcher.FindOne() as GroupPrincipal;
                if (targetGroup != null) {
                    return targetGroup.GetMembers().Select(m => m.Sid).Contains(userSid);
                }
                return false;
            } catch {
                return false;
            }
        }
    }

}