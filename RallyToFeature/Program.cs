using System;
using System.IO;
using System.Linq;

using Rally.RestApi;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RallyToFeature
{

    class Program
    {

        private static Settings _rallySettings;
        private const string RallyApiVersion = "1.37";
        private const string RallySettingsFile = "Rally.Settings.xml";

        // TODO: Update the field list

        //private readonly string[] FetchFields = new string[] { "FormattedID", "Name", "Notes", "Description", "ScheduleState", "CreationDate", "ArchieID", "Severity", "Priority", "Release", "Requirement", "Resolution", "Iteration", "AcceptedDate", "InProgressDate", "Rank", "ArchieSeverity", "ArchiePriority", "ArchieSRCount", "Attachments", "StoryType" };
        private static readonly string[] FetchFields = new string[] { "FormattedID", "Name", "Description" };

        private static bool _hasLoadedSettings = false;
        private static DynamicJsonObject _jsonProject = null;     // Stores a reference to the project we are working with
        private static DynamicJsonObject _jsonWorkspace = null;   // Stores a reference to the workspace we are working in

        private static string GetErrorList(IEnumerable<string> errors)
        {
            return errors.Aggregate(string.Empty, (current, error) => current + (error + "\n"));
        }

        public static string CreateRallyObject(string type, DynamicJsonObject obj)
        {
            var restApi = Connect();
            var result = restApi.Create(type, obj);
            if (result.Success)
            {
                return result.Reference;
            }
            throw new LoggedException("Could not create {0} due to the following errors\n{1}", type, GetErrorList(result.Errors));
        }

        private static void FetchServerDetails(RallyRestApi restApi)
        {

            // Get the subscription information and find the Workspace etc specified. 
            // ----------------------------------------------------------------------
            dynamic subsData;

            try
            {
                subsData = restApi.GetSubscription("Workspaces,Name,Projects,TypeDefinitions");
                if (subsData == null)
                {
                    throw new ApplicationException("Blank subscription data returned.");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not obtain Subscription Information. ", ex);
            }

            // Find the Workspace and Project as specified. 
            // ----------------------------------------------------------------------
            try
            {
                foreach (var workspace in subsData["Workspaces"])
                {

                    _jsonProject = null;
                    _jsonWorkspace = null;

                    if (workspace["Name"].Equals(_rallySettings["Workspace"].CheckedValue))
                    {
                        _jsonWorkspace = workspace;

                        foreach (var project in workspace["Projects"])
                        {
                            if (!project["Name"].Equals(_rallySettings["Project"].CheckedValue)) continue;
                            _jsonProject = project;
                            break;
                        }
                    }

                    if (_jsonProject != null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not find Workspace/Project information indicating data error?", ex);
            }

            if (_jsonProject == null)
            {
                throw new ApplicationException("Could not find the specified Workspace/Project to read WorkItems from.");
            }

            if (string.IsNullOrEmpty(_jsonProject["Name"]))
            {
                throw new ApplicationException("Could not find the specified Workspace/Project.");
            }
            _hasLoadedSettings = true;

        }

        private static RallyRestApi Connect()
        {
            // Connect to Rally and check for any exceptions
            // ----------------------------------------------
            RallyRestApi restApi;
            try
            {
                restApi = new RallyRestApi(_rallySettings["Username"].CheckedValue, _rallySettings["Password"].CheckedValue, webServiceVersion: RallyApiVersion);
                if (!_hasLoadedSettings) FetchServerDetails(restApi);

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not connect to Rally. ", ex);
            }
            return restApi;
        }

        private static Settings LoadConfigDataForStories(RallyConnectionSettings Config)
        {
            var workspace = "";
            var project = "";
            var targetFolder = "";
            var username = "";
            var password = "";
            var queryString = "";
            Settings rallySettings = null;

            foreach (var dataTarget in Config.DataTargets)
            {
                if (dataTarget.IsActive && dataTarget.Type == DataTargetTypeEnum.RallyDev)
                {
                    var configValue = dataTarget.Settings.Get<string>("Workspace");
                    if (!string.IsNullOrEmpty(configValue))
                        workspace = configValue;

                    configValue = dataTarget.Settings.Get<string>("Project");
                    if (!string.IsNullOrEmpty(configValue))
                        project = configValue;

                    configValue = dataTarget.Settings.Get<string>("TargetFolder");
                    if (!string.IsNullOrEmpty(configValue))
                        targetFolder = configValue;

                    configValue = dataTarget.Settings.Get<string>("Username");
                    if (!string.IsNullOrEmpty(configValue))
                        username = configValue;

                    configValue = dataTarget.Settings.Get<string>("Password");
                    if (!string.IsNullOrEmpty(configValue))
                        password = configValue;

                    configValue = dataTarget.Settings.Get<string>("QueryString");
                    if (!string.IsNullOrEmpty(configValue))
                        queryString = configValue;

                    break;
                }
            }

            // Create the Rally Configuration and Connection
            // ------------------------------------------------------------------------------
            if (string.IsNullOrEmpty(workspace) || string.IsNullOrEmpty(project) || string.IsNullOrEmpty(targetFolder))
            {
                rallySettings = null;
            }
            else
            {
                rallySettings = new Settings
                    {
                        new Setting("Username", username),
                        new Setting("Password", password),
                        new Setting("QueryString", queryString),
                        new Setting("URL", "http://rally1.rallydev.com"),
                        new Setting("Workspace", workspace, "Specifies the workspace to read from."),
                        new Setting("Project", project, "Spefifies the project name where new tickets will be placed."),
                        new Setting("Type", "TESTCASE", "Type should be STORY, DEFECT, TESTCASE"),
                        new Setting("ScopeUp", "false"),
                        new Setting("ScopeDown", "true")
                    };
            }

            return rallySettings;
        }

        public static void QueryUserStoryList()
        {

            var rallyConnectionSettings = RallyConnectionSettings.Load(RallySettingsFile);  // Load Rally.Settings.xml by default

            _rallySettings = LoadConfigDataForStories(rallyConnectionSettings);

            var restApi = Connect();

            var userStoryQuery = new Request("HierarchicalRequirement")
                                   {
                                       Workspace = _jsonWorkspace["_ref"],
                                       Project = _jsonProject["_ref"],
                                       ProjectScopeUp = _rallySettings.Get<bool>("ScopeUp", false),
                                       ProjectScopeDown = _rallySettings.Get<bool>("ScopeDown", true),
                                       //Query = new Query("Tags.Name", Query.Operator.Equals, "featureexport"),
                                       Query = new Query(_rallySettings.Get<string>("QueryString")),
                                       Fetch = FetchFields.ToList()
                                   };

            var userStoryQueryResult = restApi.Query(userStoryQuery);

            foreach (var result in userStoryQueryResult.Results)
            {
                string description = result["Description"];
                string itemFormattedId = result["FormattedID"];
                string itemName = result["Name"];
                Console.WriteLine("FormattedID: {0} Name: {1}", itemFormattedId, itemName);
                Console.WriteLine("Description:");
                Console.WriteLine(description);
                Console.WriteLine("Formatted Description:");
                var formattedDescription = HtmlHandler.FormatHtmlTags(description);
                Console.WriteLine(formattedDescription);
                File.WriteAllText(_rallySettings.Get<string>("TargetFolder") + itemFormattedId + ".feature", formattedDescription);
            }

        }

        static void Main(string[] args)
        {

            try
            {
                // Check for Rally Settings file if it exists
                if (!File.Exists(RallySettingsFile))
                {
                    throw new Exception(RallySettingsFile + " not found!");
                }
                QueryUserStoryList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main : " + ex.Message);
                System.Environment.Exit(1);
            }

            Environment.Exit(0);

        }
    }
}
