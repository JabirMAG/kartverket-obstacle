namespace FirstWebApplication.Models
{
    // Representerer tilbakemelding fra en bruker. Brukes når noen fyller ut og sender inn et tilbakemeldingsskjema
    public class Advice
    {
        // Unik ID for tilbakemeldingen (primærnøkkel i database)
        public int adviceID { get; set; }

        // Meldingen/innholdet i tilbakemeldingen
        public string adviceMessage { get; set; }

        // E-postadresse til personen som sender inn tilbakemeldingen
        public string Email { get; set; }
    }
}
