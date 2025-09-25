namespace FirstWebApplication.Models
{
    // ViewModel som brukes til � vise feilmeldinger i applikasjonen
    // Vises typisk n�r noe g�r galt i HomeController -> Error()
    public class ErrorViewModel
    {
        // ID som identifiserer den aktuelle foresp�rselen.
        // Kan brukes til feils�king og sporing i logger.
        public string? RequestId { get; set; }

        // Returnerer true hvis RequestId eksisterer (ikke er null eller tom)
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
