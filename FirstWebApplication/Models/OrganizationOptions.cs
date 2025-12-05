namespace FirstWebApplication.Models
{
    // Definerer gyldige organisasjonsalternativer for brukerregistrering og administrasjon. Gir en sentralisert liste over tillatte organisasjoner for å sikre konsistens i applikasjonen
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
