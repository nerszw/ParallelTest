using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var ct = new CancellationToken();
            while (true)
            {
                Console.WriteLine("Введите слова разделённые запятой: ");
                var str = Console.ReadLine();

                var res = MostFrequentTriplet(str, ct);
                Console.WriteLine(res);
            }
        }

        static string MostFrequentTriplet(string str, CancellationToken ct)
        {
            if (str == null || ct == null)
                throw new ArgumentNullException();

            //Разделение на слова
            var words = str.Split(',').AsParallel().WithCancellation(ct)
                .Select(x => x.Trim()).Where(x => x != String.Empty);

            if (words.Count() == 0)
                throw new ArgumentException(nameof(str));

            //Подсчёт кол-ва повторений триплетов
            var counters = new ConcurrentDictionary<string, int>();
            var options = new ParallelOptions { CancellationToken = ct };

            Parallel.ForEach(words, options, word =>
            {
                Parallel.For(0, word.Length - 2, options, (j, s) =>
                {
                    var key = word.Substring(j, 3);
                    counters.AddOrUpdate(key, 1, (k, i) => i + 1);
                });
            });

            //Максимальное число совпадений
            var max = counters.AsParallel()
                .WithCancellation(ct).Max(x => x.Value);

            //Все триплеты с max числом вхождений
            var res = counters.AsParallel().WithCancellation(ct)
                .Where(x => x.Value == max).Select(x => x.Key);

            //Форматирование результата
            return String.Format("{0}\t{1}", String.Join(", ", res), max);
        }
    }
}
