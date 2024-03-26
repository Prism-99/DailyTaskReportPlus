using StardewValley.TokenizableStrings;

namespace DailyTasksReport.TaskEngines
{
    abstract class TaskEngine
    {
        //internal IModHelper _helper;
        public TaskReportConfig _config;

        internal static readonly Dictionary<string, string> ObjectsNames = new Dictionary<string, string>();
        internal bool Enabled = true;
        public string TaskClass { get; set; }
        public string TaskSubClass { get; set; }
        public int TaskId { get; set; }
        internal abstract void FirstScan();
        public virtual void UpdateList() { }

        public abstract void Clear();

        public abstract List<ReportReturnItem> GeneralInfo();

        public abstract List<ReportReturnItem> DetailedInfo();
        public virtual void SetEnabled() { }
        public virtual void FinishedReport()
        {
        }

        public virtual void OnDayStarted()
        {
            Clear();
            FirstScan();
        }

        internal string GetLocationDisplayName(string locationName)
        {
            if(Game1.getLocationFromName(locationName) != null){
                return Game1.getLocationFromName(locationName).DisplayName;
            }

            return locationName;
        }
        internal string Pluralize(string name, int number)
        {
            if (number < 2)
                return name;
            if (name.EndsWith("y", StringComparison.CurrentCulture))
                return name.Substring(0, name.Length - 1) + "ies";
            if (name == "Peach")
                return name + "es";
            return name + "s";
        }
        internal static void PopulateObjectsNames()
        {
            foreach (var pair in Game1.objectData)
                ObjectsNames[pair.Key] = TokenParser.ParseText(pair.Value.DisplayName);
        }
    }
}

