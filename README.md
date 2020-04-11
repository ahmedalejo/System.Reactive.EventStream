<img src="art/event-stream.png" alt="drawing" width="100"/>


System.Reactive.EventStream 
=========
[![Build status](https://ahmedalejo.visualstudio.com/System.Reactive.EventStream/_apis/build/status/System.Reactive.EventStream-CI)](https://ahmedalejo.visualstudio.com/System.Reactive.EventStream/_build/latest?definitionId=3)

Provides the implementation for a reactive extensions event stream, 
allowing trending and analysis queries to be performed in real-time 
over the events pushed through the stream. 

### What is an Event Stream?

An event stream represents a logical flow of events which when pushed into the stream can be seen down stream by observers.

### What can be done with an Event Stream?

```csharp
void Main()
{
    var stream = EventStream.DefaultStream;

    //ATM Machine 
    var withdraw = new CustomerWithdrew(
                            customerID: Guid.NewGuid(),
                            timestamp: DateTimeOffset.Now,
                            amount: 2000);
    Stream.Push(withdraw);

    //Potential Fraud detector
    var processingWindow = TimeSpan.FromDays(90);
    (
        from withdrawal in stream.OfType<CustomerWithdrew>()

        group withdrawal by withdrawal.CustomerID into withdrawals

        from window in withdrawals.Window(processingWindow)

        from period in window.Scan(seed: new WithdrawalPeriod(customerID: withdrawals.Key),
                                   accumulator: (period, withdrawal) => period.Add(withdrawal))

        where
        (
            //These are atually two scenários and could have been instrumented sepearately
            //And then combined
            (
                period.Frequency > 10
                &&
                period.TotalAmount < 2000 && period.Interval < TimeSpan.FromDays(30)
            )
            ||
            (
                period.Frequency < 50
                &&
                period.Last.Amount >= 10000 && period.Interval < TimeSpan.FromDays(10)
            )
         )

        select period
    )
    .Subscribe(SendReport);

    void SendReport(WithdrawalPeriod period)
    {
        /*REPORT back into the stream for others to handle, or to a service bus or via signalR*/
        stream.Push(new FraudDetected(period.CustomerID)); //we could include the period info as well
    }
}
```

```csharp
public class Withdrawal
{
    public Withdrawal(DateTimeOffset timestamp, decimal amount)
    {
        this.Timestamp = timestamp;
        this.Amount = amount;
    }
    public DateTimeOffset Timestamp { get; }
    public decimal Amount { get; }
}
```

Representation of a custormer withdrawal occurrence 

```csharp
public class CustomerWithdrew : Withdrawal
{
    public CustomerWithdrew(Guid customerID, DateTimeOffset timestamp, decimal amount)
        :base(timestamp, amount) =>
        this.CustomerID = customerID;
        
    public Guid CustomerID { get; }
}
```

Then we have an aggregate for keeping state

```csharp
public class WithdrawalPeriod
{
    public WithdrawalPeriod(Guid customerID) =>
        this.CustomerID = customerID;

    public Guid CustomerID { get; }
    public Decimal TotalAmount { get; private set; }
    public Withdrawal First { get; private set; }
    public Withdrawal Last { get; private set; }
    public long Count { get; private set; }
    public double Frequency => Count / Interval.TotalDays;
    public TimeSpan Interval => this.Last.Timestamp - this.First.Timestamp;

    public WithdrawalPeriod Add(Withdrawal withdrawal)
    {
        this.Count++;
        this.Last = withdrawal;

        this.TotalAmount += withdrawal.Amount;
        
        if(this.First is null)
            this.First = withdrawal;
            
        return this;
    }
}
```

The fraud detection event that can be used to trigger a detailed analysis somewhere else

```csharp
public class FraudDetected
{
    public FraudDetected(Guid customerID) =>
        this.CustomerID = customerID;

    public Guid CustomerID { get; }
}
```



<div>Icon made by <a href="https://www.flaticon.com/authors/becris" title="Becris">Becris</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>
