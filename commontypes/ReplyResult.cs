using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommonTypes.message;

namespace CommonTypes
{
    public class ReplyResult
    {
        // <calledRemoteUrl, responseMessage>
        private readonly ConcurrentDictionary<string, Message> _replies;
        private readonly List<string> _waitingForReply;

        public ReplyResult()
        {
            _replies = new ConcurrentDictionary<string, Message>();
            _waitingForReply = new List<string>();
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
            _waitingForReply.Remove(remoteUrl);
            _replies.AddOrUpdate(remoteUrl,result,(s, message) => message);
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
            return _replies.Values.First();
        }
        
        public IEnumerable<Message> GetAllResults()
        {
            return _replies.Values;
        }
    }
}