using System;
using System.Collections.Concurrent;

namespace Pluralsight.ConcurrentImmutable.SalesBonuses
{


	public class ToDoQueue
	{
		private readonly BlockingCollection<Trade> _queue
			= new BlockingCollection<Trade>(new ConcurrentBag<Trade>());
		private readonly StaffLogsForBonuses _staffLogs;

		public ToDoQueue(StaffLogsForBonuses staffResults)
		{
			_staffLogs = staffResults;
		}

		public void AddTrade(Trade transaction)
		{
			_queue.Add(transaction);
		}

		public void CompleteAdding()
		{
			_queue.CompleteAdding();
		}



		public void MonitorAndLogTrades()
		{
			while (true)
			{
				try
				{
					Trade nextTransaction = _queue.Take();
					_staffLogs.ProcessTrade(nextTransaction);
					Console.WriteLine("Processing transaction from " + nextTransaction.Person.Name);
				}
				catch (InvalidOperationException ex)
				{
					Console.WriteLine(ex.Message);
					return;
				}
			}
		}


	}

}
