using Acumatica.RESTClient.Api;
using Acumatica.RESTClient.Client;
using Acumatica.RESTClient.CustomizationApi.Model;
using Acumatica.RESTClient.CustomizationApi;
using Microsoft.VisualBasic;
using System.Text;
using System.Diagnostics;

namespace AcumaticaCustomizationManagementCLI
{
    public class CustomizationApiExt : CustomizationApi
    {
        public CustomizationApiExt(ApiClient configuration) : base(configuration) { }

        public void PublishProjectsInCurrentTenant(string projectListPath, int replayDBScripts = 0)
        {
            List<string> customizationProjectsList = File.ReadAllLines(projectListPath)
                .Select(l => l.Trim())
                .Where(l => !l.StartsWith("#") && !l.StartsWith("//"))
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .ToList();

            PublishProjects(customizationProjectsList, replayDBScripts);
        }

        public void PublishProjects(List<string> customizationProjectsList, int replayDBScripts = 0)
        {
            if (customizationProjectsList == null)
            {
                Console.WriteLine("");
                Console.WriteLine("List of projects is null");
                return;
            }

            else if (customizationProjectsList.Count == 0)
            {
                Console.WriteLine("");
                Console.WriteLine("List of projects is empty");
                return;
            }
            else
            {

                Console.WriteLine("");
                Console.WriteLine("List of projects to publish:");
                foreach (var item in customizationProjectsList)
                {
                    Console.WriteLine(item);
                }

                Console.WriteLine("");
                Console.WriteLine("Starting publishing...");

                bool publishingStarted = false;
                bool publishingError = false;
                int millisecondsInterval = 5000;
                bool replay = false;
                if (replayDBScripts > 0) { replay = true; }

                if (!publishingStarted && !publishingError)
                {
                    try
                    {
                        CustomizationPublishEnd processResult = this.PublishBeginExt(customizationProjectsList, tenantMode: TenantMode.Current, isReplayPreviouslyExecutedScripts: replay);

                        if (processResult.Log != null)
                        {
                            foreach (var item in processResult.Log)
                            {
                                Console.WriteLine(String.Format("[{0}]\t{1}", item.Timestamp.Substring(0, 19), item.Message));
                            }

                            if (processResult.Log.Any(x => x.Message == "Publishing has started."))
                            {
                                publishingStarted = true;
                            }
                            else
                            {
                                publishingError = true;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        publishingError = true;
                        Console.WriteLine(String.Format("Failed to start publishing, {0}", ex.Message));
                    }
                }

                Thread.Sleep(millisecondsInterval);

                if (publishingError)
                {
                    Console.WriteLine("[PROBLEM] Could not start publishing, exiting...");
                    return;
                }

                int logLenth = 0;

                while (true)
                {
                    var processResult = this.CustomizationPublishEndExt();
                    var logIndex = 0;

                    if (processResult.Log != null)
                    {
                        foreach (var logItem in processResult.Log)
                        {
                            logIndex++;

                            if (logIndex > logLenth)
                            {
                                Console.WriteLine(String.Format("[{0}]\t{1}", logItem.Timestamp.Substring(0, 19), logItem.Message));
                            }
                        }

                        logLenth = processResult.Log.Count;

                        if (processResult.isFailed)
                        {
                            StringBuilder log = new StringBuilder();
                            processResult.Log.ForEach(_ => log.Append(_.Message).Append(Environment.NewLine));
                          throw new AcumaticaException();
                        }
                        else if (processResult.IsCompleted)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Publishing completed!");
                            return;
                        }
                    }
                    
                    Thread.Sleep(millisecondsInterval);
                }
            }
        }

        public void UnpublishAllProjects()
        {
            Console.WriteLine("");
            Console.WriteLine("Unpublishing all projects");

            var processResult = this.UnpublishAll();

            foreach (var logItem in processResult.Log)
            {
                Console.WriteLine(String.Format("[{0}]\t{1}", logItem.Timestamp.Substring(0, 19), logItem.Message));
            }

            Console.WriteLine("");
            Console.WriteLine("Waiting for 2 min to let the system finish unpublishing");

            var millisecondsInterval = 2*60*1000;
            Thread.Sleep(millisecondsInterval);
        }
        public CustomizationPublishEnd CustomizationPublishEndExt()
        {
            try
            {
                HttpResponseMessage localVarResponse = base.ApiClient.CallApiAsync(
                    "/CustomizationApi/PublishEnd",
                    HttpMethod.Post,
                    null,
                    "",
                    HeaderContentType.Json,
                    HeaderContentType.Json).Result;

                return DeserializeResponse<CustomizationPublishEnd>(localVarResponse).Data;
            }
            catch (Exception ex)
            {
                return new CustomizationPublishEnd();
            }
        }

        public CustomizationPublishEnd UnpublishAll()
        {
            CustomizationProjectUnublishRequest unpublishRequest = new CustomizationProjectUnublishRequest();
            unpublishRequest.Mode = TenantMode.All;

            try
            {
                HttpResponseMessage localVarResponse = base.ApiClient.CallApiAsync(
                "/CustomizationApi/UnpublishAll",
                HttpMethod.Post,
                null,
                unpublishRequest,
                HeaderContentType.Json,
                HeaderContentType.Json).Result;

                return DeserializeResponse<CustomizationPublishEnd>(localVarResponse).Data;
            }
            catch (Exception ex)
            {
                return new CustomizationPublishEnd();
            }
        }

        public CustomizationProjectList GetPublishedCustomizations()
        {
            HttpResponseMessage localVarResponse = base.ApiClient.CallApiAsync(
            "/CustomizationApi/GetPublished",
            HttpMethod.Post,
            null,
            "",
            HeaderContentType.Json,
            HeaderContentType.Json).Result;

            VerifyResponse(localVarResponse, "GetPublished");
            return DeserializeResponse<CustomizationProjectList>(localVarResponse).Data;
        }

        public void ImportProjects(string customizationsFolderPath)
        {
            List<string> customizationProjectsList = Directory.GetFiles(customizationsFolderPath).ToList();

            Console.WriteLine("");
            Console.WriteLine("List of project zip files found to import:");
            foreach (var projectFilePath in customizationProjectsList)
            {
                Console.WriteLine(projectFilePath);
            }

            Console.WriteLine("");

            foreach (var projectFilePath in customizationProjectsList)
            {
                string projectName = Path.GetFileNameWithoutExtension(projectFilePath);
                ImportProject(projectName, customizationsFolderPath);
            }

        }

        public void ImportProject(string projectName, string customizationsFolderPath)
        {
            string zipPath = customizationsFolderPath + projectName + @".zip";

            if (File.Exists(zipPath))
            {
                Console.WriteLine(String.Format("Importing {0}...", projectName + @".zip"));

                try
                {

                    using (var stream = File.Open(zipPath, FileMode.Open))
                    {
                        CustomizationPublishEnd processResult = this.ImportExt(stream, projectName, projectDescription: null);

                        if (processResult.Log != null)
                        {
                            foreach (var item in processResult.Log)
                            {
                                Console.WriteLine(String.Format("[{0}]\t{1}", item.Timestamp.Substring(0, 19), item.Message));
                            }
                        }
                    }

                    File.Delete(zipPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine(String.Format("File {0} not found... Skipping import.", projectName + @".zip"));
            }
            Console.WriteLine("");
        }

        public CustomizationPublishEnd ImportExt(Stream customizationPackageContent, string projectName, string projectDescription = "", bool replaceIfExists = true, int? level = null)
        {
            if (customizationPackageContent == null)
            {
                ThrowMissingParameter("Import", "customizationPackageContent");
            }

            if (projectName == null)
            {
                ThrowMissingParameter("Import", "projectName");
            }

            CustomizationImport customizationImport = new CustomizationImport();
            customizationImport.ProjectDescription = projectDescription;
            customizationImport.ProjectName = projectName;
            customizationImport.ProjectLevel = level;
            customizationImport.IsReplaceIfExists = replaceIfExists;
            customizationImport.ProjectContentBase64 = ConvertToBase64(customizationPackageContent);
            HttpResponseMessage result = base.ApiClient.CallApiAsync("/CustomizationApi/Import", HttpMethod.Post, null, customizationImport, HeaderContentType.Json, HeaderContentType.Json).Result;

            return DeserializeResponse<CustomizationPublishEnd>(result).Data;
        }

        private static string ConvertToBase64(Stream stream)
        {
            byte[] inArray;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                inArray = memoryStream.ToArray();
            }

            return Convert.ToBase64String(inArray);
        }

        public CustomizationPublishEnd PublishBeginExt(List<string> projectNames,
            bool isMergeWithExistingPackages = false,
            bool isOnlyValidation = false,
            bool isOnlyDbUpdates = false,
            bool isReplayPreviouslyExecutedScripts = true,
            TenantMode tenantMode = TenantMode.Current)
        {
            if (projectNames == null)
            {
                ThrowMissingParameter("PublishBegin", "projectNames");
            }

            string resourcePath = "/CustomizationApi/PublishBegin";
            CustomizationPublishParams customizationPublishParams = new CustomizationPublishParams();
            customizationPublishParams.ProjectNames = projectNames;
            customizationPublishParams.IsMergeWithExistingPackages = isMergeWithExistingPackages;
            customizationPublishParams.IsOnlyValidation = isOnlyValidation;
            customizationPublishParams.IsOnlyDbUpdates = isOnlyDbUpdates;
            customizationPublishParams.IsReplayPreviouslyExecutedScripts = isReplayPreviouslyExecutedScripts;
            switch (tenantMode)
            {
                case TenantMode.Current:
                    customizationPublishParams.TenantMode = "Current";
                    break;
                case TenantMode.List:
                    throw new NotImplementedException();
                case TenantMode.All:
                    customizationPublishParams.TenantMode = "All";
                    break;
            }

            HttpResponseMessage result = base.ApiClient.CallApiAsync(resourcePath, HttpMethod.Post, null, customizationPublishParams, HeaderContentType.Json, HeaderContentType.Json).Result;

            return DeserializeResponse<CustomizationPublishEnd>(result).Data;
        }

        internal void GetPublishedProjects()
        {
            Console.WriteLine("Getting list of published customization projects...");

            CustomizationProjectList customizationProjectsList = new CustomizationProjectList();
            customizationProjectsList = this.GetPublishedCustomizations();

            if (customizationProjectsList.Projects?.Count > 0)
            {
                foreach (var item in customizationProjectsList.Projects)
                {
                    Console.WriteLine(item.Name);
                }
            }
            else
            {
                Console.WriteLine("No published projects found.");
            }

            Console.WriteLine("");
        }
    }
}