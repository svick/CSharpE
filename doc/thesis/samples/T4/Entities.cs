using System;

class Person
    : IComparable<Person>
{
    public String Name { get; set; }
    public Int32 Age { get; set; }

    public int CompareTo(Person other)
    {
        int result;

        result = this.Name.CompareTo(
            other.Name);
        if (result != 0)
            return result;

        result = this.Age.CompareTo(
            other.Age);
        if (result != 0)
            return result;

        return 0;
    }
}
