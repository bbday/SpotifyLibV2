﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SpotifyLibV2.Audio.Storage;

namespace SpotifyLibV2.Audio
{
    public class ChunkException : Exception
    {
        public ChunkException([NotNull] Exception cause) : base(cause.Message)
        {
        }

        protected ChunkException()
        {
        }

        public static ChunkException FromStreamError(short streamError)
        {
            return new ChunkException(new Exception("Failed due to stream error, code: " + streamError));
        }
    }
    public abstract class AbsChunkedInputStream :Stream, IHaltListener, IDisposable
    {
        private static readonly int PRELOAD_AHEAD = 5;
        private static readonly int PRELOAD_CHUNK_RETRIES = 2;
        private static readonly int MAX_CHUNK_TRIES = 128;
        public readonly ManualResetEvent waitLock = new ManualResetEvent(false);
        public int[] retries;
        private readonly bool retryOnChunkError;
        public volatile int waitForChunk = -1;
        private volatile ChunkException chunkException = null;
        public int pos = 0;
        public int mark = 0;
        public volatile bool closed = false;
        public int DecodedLength = 0;

        protected AbsChunkedInputStream(bool retryOnChunkError)
        {
            this.retryOnChunkError = retryOnChunkError;
        }
        protected abstract bool[] RequestedChunks();

        protected abstract bool[] AvailableChunks();

        protected abstract int Chunks();

        protected abstract Task RequestChunkFromStream(int index);

        protected abstract byte[][] Buffer();

        public virtual int Size
        {
            get;
            set;
        }

        private bool ShouldRetry(int chunk)
        {
            if (retries[chunk] < 1) return true;
            if (retries[chunk] > MAX_CHUNK_TRIES) return false;
            return !retryOnChunkError;
        }
        public async Task CheckAvailability(int chunk, bool wait, bool halted)
        {
            if (halted && !wait) throw new Exception("Illegal State");

            if (!RequestedChunks()[chunk])
            {
               await RequestChunkFromStream(chunk);
                RequestedChunks()[chunk] = true;
            }

            for (int i = chunk + 1; i <= Math.Min(Chunks() - 1, chunk + PRELOAD_AHEAD); i++)
            {
                if (!RequestedChunks()[i] && retries[i] < PRELOAD_CHUNK_RETRIES)
                {
                    await RequestChunkFromStream(i);
                    RequestedChunks()[i] = true;
                }
            }

            var retry = false;

            if (wait)
            {
                if (AvailableChunks()[chunk]) return;

                if (!halted) StreamReadHalted(chunk, StorageFeedHelper.CurrentTimeMillis());

                chunkException = null;
                waitForChunk = chunk;
                waitLock.WaitOne();

                if (closed) return;

                if (chunkException != null)
                {
                    if (ShouldRetry(chunk)) retry = true;
                    else throw chunkException;
                }

                if (!retry) StreamReadResumed(chunk, StorageFeedHelper.CurrentTimeMillis());
            }

            if (retry)
            {
                try
                {
                    Thread.Sleep((int)(Math.Log10(retries[chunk]) * 1000));
                }
                catch (Exception)
                {
                    // ignored
                }

                await CheckAvailability(chunk, true, true); // We must exit the synchronized block!
            }
        }
        public void NotifyChunkError(int index, [NotNull] ChunkException ex)
        {
            AvailableChunks()[index] = false;
            RequestedChunks()[index] = false;
            retries[index] += 1;
            if (index == waitForChunk && !closed)
            {
                chunkException = ex;
                waitForChunk = -1;
                waitLock.Set();
            }
        }

        public abstract void StreamReadHalted(int chunk, long time);

        public abstract void StreamReadResumed(int chunk, long time);

        public void Dispose()
        {
            waitLock?.Dispose();
            Dispose(true);
        }

        public virtual void Dispose(bool v)
        {

        }
    }
}
