using System;

class Person : IComparable<Person>
{
    public string Name { get; }
    public int Age { get; }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public int CompareTo(Person other)
    {
        if (other is null)
            return 1;

        int result = this.Name.CompareTo(other.Name);
        if (result != 0)
            return result;

        return this.Age.CompareTo(other.Age);
    }
}
