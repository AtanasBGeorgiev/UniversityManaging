using System;
using BCrypt.Net;

namespace API.Infrastructure;

public class HashPassword
{
    public static string Hash(string password)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 13);

        return hash;
    }
}
