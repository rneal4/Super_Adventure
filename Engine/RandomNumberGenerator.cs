using System;

namespace Engine
{
    public static class RandomNumberGenerator
    {
        private static Random _genertor = new Random();

        public static int NumberBetween(int minimumValue, int maximumValue)
        {
            return _genertor.Next(minimumValue, maximumValue);
        }
    }
}
