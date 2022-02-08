

using System.Diagnostics;

namespace du
{
    
    public class Program
    {
        
        public static void Main(string[] args)
        {
     
            
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
            
            // Deter commands
            switch (args[0])
            {
                case "-s":
                    DoSeq(args[1]);
                    break;
                // TODO - wants to run a project when run -p
                /*
                 * ←[33mWarning NETSDK1174: The abbreviation of -p for --project is deprecated. Please use --project.←[39m
                    Couldn't find a project to run. Ensure a project exists in C:\Program Files\JetBrains, 
                    or pass the path to the project using --project.
                 */
                case "-p":
                    DoPar(args[1]);
                    break;
                case "-b":
                    DoSeq(args[1]);
                    DoPar(args[1]);
                    break;
                default:
                    return;
            }
            
            
        }

        private static void DoSeq(string src)
        {
            var sw = new Stopwatch();
            long[] info;
            sw.Start();
            info = ParseSeq(src, new long[3]);
            sw.Stop();
            PrintResults(sw, "Sequential", info);
        }
        
        private static void DoPar(string src)
        {
            var sw = new Stopwatch();
            long[] info;
            sw.Start();
            info = ParsePar(src, new long[3]);
            sw.Stop();
            PrintResults(sw, "Parallel", info);
            
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

        private static void PrintResults(Stopwatch sw, string mode, long[] info)
        {
            // .elapsed

            Console.WriteLine("\n" + mode + " Calculated in: {0}s, ", sw.Elapsed );
            Console.WriteLine("{0:n0} folders, {1:n0} files, {2:n0} bytes\n", info[0], info[1], info[2]);
        }

        
        // TODO folders + files correct, byte count slightly off
        private static long[] ParseSeq(string src, long[] info)
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

                
                info[1]++;

                try
                {
                    var file = File.Open(fileName, FileMode.Open);
                    info[2] += file.Length;
                    file.Close();
                }
                catch(Exception)
                {
                    
                }
            }
            
            return info;
        }
        
        
        
        // TODO Folders, files, byte count All off - where to plac locks?
        private static Object _parLock = new Object();
        private static long[] ParsePar(string src, long[] info)
        {
            try
            {
                
                {
                    Directory.SetCurrentDirectory(src);
                    Parallel.ForEach(Directory.GetDirectories(src), dir =>
                    {
                        lock (_parLock)
                        {
                            info[0]++;
                        }
                            

                        ParseSeq(dir, info);

                    });
                }
            }
            catch (Exception)
            {
               
            }

            
            {
                Parallel.ForEach(Directory.GetFiles(src), fileName =>
                {

                    lock (_parLock)
                    {
                        info[1]++;
                    }

                    try
                    {
                        var file = File.Open(fileName, FileMode.Open);
                        lock (_parLock)
                        {
                            
                            info[2] += file.Length;
                            
                        }
                        file.Close();

                        
                    }
                    catch (Exception)
                    {

                    }
                });
            }

            return info;
        }


    }


}
