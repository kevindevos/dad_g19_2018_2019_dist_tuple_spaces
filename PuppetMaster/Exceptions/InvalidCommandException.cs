using System;

namespace PuppetMaster.Exceptions
{
    class InvalidCommandException : ApplicationException
    {
        public string command;

        public InvalidCommandException(string command) : base(command) {
            this.command = command;
        }
    }
}
