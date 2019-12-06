using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;

namespace System.Reactive.EventStream
{
    public static class INotifyPropertyChangedExtensions
    {
        public static IObservable<PropertyChangedEventArgs> ObservePropertyChangedEvents<T>(this T notifier)
            where T : INotifyPropertyChanged =>

            Observable.FromEventPattern<PropertyChangedEventArgs>(
                target: notifier,
                eventName: nameof(INotifyPropertyChanged.PropertyChanged))
                .Select(e => e.EventArgs);

        public static IObservable<PropertyChangedEventArgs> ObservePropertyChangedEvents<T>(this T notifier, string property)
            where T : INotifyPropertyChanged =>

            notifier.ObservePropertyChangedEvents()
                .Where(e => e.PropertyName == property);

        public static IObservable<string> ObservePropertyChanges<T>(this T notifier)
            where T : INotifyPropertyChanged =>

            notifier.ObservePropertyChangedEvents()
                .Select(e => e.PropertyName);


        /// <summary>
        /// Observes property changes on a <see cref="INotifyPropertyChanged"/> <see cref="object"/>
        /// when the value of the property specified by <paramref name="property"/> is changes
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="INotifyPropertyChanged"/></typeparam>
        /// <param name="notifier">The <see cref="object"/> implementing the <see cref="INotifyPropertyChanged"/></param>
        public static IObservable<Unit> ObservePropertyChanges<T>(this T notifier, string property)
            where T : INotifyPropertyChanged =>

            notifier.ObservePropertyChangedEvents(property: property)
                .Select(x => Unit.Default);


        /// <summary>
        /// Observes property changes on a <see cref="INotifyPropertyChanged"/> <see cref="object"/>
        /// returning the value of the property specified by <paramref name="property"/>
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="INotifyPropertyChanged"/></typeparam>
        /// <typeparam name="TProperty">Type of the property to monitor</typeparam>
        /// <param name="notifier">The <see cref="object"/> implementing the <see cref="INotifyPropertyChanged"/></param>
        /// <param name="property">The property selector <see cref=" Expression{Func{T,TProperty}}"/></param>
        public static IObservable<Unit> ObservePropertyChanges<T, TProperty>(
            this T notifier,
            Expression<Func<T, TProperty>> property)
            where T : INotifyPropertyChanged
        {
            if (!(property.Body is MemberExpression body))
                throw new ArgumentException("'expression' should be a member expression");

            var propertyName = body.Member.Name;

            return notifier.ObservePropertyChangedEvents(property: propertyName)
                .Select(x => Unit.Default);
        }

        /// <summary>
        /// Observes property changes on a <see cref="INotifyPropertyChanged"/> <see cref="object"/>
        /// returning the (name and value) of the property as <see cref="Tuple{string, object}"/>
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="INotifyPropertyChanged"/></typeparam>
        /// <param name="notifier">The <see cref="object"/> implementing the <see cref="INotifyPropertyChanged"/></param>
        public static IObservable<(string Name, object Value)> ObservePropertyChangedValues<T>(
            this T notifier)
            where T : INotifyPropertyChanged
        {
            var propertyGetter = (Func<string, PropertyInfo>)notifier.GetType().GetProperty;

            return notifier.ObservePropertyChanges()
                .Select(name => (name, propertyGetter(name)?.GetValue(notifier)));
        }

        /// <summary>
        /// Observes property changes on a <see cref="INotifyPropertyChanged"/> <see cref="object"/>
        /// returning the value of the property specified by <paramref name="property"/>
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="INotifyPropertyChanged"/></typeparam>
        /// <typeparam name="TProperty">Type of the property to monitor</typeparam>
        /// <param name="notifier">The <see cref="object"/> implementing the <see cref="INotifyPropertyChanged"/></param>
        /// <param name="property">The property selector <see cref=" Expression{TDelegate}"/></param>
        public static IObservable<TProperty> ObservePropertyChangedValues<T, TProperty>(
            this T notifier,
            Expression<Func<T, TProperty>> property)
            where T : INotifyPropertyChanged
        {
            var body = property.Body as MemberExpression;

            if (body is null)
                throw new ArgumentException("'expression' should be a member expression");

            var propertyName = body.Member.Name;

            var invoker = (Func<T, TProperty>)property.Compile().Invoke;

            return notifier.ObservePropertyChangedEvents(property: propertyName)
                .Select(x => invoker(notifier));
        }
    }
}
