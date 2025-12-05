namespace FirstWebApplication.Repositories
{
    // Repository-grensesnitt for generering av tidsbaserte hilsener
    public interface IGreetingRepository
    {
        // Henter en hilsen basert på nåværende tid på dagen
        string GetTimeBasedGreeting();
        
        // Henter en hilsen basert på en spesifikk time på dagen
        string GetGreetingForHour(int hour);
    }
}
