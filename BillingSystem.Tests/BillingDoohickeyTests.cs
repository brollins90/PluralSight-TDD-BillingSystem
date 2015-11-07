namespace BillingSystem.Tests
{
    using Moq;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class BillingDoohickeyTests
    {
        [Fact]
        public void Monkey()
        {
            // source of customers

            // service of charging customers
            ICustomerRepository repo = new Mock<ICustomerRepository>();
            ICreditCardCharger charger = new Mock<ICreditCardCharger>();
            BillingDoohickey thing = new BillingDoohickey(repo, charger);

            thing.ProcessMonth(2011, 8);

        }
        // Monthly Billing
        // Grace period for missed payments ("dunning" status)
        // not all customers are subscribers
        // idle customers should be automatically unsubscribed


    }

    public class Customer
    {

    }
}