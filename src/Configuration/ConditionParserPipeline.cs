// UrlRewriter - A .NET URL Rewriter module
// 
//
// Copyright 2011 Intelligencia
// Copyright 2011 Seth Yates
// 

using System.Collections.Generic;

namespace Intelligencia.UrlRewriter.Configuration
{
    /// <summary>
    /// Pipeline for creating the Condition parsers.
    /// </summary>
    public class ConditionParserPipeline : List<IRewriteConditionParser>
    {
        /*
        /// <summary>
        /// Adds a parser.
        /// </summary>
        /// <param name="parserType">The parser type.</param>
        public void AddParser(string parserType)
        {
            AddParser((IRewriteConditionParser)TypeHelper.Activate(parserType, null));
        }

        /// <summary>
        /// Adds a parser.
        /// </summary>
        /// <param name="parser">The parser.</param>
        public void AddParser(IRewriteConditionParser parser)
        {
            Add(parser);
        }
         */
    }
}
