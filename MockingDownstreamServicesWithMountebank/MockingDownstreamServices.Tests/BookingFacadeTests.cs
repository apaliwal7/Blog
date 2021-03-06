﻿using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using MockingDownstreamServices.Facade;
using MockingDownstreamServices.Facade.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MockingDownstreamServices.Tests.Integration
{
    [TestFixture]
    public class BookingFacadeTests
    {
        private ServiceHost serviceHost;
        private const string Address = "http://localhost:8080/BookingFacade/Price";

        [SetUp]
        public void SetUp()
        {
            this.serviceHost = new ServiceHost(typeof(BookingFacade));
            serviceHost.Open();
        }

        [TearDown]
        public void TearDown()
        {
            serviceHost.Close();
        }

        [Test]
        public void PriceShouldSetTradingDatesTest()
        {
            var response = this.Price(new GetPriceRequest());
            Assert.IsNotNull(response);
            var result = response.Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TradingDates);
            Assert.AreNotEqual(result.TradingDates.MaturityDate, DateTime.MinValue);
            Assert.AreNotEqual(result.TradingDates.SettlementDate, DateTime.MinValue);
        }


        [Test]
        public void PriceShouldNotReturnErrorMessagesTest()
        {
            var response = this.Price(new GetPriceRequest());
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Result);
            CollectionAssert.IsEmpty(response.Messages.Where(val => val.StatusCode == StatusCodes.Error));
        }

        [Test]
        public void PriceShouldAddWarningMessageWhenAdjustingSpotValueTest()
        {
            var response = this.Price(new GetPriceRequest
            {
                IsAdvised = true
            });
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Result);
            CollectionAssert.IsNotEmpty(response.Messages.Where(val => val.StatusCode == StatusCodes.Warning));
        }

        [Test]
        public void CanSuccessfullyPriceTest()
        {
            var response = this.Price(new GetPriceRequest());
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Result.Id));
            CollectionAssert.IsEmpty(response.Messages.Where(val => val.StatusCode == StatusCodes.Error));
        }


        private Response<Price> Price(GetPriceRequest request)
        {
            using (var webClient = new WebClient
            {
                Encoding = Encoding.UTF8,
                Headers = { { "Content-type", "application/json" } }
            })
            {
                var responseString = webClient.UploadString(Address, JsonConvert.SerializeObject(request));
                var response = JsonConvert.DeserializeObject<Response<Price>>(responseString);
                return response;
            }
        }
    }
}

