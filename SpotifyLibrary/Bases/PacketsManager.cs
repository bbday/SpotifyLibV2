using System;
using SpotifyLibrary.Interfaces;
using SpotifyLibrary.Models;

namespace SpotifyLibrary.Bases
{
    public abstract class PacketsManager : IPacketsManager
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

        void IPacketsManager.AppendToQueue(MercuryPacket packet) => AppendToQueue(packet);

        void IPacketsManager.Handle(MercuryPacket packet) => Handle(packet);

        void IPacketsManager.Exception(Exception ex) => Exception(ex);
    }
}