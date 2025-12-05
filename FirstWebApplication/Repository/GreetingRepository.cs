namespace FirstWebApplication.Repositories
{
    // Repository for generering av tidsbaserte hilsener
    public class GreetingRepository : IGreetingRepository
    {
        // Henter en hilsen basert på nåværende tid på dagen
        public string GetTimeBasedGreeting()
        {
            var hour = DateTime.Now.Hour;
            return GetGreetingForHour(hour);
        }

        // Henter en hilsen basert på en spesifikk time på dagen
        public string GetGreetingForHour(int hour)
        {
            if (hour < 12)
                return "God morgen!";
            else if (hour < 18)
                return "God ettermiddag!";
            else
                return "God kveld!";
        }
    }
}
