using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Extensions
{
    public static class UniRxExtensions
    {
        public static IObservable<bool> IfTrue(this IObservable<bool> observable)
        {
            return observable.Where(value => value);
        }

        public static IObservable<bool> IfFalse(this IObservable<bool> observable)
        {
            return observable.Where(value => !value);
        }

        public static IObservable<T> IfNotNull<T>(this IObservable<T> obervable)
        {
            return obervable.Where(value => value != null);
        }

        public static IObservable<T> DoLog<T>(this IObservable<T> observable, string dumpText)
        {
            return observable.Do(v => Debug.LogFormat("{0}: {1}", dumpText, v));
        }
        
        public static IObservable<Unit> ObserveAnyChange<T>(this IReadOnlyReactiveCollection<T> reactiveCollection)
        {
            return Observable.Merge(
                reactiveCollection.ObserveReset().AsUnitObservable(),
                reactiveCollection.ObserveAdd().AsUnitObservable(),
                reactiveCollection.ObserveMove().AsUnitObservable(),
                reactiveCollection.ObserveRemove().AsUnitObservable(),
                reactiveCollection.ObserveReplace().AsUnitObservable());
        }
        
        public static IObservable<Unit> ObserveAnyChange<TKey, TValue>(this IReadOnlyReactiveDictionary<TKey, TValue> reactiveCollection)
        {
            return Observable.Merge(
                reactiveCollection.ObserveReset().AsUnitObservable(),
                reactiveCollection.ObserveAdd().AsUnitObservable(),
                reactiveCollection.ObserveRemove().AsUnitObservable(),
                reactiveCollection.ObserveReplace().AsUnitObservable());
        }
        
        public static IObservable<T> OncePerFrame<T>(this IObservable<T> observable)
        {
            return observable.BatchFrame().Select(batch => batch.Last());
        }
    }
}