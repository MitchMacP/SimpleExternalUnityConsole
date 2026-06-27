namespace SimpleExternalUnityConsole.Model
{
    /// <summary>
    /// Object that contains all data from a Unity log.
    /// </summary>
    public class UnityLogEntry
    {
        public string LogType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }
}
