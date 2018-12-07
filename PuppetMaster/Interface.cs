using System;
using System.IO;
using PuppetMaster.Exceptions;

namespace PuppetMaster
{
    class Interface
    {
        private readonly string PCS_ADDR_REL_PATH = "/Resources/pcs_addrs.txt";
        private readonly string SCRIPT_REL_PATH = "/Resources/script.txt";

        private readonly string PROJECT_PATH = Directory.GetParent(Directory.GetCurrentDirectory())
            .Parent.FullName;

        private Controller controller;

        public Interface()
        {
            Console.WriteLine("===== Welcome to the Puppet Master =====");

            string[] pcs_addrs = File.ReadAllLines(@PROJECT_PATH + PCS_ADDR_REL_PATH);
            controller = new Controller(pcs_addrs);
        }

        public void Start()
        {
            int value = -1;
            string response, input;
            string[] script;
            char action = '\0';

            while (true)
            {
                Console.WriteLine("1. Read configuration script");
                Console.WriteLine("2. Manually input commands");
                Console.WriteLine("3. To exit");

                while (value != 1 && value != 2 && value != 3)
                {
                    Console.Write("\n[1|2|3] ");
                    response = Console.ReadLine();
                    Int32.TryParse(response, out value);
                }

                if (value == 1)
                {
                    script = ReadScript();
                    HelpMenu(true);

                    foreach (string command in script)
                    {
                        Console.WriteLine("-> {0}", command);

                        while (action != 'r' && action != 'n')
                        {
                            Console.Write("\n$ ");
                            input = Console.ReadLine();

                            if (input.Length == 1)
                                action = input[0];

                            else
                            {
                                try
                                {
                                    controller.ParseCommand(input);
                                }

                                catch (InvalidCommandException ice)
                                {
                                    Console.WriteLine("[ERROR] Invalid command '{0}'", ice.command);
                                }
                            }

                            if (action == 'h')
                                HelpMenu(true);

                            else if (action == 'e')
                                break;
                        }

                        if (action == 'n')
                            action = '\0';

                        else if (action == 'e')
                            break;

                        try
                        {
                            controller.ParseCommand(command);
                        }

                        catch (InvalidCommandException ice)
                        {
                            Console.WriteLine("[ERROR] Invalid command '{0}'", ice.command);
                        }
                    }
                }

                if (value == 2)
                {
                    while (action != 'e')
                    {
                        Console.Write("$ ");
                        input = Console.ReadLine();

                        if (input.Length == 1)
                            action = input[0];

                        else
                        {
                            try
                            {
                                controller.ParseCommand(input);
                            }

                            catch (InvalidCommandException ice)
                            {
                                Console.WriteLine("[ERROR] Invalid command '{0}'", ice.command);
                            }
                        }

                        if (action == 'h')
                            HelpMenu();
                    }
                }

                if (value == 3)
                {
                    Console.WriteLine("Exiting...");
                    Console.Write("[enter] to close");
                    Console.ReadLine();
                    break;
                }

                value = -1;
            }
        }

        private string[] ReadScript()
        {
            string[] script = File.ReadAllLines(@PROJECT_PATH + SCRIPT_REL_PATH);
            return script;
        }


        private void HelpMenu(bool script = false)
        {
            Console.WriteLine("\n===== Help menu =====");
            if (script)
            {
                Console.WriteLine("$ n");
                Console.WriteLine("  Execute next loaded command");

                Console.WriteLine("$ r");
                Console.WriteLine("  Execute all loaded commands");
            }

            Console.WriteLine("$ h");
            Console.WriteLine("  Display this help menu");

            Console.WriteLine("$ e");
            Console.WriteLine("  Exit program");

            Console.WriteLine();

            Console.WriteLine("$ Server [server_id] [URL] [min_delay] [max_delay]");
            Console.WriteLine("$ Client [client_id] [URL] [script_file]");
            Console.WriteLine("$ Status");
            Console.WriteLine("$ Crash [processname]");
            Console.WriteLine("$ Freeze [processname]");
            Console.WriteLine("$ Unfreeze [processname]");
            Console.WriteLine("$ Wait [x_ms]");

            Console.WriteLine("=====================\n");
        }
    }
}
