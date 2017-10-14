using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RoastMe.Controllers
{
    public class JokeService
    {
        public string GetJoke(List<Trait> traits)
        {
            if (traits.Count == 0)
                traits.Add(new Trait { Name = "neutral", Accuracy = 1 });
            var allTraits = String.Join(",", traits.Select(x => x.Name));

            string conn = ConfigurationManager.ConnectionStrings["RoastMeDbEntities"].ConnectionString;
            //The query to use
            SqlConnection connection = new SqlConnection(conn);


            SqlCommand cmd = new SqlCommand("GiveMeJoke", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@keyword", SqlDbType.VarChar).Value = allTraits;
            //SqlParameter param = cmd.Parameters.AddWithValue("@joke", SqlDbType.NVarChar);

            //param.Direction = ParameterDirection.Output;
            //SqlParameter returnValue = new SqlParameter();
            //returnValue.SqlDbType = SqlDbType.NVarChar;
            //returnValue.Direction = ParameterDirection.ReturnValue;
            //cmd.Parameters.Add(returnValue);
            SqlDataReader reader;

            connection.Open();

            var joke = "";

            reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    joke = reader.GetString(0);
                    break;
                }
            }
            connection.Close();

            //if(traits.Count <= 0)
            //    traits.Add(new Trait { Name = "neutral", Accuracy = 1 });

            //var jokes = new List<string>();

            //for(var i = 0; i < ds.Tables[0].Rows.Count; i++)
            //{
            //    if (traits.Any(x => x.Name == ds.Tables[0].Rows[i][1].ToString()))
            //    {
            //        jokes.Add(ds.Tables[0].Rows[i][0].ToString());
            //    }
            //}

            //var rnd = new Random();
            //var jokeNo = rnd.Next(0, jokes.Count);
            //return jokes.ElementAt(jokeNo);

            return joke;
        }
    }
}