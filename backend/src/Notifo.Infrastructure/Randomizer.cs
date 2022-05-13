// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Notifo.Infrastructure
{
    public class Randomizer
    {
        private readonly Random random = new Random();

        public virtual double NextDouble()
        {
            return random.NextDouble();
        }
    }
}
