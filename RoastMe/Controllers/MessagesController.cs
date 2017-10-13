using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using RoastMe.Controllers;
using System.Collections.Generic;

namespace RoastMe
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                try
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;
                    var faceTraits = new List<Trait>();
                    // return our reply to the user

                    var replyText = WatsonService.TalkToWatson(activity.Text, activity.Conversation.Id).Result;
                    if (activity.Attachments.Count == 1)
                    {
                        FaceConnector faceConnector = new FaceConnector();
                        var faces = await faceConnector.UploadAndDetectFaces(activity.Attachments[0].ContentUrl);
                        if (faces.Length > 0)
                        {
                            faceTraits = FaceAnalizer.GetTraitsFromFace(faces[0]);
                        }
                            
                    }
                    // faceConnector.UploadAndDetectFaces(activity.Attachments)

                    Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters:");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }

                catch {

                }
             
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}