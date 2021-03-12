namespace RealTimeKql
{
    public class Argument
    {
        public string FriendlyName { get; private set; }
        public string HelpText { get; private set; }
        public bool IsRequired { get; private set; }
        public string Value { get; set; }

        public Argument(string friendlyName, string helpText, bool isRequired=false)
        {
            FriendlyName = friendlyName;
            HelpText = helpText;
            IsRequired = isRequired;
        }
    }
}
