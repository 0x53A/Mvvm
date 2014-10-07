using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mvvm
{
    public class ReaderWriterLock
    {
        object _reader;
        object _writer;

        public static ReaderWriterLock CreateFromExisting<TReader, TWriter>(TReader reader, TWriter writer)
            where TReader : class
            where TWriter : class
        {
            return new ReaderWriterLock(reader, writer);
        }

        private ReaderWriterLock(object reader, object writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public ReaderWriterLock()
            : this(new object(), new object())
        {

        }

        public IDisposable Reader()
        {
            Monitor.Enter(_reader);
            return UniquePtr.Create(this, x => Monitor.Exit(x._reader));
        }

        public IDisposable Writer()
        {
            Monitor.Enter(_reader);
            Monitor.Enter(_writer);
            return UniquePtr.Create(this, x =>
            {
                Monitor.Exit(x._writer);
                Monitor.Exit(x._reader);
            });
        }
    }
}
