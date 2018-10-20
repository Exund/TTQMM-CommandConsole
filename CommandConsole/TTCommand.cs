using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exund.CommandConsole
{
    public class TTCommand
    {
        public Dictionary<string, string> ArgumentsDescriptions { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Func<Dictionary<string, string>, string> Call { get; private set; }

        public TTCommand(string Name, string Description, Func<Dictionary<string, string>, string> Call, Dictionary<string, string> ArgumentsDescriptions = null)
        {
            this.Name = Name.Replace(" ", "");
            this.Description = Description;
            this.ArgumentsDescriptions = ArgumentsDescriptions ?? new Dictionary<string, string>();
            this.Call = Call;

            if (CommandHandler.Commands.ContainsKey(this.Name))
            {
                Console.WriteLine("The command " + this.Name + " already exists !");
            }
            else
            {
                CommandHandler.Commands.Add(this.Name, this);
            }
        }
    }
}
