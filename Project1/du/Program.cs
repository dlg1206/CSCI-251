

using System.Diagnostics;

namespace du
{
    public class Program
    {

        public static void Main(string[] args)
        {
            // todo account for spaces? ie "-b Program Files" -> [-b, Program, Files-
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
            var sw = new Stopwatch();
            // Deter commands
            switch (input[0])
            {
                case "-s":
                    sw.Start();
                    int[] info = ParseSeq(input[1], new int[3]);
                    sw.Stop();
                    PrintResults(sw, "Sequential", info);
                    break;
                case "-p":
                    ParsePar(input[1]);
                    break;
                case "-b":
                    // todo order? err says seq then par, but demo shows par then seq : PAR -> SEQ
                    ParseSeq(input[1], new int[3]);
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

        private static void PrintResults(Stopwatch sw, string mode, int[] info)
        {
            // .elapsed
            var elapsed = String.Format("{0:0}.{1:0}", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds / 10);
            Console.WriteLine("\n" + mode + " Calculated in: " + elapsed + "s" );
            Console.WriteLine("{0:n0} folders, {1:n0} files, {2:n0} bytes\n", info[0], info[1], info[2]);
        }

        
        /// <summary>
        ///
        /// 0 folders
        /// 1 files
        /// 2 bytes
        /// </summary>
        /// <param name="src"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private static int[] ParseSeq(string src, int[] info)
        {
            try
            {
                Directory.SetCurrentDirectory(src);
                foreach (var dir in Directory.GetDirectories(src))
                {
                    info[0]++;
                    ParseSeq(dir, info);
                }
            }
            catch (Exception)
            {
                
            }
        
            
            
            foreach (var fileName in Directory.GetFiles(src))
            {
                // todo update count if can't read file? - NO
                
                info[1]++;

                try
                {
                    var file = File.Open(fileName, FileMode.Open);
                    info[2] += (int) file.Length;
                    file.Close();
                }
                catch(Exception)
                {
                    
                }
            }
            
            return info;
        }

        private static void ParsePar(string src)
        {
            Console.WriteLine("do //");
        }


    }


}
