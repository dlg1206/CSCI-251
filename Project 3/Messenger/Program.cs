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
        /// If the command is invalid or unrecognized, the correct usage is displayed
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
            var keyManager = new KeyManager();
            var webClient = new WebClient();

            // Attempt to execute the given commands
            switch (args[0])
            {
                // Attempt generate a local public - private key
                case "keyGen":
                    if (args.Length == 2 && int.TryParse(args[1], out var keySize))
                    {
                        keyManager.KeyGen(keySize);
                    }
                    else
                    { p.PrintUsage(); }

                    break;

                // Attempt to send key to server
                case "sendKey":
                    if (args.Length == 2)
                    {
                        await webClient.SendKey(keyManager, args[1]); // send key
                    }
                    else { p.PrintUsage(); }
                        
                    break;

                // Attempt to get a key from the server
                case "getKey":
                    if (args.Length == 2)
                    {
                        await webClient.GetKey(args[1]);
                    }
                    else { p.PrintUsage(); }
                    
                    break;

                // Attempt to send a message to the server
                case "sendMsg":
                    if (args.Length == 3)
                    {
                        await webClient.SendMsg(keyManager, args[1], args[2]);
                    }
                    else { p.PrintUsage(); }
                    
                    break;

                // Attempt to get a message from the server
                case "getMsg":
                    if (args.Length == 2)
                    {
                        await webClient.GetMsg(keyManager, args[1]);
                    }
                    else { p.PrintUsage(); }
                        
                    break;

                // The command was unrecognized
                default:
                    p.PrintUsage();
                    break;
            }
        }

    }
}