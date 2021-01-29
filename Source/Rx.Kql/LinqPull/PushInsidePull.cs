// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    /// <summary>
    ///     This is operator that executes push pipeline inside pull runtime
    /// </summary>
    /// <typeparam name="TIn">Input event type</typeparam>
    /// <typeparam name="TOut">Potpit event type</typeparam>
    internal class PushInsidePull<TIn, TOut> : IEnumerable<TOut>
    {
        private readonly IEnumerable<TIn> _input;
        private readonly Func<IObservable<TIn>, IObservable<TOut>> _pushPipe;

        public PushInsidePull(IEnumerable<TIn> input, Func<IObservable<TIn>, IObservable<TOut>> pushPipe)
        {
            _input = input;
            _pushPipe = pushPipe;
        }

        public IEnumerator<TOut> GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator(), _pushPipe);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, _input.GetEnumerator(), _pushPipe);
        }

        private class Enumerator : IEnumerator<TOut>
        {
            private readonly IEnumerator<TIn> _input;
            private PushInsidePull<TIn, TOut> _parent;
            private readonly IObservable<TOut> _pushOutput;
            private readonly PushResultHolder<TOut> _result = new PushResultHolder<TOut>();

            private readonly Subject<TIn> _subject = new Subject<TIn>();

            public Enumerator(PushInsidePull<TIn, TOut> parent, IEnumerator<TIn> input,
                Func<IObservable<TIn>, IObservable<TOut>> pushPipe)
            {
                _parent = parent;
                _input = input;
                _pushOutput = pushPipe(_subject); // this constructs the pipeline, but does not let it go yet
                _pushOutput.Subscribe(
                    _result); // this enables the flow... assumind someone pushed events into the _subject
            }

            public bool MoveNext()
            {
                while (true)
                {
                    if (!_input.MoveNext())
                    {
                        return false;
                    }

                    _result.HasValue = false;
                    _subject.OnNext(_input.Current);

                    if (_result.HasValue)
                    {
                        return true;
                    }
                }
            }

            public TOut Current => _result.Value;

            object IEnumerator.Current => _result.Value;

            public void Dispose()
            {
                ;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        private class PushResultHolder<T> : IObserver<T>
        {
            public bool HasValue;
            public T Value;

            public void OnNext(T value)
            {
                Value = value;
                HasValue = true;
            }

            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }
        }
    }
}