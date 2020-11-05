namespace Models
{
    public record Person
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }

    public record Teacher : Person { }
    public record Student : Person { }

}
