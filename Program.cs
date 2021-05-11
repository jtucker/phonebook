using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using phonebook.Models;
using System;
using BenchmarkDotNet.Running;

namespace phonebook
{
    static class Program
    {
        static void Main(string[] args)
        {
            var phonebookFilename = SetupDataFile();

            var addCommand = new Command("add")
            {
                Handler = CommandHandler.Create<string, string, string>(AddAsync)
            };
            addCommand.AddArgument(new Argument<string>("name"));
            addCommand.AddArgument(new Argument<string>("phone"));
            addCommand.AddArgument(new Argument<string>("phonebookFilename", () => phonebookFilename)
            {
                IsHidden = true
            });

            var listCommand = new Command("list")
            {
                Handler = CommandHandler.Create<string, int, int>(ListAsync)
            };
            listCommand.AddArgument(new Argument<int>("skip"));
            listCommand.AddArgument(new Argument<int>("limit"));
            listCommand.AddArgument(new Argument<string>("phonebookFilename", () => phonebookFilename)
            {
                IsHidden = true
            });

            var benchmarkCommand = new Command("benchmark")
            {
                Handler = CommandHandler.Create(() => BenchmarkRunner.Run<PhonebookBenchies>())
            };

            var rootCommand = new RootCommand()
            {
                addCommand,
                listCommand,
                benchmarkCommand
            };
            rootCommand.InvokeAsync(args).Wait();
        }

        public static string SetupDataFile()
        {
            var phonebookDataPath = Path.Join(Directory.GetCurrentDirectory(), "data");
            var phonebookFilename = Path.Join(phonebookDataPath, "phonebook.bin");

            if (!Directory.Exists(phonebookDataPath))
            {
                Directory.CreateDirectory(phonebookDataPath);
            }

            return phonebookFilename;
        }

        public static async Task AddAsync(string phonebookFilename,
            string name,
            string phone,
            Action<PhonebookItem[], int, int> sortingAlgorithm)
        {
            var phoneEntries = GetPhonebookEntriesAsync(phonebookFilename);
            var pbook = await phoneEntries.ToArrayAsync().ConfigureAwait(false);

            // add record
            var tempPbook = new PhonebookItem[pbook.Length + 1];
            pbook.CopyTo(tempPbook, 0);
            tempPbook[^1] = new PhonebookItem(name, phone);
            sortingAlgorithm(tempPbook, 0, tempPbook.Length - 1);

            // save file
            using var fileStream = new FileStream(Path.Join(phonebookFilename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);
            using var stream = new StreamWriter(fileStream, Encoding.UTF8);
            foreach (var pbookItem in tempPbook)
            {
                await stream.WriteLineAsync(pbookItem.ToString()).ConfigureAwait(false);
            }
        }

        public static async Task ListAsync(string phonebookFilename, int skip, int limit)
        {
            var phoneEntries = GetPhonebookEntriesAsync(phonebookFilename);
            var pbook = await phoneEntries.ToListAsync().ConfigureAwait(false);

            foreach (var entry in pbook.Skip(skip).Take(limit))
            {
                Console.WriteLine($"Name:\t  {entry.Name}");
                Console.WriteLine($"Phone:\t {entry.PhoneNumber}");
            }
        }

        public static void Sort(PhonebookItem[] phonebookItems, int from, int to)
        {
            if (from == to) return;

            var midpoint = (from + to) / 2;
            Sort(phonebookItems, from, midpoint); // merge the first part
            Sort(phonebookItems, midpoint + 1, to); // merge the second part
            Merge(phonebookItems, from, midpoint, to); // combine
        }

        private static PhonebookItem[] Merge(PhonebookItem[] phonebookItems, int from, int midpoint, int to)
        {
            var total = to - from + 1;
            var tempArray = new PhonebookItem[total];
            var startFrom = from;
            var nextItem = midpoint + 1;
            var pos = 0;

            while (startFrom <= midpoint && nextItem <= to)
            {
                if (phonebookItems[startFrom].CompareTo(phonebookItems[nextItem]) == -1)
                {
                    tempArray[pos] = phonebookItems[startFrom];
                    startFrom++;
                }
                else
                {
                    tempArray[pos] = phonebookItems[nextItem];
                    nextItem++;
                }
                pos++;
            }

            // finish up the first part
            while (startFrom <= midpoint)
            {
                tempArray[pos] = phonebookItems[startFrom];
                startFrom++;
                pos++;
            }

            // finish up the second part
            while (nextItem <= to)
            {
                tempArray[pos] = phonebookItems[nextItem];
                nextItem++;
                pos++;
            }

            for (pos = 0; pos < total; pos++)
            {
                phonebookItems[from + pos] = tempArray[pos];
            }

            return phonebookItems;
        }

        private static async IAsyncEnumerable<PhonebookItem> GetPhonebookEntriesAsync(string phonebookFilename)
        {
            using var fileStream = new FileStream(Path.Join(phonebookFilename), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);
            using var stream = new StreamReader(fileStream, Encoding.UTF8);
            while (stream.Peek() > 0)
            {
                var entry = await stream.ReadLineAsync().ConfigureAwait(false);
                var pbookItem = entry.Split(";");
                yield return new PhonebookItem(pbookItem[0], pbookItem[1]);
            }
        }
    }
}
