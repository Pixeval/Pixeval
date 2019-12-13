namespace Pixeval.Core
{
    public sealed class PixivClient
    {
        private static volatile PixivClient _instance;

        private static readonly object Locker = new object();

        public static PixivClient Instance
        {
            get
            {
                if (_instance == null)
                    lock (Locker)
                    {
                        if (_instance == null)
                            _instance = new PixivClient();
                    }

                return _instance;
            }
        }
    }
}