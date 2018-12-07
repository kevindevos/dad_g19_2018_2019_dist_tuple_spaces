using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using CommonTypes;
using CommonTypes.message;
using PCS;
using PuppetMaster.Exceptions;

namespace PuppetMaster
{
    class Controller
    {
        private readonly string CLIENT_SCRIPTS_REL_PATH = "/Resources/";
        private readonly string PROJECT_PATH = Directory.GetParent(Directory.GetCurrentDirectory())
            .Parent.FullName;

        private readonly Dictionary<string, PCSRemotingAbstract> pcs;
        private readonly Dictionary<string, string> processes;
        private readonly TcpChannel channel;

        private readonly int PCS_PORT = 10000;

        public Controller(string[] addrs)
        {
            pcs = new Dictionary<string, PCSRemotingAbstract>();
            processes = new Dictionary<string, string>();

            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            foreach (string addr in addrs)
            {
                string URL = "tcp://{0}:{1}/PCS";

                PCSRemotingAbstract obj = (PCSRemotingAbstract)Activator.GetObject(
                    typeof(PCSRemotingAbstract),
                    string.Format(URL, addr, PCS_PORT)
                );

                pcs.Add(addr, obj);
            }
        }

        public void ParseCommand(string command)
        {
            string server_id, client_id, URL, script_file, processname;
            int min_delay, max_delay, ms;
            string[] words = command.Split(new char[] { ' ' });

            switch (words[0])
            {
                case "Server":
                    if (words.Length < 5)
                        throw new InvalidCommandException(command);

                    server_id = words[1];
                    URL = words[2];
                    min_delay = int.Parse(words[3]);
                    max_delay = int.Parse(words[4]);

                    CreateServer(server_id, URL, min_delay, max_delay);
                    break;

                case "Client":
                    if (words.Length < 4)
                        throw new InvalidCommandException(command);

                    client_id = words[1];
                    URL = words[2];
                    script_file = words[3];

                    CreateClient(client_id, URL, script_file);
                    break;

                case "Status":
                    Status();
                    break;

                case "Crash":
                    if (words.Length < 2)
                        throw new InvalidCommandException(command);

                    processname = words[1];
                    Crash(processname);
                    break;

                case "Freeze":
                    if (words.Length < 2)
                        throw new InvalidCommandException(command);

                    processname = words[1];
                    Freeze(processname);
                    break;

                case "Unfreeze":
                    if (words.Length < 2)
                        throw new InvalidCommandException(command);

                    processname = words[1];
                    Unfreeze(processname);
                    break;

                case "Wait":
                    if (words.Length < 2)
                        throw new InvalidCommandException(command);

                    ms = int.Parse(words[1]);
                    Thread.Sleep(ms);
                    break;

                default:
                    throw new InvalidCommandException(command);
            }
        }

        private void CreateServer(string server_id, string URL, int min_delay, int max_delay)
        {
            Console.WriteLine("[DEBUG] Creating server [id={0}, URL={1}, min_delay={2}, max_delay={3}]",
                server_id, URL, min_delay, max_delay);
            string addr = AddrFromURL(URL);

            if (pcs.ContainsKey(addr))
            {
                PCSRemotingAbstract p = pcs[addr];
                IEnumerable<string> serverUrls = processes.Values.ToList();
                AsyncCallServer(p.Server, server_id, URL, min_delay, max_delay, serverUrls);

                ConnectToProcess(server_id, URL);
            }

            else
            {
                throw new PCSNotFoundException(addr);
            }
        }

        private void CreateClient(string client_id, string URL, string script_file)
        {
            Console.WriteLine("[DEBUG] Creating client [id={0}, URL={1}, script_file={2}]",
                client_id, URL, script_file);
            string addr = AddrFromURL(URL);

            if (pcs.ContainsKey(addr))
            {
                PCSRemotingAbstract p = pcs[addr];
                string[] script = File.ReadAllLines(
                    @PROJECT_PATH + CLIENT_SCRIPTS_REL_PATH + script_file);
                
                IEnumerable<string> serverUrls = processes.Values.ToList();
                AsyncCallClient(p.Client, client_id, URL, script, serverUrls);
            }

            else
            {
                throw new PCSNotFoundException(addr);
            }
        }

        private void Status()
        {
            Console.WriteLine("[DEBUG] Status");
            Console.WriteLine("[DEBUG] PCS: {0}", pcs.Keys.Count);
            
            foreach (var remoteUrl in pcs.Keys.ToArray())
            {
                PCSRemotingAbstract remote = pcs[remoteUrl];
                AsyncCallStatus(remote.Status);
            }
        }

