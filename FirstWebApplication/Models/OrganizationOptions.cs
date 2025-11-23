namespace FirstWebApplication.Models
{
    /// <summary>
    /// Defines valid organization options for user registration and management. Provides a centralized list of allowed organizations to ensure consistency across the application
    /// </summary>
    public static class OrganizationOptions
    {
        public const string Kartverket = "Kartverket";
        public const string Politi = "Politi";
        public const string Ambulanse = "Ambulanse";

        public static readonly string[] All =
        {
            Kartverket,
            Politi,
            Ambulanse
        };
    }
}

