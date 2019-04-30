using System;
using System.Threading.Tasks;
using CSharpE.Extensions.Actor;
using CSharpE.Extensions.Json;

class Program
{
    static async Task Main()
    {
        string json = "[ { name: 'Neal', books: [ { title: 'Anathem' } ] } ]";

        await new JsonProcessor().PrintAuthors(json);
    }
}

[Json("[ { name: 'Neal', books: [ { title: 'Seveneves' } ] } ]")]
class Authors { }

[Actor]
class JsonProcessor
{
    public void PrintAuthors(string json)
    {
        foreach (var author in Authors.Parse(json))
        {
            Console.WriteLine(author.name);

            foreach (var book in author.books)
                Console.WriteLine(book.title);
        }
    }
}