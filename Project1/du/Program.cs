

using System.Diagnostics;

namespace du
{
    public class Program
    {
        /// <summary>
        /// Parses command line to count files and directories using
        /// sequential or parallel methods
        /// </summary>
        /// <param name="args">summary method and root directory</param>
        public static void Main(string[] args)
        {
            // Error if args aren't valid
            if (!ValArgs(args))
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
            
            // Switches based on given summary method
            switch (args[0])
            {
                // Sequential
                case "-s":
                    DoSeq(args[1]);
                    break;
                // Parallel
                case "-p":
                    DoPar(args[1]);
                    break;
                // Both
                case "-b":
                    DoSeq(args[1]);
                    DoPar(args[1]);
                    break;
                default:
                    return;
            }
        }
        
        
        /// <summary>
        /// Validate input arguments
        /// </summary>
        /// <param name="args">input args by user</param>
        /// <returns>whether the input arguments are valid</returns>
        private static bool ValArgs(string[] args)
        {
            // Only expecting 2 args
            if (args.Length != 2) { return false; }
   
            // Check if directory exists
            if (!Directory.Exists(args[1])) { return false; }
        
            // Check if valid parse method
            switch (args[0])
            {
                // Sequential
                case "-s":
                    return true;
                // Parallel
                case "-p":
                    return true;
                // Both
                case "-b":
                    return true;
                // Command wasn't recognized
                default:
                    return false;
            }
          
        }
        
        
        /// <summary>
        /// Encompasses stopwatch and output for Sequential parsing
        /// </summary>
        /// <param name="src">root directory</param>
        private static void DoSeq(string src)
        {
            var sw = new Stopwatch();
            sw.Start();
            var info = ParseSeq(src, new long[3]);  // Sequential Parsing
            sw.Stop();
            PrintResults(sw, "Sequential", info);
        }
        
        
        /// <summary>
        /// Encompasses stopwatch and output for Parallel parsing
        /// </summary>
        /// <param name="src"></param>
        private static void DoPar(string src)
        {
            var sw = new Stopwatch();
            sw.Start();
            var info = ParsePar(src, new long[3]);  // Parallel Parsing
            sw.Stop();
            PrintResults(sw, "Parallel", info);
        }
        
        /// <summary>
        /// Print the results of a parse search
        /// </summary>
        /// <param name="sw">stopwatch</param>
        /// <param name="mode">parsing mode</param>
        /// <param name="info">data with directory, file, and byte information</param>
        private static void PrintResults(Stopwatch sw, string mode, long[] info)
        {
            Console.WriteLine("\n" + mode + " Calculated in: {0}s, ", sw.Elapsed );
            Console.WriteLine("{0:n0} folders, {1:n0} files, {2:n0} bytes\n", info[0], info[1], info[2]);
        }

        
        /// <summary>
        /// Sequentially parse with recursion
        /// </summary>
        /// <param name="src">root directory</param>
        /// <param name="info">data with directory, file, and byte information</param>
        /// <returns>data with directory, file, and byte information</returns>
        private static long[] ParseSeq(string src, long[] info)
        {
            // Attempt to open directory
            try
            {
                Directory.SetCurrentDirectory(src);
                // Recursively go through directories
                foreach (var dir in Directory.GetDirectories(src))
                {
                    info[0]++;      // update directory count
                    ParseSeq(dir, info);
                }
            }
            // Catch if unable to open directory
            catch (Exception)
            {
               
            }
            
            // Count each file once no more directories to go into
            foreach (var fileName in Directory.GetFiles(src))
            {
                info[1]++;  // update file count
                
                // Attempt to open file
                try
                {
                    var file = File.Open(fileName, FileMode.Open);
                    info[2] += file.Length;     // update byte count
                    file.Close();
                }
                // Catch if unable to open file
                catch(Exception)
                {
                    
                }
            }
            
            // Return the updated information
            return info;
        }
        
        
        private static Object _parLock = new Object();          // Lock for threads
        /// <summary>
        /// Parallel parse with recursion
        /// </summary>
        /// <param name="src">root directory</param>
        /// <param name="info">data with directory, file, and byte information</param>
        /// <returns>data with directory, file, and byte information</returns>
        private static long[] ParsePar(string src, long[] info)
        {
            // Attempt to open directory
            try
            {

                Directory.SetCurrentDirectory(src);
                
                Parallel.ForEach(Directory.GetDirectories(src), dir =>
                {
                    // Update directory count
                    lock (_parLock)
                    {
                        info[0]++;
                    }
                    
                    // Parse next directory
                    ParseSeq(dir, info);
                });
            }
            // Catch if unable to open directory
            catch (Exception)
            {
               
            }
            
            // Count each file once no more directories to go into
            Parallel.ForEach(Directory.GetFiles(src), fileName =>
            {
                // Update file count
                lock (_parLock)
                {
                    info[1]++;
                }
                
                // Attempt to open file
                try
                {
                    // Open and read file
                    
                    lock (_parLock)
                    {
                        var file = File.Open(fileName, FileMode.Open);
                        info[2] += file.Length;     // update byte count
                        file.Close();
                    }
                    
                }
                // Catch if unable to open file
                catch (Exception)
                {

                }
            });
            
            // Return the updated information
            return info;
        }
    }
}
