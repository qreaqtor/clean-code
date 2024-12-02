using FluentAssertions;
using Markdown.Builder;
using Markdown.Checker;
using Markdown.Config;
using Markdown.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown
{
    [TestFixture]
    public class Markdown_Tests
    {
        private Md md;

        [SetUp]
        public void Setup()
        {
            var builder = new HtmlBuilder(MarkdownConfig.HtmlTags);
            var parser = new MarkdownParser(new TagChecker(), MarkdownConfig.DifferentTags, MarkdownConfig.MdTags);
            md = new Md(parser, builder);
        }

        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.SimpleTests))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWhenPairTagNotExists))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWhenPairTagsInDifferentWords))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWhenSpacesBetweenTagAndWord))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWhenTagsHighlightPartOfWord))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWithEscape))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWithIntersectionTags))]
        [TestCaseSource(typeof(Md_Test_Cases), nameof(Md_Test_Cases.TestsWithoutWords))]
        public void Correct_Render_WhenInputValidString(string markdownText, string htmlText)
        {
            md.Render(markdownText).Should().Be(htmlText);
        }

        [TestCase("", TestName = "Return empty string if string empty")]
        [TestCase(null, TestName = "Return empty string if string null")]
        public void ThrowArgumentException_When_InvalidInputString(string markdownText)
        {
            Action action = () =>
            {
                var htmlText = md.Render(markdownText);
            };

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
