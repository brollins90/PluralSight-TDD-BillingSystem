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
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();
            var customer = new Customer();
            repo.Setup(r => r.Customers)
                .Returns(new Customer[] { customer });
            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();
            var customer = new Customer { Subscribed = true };
            repo.Setup(r => r.Customers)
                .Returns(new Customer[] { customer });
            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(c => c.ChargeCustomer(customer), Times.Once);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsCurrentDoesNotGetCharged()
        {

        }

        // Monthly Billing
        // Grace period for missed payments ("dunning" status)
        // not all customers are subscribers
        // idle customers should be automatically unsubscribed


    }

    public interface ICustomerRepository
    {
        IEnumerable<Customer> Customers { get; }
    }

    public interface ICreditCardCharger
    {
        void ChargeCustomer(Customer customer);
    }

    public class Customer
    {
        public bool Subscribed { get; internal set; }
    }

    public class BillingDoohickey
    {
        private ICreditCardCharger _charger;
        private ICustomerRepository _repo;

        public BillingDoohickey(ICustomerRepository repo, ICreditCardCharger charger)
        {
            _repo = repo;
            _charger = charger;
        }

        public void ProcessMonth(int year, int month)
        {

            var customer = _repo.Customers.Single();

            if (customer.Subscribed)
            {
                _charger.ChargeCustomer(customer);
            }
        }
    }
}