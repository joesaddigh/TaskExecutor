using Logger;
using System;
using System.Xml;

namespace TaskExecutorLib.Helpers
{
    public static class XmlUtils
    {
        public static XmlNodeList GetElementsByTagName(XmlDocument xmlDocument, string elementName)
        {
            var value = default(XmlNodeList);

            try
            {
                if (xmlDocument != null)
                {
                    value = xmlDocument.GetElementsByTagName(elementName);
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("GetElementByTagName exception: {0}", ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return value;
        }

        public static string GetElementByTagNameAsString(XmlElement xmlElement, string elementName)
        {
            string value = string.Empty;

            try
            {
                if (xmlElement != null)
                {
                    value = xmlElement.GetElementsByTagName(elementName).Item(0).InnerText;
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("{0} exception: {1}", nameof(GetElementByTagNameAsString), ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return value;
        }

        internal static bool GetElementByTagNameAsBool(XmlElement xmlnode, string elementName)
        {
            bool value = false;

            try
            {
                var sVal = GetElementByTagNameAsString(xmlnode, elementName);
                Boolean.TryParse(sVal, out value);
            }
            catch (Exception ex)
            {
                string sLog = string.Format("{0} exception: {1}", nameof(GetElementByTagNameAsBool), ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return value;
        }

        internal static string GetSubElementByTagNameAsString(XmlNode xmlNode, string elementName)
        {
            string value = string.Empty;

            try
            {
                if (xmlNode != null)
                {
                    var el = xmlNode[elementName];
                    if (el != null)
                    {
                        value = el.InnerText;
                    }
                }
            }
            catch (Exception ex)
            {
                string sLog = string.Format("{0} exception: {1}", nameof(GetSubElementByTagNameAsString), ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return value;

        }

        internal static bool GetSubElementByTagNameAsBool(XmlNode xmlNode, string elementName)
        {
            bool value = false;

            try
            {
                var sVal = GetSubElementByTagNameAsString(xmlNode, elementName);
                Boolean.TryParse(sVal, out value);
            }
            catch (Exception ex)
            {
                string sLog = string.Format("{0} exception: {1}", nameof(GetSubElementByTagNameAsBool), ex.ToString());
                Log.Instance.OutputLog(sLog, Log.LogLevel.Error);
            }

            return value;
        }
    }
}