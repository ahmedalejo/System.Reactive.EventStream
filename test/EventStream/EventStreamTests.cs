using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;


namespace System.Reactive.EventStream
{
    [TestClass]
    public class EventStreamTests
    {
        private EventStream SystemUnderTest { get; set; }
        public IObserver<object> watcher { get; }
        public IObserver<Unit> unitWatcher { get; }

        public EventStreamTests()
        {
            this.SystemUnderTest = new EventStream
            (
                scheduler: ImmediateScheduler.Instance
              //scheduler: TaskPoolScheduler.Default
            );
            this.watcher = A.Fake<IObserver<object>>();
            this.unitWatcher = A.Fake<IObserver<Unit>>(); 
            this.GC = new CompositeDisposable();
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(500)]
        [DataRow(1000)]
        public void Should_emit_events_really_fast(int subscribers)
        {
            //Arrange
            bool running = true;

            var observable = this.SystemUnderTest
                .OfType<object>();
            Subscribe();

            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Take(1)
                .Subscribe(_ => running = false)
                .DisposeWith(this.GC);

            //Act
            var obj = new { };
            while (running)
                this.SystemUnderTest.Push(new { });

            void Subscribe()
            {
                for (int i = 0; i < subscribers; i++)
                {
                    var count = 0;
                    var index = i+1;
                    observable
                        .Subscribe(_ => count++)
                        .DisposeWith(this.GC,
                            callback: () => Console.WriteLine($"Subscriber: {index} Count: {count}"));
                }
            }
        }


        [TestMethod]
        async public Task Should_emits_events_also_for_readable_eventStream_usage()
        {
            //Arrange
            var eventPushedCs = new TaskCompletionSource<Exception>();
            var domainExceptionPushedCs = new TaskCompletionSource<Exception>();

            this.SystemUnderTest
                .OfType<Exception>()
                .Subscribe(exception => eventPushedCs.SetResult(exception))
                .DisposeWith(this.GC);

            this.SystemUnderTest
                .OfType<MyTestDomainException>()
                .Subscribe(exception => domainExceptionPushedCs.SetResult(exception))
                .DisposeWith(this.GC);
            //Act
            this.SystemUnderTest.Push(new MyTestDomainException());


            ////Avoid deadlocks
            Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Take(1)
                .Subscribe(_ =>
                {
                    eventPushedCs.SetResult(null);
                    domainExceptionPushedCs.SetResult(null);
                })
                .DisposeWith(this.GC);

            //Assert
            (await eventPushedCs.Task)
                .Should()
                .NotBeNull(because: "Pushing null events isn´t allowed nor are they published")
                .And
                .BeAssignableTo<Exception>(because: "An exception was raised");

            (await domainExceptionPushedCs.Task)
                .Should()
                .NotBeNull(because: "Pushing null events isn´t allowed nor are they published")
                .And
                .BeOfType<MyTestDomainException>(because: $"A  {typeof(MyTestDomainException)} was raised");
        }


        [TestMethod]
        public void Named_events_should_be_observable()
        {
            //Arrange
            var triggers = this.SystemUnderTest
                .OfName("ADE");

            triggers
                .Subscribe(this.unitWatcher)
                .DisposeWith(this.GC);

            //Act
            this.SystemUnderTest.Push("ADE");

            //Assert
            A.CallTo(() => unitWatcher.OnNext(A<Unit>._))
                .MustHaveHappened();
        }

        [TestMethod]
        public void Named_events_with_payloads_should_be_observable()
        {
            //Arrange
            var triggers = this.SystemUnderTest
                .OfName<Dummy>("ADE");

            triggers
                .Subscribe(this.watcher)
                .DisposeWith(this.GC);

            var payload = new Dummy();

            //Act
            this.SystemUnderTest.Push("ADE", payload);

            //Assert
            A.CallTo(() => watcher.OnNext(A<object>.That.IsSameAs(payload)))
                .MustHaveHappened();
        }

        [TestMethod]
        public void Named_events_with_payloads_should_NOT_be_observable_with_incorrect_type()
        {
            //Arrange
            var triggers = this.SystemUnderTest
                .OfName<EventStream>("ADE");

            triggers
                .Subscribe(this.watcher)
                .DisposeWith(this.GC);

            var payload = new Dummy();

            //Act
            this.SystemUnderTest.Push("ADE", payload);

            //Assert
            A.CallTo(() => watcher.OnNext(A<object>._))
                .MustNotHaveHappened();
        }

