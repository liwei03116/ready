using ready.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Text.Json;
using System.Reflection;

namespace CoffeeMachineAPI.Tests
{
    public class CoffeeMachineControllerTests
    {
        [Fact]
        public async Task BrewCoffee_Success()
        {
            // Arrange
            var controller = new CoffeeController();

            // Act
            var result = await controller.BrewCoffee() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task BrewCoffee_OutOfCoffee()
        {
            // Arrange
            var controller = new CoffeeController();
            SetCallCount(controller, 4); // Set call count to 4 to reach the threshold

            // Act
            var result = await controller.BrewCoffee() as StatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(503, result.StatusCode);
        }

        private void SetCallCount(CoffeeController controller, int count)
        {
            var fieldInfo = typeof(CoffeeController).GetField("_callCount", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo.SetValue(null, count);
        }

        [Fact]
        public async Task BrewCoffee_AprilFirstTeapot()
        {
            // Arrange
            var controller = new CoffeeController();

            // Act
            System.DateTime AprilFirst = new System.DateTime(DateTime.Now.Year, 4, 1);
            DateTime AprilFirst_UTC = AprilFirst.ToUniversalTime();
            System.DateTime today = System.DateTime.Now;
            System.DateTime today_UTC = today.ToUniversalTime();
            System.TimeSpan difference = AprilFirst_UTC.Subtract(today_UTC);

            if (difference.Days == 0)
            {
                var result = await controller.BrewCoffee() as StatusCodeResult;

                // Assert
                Assert.NotNull(result);
                Assert.Equal(418, result.StatusCode);
            }
        }

        [Fact]
        public async Task BrewCoffee_HotWeather_IcedCoffee()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new WeatherResponse
                    {
                        main = new Main
                        {
                            temp = 35 // Greater than 30°C
                        }
                    }))
                });

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.RequestServices).Returns(new ServiceCollection()
                .AddTransient(_ => mockHttpClient.Object)
                .BuildServiceProvider());


            var controller = new CoffeeController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.Request.HttpContext.RequestServices = new ServiceCollection()
                .AddTransient(_ => mockHttpClient.Object)
                .BuildServiceProvider();

            // Act
            var result = await controller.BrewCoffee() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Equal("Your refreshing iced coffee is ready", ((dynamic)result.Value).message);
        }

        [Fact]
        public async Task BrewCoffee_ColdWeather_HotCoffee()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            mockHttpClient
                .Setup(client => client.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new WeatherResponse
                    {
                        main = new Main
                        {
                            temp = 25 // Less than or equal to 30°C
                        }
                    }))
                });

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.RequestServices).Returns(new ServiceCollection()
                .AddTransient(_ => mockHttpClient.Object)
                .BuildServiceProvider());


            var controller = new CoffeeController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            controller.Request.HttpContext.RequestServices = new ServiceCollection()
                .AddTransient(_ => mockHttpClient.Object)
                .BuildServiceProvider();

            // Act
            var result = await controller.BrewCoffee() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.Value);
            Assert.Equal("Your piping hot coffee is ready", ((dynamic)result.Value).message);
        }
    }
}
