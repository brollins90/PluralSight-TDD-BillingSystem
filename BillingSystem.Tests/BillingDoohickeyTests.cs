﻿namespace BillingSystem.Tests
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class BillingDoohickeyTests
    {
        [Fact]
        public void CustomerWhoDoesNotHaveASubscriptionDoesNotGetCharged()
        {
            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();
            var customer = new Customer(); // what does not having a subscription mean?
            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);

            thing.ProcessMonth(2011, 8);

            charger.Verify(c => c.ChargeCustomer(customer), Times.Never);
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {

        }

        // Monthly Billing
        // Grace period for missed payments ("dunning" status)
        // not all customers are subscribers
        // idle customers should be automatically unsubscribed


    }

    public interface ICustomerRepository
    {

    }

    public interface ICreditCardCharger
    {
        void ChargeCustomer(Customer customer);
    }

    public class Customer
    {

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
        }
    }
}