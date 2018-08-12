using System;

namespace Flantter.MilkyWay.Models.Apis.Objects
{
    public class StreamingMessage
    {
        public enum MessageType
        {
            DeleteStatus = 0,
            DeleteDirectMessage = 1,
            Event = 2,
            Create = 3,
            DirectMesssage = 4
        }

        public StreamingMessage(CoreTweet.Streaming.StreamingMessage m)
        {
            switch (m.Type)
            {
                case CoreTweet.Streaming.MessageType.Create:
                    var tweet = m as CoreTweet.Streaming.StatusMessage;
                    Type = MessageType.Create;
                    Status = new Status(tweet.Status);
                    break;
                case CoreTweet.Streaming.MessageType.Event:
                    var eventMessage = m as CoreTweet.Streaming.EventMessage;
                    Type = MessageType.Event;
                    EventMessage = new EventMessage(eventMessage);
                    break;
                case CoreTweet.Streaming.MessageType.DeleteStatus:
                    var deleteStatus = m as CoreTweet.Streaming.DeleteMessage;
                    Type = MessageType.DeleteStatus;
                    DeletedStatusId = deleteStatus.Id;
                    break;
            }
        }

        public StreamingMessage(Status status)
        {
            Type = MessageType.Create;
            Status = status;
        }

        public StreamingMessage(DirectMessage dm)
        {
            Type = MessageType.DirectMesssage;
            DirectMessage = dm;
        }

        public StreamingMessage(EventMessage em)
        {
            Type = MessageType.Event;
            EventMessage = em;
        }

        public StreamingMessage(long id, bool isdm = false)
        {
            Type = isdm ? MessageType.DeleteDirectMessage : MessageType.DeleteStatus;
            if (isdm)
                DeletedDirectMessageId = id;
            else
                DeletedStatusId = id;
        }

        public MessageType Type { get; set; }

        public long DeletedStatusId { get; set; }
        public long DeletedDirectMessageId { get; set; }
        public EventMessage EventMessage { get; set; }
        public Status Status { get; set; }
        public DirectMessage DirectMessage { get; set; }
    }
}