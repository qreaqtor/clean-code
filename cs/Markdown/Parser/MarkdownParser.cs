﻿using Markdown.Domain.Tags;
using Markdown.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markdown.Parser
{
    public class MarkdownParser : IMarkdownParser
    {
        private readonly ITagChecker _tagChecker;
        private readonly Stack<MdTag> needClosingTags;
        private readonly Queue<Tag> offsetTags;

        private readonly Dictionary<Tag, Tag> _differentTagTypes;
        private readonly Dictionary<Tag, string> _mdTags;

        private int currentIndex;

        public MarkdownParser(ITagChecker tagChecker, Dictionary<Tag, Tag> differentTagTypes, Dictionary<Tag, string> mdTags)
        {
            _tagChecker = tagChecker;

            _differentTagTypes = differentTagTypes;
            _mdTags = mdTags;

            needClosingTags = new Stack<MdTag>();
            offsetTags = new Queue<Tag>();
        }

        public List<Token> ParseMarkdown(string markdownText)
        {
            var lines = markdownText.Split('\n');
            var fountedTokens = new List<Token>();

            foreach (var line in lines)
            {
                needClosingTags.Clear();
                offsetTags.Clear();

                currentIndex = 0;

                SearchTokensInLine(line, fountedTokens);
            }

            return fountedTokens;
        }

        private void SearchTokensInLine(string line, List<Token> fountedTokens)
        {
            while (currentIndex < line.Length)
            {
                var tagTypeIndex = _tagChecker.GetTagType(line, currentIndex);
                currentIndex = tagTypeIndex.Item2;
                AnalyzeTag(line, tagTypeIndex.Item1, fountedTokens);

                currentIndex++;
            }
        }

        private void AnalyzeTag(string line, Tag tagType, List<Token> fountedTokens)
        {
            switch (tagType)
            {
                case Tag.Header:
                    fountedTokens.Add(new Token(Tag.Header, 0, line.Length - 1));
                    break;
                case Tag.EscapedSymbol:
                    fountedTokens.Add(new Token(Tag.EscapedSymbol, currentIndex, currentIndex));
                    currentIndex += 1;
                    break;
                case Tag.None:
                    break;
                default:
                    TryAddToken(tagType, line, fountedTokens);
                    break;
            }
        }

        private void TryAddToken(Tag tagType, string line, List<Token> fountedTokens)
        {
            var openingTag = FindOpeningTag(tagType, currentIndex);

            if (openingTag.Tag == Tag.None)
            {
                HandleNotATag(tagType, line);
                return;
            }

            HandleExistingTag(tagType, line, fountedTokens, openingTag);
        }

        private void HandleExistingTag(Tag tagType, string line, List<Token> fountedTokens, MdTag openingTag)
        {
            var token = new Token(tagType, openingTag.Index, currentIndex);

            if (IsPossibleToAdd(token, line))
            {
                fountedTokens.Add(token);
                return;
            }

            if (offsetTags.Count > 0 && offsetTags.Peek() == tagType)
            {
                needClosingTags.Push(new MdTag(tagType, currentIndex));
                offsetTags.Dequeue();
            }
        }

        private void HandleNotATag(Tag tagType, string line)
        {
            if (currentIndex < line.Length - 1 && !char.IsWhiteSpace(line[currentIndex + 1]))
                needClosingTags.Push(new MdTag(tagType, currentIndex));
        }

        private bool IsPossibleToAdd(Token token, string line)
        {
            var shift = _mdTags[token.TagType].Length;
            var diffTagType = _differentTagTypes[token.TagType];
            var anyWhiteSpace = line.Substring(token.StartIndex + 1, token.EndIndex - token.StartIndex - 1).Any(char.IsWhiteSpace);

            return !(char.IsWhiteSpace(line[token.EndIndex - shift])
                || offsetTags.Dequeue() == diffTagType
                || token.EndIndex < line.Length - 1 && !char.IsWhiteSpace(line[token.EndIndex + 1]) && anyWhiteSpace
                || token.TagType == Tag.Bold && needClosingTags.Any(tag => tag.Tag == diffTagType)
                || token.StartIndex - 1 > 0 && !char.IsWhiteSpace(line[token.StartIndex - shift]) && anyWhiteSpace
                );
        }

        private MdTag FindOpeningTag(Tag tagType, int index)
        {
            var openingTag = new MdTag(Tag.None, index);

            while (needClosingTags.Any(tag => tag.Tag == tagType))
            {
                var removeClosingTag = needClosingTags.Pop();
                openingTag = new MdTag(removeClosingTag.Tag, removeClosingTag.Index);
                offsetTags.Enqueue(removeClosingTag.Tag);
            }

            return openingTag;
        }
    }
}
