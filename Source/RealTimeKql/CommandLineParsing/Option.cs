namespace RealTimeKql
{
    public class Option
    {
        public string LongName { get; private set; }
        public string ShortName { get; private set; }
        public string HelpText { get; private set; }
        public bool IsRequired { get; private set; }
        public bool IsFlag { get; private set; }
        public bool WasSet { get; set; }
        public string Value { get; set; }
        
        public Option(
            string longName,
            string shortName,
            string helpText,
            bool isRequired=false,
            bool isFlag=false)
        {
            LongName = longName;
            ShortName = shortName;
            HelpText = helpText;
            IsRequired = isRequired;
            IsFlag = isFlag;
            WasSet = false;
        }

        public bool IsEqual(string option)
        {
            var tmp = option.Trim('-');
            var name = tmp.Split('=')[0];
            return name == LongName || name == ShortName;
        }
    }
}
