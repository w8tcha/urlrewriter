// UrlRewriter - A .NET URL Rewriter module
// 
//
// Copyright 2011 Intelligencia
// Copyright 2011 Seth Yates
// 

using System;

namespace Intelligencia.UrlRewriter.Actions
{
    /// <summary>
    /// Action that adds a given header.
    /// </summary>
    public class AddHeaderAction : IRewriteAction
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="header">The header name.</param>
        /// <param name="value">The header value.</param>
        public AddHeaderAction(string header, string value)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            _header = header;
            _value = value;
        }

        /// <summary>
        /// The header name.
        /// </summary>
        public string Header
        {
            get { return _header; }
        }

        /// <summary>
        /// The header value.
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="context">The rewrite context.</param>
        public RewriteProcessing Execute(IRewriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.ResponseHeaders.Add(Header, Value);

            return RewriteProcessing.ContinueProcessing;
        }

        private string _header;
        private string _value;
    }
}
