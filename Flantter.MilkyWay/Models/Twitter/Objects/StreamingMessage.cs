using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class StreamingMessage
    {
        public StreamingMessage(CoreTweet.Streaming.StreamingMessage m)
        {
            switch (m.Type)
            {
                case CoreTweet.Streaming.MessageType.Create:
                    var tweet = m as CoreTweet.Streaming.StatusMessage;
                    this.Type = MessageType.Create;
                    this.Status = new Twitter.Objects.Status(tweet.Status);
                    break;
                case CoreTweet.Streaming.MessageType.DirectMesssage:
                    var directMessage = m as CoreTweet.Streaming.DirectMessageMessage;
                    this.Type = MessageType.DirectMesssage;
                    this.DirectMessage = new Twitter.Objects.DirectMessage(directMessage.DirectMessage);
                    break;
                case CoreTweet.Streaming.MessageType.Event:
                    var eventMessage = m as CoreTweet.Streaming.EventMessage;
                    this.EventMessage = new Twitter.Objects.EventMessage(eventMessage);
                    break;
                case CoreTweet.Streaming.MessageType.DeleteStatus:
                    var deleteStatus = m as CoreTweet.Streaming.DeleteMessage;
                    this.Type = MessageType.DeleteStatus;
                    this.DeletedStatusId = deleteStatus.Id;
                    break;
                case CoreTweet.Streaming.MessageType.DeleteDirectMessage:
                    var deleteDirectMessage = m as CoreTweet.Streaming.DeleteMessage;
                    this.Type = MessageType.DeleteDirectMessage;
                    this.DeletedDirectMessageId = deleteDirectMessage.Id;
                    break;
            }
        }

        public StreamingMessage(Status status)
        {
            this.Type = MessageType.Create;
            this.Status = status;
        }

        public StreamingMessage(DirectMessage dm)
        {
            this.Type = MessageType.DirectMesssage;
            this.DirectMessage = dm;
        }

        public StreamingMessage(EventMessage em)
        {
            this.Type = MessageType.Event;
            this.EventMessage = em;
        }

        public StreamingMessage(long id, bool isdm = false)
        {
            this.Type = isdm ? MessageType.DeleteDirectMessage : MessageType.DeleteStatus;
            if (isdm)
                this.DeletedDirectMessageId = id;
            else
                this.DeletedStatusId = id;
        }

        public enum MessageType
        {
            DeleteStatus = 0,
            DeleteDirectMessage = 1,
            Event = 2,
            Create = 3,
            DirectMesssage = 4
        }

        public MessageType Type { get; set; }

        public long DeletedStatusId { get; set; }
        public long DeletedDirectMessageId { get; set; }
        public EventMessage EventMessage { get; set; }
        public Status Status { get; set; }
        public DirectMessage DirectMessage { get; set; }
    }
}
