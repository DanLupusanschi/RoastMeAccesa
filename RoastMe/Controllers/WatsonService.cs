
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoastMe.Controllers
{
    public class WatsonService
    {
        private static Dictionary<string, string> conversationContextDictionary = new Dictionary<string, string>();

        public static async Task<JObject> TalkToWatson(string input, string conversationId)
        {
            var baseurl = "https://gateway.watsonplatform.net/conversation/api";
            //var workspace = "4a486a7c-8861-4614-a647-a34aeec12526";
            //var username = "4dbad6ed-577d-4190-92c4-43812cae3e9a";
            //var password = "JfK2PXSnyhgp";
            var workspace = "2fb2487c-cb7f-4303-8811-a33bbef368d7";
            var username = "4dbad6ed-577d-4190-92c4-43812cae3e9a";
            var password = "JfK2PXSnyhgp";
            var context = null as object;

            if (conversationContextDictionary.ContainsKey(conversationId))
            {
                context = JsonConvert.DeserializeAnonymousType(conversationContextDictionary[conversationId], context);
            }

            var message = new { input = new { text = input }, context };

            var resp = await baseurl
                .AppendPathSegments("v1", "workspaces", workspace, "message")
                .SetQueryParam("version", "2016-11-21")
                .WithBasicAuth(username, password)
                .AllowAnyHttpStatus()
                .PostJsonAsync(message)
                .ConfigureAwait(false);

            var json = await resp.Content.ReadAsStringAsync();

            //var answer = new
            //{
            //    intents = default(object),
            //    entities = default(object),
            //    input = default(object),
            //    output = new
            //    {
            //        text = default(string[])
            //    },
            //    context = default(object)
            //};

            //var answer = JsonConvert.DeserializeObject<>(json);
            return JObject.Parse(json);
            //if (conversationContextDictionary.ContainsKey(conversationId))
            //    conversationContextDictionary[conversationId] = answer.context.ToString();
            //else
            //    conversationContextDictionary.Add(conversationId, answer.context.ToString());

            //return answer.entities;

            //var output = "";
            //if (answer != null && answer.output != null && answer.output.text != null)
            //    output = answer.output.text.Aggregate(
            //       new StringBuilder(),
            //       (sb, l) => sb.AppendLine(l),
            //       sb => sb.ToString());

            //var response = string.Format(output);
            //return JsonConvert.SerializeObject(answer);
        }
    
    }

    //var answer = new
    //{
    //    intents = default(object),
    //    entities = default(object),
    //    input = default(object),
    //    output = new
    //    {
    //        text = default(string[])
    //    },
    //    context = default(object)
    //};


    //public class Answer
    //{
    //    public object intents;
    //    public IEnumerable<object>
    //}
}