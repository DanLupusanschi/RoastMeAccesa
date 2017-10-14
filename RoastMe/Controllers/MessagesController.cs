using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using RoastMe.Controllers;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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
        ConnectorClient connector = null;
        try
        {
          connector = new ConnectorClient(new Uri(activity.ServiceUrl));
          // calculate something for us to return
          var faceTraits = new List<Trait>();
          // return our reply to the user


          if (activity.Attachments.Count == 1)
          {
            FaceConnector faceConnector = new FaceConnector();
            var faces = await faceConnector.UploadAndDetectFaces(activity);
            if (faces.Length > 0)
            {
              faceTraits = FaceAnalizer.GetTraitsFromFace(faces[0]);
            }
            else
            {
              await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("You're so dumb, you can't even take a selfie"));
              return Request.CreateResponse(HttpStatusCode.OK); ;
            }


            var jokeService = new JokeService();
            var joke = jokeService.GetJoke(faceTraits);

            Activity reply = activity.CreateReply(joke);
            await connector.Conversations.ReplyToActivityAsync(reply);
          }
          else
          {
            JObject r = await WatsonService.TalkToWatson(activity.Text, activity.Conversation.Id);
            Dictionary<string, double> map = new Dictionary<string, double>();
            foreach (JObject entity in r["entities"])
            {
              map[entity["value"].Value<string>()] = entity["confidence"].Value<double>();
            }
            if (map.Keys.Count > 0)
            {
              var jokeService = new JokeService();
              var joke = jokeService.GetJoke(new List<Trait> { new Trait { Name = map.Keys.First(), Accuracy = 1.0 } });

              Activity reply = activity.CreateReply($"{joke}");
              await connector.Conversations.ReplyToActivityAsync(reply);
            }

            //var entities = ((IEnumerable<object>)r.Result).ToList()[1];
            // var replyText = ((Newtonsoft.Json.Linq.JProperty )entities);

          }
          // faceConnector.UploadAndDetectFaces(activity.Attachments)

        }

        catch (Exception ex)
        {
          if (connector != null)
          {
            Activity reply = activity.CreateReply(ex.Message);
            await connector.Conversations.ReplyToActivityAsync(reply);
          }
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