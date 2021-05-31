namespace Spotify.Lib.Connect
{
    internal struct TransitionInfo

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
        internal int EndedWhen;

        internal TransitionInfo(
            TransitionReason endedReason,
            TransitionReason startedReason)
        {
            StartedReason = startedReason;
            EndedReason = endedReason;
            EndedWhen = -1;
        }

        /// <summary>
        /// Context changed.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="withSkip"></param>
        /// <returns></returns>
        internal static TransitionInfo ContextChange(bool withSkip,
            int position)
        {
            var trans = new TransitionInfo(TransitionReason.endplay,
                withSkip
                    ? TransitionReason.clickrow
                    : TransitionReason.playbtn);
            trans.EndedWhen = position;
            return trans;
        }

        ///// <summary>
        ///// Skipping to another track in the same context.
        ///// </summary>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //internal static TransitionInfo SkipTo(LocalStateWrapper state)
        //{
        //    var trans = new TransitionInfo(
        //        TransitionReason.endplay,
        //        TransitionReason.clickrow);
        //    if (state.GetPlayableItem != null) trans.EndedWhen = (int)state.GetPosition();
        //    return trans;
        //}

        ///// <summary>
        ///// Skipping to previous track.
        ///// </summary>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //internal static TransitionInfo SkippedPrev(LocalStateWrapper state)
        //{
        //    var trans = new TransitionInfo(TransitionReason.backbtn, TransitionReason.backbtn);
        //    if (state.GetPlayableItem != null) trans.EndedWhen = (int) state.GetPosition();
        //    return trans;
        //}

        ///// <summary>
        ///// Skipping to next track.
        ///// </summary>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //internal static TransitionInfo SkippedNext(LocalStateWrapper state)
        //{
        //    var trans = new TransitionInfo(TransitionReason.fwdbtn, TransitionReason.fwdbtn);
        //    if (state.GetPlayableItem != null) trans.EndedWhen = (int)state.GetPosition();
        //    return trans;
        //}
    }
}
