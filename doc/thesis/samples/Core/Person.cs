using System;

class Person : IComparable<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }

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