        private void Crash(string processname)
        {
            Console.WriteLine("[DEBUG] Crashing {0}", processname);

            foreach (var remoteUrl in pcs.Keys.ToArray())
            {
                PCSRemotingAbstract remote = pcs[remoteUrl];
                AsyncCallCrash(remote.Crash, processname);
            }
        }

        private void Freeze(string processname)
        {
            Console.WriteLine("[DEBUG] Freezing {0}", processname);

            if (processes.ContainsKey(processname))
            {
                processes.TryGetValue(processname, out var remoteUrl);
                if (remoteUrl == null) return;
                
                var remotingEndpoint = RemotingEndpoint.GetRemoteEndpoint(remoteUrl);
                AsyncCallVoid(remotingEndpoint.Freeze);
            }

            else
            {
                throw new ProcessNotFoundException(processname);
            }
        }

        private void Unfreeze(string processname)
        {
            Console.WriteLine("[DEBUG] Unfreezing {0}", processname);

            if (processes.ContainsKey(processname))
            {
                processes.TryGetValue(processname, out var remoteUrl);
                if (remoteUrl == null) return;
                
                var remotingEndpoint = RemotingEndpoint.GetRemoteEndpoint(remoteUrl);
                AsyncCallVoid(remotingEndpoint.Unfreeze);
            }

            else
            {
                throw new ProcessNotFoundException(processname);
            }
        }

        private string AddrFromURL(string URL)
        {
            string[] fields = URL.Split(new char[] { '/', ':' });
            return fields[3];
        }

        private void ConnectToProcess(string processname, string URL)
        {
            /*RemotingEndpoint obj = (RemotingEndpoint)Activator.GetObject(
                typeof(RemotingEndpoint), URL);*/

            processes.Add(processname, URL);
        }

        private void AsyncCallStatus(StatusDelegate caller)
        {
            caller.BeginInvoke(asyncResult =>
            {
                AsyncResult ar = (AsyncResult)asyncResult;
                StatusDelegate remoteDel = (StatusDelegate)ar.AsyncDelegate;

                string response = remoteDel.EndInvoke(asyncResult);
                Console.Write(response);
            }, null);
        }
        
        private void AsyncCallVoid(VoidDelegate caller)
        {
            caller.BeginInvoke(asyncResult =>
            {
                AsyncResult ar = (AsyncResult)asyncResult;
                VoidDelegate remoteDel = (VoidDelegate)ar.AsyncDelegate;
                remoteDel.EndInvoke(asyncResult);
            }, null);
        }
        
        private void AsyncCallCrash(CrashDelegate caller, string processname)
        {
            caller.BeginInvoke(processname, asyncResult =>
            {
                AsyncResult ar = (AsyncResult)asyncResult;
                CrashDelegate remoteDel = (CrashDelegate)ar.AsyncDelegate;
                remoteDel.EndInvoke(asyncResult);
            }, null);
        }
        
        private void AsyncCallClient(ClientDelegate caller, string clientId, string URL, string[] script,
            IEnumerable<string> serverUrls)
        {
            caller.BeginInvoke(clientId, URL, script, serverUrls, asyncResult =>
            {
                AsyncResult ar = (AsyncResult)asyncResult;
                ClientDelegate remoteDel = (ClientDelegate)ar.AsyncDelegate;
                remoteDel.EndInvoke(asyncResult);
            }, null);
        }
        
        private void AsyncCallServer(ServerDelegate caller, string server_id, string URL, int min_delay, int max_delay,
            IEnumerable<string> serverUrls)
        {
            caller.BeginInvoke(server_id, URL, min_delay,max_delay, serverUrls, asyncResult =>
            {
                AsyncResult ar = (AsyncResult)asyncResult;
                ServerDelegate remoteDel = (ServerDelegate)ar.AsyncDelegate;
                remoteDel.EndInvoke(asyncResult);
            }, null);
        }
    }

    internal delegate void VoidDelegate();
    internal delegate string StatusDelegate();
    internal delegate void CrashDelegate(string processname);
    internal delegate void ClientDelegate(string clientId, string URL, string[] script, IEnumerable<string> serverUrls);
    internal delegate void ServerDelegate(string server_id, string URL, int min_delay, int max_delay,
        IEnumerable<string> serverUrls);
}
