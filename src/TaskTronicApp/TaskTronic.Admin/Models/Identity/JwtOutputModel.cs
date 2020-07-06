namespace TaskTronic.Admin.Models.Identity
{
    public class JwtOutputModel
    {
        public JwtOutputModel(string token)
        {
            this.Token = token;
        }

        public string Token { get; }
    }
}
