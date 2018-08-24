using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetStockQuote
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = ShowQuotes(new[] { "MSFT", "GOOG" });
            task.Wait();
        }

        static Task ShowQuotes2(IEnumerable<string> symbols)
        {
            IEnumerator<string> en = null;
            var tcs = new TaskCompletionSource<object>();
            bool firstIteration = true;

            Action<Task<Quote>> iterate = null;
            iterate = t =>
            {
                bool isDone = false;
                Exception error = null;
                try
                {
                    if (!firstIteration)
                    {
                        Quote q = t.Result;
                        Console.WriteLine("{0} ('{1}'): {2}", q.Name, q.Symbol, q.LastTrade);
                    }
                    else
                    {
                        en = symbols.GetEnumerator();
                        firstIteration = false;
                    }

                    if (!en.MoveNext())
                    {
                        isDone = true;
                    }
                    else
                    {
                        string symbol = en.Current;
                        GetQuote(symbol).ContinueWith(iterate);
                    }
                }
                catch (Exception x)
                {
                    error = x;
                    isDone = true;
                }

                if (isDone)
                {
                    if (en != null)
                    {
                        try
                        {
                            en.Dispose();
                        }
                        catch (Exception x)
                        {
                            error = x;
                        }
                    }
                    if (error == null)
                    {
                        tcs.SetResult(null);
                    }
                    else
                    {
                        var aggx = error as AggregateException;
                        if (aggx == null)
                        {
                            tcs.SetException(error);
                        }
                        else
                        {
                            tcs.SetException(aggx.InnerExceptions);
                        }
                    }
                }
            };

            iterate(null);

            return tcs.Task;
        }

        static async Task ShowQuotes(IEnumerable<string> symbols)
        {
            foreach (string symbol in symbols)
            {
                try
                {
                    Quote q = await GetQuote(symbol);
                    Console.WriteLine("{0} ('{1}'): {2}", q.Name, q.Symbol, q.LastTrade);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }


        static async Task<Quote> GetQuote(string id)
        {
            string url = "http://finance.yahoo.xcom/d/quotes.csv?s=" + id + "&f=snl1";
            string response = await new WebClient().DownloadStringTaskAsync(url);
            string[] parts = response.Split(',');
            return new Quote
            {
                Symbol = parts[0].Trim('\"'),
                Name = parts[1].Trim('\"'),
                LastTrade = double.Parse(parts[2])
            };
        }
    }

    class Quote
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double LastTrade { get; set; }
    }
}
