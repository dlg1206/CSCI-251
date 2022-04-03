/*
 * Project 2
 * Finds Primes of a given bit size
 * 
 * @author Derek Garcia
 */


namespace Messenger
{
    public class Program
    {
        
        private readonly KeyManger _keyManager = new KeyManger();
        private readonly WebClient _webClient = new WebClient();

        private async Task ParseInput(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            switch (args[0])
            {
                case "keyGen":
                    if (args.Length != 2 || !int.TryParse(args[1], out var keySize))
                    {
                        PrintUsage();
                    }
                    else
                    {
                        _keyManager.KeyGen(keySize);
                    }
                    break;
                
                case "sendKey":
                    if (args.Length != 2)
                    {
                        PrintUsage();
                    }
                    else
                    {
                        // send public
                        // add email to private key email
                        await _webClient.SendKey(args[1], _keyManager);
                    }
                    break;
                
                case "getKey":
                    break;
                
                case "sendMsg":
                    break;
                
                case "getMsg":
                    break;
                
                default:
                    PrintUsage();
                    return;
            }
        }

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

        public static async Task Main(string[] args)
        {

            var p = new Program();
            // var ws = new WebClient();   // init web client

            // await ws.Connect("http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu");
            // Print error if 
            await p.ParseInput(args);
            

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