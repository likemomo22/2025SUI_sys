namespace UnityThreading
{
    public class SwitchTo
    {
        public enum TargetType
        {
            Main,
            Thread
        }

        /// <summary>
        ///     Changes the context of the following commands to the MainThread when yielded.
        /// </summary>
        public static readonly SwitchTo MainThread = new(TargetType.Main);

        /// <summary>
        ///     Changes the context of the following commands to the WorkerThread when yielded.
        /// </summary>
        public static readonly SwitchTo Thread = new(TargetType.Thread);

        private SwitchTo(TargetType target)
        {
            Target = target;
        }

        public TargetType Target { get; private set; }
    }
}