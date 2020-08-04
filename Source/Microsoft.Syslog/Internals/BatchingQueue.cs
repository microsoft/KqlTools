// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    ///   Implements a batching queue - concurrent no-lock enqueue-one; dequeue many with lock.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class BatchingQueue<T> : IObserver<T>
    {

        public int Count => _count;

        class Node
        {
            public T Item;
            public Node Next;
        }

        // We use Interlocked operations when accessing last-in (to push items);
        // we use lock when accessing _lastOut element (dequeuing multiple items)
        volatile Node _lastIn; //must be marked as volatile for interlocked ops
        Node _lastOut;
        object _dequeueLock = new object();
        volatile int _count;

        public BatchingQueue()
        {
            // There's always at least one empty node in linked list; so empty queue holds just one node.
            //  this is necessary to avoid problem with pushing first node (or popping the last one) - when you have
            // to modify both pointers to start and end of the list from null to this first element.  
            _lastIn = _lastOut = new Node();
        }

        public void Enqueue(T item)
        {
            // 1. Get node from pool or create new one
            var newNode = NodePoolTryPop() ?? new Node();
            newNode.Item = item;
            // 2. Quick path
            var oldLastIn = _lastIn;
            if (Interlocked.CompareExchange(ref _lastIn, newNode, oldLastIn) == oldLastIn)
                oldLastIn.Next = newNode;
            else
                EnqueueSlowPath(newNode);
            Interlocked.Increment(ref _count);
        }

        // Same as Enqueue but in a loop with spin
        private void EnqueueSlowPath(Node newNode)
        {
            var spinWait = new SpinWait();
            Node oldLastIn;
            do
            {
                spinWait.SpinOnce();
                oldLastIn = _lastIn;
            } while (Interlocked.CompareExchange(ref _lastIn, newNode, oldLastIn) != oldLastIn);
            oldLastIn.Next = newNode;
        }


        public IList<T> DequeueMany(int maxCount = int.MaxValue)
        {
            // iterate over list starting with _lastOut
            var list = new List<T>(maxCount);
            lock (_dequeueLock)
            {
                while (_lastOut.Next != null && list.Count < maxCount)
                {
                    // save the ref to the node to return it to the pool at the end
                    var savedLastOut = _lastOut;
                    // Advance _lastOut, copy item to result list.
                    _lastOut = _lastOut.Next;
                    list.Add(_lastOut.Item);
                    _lastOut.Item = default(T); //clear the ref to data
                    Interlocked.Decrement(ref _count);
                    NodePoolTryPush(savedLastOut); // return the node to the pool
                }
                return list;
            } //lock
        } //method

        #region Node pooling
        // We pool/reuse Node objects; we save nodes in a simple concurrent stack.
        // The stack is not 100% reliable - it might fail occasionally when pushing/popping up nodes
        volatile Node _nodePoolHead;

        private Node NodePoolTryPop()
        {
            var head = _nodePoolHead;
            if (head == null) // stack is empty
                return null;
            if (Interlocked.CompareExchange(ref _nodePoolHead, head.Next, head) == head)
            {
                head.Next = null;
                return head;
            }
            // Node pool is not reliable (push and pop),
            // Hypotethically this may result in pool growth over time: let's say all pushes succeed, but some rare pops fail.  
            // To prevent this from ever happening, we drop the entire pool if we ever fail to pop.
            // This is an EXTREMELY rare event, and if it happens - no impact, just extra objects for GC to collect
            _nodePoolHead = null; //drop the pool
            return null;
        }

        private void NodePoolTryPush(Node node)
        {
            node.Item = default(T);
            node.Next = _nodePoolHead;
            // we make just one attempt; if it fails, we don't care - node will be GC-d
            Interlocked.CompareExchange(ref _nodePoolHead, node, node.Next);
        }
        #endregion

        #region IObserver<T> implementation
        void IObserver<T>.OnCompleted()
        {
        }

        void IObserver<T>.OnError(Exception error)
        {
        }

        void IObserver<T>.OnNext(T value)
        {
            this.Enqueue(value);
        }
        #endregion

    } //class
}
