#nullable enable
using System;
using System.Collections.Generic;
using R3;

namespace Utilities.Messages
{
    public class GroupEvents<TSender> : IDisposable where TSender : notnull
    {
        private Dictionary<object, CompositeDisposable> _disposable = new();
        private Dictionary<Type, object> _events = new();

        public void Dispose()
        {
            _disposable.Values.ForEach(disposable => disposable.Dispose());
            _disposable.Clear();
            _events.Clear();
        }

        public void Add<TMessage>(TSender sender, Observable<TMessage> observable) where TMessage : notnull
        {
            if (!_disposable.ContainsKey(sender))
            {
                _disposable[sender] = new CompositeDisposable();
            }

            _disposable[sender].Add(observable.Select(message => (sender, message)).RelayTo(GetSubject<TMessage>()));
        }

        public void Add<TMessage>(object obj, Observable<(TSender, TMessage)> observable)
        {
            if (!_disposable.ContainsKey(obj))
            {
                _disposable[obj] = new CompositeDisposable();
            }

            _disposable[obj].Add(observable.RelayTo(GetSubject<TMessage>()));
        }

        public void Remove(object sender)
        {
            _disposable[sender].Dispose();
            _disposable.Remove(sender);
        }

        public MessageSubject<TSender, TMessage> GetSubject<TMessage>()
        {
            if (!_events.ContainsKey(typeof(TMessage)))
            {
                _events[typeof(TMessage)] = new MessageSubject<TSender, TMessage>();
            }

            return (MessageSubject<TSender, TMessage>)_events[typeof(TMessage)];
        }

        public Observable<(TSender, TMessage)> GetObservable<TMessage>()
        {
            if (!_events.ContainsKey(typeof(TMessage)))
            {
                _events[typeof(TMessage)] = new MessageSubject<TSender, TMessage>();
            }

            return ((MessageSubject<TSender, TMessage>)_events[typeof(TMessage)]).AsObservable();
        }
    }
}