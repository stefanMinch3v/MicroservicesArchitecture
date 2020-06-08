namespace TaskTronic.Web.Models
{
    using System;
    using System.Collections.Generic;

    public class JwtModel
    {
        public string Token { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime Expiration { get; set; }
    }
}
