// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking.Internal
{
    public class EntityKeyFactorySourceTest
    {
        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(int));

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(new[] { keyMock.Object }));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_nullable_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(int?));

            Assert.IsType<SimpleEntityKeyFactory<int>>(CreateKeyFactorySource().GetKeyFactory(new[] { keyMock.Object }));
        }

        [Fact]
        public void Returns_different_simple_entity_key_factory_for_different_properties()
        {
            var keyMock1 = new Mock<IProperty>();
            keyMock1.Setup(m => m.PropertyType).Returns(typeof(Guid));

            var keyMock2 = new Mock<IProperty>();
            keyMock2.Setup(m => m.PropertyType).Returns(typeof(Guid));

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(new[] { keyMock1.Object }), factorySource.GetKeyFactory(new[] { keyMock2.Object }));
        }

        [Fact]
        public void Returns_different_simple_nullable_entity_key_factory_for_different_properties()
        {
            var keyMock1 = new Mock<IProperty>();
            keyMock1.Setup(m => m.PropertyType).Returns(typeof(Guid?));

            var keyMock2 = new Mock<IProperty>();
            keyMock2.Setup(m => m.PropertyType).Returns(typeof(Guid?));

            var factorySource = CreateKeyFactorySource();
            Assert.NotSame(factorySource.GetKeyFactory(new[] { keyMock1.Object }), factorySource.GetKeyFactory(new[] { keyMock2.Object }));
        }

        [Fact]
        public void Returns_same_simple_entity_key_factory_for_same_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(Guid));

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { keyMock.Object }), factorySource.GetKeyFactory(new[] { keyMock.Object }));
        }

        [Fact]
        public void Returns_same_nullable_simple_entity_key_factory_for_same_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(Guid?));

            var factorySource = CreateKeyFactorySource();
            Assert.Same(factorySource.GetKeyFactory(new[] { keyMock.Object }), factorySource.GetKeyFactory(new[] { keyMock.Object }));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_composite_property_key()
        {
            Assert.IsType<CompositeEntityKeyFactory>(
                CreateKeyFactorySource().GetKeyFactory(new[] { new Mock<IProperty>().Object, new Mock<IProperty>().Object }));
        }

        [Fact]
        public void Returns_a_simple_entity_key_factory_for_single_reference_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(string));

            Assert.IsType<SimpleEntityKeyFactory<string>>(CreateKeyFactorySource().GetKeyFactory(new[] { keyMock.Object }));
        }

        [Fact]
        public void Returns_a_composite_entity_key_factory_for_single_structural_property()
        {
            var keyMock = new Mock<IProperty>();
            keyMock.Setup(m => m.PropertyType).Returns(typeof(byte[]));

            Assert.IsType<CompositeEntityKeyFactory>(CreateKeyFactorySource().GetKeyFactory(new[] { keyMock.Object }));
        }

        private static EntityKeyFactorySource CreateKeyFactorySource()
        {
            return new EntityKeyFactorySource();
        }
    }
}
