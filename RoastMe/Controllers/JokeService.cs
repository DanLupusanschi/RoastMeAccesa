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
            string conn = ConfigurationManager.ConnectionStrings["RoastMeDbEntities"].ConnectionString;
            //The query to use
            string query = "SELECT * FROM Jokes";
            SqlConnection connection = new SqlConnection(conn);
            //Create a Data Adapter
            SqlDataAdapter dadapter = new SqlDataAdapter(query, connection);
            //Create the dataset
            DataSet ds = new DataSet();
            //Open the connection
            connection.Open();
            //Fill the Data Adapter
            dadapter.Fill(ds, "Jokes");
            connection.Close();

            if(traits.Count <= 0)
                traits.Add(new Trait { Name = "neutral", Accuracy = 1 });

            var jokes = new List<string>();

            for(var i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                if (traits.Any(x => x.Name == ds.Tables[0].Rows[i][1].ToString()))
                {
                    jokes.Add(ds.Tables[0].Rows[i][0].ToString());
                }
            }

            var rnd = new Random();
            var jokeNo = rnd.Next(0, jokes.Count);
            return jokes.ElementAt(jokeNo);
        }
    }
}