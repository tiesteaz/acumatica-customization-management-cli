using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcumaticaCustomizationManagementCLI
{
    sealed class AppOptions
    {
        private static volatile AppOptions? instance;
        public static AppOptions Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppOptions();
                }
                return instance;
            }
        }

        public string? Command { get; set; }
        public string? AcumaticaSite { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int ReplayDBScripts { get; set; }
        public string? ProjectListPath { get; set; }
        public string? CustomizationsFolderPath { get; set; }
        public void Validate(IList<string> extraArgs)
        {
            var exceptions = new List<Exception>();

            if (extraArgs != null && extraArgs.Count > 0)
            {
                var unrecognizedArgsString = string.Join(" ", extraArgs);
                var message = $"Error: Unrecognized arguments string \"{unrecognizedArgsString}\"";
                exceptions.Add(new ApplicationException(message));
            }

            if (
                (Command == "unpublish" || Command == "getPublished") && (
                string.IsNullOrWhiteSpace(AcumaticaSite) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password)
                ))
            {
                exceptions.Add(new ApplicationException("Error: AcumaticaSite, Username, Password are mandatory parameters for 'unpublish' and 'getPublished' commands."));
            }

            if (
                (Command == "import" ) && (
                string.IsNullOrWhiteSpace(AcumaticaSite) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(CustomizationsFolderPath)
                ))
            {
                exceptions.Add(new ApplicationException("Error: AcumaticaSite, Username, Password, customizationsFolderPath are mandatory parameters for 'import' command."));
            }

            if (
                (Command == "publish") && (
                string.IsNullOrWhiteSpace(AcumaticaSite) ||
                string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ProjectListPath)
                ))
            {
                exceptions.Add(new ApplicationException("Error: AcumaticaSite, Username, Password, ProjectListPath are mandatory and ReplayDBScripts is optional parameter for 'publish' command."));
            }

            // throw exception if needed
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
            else if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
