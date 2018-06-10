using System;

namespace LightRAT.Core.Data
{
    [Serializable]
    public class Account
    {
        public Account()
        {
        }

        public Account(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}