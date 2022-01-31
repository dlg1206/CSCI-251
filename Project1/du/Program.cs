

namespace du
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var input = args[0].Split(" ");
            if (args.Length == 0 || !ValArgs(input))
            {
                Console.WriteLine("Usage: du [-s] [-p] [-b] <path>");
                Console.WriteLine("Summarize disk usage of the set of FILEs, recursively for directories.\n");
                Console.WriteLine("You MUST specify one of the parameters, -s, -p, or -b");
                Console.WriteLine("-s       Run in single threaded mode");
                Console.WriteLine("-p       Run in parallel mode (uses all available processors)");
                Console.WriteLine("-b       Run in both single threaded and parallel mode.");
                Console.WriteLine("         Runs sequential followed by parallel mode");
                return;
            }
            
            // Deter commands
            switch (input[0])
            {
                case "-s":
                    ParseSeq(input[1]);
                    break;
                case "-p":
                    ParsePar(input[1]);
                    break;
                case "-b":
                    // todo order? err says seq then par, but demo shows par then seq
                    ParseSeq(input[1]);
                    ParsePar(input[1]);
                    break;
                default:
                    return;
            }
            
            
        }
        
        private static bool ValArgs(string[] args)
        {
            // only expecting 2 args
            if (args.Length != 2) { return false; }
   
            // Check directory
            if (!Directory.Exists(args[1])) { return false; }
        
            // check if cmd is correct
            switch (args[0])
            {
                case "-s":
                    return true;
                case "-p":
                    return true;
                case "-b":
                    return true;
                default:
                    return false;
            }
          
        }


        private static void ParseSeq(string src)
        {
            Console.WriteLine("Do sequential");
        }

        private static void ParsePar(string src)
        {
            Console.WriteLine("do //");
        }


    }


}
