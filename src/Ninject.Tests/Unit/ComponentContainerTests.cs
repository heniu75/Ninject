﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject.Components;
using Ninject.Infrastructure.Disposal;
using Xunit;

namespace Ninject.Tests.Unit.ComponentContainerTests
{
	public class ComponentContainerContext
	{
		protected readonly ComponentContainer container;
		protected readonly Mock<IKernel> kernelMock;

		public ComponentContainerContext()
		{
			container = new ComponentContainer();
			kernelMock = new Mock<IKernel>();

			container.Kernel = kernelMock.Object;
		}
	}

	public class WhenGetIsCalled : ComponentContainerContext
	{
		[Fact]
		public void ThrowsExceptionIfNoImplementationRegisteredForService()
		{
			Assert.Throws<InvalidOperationException>(() => container.Get<ITestService>());
		}

		[Fact]
		public void ReturnsInstanceWhenOneImplementationIsRegistered()
		{
			container.Add<ITestService, TestServiceA>();
			var service = container.Get<ITestService>();

			Assert.NotNull(service);
			Assert.IsType<TestServiceA>(service);
		}

		[Fact]
		public void ReturnsInstanceOfFirstRegisteredImplementation()
		{
			container.Add<ITestService, TestServiceA>();
			container.Add<ITestService, TestServiceB>();
			var service = container.Get<ITestService>();

			Assert.NotNull(service);
			Assert.IsType<TestServiceA>(service);
		}

		[Fact]
		public void InjectsEnumeratorOfServicesWhenConstructorArgumentIsIEnumerable()
		{
			container.Add<ITestService, TestServiceA>();
			container.Add<ITestService, TestServiceB>();
			container.Add<IAsksForEnumerable, AsksForEnumerable>();
			var asks = container.Get<IAsksForEnumerable>();

			Assert.NotNull(asks);
			Assert.NotNull(asks.SecondService);
			Assert.IsType<TestServiceB>(asks.SecondService);
		}
	}

	public class WhenGetAllIsCalledOnComponentContainer : ComponentContainerContext
	{
		[Fact]
		public void ReturnsSeriesWithSingleItem()
		{
			container.Add<ITestService, TestServiceA>();
			var services = container.GetAll<ITestService>().ToList();

			Assert.NotNull(services);
			Assert.Equal(1, services.Count);
			Assert.IsType<TestServiceA>(services[0]);
		}

		[Fact]
		public void ReturnsInstanceOfEachRegisteredImplementation()
		{
			container.Add<ITestService, TestServiceA>();
			container.Add<ITestService, TestServiceB>();
			var services = container.GetAll<ITestService>().ToList();

			Assert.NotNull(services);
			Assert.Equal(2, services.Count);
			Assert.IsType<TestServiceA>(services[0]);
			Assert.IsType<TestServiceB>(services[1]);
		}

		[Fact]
		public void ReturnsSameInstanceForTwoCallsForSameService()
		{
			container.Add<ITestService, TestServiceA>();

			var service1 = container.Get<ITestService>();
			var service2 = container.Get<ITestService>();

			Assert.NotNull(service1);
			Assert.NotNull(service2);
			Assert.Same(service1, service2);
		}
	}

	public class WhenRemoveAllIsCalled : ComponentContainerContext
	{
		[Fact]
		public void RemovesAllMappings()
		{
			container.Add<ITestService, TestServiceA>();
			var service1 = container.Get<ITestService>();
			Assert.NotNull(service1);

			container.RemoveAll<ITestService>();

			Assert.Throws<InvalidOperationException>(() => container.Get<ITestService>());
		}

		[Fact]
		public void DisposesOfAllInstances()
		{
			container.Add<ITestService, TestServiceA>();
			container.Add<ITestService, TestServiceB>();
			var services = container.GetAll<ITestService>().ToList();

			Assert.NotNull(services);
			Assert.Equal(2, services.Count);

			container.RemoveAll<ITestService>();

			Assert.True(services[0].IsDisposed);
			Assert.True(services[1].IsDisposed);
		}
	}

	internal class AsksForEnumerable : NinjectComponent, IAsksForEnumerable
	{
		public ITestService SecondService { get; set; }

		public AsksForEnumerable(IEnumerable<ITestService> services)
		{
			SecondService = services.Skip(1).First();
		}
	}

	internal interface IAsksForEnumerable : INinjectComponent
	{
		ITestService SecondService { get; set; }
	}

	internal class TestServiceA : NinjectComponent, ITestService { }
	internal class TestServiceB : NinjectComponent, ITestService { }

	internal interface ITestService : INinjectComponent, INotifyWhenDisposed { }
}