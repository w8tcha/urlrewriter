// UrlRewriter - A .NET URL Rewriter module
// 
//
// Copyright 2011 Intelligencia
// Copyright 2011 Seth Yates
// 

using System;
using System.Xml;
using System.Configuration;
using System.Collections.Specialized;

using Intelligencia.UrlRewriter.Utilities;
using Intelligencia.UrlRewriter.Errors;
using Intelligencia.UrlRewriter.Transforms;
using Intelligencia.UrlRewriter.Logging;

namespace Intelligencia.UrlRewriter.Configuration
{
    /// <summary>
    /// Reads configuration from an XML Node.
    /// </summary>
    public static class RewriterConfigurationReader
    {
        /// <summary>
        /// Reads configuration information from the given XML Node.
        /// </summary>
        /// <param name="config">The rewriter configuration object to populate.</param>
        /// <param name="section">The XML node to read configuration from.</param>
        /// <returns>The configuration information.</returns>
        public static void Read(IRewriterConfiguration config, XmlNode section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            foreach (XmlNode node in section.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.LocalName == Constants.ElementErrorHandler)
                    {
                        ReadErrorHandler(node, config);
                    }
                    else if (node.LocalName == Constants.ElementDefaultDocuments)
                    {
                        ReadDefaultDocuments(node, config);
                    }
                    else if (node.LocalName == Constants.ElementRegister)
                    {
                        if (node.Attributes[Constants.AttrParser] != null)
                        {
                            ReadRegisterParser(node, config);
                        }
                        else if (node.Attributes[Constants.AttrTransform] != null)
                        {
                            ReadRegisterTransform(node, config);
                        }
                        else if (node.Attributes[Constants.AttrLogger] != null)
                        {
                            ReadRegisterLogger(node, config);
                        }
                    }
                    else if (node.LocalName == Constants.ElementMapping)
                    {
                        ReadMapping(node, config);
                    }
                    else
                    {
                        ReadRule(node, config);
                    }
                }
            }
        }

        private static void ReadRegisterTransform(XmlNode node, IRewriterConfiguration config)
        {
            if (node.ChildNodes.Count > 0)
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNoElements, Constants.ElementRegister), node);
            }

            var type = node.GetRequiredAttribute(Constants.AttrTransform);

            // Transform type specified.
            // Create an instance and add it as the mapper handler for this map.
            var transform = TypeHelper.Activate(type, null) as IRewriteTransform;
            if (transform == null)
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.InvalidTypeSpecified, type, typeof(IRewriteTransform)), node);
            }

            config.TransformFactory.Add(transform);
        }

        private static void ReadRegisterLogger(XmlNode node, IRewriterConfiguration config)
        {
            if (node.ChildNodes.Count > 0)
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNoElements, Constants.ElementRegister), node);
            }

            var type = node.GetRequiredAttribute(Constants.AttrLogger);

            // Logger type specified.  Create an instance and add it
            // as the mapper handler for this map.
            var logger = TypeHelper.Activate(type, null) as IRewriteLogger;
            if (logger != null)
            {
                config.Logger = logger;
            }
        }

        private static void ReadRegisterParser(XmlNode node, IRewriterConfiguration config)
        {
            if (node.ChildNodes.Count > 0)
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNoElements, Constants.ElementRegister), node);
            }

            var type = node.GetRequiredAttribute(Constants.AttrParser);

            var parser = TypeHelper.Activate(type, null);
            var actionParser = parser as IRewriteActionParser;
            if (actionParser != null)
            {
                config.ActionParserFactory.Add(actionParser);
            }

            var conditionParser = parser as IRewriteConditionParser;
            if (conditionParser != null)
            {
                config.ConditionParserPipeline.Add(conditionParser);
            }
        }

        private static void ReadDefaultDocuments(XmlNode node, IRewriterConfiguration config)
        {
            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && childNode.LocalName == Constants.ElementDocument)
                {
                    config.DefaultDocuments.Add(childNode.InnerText);
                }
            }
        }

        private static void ReadErrorHandler(XmlNode node, IRewriterConfiguration config)
        {
            var code = node.GetRequiredAttribute(Constants.AttrCode);

            XmlNode typeNode = node.Attributes[Constants.AttrType];
            XmlNode urlNode = node.Attributes[Constants.AttrUrl];
            if (typeNode == null && urlNode == null)
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.AttributeRequired, Constants.AttrUrl), node);
            }

            IRewriteErrorHandler handler = null;
            if (typeNode != null)
            {
                // <error-handler code="500" url="/oops.aspx" />
                handler = TypeHelper.Activate(typeNode.Value, null) as IRewriteErrorHandler;
                if (handler == null)
                {
                    throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.InvalidTypeSpecified, typeNode.Value, typeof(IRewriteErrorHandler)), node);
                }
            }
            else
            {
                handler = new DefaultErrorHandler(urlNode.Value);
            }

            int statusCode;
            if (!Int32.TryParse(code, out statusCode))
            {
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.InvalidHttpStatusCode, code), node);
            }

            config.ErrorHandlers.Add(statusCode, handler);
        }

        private static void ReadMapping(XmlNode node, IRewriterConfiguration config)
        {
            // Name attribute.
            var mappingName = node.GetRequiredAttribute(Constants.AttrName);

            // Mapper type not specified.  Load in the hash map.
            var map = new StringDictionary();
            foreach (XmlNode mapNode in node.ChildNodes)
            {
                if (mapNode.NodeType == XmlNodeType.Element)
                {
                    if (mapNode.LocalName == Constants.ElementMap)
                    {
                        var fromValue = mapNode.GetRequiredAttribute(Constants.AttrFrom, true);
                        var toValue = mapNode.GetRequiredAttribute(Constants.AttrTo, true);

                        map.Add(fromValue, toValue);
                    }
                    else
                    {
                        throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNotAllowed, mapNode.LocalName), node);
                    }
                }
            }

            IRewriteTransform mapping = new StaticMappingTransform(mappingName, map);

            config.TransformFactory.Add(mapping);
        }

        private static void ReadRule(XmlNode node, IRewriterConfiguration config)
        {
            var parsed = false;
            var parsers = config.ActionParserFactory.GetParsers(node.LocalName);
            if (parsers != null)
            {
                foreach (var parser in parsers)
                {
                    if (!parser.AllowsNestedActions && node.ChildNodes.Count > 0)
                    {
                        throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNoElements, parser.Name), node);
                    }

                    if (!parser.AllowsAttributes && node.Attributes.Count > 0)
                    {
                        throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNoAttributes, parser.Name), node);
                    }

                    var rule = parser.Parse(node, config);
                    if (rule != null)
                    {
                        config.Rules.Add(rule);
                        parsed = true;
                        break;
                    }
                }
            }

            if (!parsed)
            {
                // No parsers recognised to handle this node.
                throw new ConfigurationErrorsException(MessageProvider.FormatString(Message.ElementNotAllowed, node.LocalName), node);
            }
        }
    }
}
