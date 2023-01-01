using System;

namespace CurrencyConverter.exceptions
{
    class WrongInputException : Exception
    {
        public WrongInputException() { }

        public WrongInputException(string message) : base(message) { }

        public WrongInputException(string message, Exception exception) : base(message, exception) { }

    }
}
