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




            return traits;
        }

       
        private static void ProcessGlasses(FaceAttributes faceAttributes, List<Trait> traits) {
            if (faceAttributes.Glasses == Glasses.ReadingGlasses)
                traits.Add(new Trait {Name = "eyeglasses", Accuracy = 1.0 });
        }
    }
}