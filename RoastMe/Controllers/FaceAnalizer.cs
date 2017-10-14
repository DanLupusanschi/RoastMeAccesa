using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoastMe.Controllers
{
    public class Trait
    {
        public string Name { get; set; }
        public double Accuracy { get; set; }
    }

    public static class FaceAnalizer
    {
        public static List<Trait> GetTraitsFromFace(Face face)
        {
            var traits = new List<Trait>();
            ProcessGlasses(face.FaceAttributes, traits);
            ProcessHairColor(face.FaceAttributes, traits);
            ProcessBald(face.FaceAttributes, traits);
            ProcessNose(face.FaceRectangle, face.FaceLandmarks, face.FaceAttributes, traits);
            ProcessBeard(face.FaceAttributes, traits);
            ProcessEmotion(face.FaceAttributes, traits);
            ProcessOldAge(face.FaceAttributes, traits);

            return traits;
        }

        private static void ProcessGlasses(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Glasses == Glasses.ReadingGlasses)
                traits.Add(new Trait { Name = "glasses", Accuracy = 1.0 });
        }

        private static void ProcessNose(FaceRectangle faceRectangle, FaceLandmarks faceLandmarks, FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (Math.Abs(faceAttributes.HeadPose.Yaw) < 8.0)
            {
                var noseRootSize = GetDistance(faceLandmarks.NoseRootLeft, faceLandmarks.NoseRootRight);
                var noseAlarTopSize = GetDistance(faceLandmarks.NoseLeftAlarTop, faceLandmarks.NoseRightAlarTop);
                var noseAlartTipSize = GetDistance(faceLandmarks.NoseLeftAlarOutTip, faceLandmarks.NoseRightAlarTop);
                var noseHeight = GetDistance(faceLandmarks.NoseLeftAlarOutTip, faceLandmarks.NoseRootLeft);

                var noseRootProportion = noseRootSize / faceRectangle.Width;
                var noseAlarTopProportion = noseAlarTopSize / faceRectangle.Width;
                var noseAlarTipProportion = noseAlartTipSize / faceRectangle.Width;
                var noseHeightProportion = noseHeight / faceRectangle.Height;

                var numberOfHits = 0;
                if (noseRootProportion > 0.13)
                {
                    numberOfHits++;
                }
                if (noseAlarTopProportion > 0.2)
                {
                    numberOfHits++;
                }
                if (noseAlarTipProportion > 0.25)
                {
                    numberOfHits++;
                }
                if (noseHeightProportion > 0.25)
                {
                    numberOfHits++;
                }

                if (numberOfHits >= 2)
                {
                    traits.Add(new Trait { Name = "bignose", Accuracy = 1.0 });
                }
            }
            else
            {
                var noseLeftLength = GetDistance(faceLandmarks.NoseTip, faceLandmarks.NoseLeftAlarOutTip);
                var noseRightLength = GetDistance(faceLandmarks.NoseTip, faceLandmarks.NoseRightAlarOutTip);

                var noseLength = Math.Max(noseLeftLength, noseRightLength);
                var noseLengthProportion = noseLength / (faceRectangle.Width * Math.Abs(faceAttributes.HeadPose.Yaw));

                if (noseLengthProportion > 0.008)
                {
                    traits.Add(new Trait { Name = "bignose", Accuracy = 1.0 });
                }
            }
        }

     

        private static double GetDistance(FeatureCoordinate start, FeatureCoordinate end)
        {
            return Math.Sqrt(Math.Pow((end.X - start.X), 2) + Math.Pow((end.Y - start.Y), 2));
        }

        private static void ProcessBeard(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.FacialHair.Beard >= 0.4)
                traits.Add(new Trait { Name = "beard", Accuracy = faceAttributes.FacialHair.Beard });
        }
        private static void ProcessBald(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Hair.Bald >= 0.4)
                traits.Add(new Trait { Name = "bald", Accuracy = faceAttributes.Hair.Bald });
        }
        private static void ProcessHairColor(FaceAttributes faceAttributes, List<Trait> traits)
        {
            var hair = faceAttributes.Hair.HairColor.OrderByDescending(x => x.Confidence).FirstOrDefault();
            if (hair != null && hair.Color == HairColorType.Blond && faceAttributes.Gender == "female")
                traits.Add(new Trait { Name = "blonde", Accuracy = hair.Confidence });
        }

        private static void ProcessOldAge(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Age > 40)
                traits.Add(new Trait { Name = "old", Accuracy = 1 });
        }

        private static void ProcessEmotion(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Emotion.Sadness >= 0.4)
            {
                traits.Add(new Trait { Name = "sadness", Accuracy = faceAttributes.Emotion.Sadness });
            }

            if (faceAttributes.Emotion.Surprise >= 0.4)
            {
                traits.Add(new Trait { Name = "surprise", Accuracy = faceAttributes.Emotion.Surprise });
            }
        }

        private static void ProcessMakeup(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Makeup?.EyeMakeup == true)
            {
                traits.Add(new Trait { Name = "eyemakeup", Accuracy = 1 });
            }

            if (faceAttributes.Makeup?.LipMakeup == true)
            {
                traits.Add(new Trait { Name = "lipmakeup", Accuracy = 1 });
            }
        }
    }
}