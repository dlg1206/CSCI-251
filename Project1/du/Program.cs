/// Project 1; @author Derek Garcia

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
                    var s = new SequentialSearch();
                    s.DoSeq(args[1]);
                    break;
                // Parallel
                case "-p":
                    var p = new ParallelSearch();
                    p.DoPar(args[1]);
                    break;
                // Both
                case "-b":
                    // Parallel
                    var pb = new ParallelSearch();
                    pb.DoPar(args[1]);
                    
                    // Sequential
                    var sb = new SequentialSearch();
                    sb.DoSeq(args[1]);
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
            if (args.Length != 2)
            {
                return false;
            }

            // Check if directory exists
            if (!Directory.Exists(args[1]))
            {
                return false;
            }

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
        /// Sequential Search for files
        /// </summary>
        private class SequentialSearch
        {
            private int _fileCount;
            private int _folderCount;
            private long _byteCount;
            
            
            /// <summary>
            /// Encompasses stopwatch and output for Sequential parsing
            /// </summary>
            /// <param name="src">root directory</param>
            public void DoSeq(string src)
            {
                var sw = new Stopwatch();
                sw.Start();
                ParseSeq(src); // Sequential Parsing
                sw.Stop();
                
                // Print Results
                Console.WriteLine("\nSequential Calculated in: {0}s, ", sw.Elapsed);
                Console.WriteLine("{0:n0} folders, {1:n0} files, {2:n0} bytes\n", _folderCount, _fileCount, _byteCount);
            }


            /// <summary>
            /// Sequentially parse with recursion
            /// </summary>
            /// <param name="src">root directory</param>
            /// <returns>data with directory, file, and byte information</returns>
            private void ParseSeq(string src)
            {
                
                // Attempt to open directory
                var di = new DirectoryInfo(src);
                try
                {
                    // Recursively go through directories
                    foreach (var dir in di.GetDirectories())
                    {
                        _folderCount++; // update directory count
                        ParseSeq(dir.FullName);
                    }
                }
                // Catch if unable to open directory
                catch (Exception)
                {

                }

                // Count each file once no more directories to go into
                foreach (var file in di.GetFiles())
                {
                    _fileCount++;   // update file count
                    _byteCount += file.Length;  // update byte count
                }
            }
        }
        
        
        /// <summary>
        /// Parallel Search Class
        /// </summary>
        private class ParallelSearch
        {
            private int _fileCount;
            private int _folderCount;
            private long _byteCount;
            
            /// <summary>
            /// Encompasses stopwatch and output for Parallel parsing
            /// </summary>
            /// <param name="src"></param>
            public void DoPar(string src)
            {
                var sw = new Stopwatch();
                sw.Start();
                ParsePar(src); // Parallel Parsing
                sw.Stop();

                // Print Results
                Console.WriteLine("\nParallel Calculated in: {0}s, ", sw.Elapsed);
                Console.WriteLine("{0:n0} folders, {1:n0} files, {2:n0} bytes\n", _folderCount, _fileCount, _byteCount);
            }

            
            /// <summary>
            /// Parallel parse with recursion
            /// </summary>
            /// <param name="src">root directory</param>
            /// <returns>data with directory, file, and byte information</returns>
            private void ParsePar(string src)
            {
                // Attempt to open directory
                var di = new DirectoryInfo(src);

                // Directory parsing
                try
                {
                    
                    Parallel.ForEach(di.GetDirectories(), dir =>
                    {
                        // Update directory count
                        Interlocked.Add(ref _folderCount, 1);

                        // Parse next directory
                        ParsePar(dir.FullName);
                    });
                }
                // Catch if unable to open directory
                catch (Exception)
                {

                }
                
                // Files and File sizes
                try
                {
                    // Count each file once no more directories to go into
                    Parallel.ForEach(di.GetFiles(), file =>
                    {
                        // TODO what does ref mean?

                        Interlocked.Add(ref _fileCount, 1); // Update file count

                        Interlocked.Add(ref _byteCount, file.Length); // update byte count

                    });

                }
                catch(Exception)
                {
                    
                }
            }
        }
    }
}
