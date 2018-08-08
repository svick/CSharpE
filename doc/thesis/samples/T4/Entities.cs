using System;

class Person
    : IEquatable<Person>
{
    public String Name { get; set; }
    public Int32 Age { get; set; }
}
