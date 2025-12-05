namespace FirstWebApplication.Models
{
    // ViewModel brukt til å vise feilmeldinger i applikasjonen. Vises typisk når noe går galt i HomeController -> Error()
    public class ErrorViewModel
    {
        // ID som identifiserer den nåværende forespørselen. Kan brukes for feilsøking og sporing i logger
        public string? RequestId { get; set; }

        // Returnerer true hvis RequestId eksisterer (ikke null eller tom)
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
