// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalNavigationBuilderTest
    {
        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_HasField()
        {
            var builder = CreateInternalNavigationBuilder();
            var metadata = builder.Metadata;

            Assert.NotNull(metadata.FieldInfo);
            Assert.Equal(ConfigurationSource.Convention, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_details", ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField("_details", ConfigurationSource.DataAnnotation));

            Assert.Equal("_details", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_details", ConfigurationSource.Convention));
            Assert.False(builder.CanSetField("_otherDetails", ConfigurationSource.Convention));
            Assert.NotNull(builder.HasField("_details", ConfigurationSource.Convention));
            Assert.Null(builder.HasField("_otherDetails", ConfigurationSource.Convention));

            Assert.Equal("_details", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField("_otherDetails", ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField("_otherDetails", ConfigurationSource.DataAnnotation));

            Assert.Equal("_otherDetails", metadata.FieldInfo?.Name);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetFieldInfoConfigurationSource());

            Assert.True(builder.CanSetField((string)null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.HasField((string)null, ConfigurationSource.DataAnnotation));

            Assert.Null(metadata.FieldInfo);
            Assert.Null(metadata.GetFieldInfoConfigurationSource());
        }

        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_PropertyAccessMode()
        {
            var builder = CreateInternalNavigationBuilder();
            IConventionNavigation metadata = builder.Metadata;

            Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
            Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
            Assert.False(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));
            Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferProperty, ConfigurationSource.Convention));
            Assert.Null(builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.Convention));

            Assert.Equal(PropertyAccessMode.PreferProperty, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferFieldDuringConstruction, metadata.GetPropertyAccessMode());
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetPropertyAccessModeConfigurationSource());

            Assert.True(builder.CanSetPropertyAccessMode(null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.UsePropertyAccessMode(null, ConfigurationSource.DataAnnotation));

            Assert.Equal(PropertyAccessMode.PreferField, metadata.GetPropertyAccessMode());
            Assert.Null(metadata.GetPropertyAccessModeConfigurationSource());
        }

        [ConditionalFact]
        public void Can_only_override_lower_or_equal_source_IsEagerLoaded()
        {
            var builder = CreateInternalNavigationBuilder();
            IConventionNavigation metadata = builder.Metadata;

            Assert.False(metadata.IsEagerLoaded);
            Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetIsEagerLoaded(eagerLoaded: true, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.IsEagerLoaded(eagerLoaded: true, ConfigurationSource.DataAnnotation));

            Assert.True(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetIsEagerLoaded(eagerLoaded: true, ConfigurationSource.Convention));
            Assert.False(builder.CanSetIsEagerLoaded(eagerLoaded: false, ConfigurationSource.Convention));
            Assert.NotNull(builder.IsEagerLoaded(eagerLoaded: true, ConfigurationSource.Convention));
            Assert.Null(builder.IsEagerLoaded(eagerLoaded: false, ConfigurationSource.Convention));

            Assert.True(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetIsEagerLoaded(eagerLoaded: false, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.IsEagerLoaded(eagerLoaded: false, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsEagerLoaded);
            Assert.Equal(ConfigurationSource.DataAnnotation, metadata.GetIsEagerLoadedConfigurationSource());

            Assert.True(builder.CanSetIsEagerLoaded(null, ConfigurationSource.DataAnnotation));
            Assert.NotNull(builder.IsEagerLoaded(null, ConfigurationSource.DataAnnotation));

            Assert.False(metadata.IsEagerLoaded);
            Assert.Null(metadata.GetIsEagerLoadedConfigurationSource());
        }

        private InternalNavigationBuilder CreateInternalNavigationBuilder()
        {
            var modelBuilder = (InternalModelBuilder)
                InMemoryTestHelpers.Instance.CreateConventionBuilder().GetInfrastructure();
            var orderEntityBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            var detailsEntityBuilder = modelBuilder.Entity(typeof(OrderDetails), ConfigurationSource.Convention);
            orderEntityBuilder
                .HasRelationship(detailsEntityBuilder.Metadata, nameof(Order.Details), ConfigurationSource.Convention, targetIsPrincipal: false)
                .IsUnique(false, ConfigurationSource.Convention);
            var navigation = (Navigation)orderEntityBuilder.Navigation(nameof(Order.Details));

            return new InternalNavigationBuilder(navigation, modelBuilder);
        }

        protected class Order
        {
            public int OrderId { get; set; }

            private ICollection<OrderDetails> _details;
            private readonly ICollection<OrderDetails> _otherDetails = new List<OrderDetails>();
            public ICollection<OrderDetails> Details { get => _details; set => _details = value; }
        }

        protected class OrderDetails
        {
            public int Id { get; set; }
        }
    }
}
