using CSharpE.Extensions.Record;

class Program
{
    static void Main()
    {
        var person = new Person("Adele", 21);
        var aged = person.WithAge(person.Age + 1);
    }
}

[Record]
class Person
{
    string Name;
    int Age;
}