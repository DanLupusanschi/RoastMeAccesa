using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoastMe.Controllers
{
    public class Trait
    {
        string Name { get; set; }
        string Accuracy { get; set; }

    }
    public static class FaceAnalizer
    {
        public static List<Trait> GetTraitsFromFace(Face face)
        {
            var traits = new List<Trait>();



            return traits;
        }
    }
}