﻿using MarkdownLog;
using NBi.Core;
using NBi.Framework.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBi.Framework.FailureMessage.Markdown
{
    class ItemsMessageMarkdown : IItemsMessageFormatter
    {
        private readonly IDictionary<string, ISampler<string>> samplers;

        private MarkdownContainer expected;
        private MarkdownContainer actual;
        private MarkdownContainer analysis;

        public ItemsMessageMarkdown(IDictionary<string, ISampler<string>> samplers)
        {
            this.samplers = samplers;
        }


        public void Build(IEnumerable<string> expectedItems, IEnumerable<string> actualItems, ListComparer.Result result)
        {
            expectedItems = expectedItems ?? new List<string>();
            actualItems = actualItems ?? new List<string>();
            result = result ?? new ListComparer.Result(null, null);

            expected = BuildList(expectedItems, samplers["expected"]);
            actual = BuildList(actualItems, samplers["actual"]);
            analysis = BuildIfNotEmptyList(result.Missing ?? new List<string>(), "Missing", samplers["analysis"]);
            analysis.Append(BuildIfNotEmptyList(result.Unexpected ?? new List<string>(), "Unexpected", samplers["analysis"]));
        }

        private MarkdownContainer BuildList(IEnumerable<string> items, ISampler<string> sampler)
        {
            sampler.Build(items);
            var sampledItems = sampler.GetResult();

            var container = new MarkdownContainer();
            if (items.Count() > 0)
            {
                container.Append($"Set of {items.Count()} item{(items.Count() > 1 ? "s" : string.Empty)}".ToMarkdownParagraph());
                container.Append(sampledItems.ToMarkdownBulletedList());
            }
            else
                container.Append("An empty set.".ToMarkdownParagraph());

            if (sampler.GetIsSampled())
                container.Append($"... and {sampler.GetExcludedRowCount()} others not displayed.".ToMarkdownParagraph());

            return container;
        }

        private MarkdownContainer BuildList(IEnumerable<string> items, string title, ISampler<string> sampler)
        {
            var container = new MarkdownContainer();
            container.Append((title + " items:").ToMarkdownSubHeader());
            container.Append(BuildList(items, sampler));

            return container;
        }


        private MarkdownContainer BuildIfNotEmptyList(IEnumerable<string> items, string title, ISampler<string> sampler)
        {
            if (items.Count() == 0)
                return new MarkdownContainer();
            else
                return BuildList(items, title, sampler);
        }

        public virtual string RenderExpected()
        {
            if (samplers["expected"] is NoneSampler<string>)
                return "Display skipped.";
            else
                return expected.ToMarkdown();
        }

        public virtual string RenderActual()
        {
            if (samplers["actual"] is NoneSampler<string>)
                return "Display skipped.";
            else
                return actual.ToMarkdown();
        }

        public virtual string RenderAnalysis()
        {
            if (samplers["analysis"] is NoneSampler<string>)
                return "Display skipped.";
            else
                return analysis.ToMarkdown();
        }
    }
}
