/*
 * file: Program.cs
 * Description: Main driver program that handles command line arguments
 * 
 * @author Derek Garcia
 */


namespace Messenger
{
    /// <summary>
    /// Main driver program that handles command line arguments
    /// </summary>
    public class Program
    {

        /// <summary>
        /// Prints the correct usage of the command line commands
        /// </summary>
        private void PrintUsage()
        {
            Console.WriteLine("usage: dotnet run <option> <other arguments>");
                
            Console.WriteLine(
                "\t- keyGen <keySize>: generate a keypair of size keySize bits (public and private keys) and " +
                "store them locally on the disk");
                
            Console.WriteLine(
                "\t- sendKey <email>: sends the public key that was generated in the keyGen phase and send it to " +
                "the server, with the email address given");

            Console.WriteLine(
                "\t- getKey <email>: this will retrieve public key for a particular user with that email");

            Console.WriteLine(
                "\t- sendMsg <email> <plaintext>: this will take a plaintext message, encrypt it using the public" +
                " key of the person you are sending it to, based on their email address.");

            Console.WriteLine(
                "\t- getMsg <email>: this will retrieve a message for a particular user.");
        }

        /// <summary>
        /// Parses command line inputs and executes their respective commands
        /// </summary>
        /// <param name="args">command line arguments</param>
        public static async Task Main(string[] args)
        {
            
            var p = new Program();
            
            // Check that command line args exist
            if (args.Length == 0)
            {
                p.PrintUsage();
                return;
            }

            // Init managers for commands
            var keyManager = new KeyManger();
            var webClient = new WebClient();

            // Attempt to execute the given commands
            switch (args[0])
            {
                // Attempt generate a public - private key
                case "keyGen":
                    if (args.Length != 2 || !int.TryParse(args[1], out var keySize))
                    {
                        p.PrintUsage();
                    }
                    else
                    {
                        keyManager.KeyGen(keySize);     // make key
                    }
                    break;
                
                // Attempt to send key to server
                case "sendKey":
                    if (args.Length != 2)
                    {
                        p.PrintUsage();
                    }
                    else
                    {
                        await webClient.SendKey(args[1], keyManager);   // send key
                    }
                    break;
                
                case "getKey":
                    if (args.Length != 2)
                    {
                        p.PrintUsage();
                    }
                    else
                    {
                        await webClient.GetKey(args[1], keyManager);
                    }
                    break;
                
                case "sendMsg":
                    break;
                
                case "getMsg":
                    break;
                
                default:
                    p.PrintUsage();
                    break;
            }
            

            // var web = "AAAAAwEAAQAAAQB7w4yJG+kH5BXhL9lgeCxkNqKeIIyC0zzG0FYJu5/WVa7xCdXGSmG3pEEpyEPhe81L9zb1qWpnn" +
            //           "9yoiMPPawtDoZ26Um0LA/MAx/n4UdBENyWYd807+ex1h/uJ/GHgeZI/8yZ5LapCTNXaAwXvTfSY4OTG9hEgTJ6uK7cM11hn/q" +
            //           "K07EnH1beaGoj/FOATFPqpLkDaz/fOkRQIQr6F41ks0PIJXjzmMeIJdUhBsluJaU/pllHqjTDFk2uBOSQr5g0WFeCVLfss0E" +
            //           "Ybkbx3BsLtvThDgphBc98KOU2gx3o+Tm5U1oTT/tZdUjrWq8iPWzI+JMrG1RtZEVVeewOFT5sn";


            /*
             * private key emails: list of all emails that I have sent to sever using that private key
             * pub key email: NONE< don't touch
             */
        }
        
    }
    


    
    
}