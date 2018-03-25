// UrlRewriter - A .NET URL Rewriter module
// 
//
// Copyright 2011 Intelligencia
// Copyright 2011 Seth Yates
// 

using System;
using System.Xml;

using Intelligencia.UrlRewriter.Actions;
using Intelligencia.UrlRewriter.Configuration;
using Intelligencia.UrlRewriter.Utilities;

namespace Intelligencia.UrlRewriter.Parsers
{
    /// <summary>
    /// Action parser for the set-property action.
    /// </summary>
    public sealed class SetPropertyActionParser : RewriteActionParserBase
    {
        /// <summary>
        /// The name of the action.
        /// </summary>
        public override string Name
        {
            get { return Constants.ElementSet; }
        }

        /// <summary>
        /// Whether the action allows nested actions.
        /// </summary>
        public override bool AllowsNestedActions
        {
            get { return false; }
        }

        /// <summary>
        /// Whether the action allows attributes.
        /// </summary>
        public override bool AllowsAttributes
        {
            get { return true; }
        }

        /// <summary>
        /// Parses the node.
        /// </summary>
        /// <param name="node">The node to parse.</param>
        /// <param name="config">The rewriter configuration.</param>
        /// <returns>The parsed action, or null if no action parsed.</returns>
        public override IRewriteAction Parse(XmlNode node, IRewriterConfiguration config)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var propertyName = node.GetOptionalAttribute(Constants.AttrProperty);
            if (String.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            var propertyValue = node.GetRequiredAttribute(Constants.AttrValue, true);

            return new SetPropertyAction(propertyName, propertyValue);
        }
    }
}
