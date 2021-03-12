using System.Collections.Generic;
using System.Linq;

namespace RealTimeKql
{
    public class Subcommand
    {
        public string Name { get; private set; }
        public string HelpText { get; private set; }
        public Argument Argument { get; private set; }
        public List<Option> Options { get; private set; }

        public Subcommand(
            string name,
            string helpText,
            Argument argument=null,
            List<Option> options=null)
        {
            Name = name;
            HelpText = helpText;
            Argument = argument;
            Options = options;
        }

        public int MinimumRequiredItems()
        {
            int num = 0;
            if (Argument != null && Argument.IsRequired) num++;
            if (Options == null) return num;
            num += Options.Where(opt => opt.IsRequired).Count();
            return num;
        }
    }
}
