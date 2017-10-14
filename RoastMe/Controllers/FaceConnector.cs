using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Net;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RoastMe.Controllers
{
    public class FaceConnector
    {
        private readonly IFaceServiceClient faceServiceClient =
         new FaceServiceClient("275c8bcbe86c4064a32df7945ae78784", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");

        Face[] faces;                   // The list of detected faces.
        String[] faceDescriptions;      // The list of descriptions for the detected faces.
        double resizeFactor;
        // The resize factor for the displayed image.

        public async Task<Face[]> UploadAndDetectFaces(Activity activity)
        {
            // Call the Face API.
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Authorization] = System.Web.HttpContext.Current.Request.Headers["Authorization"];
                    var byteArray = await LoadAttachmentAsBytes(activity);
                    var imageStream = new MemoryStream(byteArray);

                    Face[] faces = await faceServiceClient.DetectAsync(imageStream, true, true, Enum.GetValues(typeof(FaceAttributeType)).Cast<FaceAttributeType>().ToList());
                    return faces;
                }

            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                throw f;
                //MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error");
                throw e;
                //return new Face[0];
            }
        }

        public async Task<IEnumerable<byte[]>> GetAttachmentsAsync( Activity activity)
        {
            var attachments = activity?.Attachments?
                .Where(attachment => attachment.ContentUrl != null)
                .Select(c => Tuple.Create(c.ContentType, c.ContentUrl));
            if (attachments != null && attachments.Any())
            {
                var contentBytes = new List<byte[]>();
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    var token = await (connectorClient.Credentials as MicrosoftAppCredentials).GetTokenAsync();
                    foreach (var content in attachments)
                    {
                        var uri = new Uri(content.Item2);
                        using (var httpClient = new HttpClient())
                        {
                            if (uri.Host.EndsWith("skype.com") && uri.Scheme == "https")
                            {
                                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                            }
                            else
                            {
                                //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(content.Item1));
                            }
                            contentBytes.Add(await httpClient.GetByteArrayAsync(uri));
                        }
                    }
                }
                return contentBytes;
            }
            return null;
        }

        public async static Task<byte[]> LoadAttachmentAsBytes(Activity activity)
        {
            var attachment = activity.Attachments[0];
            using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                var content = connectorClient.HttpClient.GetStreamAsync(attachment.ContentUrl).R‌​esult;
                var memoryStream = new MemoryStream();
                content.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}