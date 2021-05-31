using System;
using Spotify.Lib.Models;

namespace Spotify.Lib
{
    public abstract class PacketsManager
    {
        private readonly Func<MercuryPacket, bool> _worker;

        internal PacketsManager(string name)
        {
            _worker = packet =>
            {
                try
                {
                    Handle(packet);
                }
                catch (Exception ex)
                {
                    Exception(ex);
                }

                return true;
            };
        }

        public void Dispose()
        {
            Dispose(true);
        }


        public void Dispatch(MercuryPacket packet)
        {
            AppendToQueue(packet);
        }

        protected virtual void AppendToQueue(MercuryPacket packet)
        {
            _worker.Invoke(packet);
        }

        protected abstract void Handle(MercuryPacket packet);

        protected abstract void Exception(Exception ex);

        public virtual void Dispose(bool dispose)
        {
        }
    }
}