﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DbContextCodeGeneratorContext
    {
        private readonly ReverseEngineeringGenerator _generator;
        private readonly IModel _model;
        private readonly string _namespaceName;
        private readonly string _className;
        private readonly string _connectionString;

        ////private Dictionary<IEntityType, string> _entityTypeToClassNameMap;
        ////private Dictionary<IProperty, string> _propertyToPropertyNameMap = new Dictionary<IProperty, string>();
        private List<string> _usedNamespaces = new List<string>() // initialize with default namespaces
                {
                    "System",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };

        public DbContextCodeGeneratorContext(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IModel model, [NotNull]string namespaceName,
            [NotNull]string className, [NotNull]string connectionString)
        {
            _generator = generator;
            _model = model;
            _namespaceName = namespaceName;
            _className = className;
            _connectionString = connectionString;
            ////InitializeMappingsToCSharpNames();
        }

        ////private void InitializeMappingsToCSharpNames()
        ////{
        ////    if (_entityTypeToClassNameMap == null)
        ////    {
        ////        _entityTypeToClassNameMap = new Dictionary<IEntityType, string>();
        ////        foreach (var entityType in _model.EntityTypes)
        ////        {
        ////            _entityTypeToClassNameMap[entityType] =
        ////                CSharpUtilities.Instance.GenerateCSharpIdentifier(
        ////                    entityType.SimpleName, _entityTypeToClassNameMap.Values);
        ////            InitializePropertyNames(entityType);
        ////        }
        ////    }
        ////}

        ////private void InitializePropertyNames(IEntityType entityType)
        ////{
        ////    // use local propertyToPropertyNameMap to ensure no clashes in Property names
        ////    // within an EntityType but to allow them for properties in different EntityTypes
        ////    var propertyToPropertyNameMap = new Dictionary<IProperty, string>();
        ////    foreach (var property in entityType.Properties)
        ////    {
        ////        propertyToPropertyNameMap[property] =
        ////            CSharpUtilities.Instance.GenerateCSharpIdentifier(
        ////                property.Name, propertyToPropertyNameMap.Values);

        ////        var propertyNamespace = property.PropertyType.Namespace;
        ////        if (!_usedNamespaces.Contains(propertyNamespace))
        ////        {
        ////            _usedNamespaces.Add(propertyNamespace);
        ////        }
        ////    }

        ////    foreach (var keyValuePair in propertyToPropertyNameMap)
        ////    {
        ////        _propertyToPropertyNameMap.Add(keyValuePair.Key, keyValuePair.Value);
        ////    }
        ////}

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(sb, ClassNamespace);
            CSharpCodeGeneratorHelper.Instance.BeginPublicPartialClass(sb, ClassName);
            GenerateProperties(sb);
            GenerateMethods(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual ReverseEngineeringGenerator Generator
        {
            get
            {
                return _generator;
            }
        }

        public virtual string ClassName
        {
            get
            {
                return _className;
            }
        }

        public virtual string ClassNamespace
        {
            get
            {
                return _namespaceName;
            }
        }

        public virtual string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        ////public Dictionary<IEntityType, string> EntityTypeToClassNameMap
        ////{
        ////    get
        ////    {
        ////        return _entityTypeToClassNameMap;
        ////    }
        ////}

        ////public Dictionary<IProperty, string> PropertyToPropertyNameMap
        ////{
        ////    get
        ////    {
        ////        return _propertyToPropertyNameMap;
        ////    }
        ////}

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            CSharpCodeGeneratorHelper.Instance.Comment(sb, "Generated using Connection String: " + ConnectionString);
            CSharpCodeGeneratorHelper.Instance.Comment(sb, string.Empty);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            // TODO - add in other namespaces
            foreach (var @namespace in _usedNamespaces)
            {
                CSharpCodeGeneratorHelper.Instance.AddUsingStatement(sb, @namespace);
            }

            if (_usedNamespaces.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateProperties(IndentedStringBuilder sb)
        {
            foreach (var entityType in OrderedEntityTypes())
            {
                CSharpCodeGeneratorHelper.Instance.AddPublicVirtualProperty(
                    sb
                    , "DbSet<" + _generator.EntityTypeToClassNameMap[entityType] + ">"
                    , _generator.EntityTypeToClassNameMap[entityType]);
            }

            if (_model.EntityTypes.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateMethods(IndentedStringBuilder sb)
        {
            GenerateOnConfiguringCode(sb);
            sb.AppendLine();
            GenerateOnModelCreatingCode(sb);
        }

        public virtual void GenerateOnConfiguringCode(IndentedStringBuilder sb)
        {
            var onConfiguringMethodParameters = new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("DbContextOptions", "options")
                };
            CSharpCodeGeneratorHelper.Instance.BeginProtectedOverrideMethod(sb, "void", "OnConfiguring", onConfiguringMethodParameters);
            sb.Append("options.UseSqlServer(");
            sb.Append(CSharpUtilities.Instance.GenerateVerbatimStringLiteral(ConnectionString));
            sb.AppendLine(");");
            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateOnModelCreatingCode(IndentedStringBuilder sb)
        {
            var onModelCreatingMethodParameters = new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("ModelBuilder", "modelBuilder")
                };
            CSharpCodeGeneratorHelper.Instance.BeginProtectedOverrideMethod(sb, "void", "OnModelCreating", onModelCreatingMethodParameters);
            foreach (var entityType in OrderedEntityTypes())
            {
                sb.Append("modelBuilder.Entity<");
                sb.Append(_generator.EntityTypeToClassNameMap[entityType]);
                sb.Append(">(");
                GenerateEntityConfiguration(sb, entityType);
                sb.AppendLine(");");
            }
            CSharpCodeGeneratorHelper.Instance.EndMethod(sb);
        }

        public virtual void GenerateEntityConfiguration(IndentedStringBuilder sb, IEntityType entityType)
        {
            sb.AppendLine("entity =>");
            using (sb.Indent())
            {
                sb.AppendLine("{");
                using (sb.Indent())
                {
                    var key = entityType.TryGetPrimaryKey();
                    if (key != null && key.Properties.Count > 0)
                    {
                        GenerateEntityKeyConfiguration(sb, key);
                    }
                    GenerateForeignKeysConfiguration(sb, entityType);
                    foreach (var property in entityType.Properties)
                    {
                        GeneratePropertyFacetsConfiguration(sb, property);
                    }
                }
                sb.AppendLine("}");
            }
        }

        public virtual void GenerateEntityKeyConfiguration(IndentedStringBuilder sb, IKey key)
        {
            sb.Append("entity.Key( e => ");
            sb.Append(ModelUtilities.Instance
                .GenerateLambdaToKey(key.Properties, PrimaryKeyPropertyOrder, "e"));
            sb.AppendLine(" );");
        }

        public abstract void GenerateForeignKeysConfiguration(IndentedStringBuilder sb, IEntityType entityType);

        public virtual void GeneratePropertyFacetsConfiguration(IndentedStringBuilder sb, IProperty property)
        {
        }


        //
        // helper methods
        //
        public abstract int PrimaryKeyPropertyOrder(IProperty property);

        public virtual IEnumerable<IEntityType> OrderedEntityTypes()
        {
            return _model.EntityTypes.OrderBy(e => _generator.EntityTypeToClassNameMap[e]);
        }
    }
}