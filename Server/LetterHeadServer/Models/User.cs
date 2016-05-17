using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LetterHeadServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Username { get; set; }
        public string DeviceGUID { get; set; }
        public DateTime SignupDate { get; set; }
        public string SessionId { get; set; }

        public void GenerateNewSessionId()
        {
            SessionId = Guid.NewGuid().ToString();
        }
    }
}