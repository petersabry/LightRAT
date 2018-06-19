using System;
namespace LightRAT.Data
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
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var account = (Account) obj;

            if (account.Username == Username)
                if (account.Password == Password)
                    return true;

            return false;
        }

        public string Username { get; set; }
        public string Password { get; set; }

    }
}