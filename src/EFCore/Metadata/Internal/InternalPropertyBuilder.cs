// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder)
            : base(property, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsRequired(bool? isRequired, ConfigurationSource configurationSource)
        {
            if (isRequired.HasValue)
            {
                return IsRequired(isRequired.Value, configurationSource);
            }

            if (configurationSource.Overrides(Metadata.GetIsNullableConfigurationSource()))
            {
                Metadata.SetIsNullable(null, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (CanSetRequired(isRequired, configurationSource))
            {
                if (!isRequired)
                {
                    using (Metadata.DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
                    {
                        foreach (var key in Metadata.GetContainingKeys().ToList())
                        {
                            if (configurationSource == ConfigurationSource.Explicit
                                && key.GetConfigurationSource() == ConfigurationSource.Explicit)
                            {
                                throw new InvalidOperationException(
                                    CoreStrings.KeyPropertyCannotBeNullable(Metadata.Name, Metadata.DeclaringEntityType.DisplayName(), key.Properties.Format()));
                            }

                            var removed = key.DeclaringEntityType.Builder.RemoveKey(key, configurationSource);
                            Debug.Assert(removed.HasValue);
                        }

                        Metadata.SetIsNullable(true, configurationSource);
                    }
                }
                else
                {
                    Metadata.SetIsNullable(false, configurationSource);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetRequired(bool isRequired, ConfigurationSource? configurationSource)
            => ((Metadata.IsNullable == !isRequired)
                || (configurationSource.HasValue
                    && configurationSource.Value.Overrides(Metadata.GetIsNullableConfigurationSource())))
               && (isRequired
                   || Metadata.ClrType.IsNullableType()
                   || (configurationSource == ConfigurationSource.Explicit)); // let it throw for Explicit

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasMaxLength(int maxLength, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.MaxLength, maxLength, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsUnicode(bool unicode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.Unicode, unicode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasValueGenerator([CanBeNull] Type valueGeneratorType, ConfigurationSource configurationSource)
        {
            if (valueGeneratorType == null)
            {
                return HasValueGenerator((Func<IProperty, IEntityType, ValueGenerator>)null, configurationSource);
            }

            if (!typeof(ValueGenerator).GetTypeInfo().IsAssignableFrom(valueGeneratorType.GetTypeInfo()))
            {
                throw new ArgumentException(
                    CoreStrings.BadValueGeneratorType(valueGeneratorType.ShortDisplayName(), typeof(ValueGenerator).ShortDisplayName()));
            }

            return HasValueGenerator(
                (_, __)
                    =>
                {
                    try
                    {
                        return (ValueGenerator)Activator.CreateInstance(valueGeneratorType);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.CannotCreateValueGenerator(valueGeneratorType.ShortDisplayName()), e);
                    }
                }, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasValueGenerator(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory,
            ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.ValueGeneratorFactory, factory, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasField([CanBeNull] string fieldName, ConfigurationSource configurationSource)
        {
            if (Metadata.FieldInfo?.GetSimpleMemberName() == fieldName)
            {
                Metadata.SetField(fieldName, configurationSource);
                return true;
            }

            if (!configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource()))
            {
                return false;
            }

            if (fieldName != null)
            {
                var fieldInfo = PropertyBase.GetFieldInfo(
                    fieldName, Metadata.DeclaringType, Metadata.Name,
                    shouldThrow: configurationSource == ConfigurationSource.Explicit);
                Metadata.SetField(fieldInfo, configurationSource);
                return true;
            }

            Metadata.SetField(fieldName, configurationSource);
            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasFieldInfo([CanBeNull] FieldInfo fieldInfo, ConfigurationSource configurationSource)
        {
            if ((configurationSource.Overrides(Metadata.GetFieldInfoConfigurationSource())
                 && (fieldInfo == null
                     || PropertyBase.IsCompatible(
                         fieldInfo, Metadata.ClrType, Metadata.DeclaringType.ClrType, Metadata.Name,
                         shouldThrow: configurationSource == ConfigurationSource.Explicit)))
                || Equals(Metadata.FieldInfo, fieldInfo))
            {
                Metadata.SetField(fieldInfo, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UsePropertyAccessMode(PropertyAccessMode propertyAccessMode, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.PropertyAccessMode, propertyAccessMode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasConversion([CanBeNull] ValueConverter valueConverter, ConfigurationSource configurationSource)
        {
            if (valueConverter != null
                && valueConverter.ModelClrType.UnwrapNullableType() != Metadata.ClrType.UnwrapNullableType())
            {
                throw new ArgumentException(
                    CoreStrings.ConverterPropertyMismatch(
                        valueConverter.ModelClrType.ShortDisplayName(),
                        Metadata.DeclaringEntityType.DisplayName(),
                        Metadata.Name,
                        Metadata.ClrType.ShortDisplayName()));
            }

            return HasAnnotation(CoreAnnotationNames.ValueConverter, valueConverter, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool HasConversion([CanBeNull] Type providerClrType, ConfigurationSource configurationSource)
            => HasAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsConcurrencyTokenConfigurationSource())
                || (Metadata.IsConcurrencyToken == concurrencyToken))
            {
                Metadata.SetIsConcurrencyToken(concurrencyToken, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool BeforeSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetBeforeSaveBehaviorConfigurationSource())
                || Metadata.GetBeforeSaveBehavior() == behavior)
            {
                Metadata.SetBeforeSaveBehavior(behavior, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool AfterSave(PropertySaveBehavior? behavior, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetAfterSaveBehaviorConfigurationSource())
                || Metadata.GetAfterSaveBehavior() == behavior)
            {
                Metadata.SetAfterSaveBehavior(behavior, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetValueGeneratedConfigurationSource())
                || Metadata.ValueGenerated == valueGenerated)
            {
                Metadata.SetValueGenerated(valueGenerated, configurationSource);

                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            var newProperty = entityTypeBuilder.Metadata.FindProperty(Metadata.Name);
            InternalPropertyBuilder newPropertyBuilder;
            var configurationSource = Metadata.GetConfigurationSource();
            var typeConfigurationSource = Metadata.GetTypeConfigurationSource();
            if (newProperty != null
                && (newProperty.GetConfigurationSource().Overrides(configurationSource)
                    || newProperty.GetTypeConfigurationSource().Overrides(typeConfigurationSource)
                    || (Metadata.ClrType == newProperty.ClrType
                        && Metadata.GetIdentifyingMemberInfo()?.Name == newProperty.GetIdentifyingMemberInfo()?.Name)))
            {
                newPropertyBuilder = newProperty.Builder;
                newProperty.UpdateConfigurationSource(configurationSource);
                if (typeConfigurationSource.HasValue)
                {
                    newProperty.UpdateTypeConfigurationSource(typeConfigurationSource.Value);
                }
            }
            else
            {
                newPropertyBuilder = Metadata.GetIdentifyingMemberInfo() == null
                    ? entityTypeBuilder.Property(Metadata.Name, Metadata.ClrType, configurationSource, Metadata.GetTypeConfigurationSource())
                    : entityTypeBuilder.Property(Metadata.GetIdentifyingMemberInfo(), configurationSource);
            }

            if (newProperty == Metadata)
            {
                return newPropertyBuilder;
            }

            newPropertyBuilder.MergeAnnotationsFrom(Metadata);

            var oldBeforeSaveBehaviorConfigurationSource = Metadata.GetBeforeSaveBehaviorConfigurationSource();
            if (oldBeforeSaveBehaviorConfigurationSource.HasValue)
            {
                newPropertyBuilder.BeforeSave(
                    Metadata.GetBeforeSaveBehavior(),
                    oldBeforeSaveBehaviorConfigurationSource.Value);
            }

            var oldAfterSaveBehaviorConfigurationSource = Metadata.GetAfterSaveBehaviorConfigurationSource();
            if (oldAfterSaveBehaviorConfigurationSource.HasValue)
            {
                newPropertyBuilder.AfterSave(
                    Metadata.GetAfterSaveBehavior(),
                    oldAfterSaveBehaviorConfigurationSource.Value);
            }

            var oldIsNullableConfigurationSource = Metadata.GetIsNullableConfigurationSource();
            if (oldIsNullableConfigurationSource.HasValue)
            {
                newPropertyBuilder.IsRequired(!Metadata.IsNullable, oldIsNullableConfigurationSource.Value);
            }

            var oldIsConcurrencyTokenConfigurationSource = Metadata.GetIsConcurrencyTokenConfigurationSource();
            if (oldIsConcurrencyTokenConfigurationSource.HasValue)
            {
                newPropertyBuilder.IsConcurrencyToken(
                    Metadata.IsConcurrencyToken,
                    oldIsConcurrencyTokenConfigurationSource.Value);
            }

            var oldValueGeneratedConfigurationSource = Metadata.GetValueGeneratedConfigurationSource();
            if (oldValueGeneratedConfigurationSource.HasValue)
            {
                newPropertyBuilder.ValueGenerated(Metadata.ValueGenerated, oldValueGeneratedConfigurationSource.Value);
            }

            var oldFieldInfoConfigurationSource = Metadata.GetFieldInfoConfigurationSource();
            if (oldFieldInfoConfigurationSource.HasValue)
            {
                newPropertyBuilder.HasFieldInfo(Metadata.FieldInfo, oldFieldInfoConfigurationSource.Value);
            }

            return newPropertyBuilder;
        }
    }
}
