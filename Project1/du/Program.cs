

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
            // Todo dotnet run args?

            var cmd = args[0];
            var isValid = false;
        
            // check if first arg is correct
            if (cmd.Equals("-s"))
            {
                isValid = true;
            } else if (cmd.Equals("-p"))
            {
                isValid = true;
            }else if (cmd.Equals("-b"))
            {
                isValid = true;
            }
        

            return isValid;
        }





    }


}
