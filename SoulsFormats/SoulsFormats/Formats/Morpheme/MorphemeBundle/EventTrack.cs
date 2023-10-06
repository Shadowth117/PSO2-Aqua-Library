using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// A structure containing any number of events and timings for said events.
    /// </summary>
    public class EventTrack : MorphemeBundle_Base
    {
        /// <summary>
        /// Number of events in this EventTrack.
        /// </summary>
        public long numEvents;

        /// <summary>
        /// The name of this EventTrack.
        /// </summary>
        public string trackName;

        /// <summary>
        /// The id of the evenet.
        /// </summary>
        public long eventId;

        /// <summary>
        /// An offset to event data
        /// </summary>
        public long offset;

        /// <summary>
        /// A list of all events contained within this EventTrack.
        /// </summary>
        public List<Event> events = new List<Event>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EventTrack()
        {
            bundleType = eBundleType.Bundle_FileHeader;
        }

        /// <summary>
        /// Constructor to read in an event track with a BinaryReaderEx.
        /// </summary>
        /// <param name="br"></param>
        public EventTrack(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Calculates the size of the EventTrack.
        /// </summary>
        /// <returns>Size of the EventTrack</returns>
        public override long CalculateBundleSize()
        {
            long size = (isX64 ? 0x20 : 0x10) + 0xC * numEvents + trackName.Length + 1;

            long remainder = size % dataAlignment;

            if (remainder != 0)
            {
                long next_integer = (size - remainder) + 16; //Adjust so that the bundle end address will be aligned to 16 bytes
                size = next_integer;
            }

            return size;
        }

        /// <summary>
        /// Reads the event track.
        /// </summary>
        /// <param name="br">A BinaryReaderEx to read this structure from.</param>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            var dataStart = br.Position;
            numEvents = br.ReadVarint();
            trackName = br.GetASCII(br.Position + br.ReadVarint());
            eventId = br.ReadVarint();

            if (numEvents > 0)
            {
                br.Position = br.Position + br.ReadVarint();
                for (int i = 0; i < numEvents; i++)
                {
                    events.Add(new Event() { start = br.ReadSingle(), duration = br.ReadSingle(), value = br.ReadInt32() });
                }
            }

            br.Position = dataStart + (long)dataSize;
        }

        /// <summary>
        /// Writes the event track.
        /// </summary>
        /// <param name="bw">A BinaryWriterEx to write this structure from.</param>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            bw.WriteVarint(events.Count);
            bw.WriteVarint(0x20 + 0xC * events.Count);
            bw.WriteVarint(eventId);

            if (events.Count > 0)
            {
                bw.WriteVarint(0x20);
                foreach (var evt in events)
                {
                    bw.WriteSingle(evt.start);
                    bw.WriteSingle(evt.duration);
                    bw.WriteInt32(evt.value);
                }
            }
            bw.WriteASCII(trackName, true);
            bw.Pad((int)dataAlignment, 0xCD);
        }
    }
}
