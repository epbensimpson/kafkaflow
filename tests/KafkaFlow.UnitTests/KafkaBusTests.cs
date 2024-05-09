using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow.Clusters;
using KafkaFlow.Configuration;
using KafkaFlow.Consumers;
using KafkaFlow.Producers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KafkaFlow.UnitTests;

[TestClass]
public class KafkaBusTests
{
    [TestMethod]
    public async Task StopAsync_ShouldBeThreadSafe()
    {
        // Arrange
        var stopCount = 0;

        var configuration = new KafkaConfiguration();

        configuration.AddClusters(new[]
        {
            new ClusterConfiguration(configuration, "test", new[] { "test" }, () => new SecurityInformation(), _ => { },
                _ => Interlocked.Increment(ref stopCount))
        });

        var kafkaBus = new KafkaBus(Mock.Of<IDependencyResolver>(), configuration, Mock.Of<IConsumerManagerFactory>(),
            Mock.Of<IConsumerAccessor>(), Mock.Of<IProducerAccessor>(), Mock.Of<IClusterManagerAccessor>());

        // Act
        var tasks = Enumerable.Range(1, 5).Select(_ => kafkaBus.StopAsync());

        await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(1, stopCount);
    }
}