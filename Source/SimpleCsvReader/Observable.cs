using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimpleCsvReader
{
    /// <summary>
    ///     Base class for components implementing the <see cref="IObservable{T}" /> interface.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <remarks>Provides sync and async broadcast mode.</remarks>
    public class Observable<T> : IObservable<T>
    {
        /// <summary>Constructs a new instance. </summary>
        public Observable()
        {
        }

        // We use copy, add/remove, replace method when adding/removing subscriptions
        // As a benefit, when we call OnNext on each subscriber, we iterate the list without locks
        private IList<Subscription> _subscriptions = new List<Subscription>();
        private readonly object _lock = new object();

        #region IObservable implementation

        /// <summary>Subscribes an observer. </summary>
        /// <param name="observer">An observer instance.</param>
        /// <returns>A disposable subscription object. Disposing the object cancels the subscription.</returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return AddSubscription(observer);
        }

        #endregion

        /// <summary>Unsubscribes an observer. </summary>
        /// <param name="observer">An observer instance.</param>
        public void Unsubscribe(IObserver<T> observer)
        {
            RemoveSubscription(observer);
        }

        /// <summary>Broadcasts an data. Calls the OnNext method of all subscribed observers. </summary>
        /// <param name="item">An data to broadcast.</param>
        /// <remarks>The subscribers are notified according to the broadcast mode - in sync or async manner.</remarks>
        public void Broadcast(T item)
        {
            ForEachSubscription(obs => obs.OnNext(item));
        }

        /// <summary>Broadcasts an error. Calls OnError method of all subscribers passing the exception as a parameter. </summary>
        /// <param name="error">The exception to broadcast.</param>
        protected void BroadcastError(Exception error)
        {
            ForEachSubscription(obs => obs.OnError(error));
        }


        /// <summary>Broadcasts OnComplete </summary>
        protected void BroadcastOnCompleted()
        {
            ForEachSubscription(obs => obs.OnCompleted());
        }

        // Subscription list operations
        // We use copy, add/remove, replace method when adding/removing subscriptions
        // As a benefit, when we call OnNext on each subscriber, we iterate the list without locks
        private Subscription AddSubscription(IObserver<T> observer)
        {
            lock (_lock)
            {
                var newList = new List<Subscription>(_subscriptions);
                var subscr = new Subscription() { Observable = this, Observer = observer };
                newList.Add(subscr);
                Interlocked.Exchange(ref _subscriptions, newList);
                return subscr;
            }
        }

        private void RemoveSubscription(IObserver<T> observer)
        {
            lock (_lock)
            {
                var newList = new List<Subscription>(_subscriptions);
                var subscr = newList.FirstOrDefault(s => s.Observer == observer);
                if (subscr != null)
                {
                    newList.Remove(subscr);
                    Interlocked.Exchange(ref _subscriptions, newList);
                }
            }
        }

        private void ForEachSubscription(Action<IObserver<T>> action)
        {
            var sList = _subscriptions;
            for (int i = 0; i < sList.Count; i++)
            {
                action(sList[i].Observer);
            }
        }

        /// <summary>Represents a subscription for an observable source. </summary>
        internal class Subscription : IDisposable
        {
            public Observable<T> Observable;
            public IObserver<T> Observer;

            public void Dispose()
            {
                Observable.Unsubscribe(Observer);
            }
        }

    } //class
}
