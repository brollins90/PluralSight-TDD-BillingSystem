using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// https://app.pluralsight.com/library/courses/play-by-play-wilson-tdd/table-of-contents

public class BillingProcessorTests
{
    public class NoSubscription
    {
        [Fact]
        public void CustomerWhoDoesNotHaveASubscriptionDoesNotGetCharged()
        {
            var customer = new Customer();
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }
    }

    public class Monthly
    {
        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {
            var subscription = new MonthlySubscription();
            var customer = new Customer { Subscription = subscription };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Once);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsCurrentDoesNotGetCharged()
        {
            var subscription = new MonthlySubscription { PaidThroughYear = 2011, PaidThroughMonth = 8 };
            var customer = new Customer { Subscription = subscription };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsCurrentThroughNextYearDoesNotGetCharged()
        {
            var subscription = new MonthlySubscription { PaidThroughYear = 2012, PaidThroughMonth = 8 };
            var customer = new Customer { Subscription = subscription };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWhoIsCurrentAndDueToPayAndFailsOnceIsStillCurrent()
        {
            var subscription = new MonthlySubscription();
            var customer = new Customer { Subscription = subscription };
            var processor = TestableBillingProcessor.Create(customer);
            processor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>()))
                             .Returns(false);


            processor.ProcessMonth(2011, 8);

            Assert.True(customer.Subscription.IsCurrent);
        }

        [Fact]
        public void CustomerWhoIsCurrentAndDueToPayAndFailsMaximumTimesIsNoLongerSubscribed()
        {
            var subscription = new MonthlySubscription();
            var customer = new Customer { Subscription = subscription };
            var processor = TestableBillingProcessor.Create(customer);
            processor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>()))
                             .Returns(false);

            for (int i = 0; i < BillingProcessor.MAX_FAILURES; i++)
            {
                processor.ProcessMonth(2011, 8);
            }

            Assert.False(customer.Subscription.IsCurrent);
        }
    }

    public class Annual
    {

    }



    //[Fact]
    //public void SuccessfulChargeOfSubscribed

    // What are kinds of subscriptions? (yearly , monthly)
    // monthly recurs, yearly does not

    // Grace period for missed payments ("dunning" status)
    // not all customers are subscribers
    // idle customers should be automatically unsubscribed
    // what about customers that signed up today?
}

public interface ICustomerRepository
{
    IEnumerable<Customer> Customers { get; }
}

public interface ICreditCardCharger
{
    bool ChargeCustomer(Customer customer);
}

public abstract class Subscription
{
    public abstract bool IsCurrent { get; }
    public abstract bool IsRecurring { get; }

    public abstract bool NeedsBilling(int year, int month);

    public virtual void RecordChargedResult(bool charged)
    {
    }
}

public class AnnualSubscription : Subscription
{
    public override bool IsCurrent
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public override bool IsRecurring { get { return false; } }

    public override bool NeedsBilling(int year, int month)
    {
        throw new NotImplementedException();
    }
}

public class MonthlySubscription : Subscription
{
    private int failureCount;

    public override bool IsCurrent
    {
        get
        {
            return true;
        }
    }

    public override bool IsRecurring { get { return true; } }

    public int PaidThroughMonth { get; set; }
    public int PaidThroughYear { get; set; }

    public override bool NeedsBilling(int year, int month)
    {
        return PaidThroughYear <= year &&
               PaidThroughMonth < month;
    }

    public override void RecordChargedResult(bool charged)
    {
        if (!charged)
        {
            failureCount++;
        }
    }
}

public class Customer
{
    public Subscription Subscription { get; set; }
}

public class BillingProcessor
{
    public const int MAX_FAILURES = 3;

    private ICreditCardCharger _charger;
    private ICustomerRepository _repo;

    public BillingProcessor(ICustomerRepository repo, ICreditCardCharger charger)
    {
        _repo = repo;
        _charger = charger;
    }

    public void ProcessMonth(int year, int month)
    {

        var customer = _repo.Customers.Single();

        if (NeedsBilling(year, month, customer))
        {
            bool charged = _charger.ChargeCustomer(customer);
            customer.Subscription.RecordChargedResult(charged);

            //if (!charged)
            //{
            //    if (++customer.PaymentFailures >= MAX_FAILURES)
            //    {
            //        customer.Subscribed = false;
            //    }
            //}
        }
    }

    private static bool NeedsBilling(int year, int month, Customer customer)
    {
        return customer.Subscription != null
            && customer.Subscription.NeedsBilling(year, month);
    }
}

public class TestableBillingProcessor : BillingProcessor
{
    public Mock<ICreditCardCharger> Charger;
    public Mock<ICustomerRepository> Repository;

    private TestableBillingProcessor(Mock<ICustomerRepository> repository,
                                    Mock<ICreditCardCharger> charger)
        : base(repository.Object, charger.Object)
    {
        Charger = charger;
        Repository = repository;
    }

    public static TestableBillingProcessor Create(params Customer[] customers)
    {
        var repository = new Mock<ICustomerRepository>();
        repository.Setup(r => r.Customers)
            .Returns(customers);

        return new TestableBillingProcessor(
            repository,
            new Mock<ICreditCardCharger>());
    }
}
