using System;

namespace PuppetMaster.Exceptions
{
    class PCSNotFoundException : ApplicationException
    {
    public string addr;

    public PCSNotFoundException(string addr) : base(addr)
    {
        this.addr = addr;
    }
}
}
