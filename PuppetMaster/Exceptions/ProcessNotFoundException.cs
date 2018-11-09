using System;

namespace PuppetMaster.Exceptions
{
    class ProcessNotFoundException : ApplicationException
    {
    public string processname;

    public ProcessNotFoundException(string processname) : base(processname)
    {
        this.processname = processname;
    }
}
}
