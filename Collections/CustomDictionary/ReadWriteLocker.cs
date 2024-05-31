namespace Collections.CustomDictionary
{
    public class ReadWriteLocker : IDisposable
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public struct WriteLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _locker;
            public WriteLock(ReaderWriterLockSlim locker)
            {
                _locker = locker;
                locker.EnterWriteLock();
            }
            public void Dispose()
            {
                _locker.ExitWriteLock();
            }
        }

        public struct ReadLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _locker;
            public ReadLock(ReaderWriterLockSlim locker)
            {
                _locker = locker;
                locker.EnterReadLock();
            }
            public void Dispose()
            {
                _locker.ExitReadLock();
            }
        }

        public ReadLock Read() => new ReadLock(_locker);
        public WriteLock Write() => new WriteLock(_locker);

        public void Dispose()
        {
            _locker.ExitReadLock();
        }
    }
}
