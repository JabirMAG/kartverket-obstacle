namespace FirstWebApplication.Models
{
    // ViewModel som brukes til å vise feilmeldinger i applikasjonen
    // Vises typisk når noe går galt i HomeController -> Error()
    public class ErrorViewModel
    {
        // ID som identifiserer den aktuelle forespørselen.
        // Kan brukes til feilsøking og sporing i logger.
        public string? RequestId { get; set; }

        // Returnerer true hvis RequestId eksisterer (ikke er null eller tom)
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
