using Acumatica.Auth.Api;
using Acumatica.Auth.Model;
using Acumatica.RESTClient.CustomizationApi;
using Acumatica.RESTClient.CustomizationApi.Model;
using Mono.Options;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;

namespace AcumaticaCustomizationManagementCLI
{
    public class Program
    {
        private const int ExitCodeOK = 0;
        private const int ExitCodeError = 1;
        private static OptionSet optionSet;

        public static int Main(string[] args)
        {
            if (!TryParseArguments(args))
            {
                return ExitCodeError;
            }

            string command = AppOptions.Instance.Command;
            string siteURL = AppOptions.Instance.AcumaticaSite;
            string username = AppOptions.Instance.Username;
            string password = AppOptions.Instance.Password;
            int replayDBScripts = AppOptions.Instance.ReplayDBScripts;
            string projectListPath = AppOptions.Instance.ProjectListPath;
            string customizationsFolderPath = AppOptions.Instance.CustomizationsFolderPath;

            Console.WriteLine(String.Format("Started CLI task with command: {0}", command));
            Console.WriteLine("");

            if (command == "import" || command == "unpublish" || command == "publish" || command == "getPublished")
            {
                AuthApi authApi = new AuthApi(siteURL);

                try
                {
                    Credentials creds = new Credentials(username, password);
                    Console.WriteLine(String.Format("Signing in to {0}...", siteURL));

                    authApi.LogIn(creds);

                    Console.WriteLine(String.Format("Signed in successfully as {0}", username));
                    Console.WriteLine("");

                    CustomizationApiExt customizationApi = new CustomizationApiExt(authApi.ApiClient);

                    if (command == "getPublished")
                    {
                        customizationApi.GetPublishedProjects();
                    }
                    else if (command == "unpublish")
                    {
                        customizationApi.UnpublishAllProjects();
                    }
                    else if (command == "import")
                    {
                        customizationApi.ImportProjects(customizationsFolderPath);
                        //customizationApi.ImportProject(projectToImport, customizationsFolderPath);
                    }
                    else if (command == "publish")
                    {
                        customizationApi.PublishProjectsInCurrentTenant(projectListPath, replayDBScripts);
                    }
                    else
                    {
                        ShowUsage();
                    }

                    Console.WriteLine(""); 
                    Console.WriteLine("CLI Task completed successfully.");
                    return ExitCodeOK;
                }
                catch (AcumaticaException e)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Exiting CLI Task due to error...");
                    return ExitCodeError;
                }
                catch (Exception e)
                {
                    ReportErrors(showUsage: false, e);
                    Console.WriteLine("");
                    Console.WriteLine("Exiting CLI Task due to error...");
                    return ExitCodeError;
                }
                finally
                {
                    Console.WriteLine("");
                    Console.WriteLine("Logging out");
                    authApi.Logout();
                }

            }
            return ExitCodeOK;

        }

        private static void ReportUnhandledErrors(Exception e)
        {
            var message = string.Format("Unhandled exception {0}:", e.GetType().FullName);
            Console.WriteLine(message);
            Console.WriteLine(e.Message);
            if (e.StackTrace != null)
            {
                e.StackTrace
                    .Split('\n', '\r')
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList()
                    .ForEach(t => Console.WriteLine(t.Trim()));
            }
        }

        private static bool TryParseArguments(string[] args)
        {
            bool help = false;

            optionSet = new OptionSet(){
                {"c|command=","Command to execute (getPublished, publish, unpublish, build, import).", (string v) => AppOptions.Instance.Command = v},
                {"s|site=","Acumatica site base URL.", (string v) => AppOptions.Instance.AcumaticaSite = v},
                {"u|username=","Acumatica username.", (string v) => AppOptions.Instance.Username = v},
                {"p|password=","Acumatica password.", (string v) => AppOptions.Instance.Password = v},
                {"r|replayDBScripts=","Replay Database Scripts.", (int v) => AppOptions.Instance.ReplayDBScripts = v},
                {"l|projectListPath=","Path to file containing customization projects to be published.", (string v) => AppOptions.Instance.ProjectListPath = v},
                {"w|customizationsFolderPath=","Path to the directory containing customization projects.", (string v) => AppOptions.Instance.CustomizationsFolderPath = v},
                {"h|help","Show this message and exit.", v => help = v != null}
            };

            List<string> extraArgs;

            try
            {
                extraArgs = optionSet.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Error while parsing arguments.");
                ShowUsage();
                return false;
            }

            try
            {
                AppOptions.Instance.Validate(extraArgs);
            }
            catch (ApplicationException e)
            {
                ReportErrors(true, e);
                return false;
            }
            catch (AggregateException e)
            {
                ReportErrors(true, e);
                return false;
            }

            if (help)
            {
                ShowUsage();
                return false;
            }

            return true;
        }

        static void ReportErrors(bool showUsage, params Exception[] exceptions)
        {
            Contract.Ensures(exceptions != null && exceptions.Length > 0);

            Console.Error.WriteLine("Application has terminated due to one or more errors:");

            foreach (var exception in exceptions)
            {
                Console.WriteLine(exception.Message);
            }

            if (showUsage)
            {
                Console.Error.WriteLine();
                ShowUsage();
            }
        }

        static void ReportErrors(bool showUsage, AggregateException exception)
        {
            Contract.Ensures(exception != null);

            if (exception.InnerExceptions.Count > 0)
            {
                ReportErrors(showUsage, exception.InnerExceptions.ToArray());
            }
            else
            {
                ReportUnhandledErrors(exception);
            }
        }

        static void ShowUsage(string narrative = "CLI tool to manage Acumatica Customization Projects.")
        {
            if (!string.IsNullOrWhiteSpace(narrative))
            {
                Console.Error.WriteLine(narrative);
                Console.Error.WriteLine();
            }
            Console.Error.WriteLine("Usage: {0} [options]+", System.AppDomain.CurrentDomain.FriendlyName);
            Console.Error.WriteLine();
            Console.Error.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Error);
        }
    }
}