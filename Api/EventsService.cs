using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpotifyLibV2.Connect.Events;
using SpotifyLibV2.Mercury;

namespace SpotifyLibV2.Api
{
    public class EventsService : IEventsService, IDisposable
    {
        private readonly IMercuryClient _mercury;

        public EventsService(IMercuryClient session)
        {
            _mercury = session;
        }
        public void Language(string lang)
        {
            var @event = new EventBuilder(EventType.LANGUAGE);
            @event.Append(lang);
            SendEvent(@event);
        }

        public void SendEvent(EventBuilder builder)
        {
            try
            {
                var body = builder.ToArray();
                var req = new RawMercuryRequest("hm://event-service/v1/events", "POST");
                req._payload.Add(body);
                req.AddUserField("Accept-Language", "en");
                req.AddUserField("X-ClientTimeStamp", TimeProvider.CurrentTimeMillis().ToString());

                var resp = _mercury.SendSync(req);
                Debug.WriteLine(
                    $"Event sent. body: {EventBuilder.ToString(body)}, result: {resp.StatusCode.ToString()}");
            }
            catch (IOException ex)
            {
                Debug.WriteLine("Failed sending event: " + builder + ex.ToString());
            }
        }

        public virtual void Dispose(bool dispose)
        {

        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
