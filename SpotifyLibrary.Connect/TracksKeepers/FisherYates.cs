using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpotifyLibrary.Helpers.Extensions;

namespace SpotifyLibrary.Connect.TracksKeepers
{
    public class FisherYates<T>
    {
        private readonly Random _random;
        private volatile int _currentSeed;
        private volatile int _sizeForSeed = -1;

        public FisherYates()
        {
            _random = new Random();
        }
        private static int[] GetShuffleExchanges(int size, int seed)
        {
            var exchanges = new int[size - 1];
            var random = new Random(seed);
            for (var i = size - 1; i > 0; i--)
            {
                var n = random.Next(i + 1); //j ← random integer such that 0 ≤ j ≤ i
                exchanges[size - 1 - 1] = n; //     exchange a[j] and a[i]
            }
            return exchanges;
        }

        public void Shuffle([NotNull] List<T> list, bool saveSeed) => Shuffle(list, 0, list.Count, saveSeed);

        public void Shuffle([NotNull] List<T> list, int from, int to, bool saveSeed)
        {
            var seed = _random.Next();
            if (saveSeed) _currentSeed = seed;

            var size = to - from; //distance
            if(saveSeed) _sizeForSeed = size;

            var exchanges = GetShuffleExchanges(size, seed);
            for (var i = size - 1; i > 0; i--)
            {
                //project the exchange on the list
                var n = exchanges[size - 1 - i];
                list.Swap(from + n, from + i);
            }
        }

        public void Unshuffle([NotNull] List<T> list) => Unshuffle(list, 0, list.Count);
        /// <summary>
        /// Reverse algorithm of <see cref="Shuffle(System.Collections.Generic.List{T},bool)"/>
        /// </summary>
        /// <param name="list"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void Unshuffle([NotNull] List<T> list, int from, int to)
        {
            if (_currentSeed == 0) throw new ArgumentException($"seed is 0", nameof(_currentSeed));
            if (_sizeForSeed != to - from) throw new ArgumentException($"Size is wrong. Check {nameof(CanUnshuffle)}");

            var size = to - from;
            var exchanges = GetShuffleExchanges(size, _currentSeed);
            for (var i = 1; i < size; i++)
            {
                //project the exchange on the list
                var n = exchanges[size - 1 - i];
                list.Swap(from + n, from + i);
            }
        }

        public bool CanUnshuffle(int size) => _currentSeed != 0 && _sizeForSeed == size;
    }
}
