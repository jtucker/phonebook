using System;

namespace phonebook.Models
{
    public class PhonebookItem : IComparable<PhonebookItem>
    {
        public PhonebookItem(string name, string number)
        {
            Name = name;
            PhoneNumber = number;
        }

        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public int CompareTo(PhonebookItem other) =>
            StringComparer.InvariantCultureIgnoreCase.Compare(this.Name, other.Name);

        public override string ToString() =>
            $"{Name};{PhoneNumber}";
    }

}
