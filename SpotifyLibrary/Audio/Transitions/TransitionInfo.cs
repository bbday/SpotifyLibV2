using JetBrains.Annotations;
using SpotifyLibrary.Connect;
using SpotifyLibrary.Connect.Transitions;

namespace SpotifyLibrary.Audio.Transitions
{
    internal class TransitionInfo

    {
        /// <summary>
        /// How the next track started
        /// </summary>
        internal readonly TransitionReason StartedReason;

        /// <summary>
        /// How the previous track ended
        /// </summary>
        internal readonly TransitionReason EndedReason;

        /// <summary>
        /// When the previous track ended
        /// </summary>
        internal int EndedWhen = -1;

        internal TransitionInfo(
            TransitionReason endedReason,
            TransitionReason startedReason)
        {
            StartedReason = startedReason;
            EndedReason = endedReason;
        }

        /// <summary>
        /// Context changed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="withSkip"></param>
        /// <returns></returns>
        internal static TransitionInfo ContextChange(
            LocalStateWrapper state, bool withSkip)
        {
            var trans = new TransitionInfo(TransitionReason.endplay,
                withSkip
                    ? TransitionReason.clickrow
                    : TransitionReason.playbtn);
            if (state.GetPlayableItem != null) trans.EndedWhen = (int)state.GetPosition();
            return trans;
        }

        /// <summary>
        /// Skipping to another track in the same context.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static TransitionInfo SkipTo(LocalStateWrapper state)
        {
            var trans = new TransitionInfo(
                TransitionReason.endplay,
                TransitionReason.clickrow);
            if (state.GetPlayableItem != null) trans.EndedWhen = (int)state.GetPosition();
            return trans;
        }

        /// <summary>
        /// Skipping to previous track.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static TransitionInfo SkippedPrev(LocalStateWrapper state)
        {
            var trans = new TransitionInfo(TransitionReason.backbtn, TransitionReason.backbtn);
            if (state.GetPlayableItem != null) trans.EndedWhen = (int) state.GetPosition();
            return trans;
        }

        /// <summary>
        /// Skipping to next track.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static TransitionInfo SkippedNext(LocalStateWrapper state)
        {
            var trans = new TransitionInfo(TransitionReason.fwdbtn, TransitionReason.fwdbtn);
            if (state.GetPlayableItem != null) trans.EndedWhen = (int)state.GetPosition();
            return trans;
        }
    }
}
