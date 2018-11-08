using System;
using System.IO;

using System.Collections;
using System.Globalization;
using System.Resources;

namespace PuppetMaster
{
    class Interface
    {
        private readonly string PCS_ADDR_PATH = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/TextFile1.txt";
        private string[] pcs_addrs;

        private Controller controller;

        public Interface()
        {
            Console.WriteLine("===== Welcome to the Puppet Master =====");
            pcs_addrs = File.ReadAllLines(@PCS_ADDR_PATH);
        }

        private string[] GetPCS()
        {
            
            Console.ReadLine();

            return lines;

        }
    }
}
