using System;

namespace Harta.Services.File.API.Infrastructure.Exceptions
{
    public class FileDomainException : Exception
    {
        public FileDomainException()
        {
        }

        public FileDomainException(string message) : base(message)
        {
        }

        public FileDomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}