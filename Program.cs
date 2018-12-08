using System;
using System.Transactions;
using System.Collections;
using System.Collections.Generic;

namespace InMemKeyValueStore
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // Process command line input
            // No verbose command line validations for simplicity
            if (args.Length < 1)
            {
                // Usage information 
                Usage();
                return;
            }
            else
            {
                // Main transaction, written as all together execution with multiple-command support
                // Instead of porting the BEGIN transaction out to the user, for simplicity, all commands are
                // under transaction.  COMMIT AND ROLLBACK are provided as last choice for the user.
                using (TransactionScope tran = new TransactionScope())
                {
                    try
                    {
                        // Hashtable for built in functions.
                        // Larger scale POCs would need something more robust and with perm storage.
                        Hashtable hashtable = new Hashtable();

                        bool active = true;

                        while (active)
                        {
                            // Loop through commands
                            foreach (string cmd in args)
                            {
                                if (cmd.ToLower() == "quit")
                                {
                                    tran.Dispose();
                                    return;
                                }

                                int index = cmd.IndexOf('=');
                                string command = (index > 0 ? cmd.Substring(0, index) : "");
                                string cmdvalue = (index > 0 ? cmd.Substring(index, cmd.Length - index) : "");
                                cmdvalue = cmdvalue.Replace("=", "");

                                string[] set = null;
                                if (cmdvalue.Contains(","))
                                    set = cmdvalue.Split(',');
                                else
                                    set = new string[] { cmdvalue, "" };

                                // Switch on commands
                                command = command.ToLower();
                                string key = set[0];
                                string value = set[1];
                                switch (command)
                                {
                                    case "put":
                                        if (hashtable.ContainsKey(key))
                                            hashtable[key] = value;
                                        else
                                            hashtable.Add(key, value);
                                        break;
                                    case "delete":
                                        if (hashtable.ContainsKey(key))
                                            hashtable.Remove(key);
                                        break;
                                    case "putref":
                                        if (hashtable.ContainsKey(hashtable[key]))
                                            hashtable[hashtable[key]] = value;
                                        else
                                            Console.WriteLine("No record for reference key");
                                        break;
                                    case "get":
                                        if (hashtable.ContainsKey(key))
                                            Console.WriteLine(key + " = " + hashtable[key]);
                                        else
                                            Console.WriteLine("No record for key");
                                        break;
                                    case "getref":
                                        if (hashtable.ContainsKey(hashtable[key]))
                                            Console.WriteLine(hashtable[key] + " = " + hashtable[hashtable[key]]);
                                        else
                                            Console.WriteLine("No record for key");
                                        break;
                                    case "quit":
                                        tran.Dispose();
                                        return;
                                    default:
                                        break;
                                }
                            }

                            // Prompt/Loop for more commands
                            Console.WriteLine("Another Command?: PUT,DELETE,PUTREF,GET,GETREF,QUIT");
                            String response = Console.ReadLine();

                            // Loop to process another command or exit on quit or usage on enter blank
                            if (response is null)
                            {
                                Usage();
                                tran.Dispose();
                                return;
                            }
                            else
                            {
                                args = response.Split(' ');
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tran.Dispose();
                        Console.WriteLine("ERROR");
                        Console.WriteLine(ex);
                    }
                 }

                // modify to be the output
                //foreach (var element in list)
                //{
                //    Console.WriteLine(element);
                //}
            }
        }

        // Basic usage method
        private static void Usage()
        {
            Console.WriteLine("Creates and maintains an in-memory table with transaction support.");
            Console.WriteLine();
            Console.WriteLine("EXAMPLE: PUT=A,1 PUT=B,2");  // Multi command example
            Console.WriteLine("PUT=<key>,<value>");         // Upsert
            Console.WriteLine("DELETE=<key>");              // Delete, No-out for success or no key found
            Console.WriteLine("PUTREF=<key>,<value>");      // Key pointer change (A = 'B', PR A, 1 => B = '1')
            Console.WriteLine("GET=<key>");                 // Success print value, no key found = error msg
            Console.WriteLine("GETREF=<key>,<value>");      // Success print value of pointer key, ( B = 1, A = B, GR A => 1)
            Console.WriteLine("QUIT");                      // Ends program
        }
    }
}