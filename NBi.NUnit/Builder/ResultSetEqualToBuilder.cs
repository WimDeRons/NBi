﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NBi.Core;
using NBi.Core.Query;
using NBi.Core.ResultSet;
using NBi.Core.ResultSet.Comparer;
using NBi.NUnit.ResultSetComparison;
using NBi.Xml.Constraints;
using NBi.Xml.Items;
using NBi.Xml.Systems;
using NBi.Core.Xml;
using NBi.Core.Transformation;
using System.Data;
using NBi.Core.ResultSet.Resolver;
using System.IO;
using NBi.Core.Query.Resolver;
using NBi.Core.ResultSet.Equivalence;
using NBi.NUnit.Builder.Helper;
using NBi.Xml.Settings;

namespace NBi.NUnit.Builder
{
    class ResultSetEqualToBuilder : AbstractResultSetBuilder
    {
        protected EqualToXml ConstraintXml { get; set; }

        protected virtual EquivalenceKind EquivalenceKind
        {
            get { return EquivalenceKind.EqualTo; }
        }

        public ResultSetEqualToBuilder()
        {

        }

        protected override void SpecificSetup(AbstractSystemUnderTestXml sutXml, AbstractConstraintXml ctrXml)
        {
            if (!(ctrXml is EqualToXml))
                throw new ArgumentException("Constraint must be a 'EqualToXml'");

            ConstraintXml = (EqualToXml)ctrXml;
        }

        protected override void SpecificBuild()
        {
            Constraint = InstantiateConstraint();
        }

        protected NBiConstraint InstantiateConstraint()
        {
            BaseResultSetComparisonConstraint ctr = null;

            //Manage transformations
            var transformationProvider = new TransformationProvider();
            foreach (var columnDef in ConstraintXml.ColumnsDef)
            {
                if (columnDef.Transformation != null)
                    transformationProvider.Add(columnDef.Index, columnDef.Transformation);
            }

            if (ConstraintXml.GetCommand() != null)
                ctr = InstantiateConstraint(((QueryXml)(ConstraintXml.BaseItem)), ConstraintXml.Settings, transformationProvider);
            else if (ConstraintXml.ResultSet != null)
                ctr = InstantiateConstraint(ConstraintXml.ResultSet, ConstraintXml.Settings, transformationProvider);
            else if (ConstraintXml.XmlSource != null)
                ctr = InstantiateConstraint(ConstraintXml.XmlSource, ConstraintXml.Settings, transformationProvider);

            if (ctr == null)
                throw new ArgumentException();

            //Manage settings for comparaison
            var builder = new SettingsEquivalerBuilder();
            if (ConstraintXml.Behavior == EqualToXml.ComparisonBehavior.SingleRow)
            {
                builder.Setup(false);
                builder.Setup(ConstraintXml.ValuesDefaultType, ConstraintXml.Tolerance);
                builder.Setup(ConstraintXml.ColumnsDef);
            }
            else
            {
                builder.Setup(ConstraintXml.KeysDef, ConstraintXml.ValuesDef);
                builder.Setup(
                    ConstraintXml.KeyName?.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct(),
                    ConstraintXml.ValueName?.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
                builder.Setup(ConstraintXml.ValuesDefaultType, ConstraintXml.Tolerance);
                builder.Setup(ConstraintXml.ColumnsDef);
            }

            builder.Build();
            var settings = builder.GetSettings();

            var factory = new EquivalerFactory();
            var comparer = factory.Instantiate(settings, EquivalenceKind);
            ctr = ctr.Using(comparer);
            ctr = ctr.Using(settings);

            //Manage parallelism
            if (ConstraintXml.ParallelizeQueries)
                ctr = ctr.Parallel();
            else
                ctr = ctr.Sequential();

            return ctr;
        }

        protected virtual BaseResultSetComparisonConstraint InstantiateConstraint(object obj, SettingsXml settings, TransformationProvider transformation)
        {
            var argsBuilder = new ResultSetResolverArgsBuilder();
            argsBuilder.Setup(obj);
            argsBuilder.Setup(settings);
            argsBuilder.Build();

            var factory = new ResultSetResolverFactory();
            var resolver = factory.Instantiate(argsBuilder.GetArgs());

            var serviceBuilder = new ResultSetServiceBuilder();
            serviceBuilder.Setup(resolver);
            if (transformation != null)
                serviceBuilder.Setup(transformation.Transform);

            var service = serviceBuilder.GetService();

            return new EqualToConstraint(service);
        }


    }
}
