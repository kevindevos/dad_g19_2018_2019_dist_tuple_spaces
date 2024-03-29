using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using CommonTypes.message;

namespace CommonTypes
{
    public class ReplyResult
    {
        // <calledRemoteUrl, responseMessage>
        private readonly ConcurrentDictionary<string, Message> _replies;
        private List<string> _waitingForReply;
        public readonly Message MessageLock;
        private readonly bool _waitForAll; //true=All, false=Any

        public ReplyResult(Message messageLock, bool waitForAll)
        {
            _replies = new ConcurrentDictionary<string, Message>();
            _waitingForReply = new List<string>();
            MessageLock = messageLock;
            _waitForAll = waitForAll;
        }
        
        // called just after BeginInvoke to signal we are waiting for that remoteUrl
        public void AddRemoteUrl(string remoteUrl)
        {
            _waitingForReply.Add(remoteUrl);
        }

        // called when viewChange and remoteUrl is not in the new view
        public void RemoveRemoteUrl(string remoteUrl)
        {
            _waitingForReply.Remove(remoteUrl);
            _replies.TryRemove(remoteUrl, out _);
        }

        // called inside the callback to save the result  
        public void AddResult(string remoteUrl, Message result)
        {
            // ignore if not on the waitingForReply List
            if (!_waitingForReply.Contains(remoteUrl)) return;
            
            _waitingForReply.Remove(remoteUrl);
            _replies.AddOrUpdate(remoteUrl, result, (s, message) => message);
        }

        // called to check if any result have been delivered
        public int NResults()
        {
            return _replies.Keys.Count;
        }

        // called to check that all results have been delivered
        public int NWaitingReply()
        {
            return _waitingForReply.Count;
        }

        public Message GetAnyResult()
        {
            return _replies.Values.Any(message => message != null) ? _replies.Values.First() : null;
        }
        
        public IEnumerable<Message> GetAllResults()
        {
            return _replies.Values.Any() ? _replies.Values : null;
        }

        public IEnumerable<string> GetWaitingForReply()
        {
            return _waitingForReply;
        }

        public bool IsDone()
        {
            if (_waitForAll)
            {
                return NWaitingReply() == 0;
            }

            return NResults() > 0 || NWaitingReply() == 0;
        }

        public void Trim(IEnumerable<string> view)
        {
            _waitingForReply = _waitingForReply.Except(view).ToList();
        }

        public void PulseMessage()
        {
            lock (MessageLock)
            {
                Monitor.PulseAll(MessageLock);
            }
        }
    }
}