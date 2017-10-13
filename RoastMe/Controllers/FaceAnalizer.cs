using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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



            return traits;
        }

       
        private static void ProcessGlasses(FaceAttributes faceAttributes, List<Trait> traits) {
            if (faceAttributes.Glasses == Glasses.ReadingGlasses)
                traits.Add(new Trait {Name = Glasses.ReadingGlasses.ToString(), Accuracy = 1.0 });
        }

        private static void ProcessBeard(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.FacialHair.Beard >= 0.5)
                traits.Add(new Trait { Name = "Beard", Accuracy = faceAttributes.FacialHair.Beard });
        }
        private static void ProcessBald(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Hair.Bald >= 0.5)
                traits.Add(new Trait { Name = "Bald", Accuracy = faceAttributes.FacialHair.Beard });
        }
        private static void ProcessHairColor(FaceAttributes faceAttributes, List<Trait> traits)
        {
            if (faceAttributes.Hair.HairColor.OrderByDescending(x=>x.Confidence).First().Color == HairColorType.Blond && faceAttributes.Gender== "female")
                traits.Add(new Trait { Name = "Blonde", Accuracy = faceAttributes.FacialHair.Beard });
        }

    }
}