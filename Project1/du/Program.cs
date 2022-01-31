

namespace du
{
    public class Program
    {

        public static void Main(string[] args)
        {
            if (!ValArgs(args[0].Split(" ")))
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
            
            Console.WriteLine("ok");
        }
        
        private static bool ValArgs(string[] args)
        {

            var cmd = args[0];
            var src = args[1];

            // Check directory
            if (!Directory.Exists(src))
            {
                return false;
            }
        
            // check if first arg is correct
            if (cmd.Equals("-s"))
            {
                return true;
            }
            if (cmd.Equals("-p"))
            {
                return true;
            }
            if (cmd.Equals("-b"))
            {
                return true;
            }
            
            // cmd not valid, fail
            return false;
        }





    }


}
