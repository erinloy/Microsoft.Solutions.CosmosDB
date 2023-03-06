// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.      

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Solutions.CosmosDB.Mongo;
using Microsoft.Solutions.CosmosDB.SQL;
using System.Collections.Generic;

namespace Microsoft.Solutions.CosmosDB.Test
{
    [TestClass]
    public class CosmosDBLibTest
    {
        public static IRepository<Person, string> repository = null;

        private static string cosmossqlConn = "{PUT YOUR COSMOS SQL Core API CONNECTION STRING}";
        private static string cosmosmongoConn = "{PUT YOUR COSMOS MONGO API CONNECTION STRING}";

        private static dynamic Repository;

        [TestInitialize]
        public void TestInit()
        {
            //Mongo Test
            //Repository = new MongoRepository(cosmosmongoConn, "MongoTEST");

            

            //SQL SDK Test
            Repository = new CosmosSQLRepository(cosmossqlConn, "SDKTEST");

        }

        [TestMethod]
        public async Task TEST01_AddAsync()
        {
            await repository.AddAsync(new Person() { name = "person1", age = 20, gender = Person.Gender.Female, title = "employee" });
            await repository.AddAsync(new Person() { name = "person2", age = 20, gender = Person.Gender.Female, title = "employee" });
            await repository.AddAsync(new Person() { name = "person3", age = 20, gender = Person.Gender.Female, title = "employee" });
            await repository.AddAsync(new Person() { name = "person4", age = 20, gender = Person.Gender.Female, title = "employee" });
        }

        [TestMethod]
        public async Task TEST02_SaveAsync()
        {
            var person1 = await repository.FindAsync(new GenericSpecification<Person>(x => x.name == "person1"));
            person1.age = 30;

            var person2 = await repository.FindAsync(new GenericSpecification<Person>(x => x.name == "person2"));
            person2.age = 32;

            var result_person1 = await repository.SaveAsync(person1);
            var result_person2 = await repository.SaveAsync(person2);

            Assert.AreEqual(result_person1.age, 30);
            Assert.AreEqual(result_person2.age, 32);

        }

        [TestMethod]
        public async Task TEST03_Find()
        {
            var people = await repository.FindAllAsync(new GenericSpecification<Person>(x => x.name.StartsWith("person")));
            Assert.IsTrue(people.Count() > 0);
        }

        [TestMethod]
        public async Task TEST04_Get()
        {
            var person = await repository.FindAsync(new GenericSpecification<Person>(x => x.name == "person1"));
            var retperson = await repository.GetAsync(person.id);

            Assert.AreEqual(person.name, retperson.name);
        }

        [TestMethod]
        public async Task TEST05_FindAllAsync()
        {
            var results = await repository.FindAllAsync(new GenericSpecification<Person>(x => x.age > 10));

            Console.WriteLine($"{results.Count()} records has been found");
            foreach (var item in results)
            {
                System.Console.WriteLine($"{item.name} - {item.gender} - {item.title} - {item.age}");
            }

            Console.WriteLine($"Order by name ASC");
            results = await repository.FindAllAsync(new GenericSpecification<Person>(x => x.age > 10, x => x.name, Order.Asc));
            foreach (var item in results)
            {
                System.Console.WriteLine($"{item.name} - {item.gender} - {item.title} - {item.age}");
            }

            Console.WriteLine($"Order by name DESC");
            results = await repository.FindAllAsync(new GenericSpecification<Person>(x => x.age > 10, x => x.name, Order.Desc));
            foreach (var item in results)
            {
                System.Console.WriteLine($"{item.name} - {item.gender} - {item.title} - {item.age}");
            }


            Assert.AreNotSame(0, results.Count());
        }

        [TestMethod]
        public async Task TEST06_GetAllAsync()
        {
            var results = await repository.GetAllAsync();
            Console.WriteLine($"{results.Count()} records has been recorded");

            Assert.AreNotEqual(results.Count(), 0);
        }

        [TestMethod]
        public async Task TEST07_FindAllAsync()
        {
            var _over30agedpeople = await repository.FindAllAsync(new GenericSpecification<Person>(x => x.age > 30));
            var results = await repository.GetAllAsync(_over30agedpeople.Select(x => x.id).ToArray());

            Console.WriteLine($"{results.Count()} objsts has been recorded");
            Assert.AreEqual(results.Count(), _over30agedpeople.Count());
        }

        [TestMethod]
        public async Task TEST08_DeleteAsync()
        {
            var _30agedperson = await repository.FindAsync(new GenericSpecification<Person>(x => x.age == 30));
            _30agedperson.title = "employeer";

            await repository.DeleteAsync(_30agedperson);

            Console.WriteLine($"{_30agedperson.id} object has been deleted");
        }

        [TestMethod]
        public async Task TEST09_DeleteAsync()
        {
            var _30agedperson = await repository.FindAsync(new GenericSpecification<Person>(x => x.age == 32));
            _30agedperson.title = "employeer";

            await repository.DeleteAsync(_30agedperson.id);

            Console.WriteLine($"{_30agedperson.id} object has been deleted");
        }
    }

    public class MongoRepository : MongoEntntyCollectionBase<Person>
    {
        public MongoRepository(string DataConnectionString, string CollectionName) : base(DataConnectionString, CollectionName)
        {
            CosmosDBLibTest.repository = this.EntityCollection;
        }
    }

    public class CosmosSQLRepository : SQLEntityCollectionBase<Person>
    {
        public CosmosSQLRepository(string DataConnectionString, string CollectionName) : base(new CosmosContext(DataConnectionString, CollectionName))
        {
            CosmosDBLibTest.repository = this.EntityCollection;
        }
    }

    public class Person : CosmosDBEntityBase
    {
        public string name { get; set; }
        public int age { get; set; }
        public string title { get; set; }
        public Gender gender { get; set; }
        public enum Gender { Male, Female, NoN }
    }
}
