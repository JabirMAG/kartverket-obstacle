namespace FirstWebApplication.Models.ViewModel
{
    /// <summary>
    /// ViewModel for user information (legacy, may not be in use)
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string EmailAdress { get; set; }
    }
}
