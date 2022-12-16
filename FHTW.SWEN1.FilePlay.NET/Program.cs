using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Text.Json;



namespace FHTW.SWEN1.FilePlay
{
    /// <summary>This program demonstrates data reading and writing from/to files and databases.</summary>
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Program data.</summary>
        private static List<District> Districts = new();



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // main entry point                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            //  Step 1: reading district data from a file using streams
            using(FileStream f = new FileStream(@"..\..\..\..\gemeinden.txt", FileMode.Open))
            {
                using(StreamReader re = new StreamReader(f))
                {
                    string? n;
                    while((n = re.ReadLine()) != null)                          // reading each line using StreamReader
                    {
                        Districts.Add(new District(n[..5], n[6..].Trim()));     // creating and storing data object
                    }
                }
            }


            // Step 2: saving data as CSV file using streams
            using(FileStream g = new FileStream(@"..\..\..\..\gemeinden.csv", FileMode.Create))
            {
                using(StreamWriter wr = new StreamWriter(g, Encoding.UTF8))
                {
                    foreach(District i in Districts.OrderBy(m => m.ID))
                    {                                                           // write data
                        wr.Write(i.ID);
                        wr.Write(';');
                        wr.Write(i.Name);
                        wr.Write(";\r\n");
                    }
                    wr.Flush();
                }
            }


            // Step 3: reading the CSV file in again
            Districts = new();                                                  // discard existing data

            string[] dat = File.ReadAllLines(@"..\..\..\..\gemeinden.csv");     // we're using File.ReadAllLines to
                                                                                // get the lines in the file as a
            foreach(string i in dat)                                            // string array and iterate
            {
                string[] q = i.Split(';');                                      // split line by seperator, get array
                Districts.Add(new(q[0], q[1]));                                 // parse array into object
            }


            // Step 4: write data as JSON
            File.WriteAllText(@"..\..\..\..\gemeinden.json",                    // serialize and write to file
                              JsonSerializer.Serialize(Districts, new JsonSerializerOptions() { WriteIndented = true }));


            // Step 5: write to SQLite database
            IDbConnection cn = new SQLiteConnection(@"Data Source=..\..\..\..\gemeinden.sqlite; Version=3");
            cn.Open();                                                          // create and open database connection

            IDbTransaction t = cn.BeginTransaction();                           // using a transaction to write all
                                                                                // data in an atomic way
            
            foreach(District i in Districts)                                    // iterate data
            {
                IDbCommand cmd = cn.CreateCommand();                            // create and initialize command
                cmd.CommandText = "INSERT INTO DISTRICTS (ID, NAME) VALUES (:id, :name)";
                cmd.Transaction = t;                                            // set transaction

                IDataParameter p = cmd.CreateParameter();                       // add parameters
                p.ParameterName = ":id";
                p.Value = i.ID;
                cmd.Parameters.Add(p);

                p = cmd.CreateParameter();
                p.ParameterName = ":name";
                p.Value = i.Name;
                cmd.Parameters.Add(p);

                cmd.ExecuteNonQuery();                                          // execute command
            }

            t.Commit();                                                         // commit transaction when all data
                                                                                // has been written
            cn.Close();                                                         // then close database

            Console.WriteLine("All done.");
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [record] District                                                                                        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>This class represents a district.</summary>
        /// <param name="ID">District ID.</param>
        /// <param name="Name">District name.</param>
        public record class District(string ID, string Name);
    }
}