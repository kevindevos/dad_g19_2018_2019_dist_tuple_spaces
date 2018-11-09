using System;
using System.IO;

using System.Collections;
using System.Globalization;
using System.Resources;

namespace PuppetMaster
{
    class Interface
    {
        private readonly string PCS_ADDR_PATH = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/pcs_addrs.txt";
        private Controller controller;

        public Interface()
        {
            Console.WriteLine("===== Welcome to the Puppet Master =====");

            string[] pcs_addrs = File.ReadAllLines(@PCS_ADDR_PATH);
            controller = new Controller(pcs_addrs);
        }

        public void Start()
        {

        }

        private void Help()
        {
            int value = -1;
            string response;

            Console.WriteLine("1. Read configuration script");
            Console.WriteLine("2. Manually input commands");

            while (value != 1 && value != 2)
            {
                Console.Write("\n[1|2] ");
                response = Console.ReadLine();
                Int32.TryParse(response, out value);
            }
        }

        private void ReadScript()
        {

        }

        private void ParseCommand()
        {

        }

    }
}
