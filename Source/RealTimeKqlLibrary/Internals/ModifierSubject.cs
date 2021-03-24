using System;
using System.Reactive.Subjects;

namespace RealTimeKqlLibrary
{
    /// <summary>
    ///     Instance class that is both an observable sequence as well as an observer
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <remarks>Allows data modification before broadcasting to next stage.</remarks>
    public class ModifierSubject<T> : Observable<T>, ISubject<T>
    {
        private readonly Func<T,T> _onNext;
        private readonly Func<Exception,Exception> _onError;
        private readonly Action _onCompleted;
        public ModifierSubject(Func<T,T> onNext=null, Func<Exception,Exception> onError=null, Action onCompleted=null)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }
        public void OnCompleted()
        {
            _onCompleted?.Invoke();
            BroadcastOnCompleted();
        }

        public void OnError(Exception error)
        {
            if(_onError != null)
            {
                BroadcastError(_onError(error));
            }
            else
            {
                BroadcastError(error);
            }
        }

        public void OnNext(T value)
        {
            if(_onNext != null)
            {
                Broadcast(_onNext(value));
            }
            else
            {
                Broadcast(value);
            }
        }
    }
}
