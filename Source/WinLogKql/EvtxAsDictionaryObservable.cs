// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLogKql
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Reactive.Disposables;
    using Tx.Windows;
    using Microsoft.EvtxEventXmlScrubber;

    public class EvtxAsDictionaryObservable
    {
        public static IObservable<IDictionary<string, object>> FromFiles(params string[] logFiles)
        {
            var enumerable = EvtxEnumerable.FromFiles(logFiles);
            var observable = Observable.Create<EventLogRecord>(x =>
            {
                foreach (var item in enumerable)
                {
                    x.OnNext(item);
                }

                x.OnCompleted();

                return Disposable.Create(() => { });
            });

            return observable.Select(e => e.Deserialize());
        }

        public static IObservable<IDictionary<string, object>> FromWecXml(string wecXml)
        {
            var enumerable = EvtxEnumerable.FromWecXml(wecXml);
            var observable = Observable.Create<EventLogRecord>(x =>
            {
                foreach (var item in enumerable)
                {
                    x.OnNext(item);
                }

                return Disposable.Create(() => { });
            });

            return observable.Select(e => e.Deserialize());
        }

        public static IObservable<IDictionary<string, object>> FromLog(string log)
        {
            var enumerable = EvtxEnumerable.FromLogQuery(log, null, null);
            var observable = Observable.Create<EventLogRecord>(x =>
            {
                foreach (var item in enumerable)
                {
                    x.OnNext(item);
                }

                return Disposable.Create(() => { });
            });

            return observable.Select(e => e.Deserialize());
        }
    }
}
