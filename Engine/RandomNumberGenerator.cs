using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    class RandomNumberGenerator
    {
        private static Random _genertor = new Random();

        public static int NumberBetween(int minimumValue, int maximumValue)
        {
            return _genertor.Next(minimumValue, maximumValue);
        }
    }
}
