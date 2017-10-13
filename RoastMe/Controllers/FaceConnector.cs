using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Net;

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

        public async Task<Face[]> UploadAndDetectFaces(string imageFilePath)
        {
            // Call the Face API.
            try
            {
                WebClient client = new WebClient();
                var imageStream = client.OpenRead(imageFilePath);

                Face[] faces = await faceServiceClient.DetectAsync(imageStream, true, false, Enum.GetValues(typeof(FaceAttributeType)).Cast<FaceAttributeType>().ToList());
                return faces;
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                //MessageBox.Show(f.ErrorMessage, f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "Error");
                return new Face[0];
            }
        }
    }
}