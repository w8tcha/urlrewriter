// UrlRewriter - A .NET URL Rewriter module
// 
//
// Copyright 2011 Intelligencia
// Copyright 2011 Seth Yates
// 

using System.Net;

namespace Intelligencia.UrlRewriter.Actions
{
    /// <summary>
    /// Returns a 404 Not Found HTTP status code.
    /// </summary>
    public sealed class NotFoundAction : SetStatusAction
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NotFoundAction()
            : base(HttpStatusCode.NotFound)
        {
        }
    }
}
