namespace Mentoring.ExpressionsIQueryable.Tests.ClassMapper.TestClasses
{
    class Foo
    {
        public string Name;

        public string LastName;

        public int Age { get; set; }

        public string Address { get { return "Foo Class Addres"; } }

        public string Month;

        public int DayOfWeek;
    }
}
