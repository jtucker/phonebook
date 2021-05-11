using Bogus;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using phonebook.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace phonebook
{
    [ShortRunJob]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class PhonebookBenchies
    {
        private readonly Faker<PhonebookItem> faker;
        private readonly string pbookFilePath;

        public PhonebookBenchies()
        {
            faker = new Faker<PhonebookItem>()
                    .CustomInstantiator(f => new PhonebookItem(f.Person.FullName, f.Person.Phone));
            pbookFilePath = $"{Program.SetupDataFile()}.bench";
        }

        [GlobalSetup]
        public void Setup()
        {
            if (File.Exists(pbookFilePath))
            {
                File.Delete(pbookFilePath);
            }
        }

        [Params(1, 10)]
        public int EntriesToCreate;

        [Benchmark]
        public async Task AddPhoneRecords()
        {
            foreach (var item in faker.Generate(EntriesToCreate))
            {
                await Program.AddAsync(this.pbookFilePath, item.Name, item.PhoneNumber).ConfigureAwait(false);
            }
        }

        [Benchmark]
        public async Task AddPhoneRecordsLazy()
        {
            foreach (var item in GenerateItems(faker, EntriesToCreate))
            {
                await Program.AddAsync(this.pbookFilePath, item.Name, item.PhoneNumber).ConfigureAwait(false);
            }
        }

        private static IEnumerable<PhonebookItem> GenerateItems(Faker<PhonebookItem> f, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return f.Generate();
            }
        }
    }

}