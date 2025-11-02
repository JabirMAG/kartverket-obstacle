namespace FirstWebApplication.Models
{
    // Advice representerer en tilbakemelding fra en bruker
    // Brukes når noen fyller ut og sender inn et skjema.
    public class Advice
    {
        // Unik ID for tilbakemeldingen (primærnøkkel i databasen).
        public int adviceID { get; set; }

        // Selve meldingen / innholdet i tilbakemeldingen
        public string adviceMessage { get; set; }

        // E-postadressen til personen som sender inn tilbakemeldingen.
        public string Email { get; set; }
    }
}