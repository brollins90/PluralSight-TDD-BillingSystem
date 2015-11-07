namespace BillingSystem.Tests
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    // https://app.pluralsight.com/library/courses/play-by-play-wilson-tdd/table-of-contents

    public class BillingDoohickeyTests
    {
        [Fact]
        public void CustomerWhoDoesNotHaveASubscriptionDoesNotGetCharged()
        {
            var customer = new Customer();
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {
            var customer = new Customer { Subscribed = true };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Once);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsCurrentDoesNotGetCharged()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2011, PaidThroughMonth = 8 };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsCurrentThroughNextYearDoesNotGetCharged()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2012, PaidThroughMonth = 1 };
            var processor = TestableBillingProcessor.Create(customer);

            processor.ProcessMonth(2011, 8);

            processor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }
        
        [Fact]
        public void CustomerWhoIsCurrentAndDueToPayAndFailsOnceIsStillSubscribed()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2012, PaidThroughMonth = 1 };
            var processor = TestableBillingProcessor.Create(customer);
            processor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>()))
                             .Returns(false);
                

            processor.ProcessMonth(2011, 8);

            Assert.True(customer.Subscribed);
        }
        // Monthly Billing
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

    public class Customer
    {
        public int PaidThroughMonth { get; internal set; }
        public int PaidThroughYear { get; internal set; }
        public bool Subscribed { get; internal set; }
    }

    public class BillingProcessor
    {
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

            if (customer.Subscribed && 
                customer.PaidThroughYear <= year &&
                customer.PaidThroughMonth < month)
            {
                _charger.ChargeCustomer(customer);
            }
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
}