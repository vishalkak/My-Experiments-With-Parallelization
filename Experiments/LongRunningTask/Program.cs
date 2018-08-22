using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

//
// Creates 100 long-running tasks to see how .NET 4 responds.  In this case,
// we create one per core, and then as tasks finish, we create more.  This 
// does not over-subscribe the system, since the work queue is empty otherwise,
// so .NET does not create additional worker threads to compensate for these
// long-running tasks.
//
namespace LongRunning
{
  class Program
  {

		public static void Main(string[] args)
		{
			int N = 100;
			int durationInMins = 0;
			int durationInSecs = 20;

			Welcome(N, durationInMins, durationInSecs);

			//
			// Create 1 per core, and then as they finish, create another:
			//
			int numCores = System.Environment.ProcessorCount;

			List<Task> tasks = new List<Task>();

			//
			// create initial set of tasks:
			//
			for (int i = 0; i < numCores - 1; i++)
			{
				Task t = CreateOneLongRunningTask(durationInMins, durationInSecs, TaskCreationOptions.None);
				tasks.Add(t);
			}

			//
			// now, as they finish, create more:
			//
			int done = 0;

			while (done < N)
			{
				int index = Task.WaitAny(tasks.ToArray());

				done++;
				tasks.RemoveAt(index);

				if (done < N)
				{
					Task t = CreateOneLongRunningTask(durationInMins, durationInSecs, TaskCreationOptions.None);
					tasks.Add(t);
				}
			}//while

			//
			// done:
			//
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("** Done!");
		}


		//
		// chews on CPU for give mins and secs:
		//
		static Task CreateOneLongRunningTask(int durationInMins, int durationInSecs, TaskCreationOptions options)
		{
			long durationInMilliSecs = durationInMins * 60 * 1000;
			durationInMilliSecs += (durationInSecs * 1000);

			Task t = Task.Factory.StartNew(() =>
				{
				  Console.WriteLine("starting task...");
				  
					var sw = System.Diagnostics.Stopwatch.StartNew();
					long count = 0;

					while (sw.ElapsedMilliseconds < durationInMilliSecs)
					{
						count++;
						if (count == 1000000000)
							count = 0;
					}
					
				  Console.WriteLine("task finished.");
				}, 
				options
			);

			return t;
		}


		//
		// Welcome the user:
		//
		static void Welcome(int N, int durationInMins, int durationInSecs)
		{
			String version, platform;

#if DEBUG
			version = "debug";
#else
			version = "release";
#endif

#if _WIN64
	platform = "64-bit";
#elif _WIN32
	platform = "32-bit";
#else
			platform = "any-cpu";
#endif

			Console.WriteLine("** Long-running Tasks App -- One per core [{0}, {1}] **", platform, version);
			Console.WriteLine("   Number of tasks: {0:#,##0}", N);
			Console.WriteLine("   Number of cores: {0:#,##0}", System.Environment.ProcessorCount);
			Console.WriteLine("   Task duration:   {0:#,##0} mins, {1:#,##0} secs", durationInMins, durationInSecs);
			Console.WriteLine();
		}

   }//class
}//namespace