        [TestMethod]
        public void Should_emit_debug_events_when_events_are_pushed()
        {
            //Arrange
            this.SystemUnderTest.EmitsMetaEvents.Should().BeTrue();
            //Act
            this.SystemUnderTest.Push(int.MaxValue);

            //Assert
            this.Should_publish_event_when_they_are_pushed<int>(
                events: new[] { int.MinValue },
                numberOfSubscribers: 1,
                expectedEventTypes: new[] { typeof(RootSubscriptionOf<int>) }
                );
        }

        [TestMethod]
        public void Should_publish_valueType_events_when_they_are_pushed()
        {
            //Arrange
            //Act
            //Assert
            this.Should_publish_event_when_they_are_pushed<int>(
                events: new int[0],
                numberOfSubscribers: 0
                );

            this.Should_publish_event_when_they_are_pushed<int>(
                events: new[] { 1 },
                numberOfSubscribers: 0
                );

            this.Should_publish_event_when_they_are_pushed<int>(
                events: new[] { 1 },
                numberOfSubscribers: 1
                );

            this.Should_publish_event_when_they_are_pushed<int>(
                events: new[] { 1, 2 },
                numberOfSubscribers: 2
                );

            this.Should_publish_event_when_they_are_pushed<int>(
                events: Enumerable.Range(0, 10),
                numberOfSubscribers: 11
                );
        }

        class Dummy { };

        [TestMethod]
        public void Should_publish_referenceType_events_when_they_are_pushed()
        {
            //Arrange
            //Act
            //Assert
            this.Should_publish_event_when_they_are_pushed<Dummy>(
                events: new Dummy[0],
                numberOfSubscribers: 0
                );

            this.Should_publish_event_when_they_are_pushed<Dummy>(
                events: new[] { new Dummy() },
                numberOfSubscribers: 0
                );

            this.Should_publish_event_when_they_are_pushed<Dummy>(
                events: new[] { new Dummy() },
                numberOfSubscribers: 1
                );

            this.Should_publish_event_when_they_are_pushed<Dummy>(
                events: new[] { new Dummy(), new Dummy() },
                numberOfSubscribers: 2
                );

            this.Should_publish_event_when_they_are_pushed<Dummy>(
                events: new[] { new Dummy(), new Dummy() },
                numberOfSubscribers: 11
                );
        }

        private void Should_publish_event_when_they_are_pushed<TEvent>(
            IEnumerable events, int numberOfSubscribers, 
            IEnumerable<object> expectedEvents = null, 
            IEnumerable<Type> expectedEventTypes = null)
        {
            //Arrange
            var eventsCount = events.Cast<object>().Count();
            expectedEvents = expectedEvents ?? events.Cast<object>();
            var expectedEventsCount = expectedEvents.Cast<object>().Count();
            var expectedNumberOfPublcations = numberOfSubscribers * expectedEventsCount;

            var reasonMessage = $"{eventsCount} '{typeof(TEvent)}' event(s) were published with {numberOfSubscribers} subscriber(s)";

            var publishedEvents = new List<object>();
            void Subscriber(TEvent @event) => publishedEvents.Add(@event);

            if (numberOfSubscribers > 0)
                this.SystemUnderTest.OfType<IStreamEvent>()
                    .Subscribe(@event => publishedEvents.Add(@event))
                    .DisposeWith(this.GC);

            for (int i = 0; i < numberOfSubscribers; i++)
            {
                this.SystemUnderTest.OfType<TEvent>()
                    .Subscribe(Subscriber)
                    .DisposeWith(this.GC);
            }

            //Act
            foreach (var @event in events)
            {
                this.SystemUnderTest.Push(@event);
            }

            Task.Delay(50).Wait();
            //Assert
            var publishedEventsOfType = publishedEvents.OfType<TEvent>().ToArray();

            if (expectedEvents?.Any() != true || expectedNumberOfPublcations == 0)
            {
                publishedEventsOfType.Should().BeEmpty(because: reasonMessage);
            }
            else
            {
                expectedEvents
                    .Should()
                    .BeSubsetOf(publishedEvents, because: reasonMessage);

                publishedEventsOfType.Should().HaveCount(expectedNumberOfPublcations, because: reasonMessage);

                if (expectedEventTypes != null)
                {
                    publishedEvents.Select(@event => @event.GetType())
                        .Should()
                        .Contain(expectedEventTypes, because: reasonMessage);
                }

                publishedEventsOfType.Distinct()
                    .Should()
                    .HaveSameCount(events, because: "Only the type of events that were pushed should be published")
                    .And
                    .BeSubsetOf(events, because: "Only the type of events that were pushed should be published");
            }
        }

        public CompositeDisposable GC { get; }

        [TestCleanup]
        public void TestCleanUp() => this.GC.DisposeSafely();
        class MyTestDomainException : Exception { }
    }
}
