
namespace DailyTasksReport
{
   internal static class DailyTaskHelper
    {
        public static IModHelper Helper;
        public static IMonitor Monitor;
        public static void Init(IModHelper helper,IMonitor monitor)
        {
            Helper = helper;
            Monitor = monitor;
        }
    }
}
