using System;
using System.Collections.Generic;
using System.Linq;
using PolicyService.Domain;
using Xunit;
using static Xunit.Assert;

namespace PolicyService.Test.Domain
{
    public class OfferTest
    {
        [Fact]
        public void CanCreateOfferBasedOnPrice()
        {
            var price = new Price(new Dictionary<string, decimal>()
            {
                ["C1"] = 100M,
                ["C2"] = 200M
            });

            var offer = Offer.ForPrice
            (
                "P1",
                DateTime.Now,
                DateTime.Now.AddDays(5),
                null,
                price
            );

            OfferAssert
                .AssertThat(offer)
                .ProductCodeIs("P1")
                .StatusIsNew()
                .PriceIs(300M);
        }

        [Fact]
        public void CanBuyNewNonExpiredOffer()
        {
            var offer = OfferFactory.NewOfferValidUntil(DateTime.Now.AddDays(5));

            var policy = offer.Buy(PolicyHolderFactory.Abc());

            OfferAssert
                .AssertThat(offer)
                .StatusIsConverted();

            PolicyAssert
                .AssertThat(policy)
                .StatusIsActive()
                .HasVersions(1)
                .HasVersion(1);

            PolicyVersionAssert
                .AssertThat(policy.Versions.WithNumber(1))
                .TotalPremiumIs(offer.TotalPrice);
        }

        [Fact]
        public void CannotBuyAlreadyConvertedOffer()
        {
            var offer = OfferFactory.AlreadyConvertedOffer();

            Exception ex = Throws<ApplicationException>(() => offer.Buy(PolicyHolderFactory.Abc()));
            Equal($"Offer {offer.Number} is not in new status and connot be bought", ex.Message);
        }

        [Fact]
        public void CannotBuyExpiredOffer()
        {
            var offer = OfferFactory.NewOfferValidUntil(DateTime.Now.AddDays(-5));
            Exception ex = Throws<ApplicationException>(() => offer.Buy(PolicyHolderFactory.Abc()));
            Equal($"Offer {offer.Number} has expired", ex.Message);
        }
    }
}