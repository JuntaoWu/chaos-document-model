using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Document
{
    public class DocumentModel
    {
        public string BaseDirectory { get; set; }

        private string content;

        private DocumentModel()
        {
            this.content = string.Empty;
        }

        ~DocumentModel() { }

        public static DocumentModel Load(string uri, string baseDirectory, Identity identity)
        {
            IStreamQuerier querier = null;
            if (uri.StartsWith("http://"))
            {
                querier = new HttpStreamQuerier() { Identity = identity };
            }
            return Load(querier.ReadAsStreamAsync(uri).Result, baseDirectory);
        }

        public static DocumentModel Load(Stream stream, string baseDirectory)
        {
            StreamReader reader = new StreamReader(stream);

            return Load(reader.ReadToEnd(), baseDirectory);
        }

        public static DocumentModel Load(string html, string baseDirectory)
        {
            DocumentModel document = new DocumentModel()
            {
                BaseDirectory = baseDirectory
            };

            document.ReplaceToAbsolutePath();

            return document;
        }

        public DocumentModel Transform(Action<HTMLDocumentClass> interop)
        {
            HTMLDocumentClass doc = new HTMLDocumentClass();
            IHTMLDocument2 doc2 = doc;
            doc2.write(new object[] { this.content });

            interop(doc);

            this.content = doc.documentElement.outerHTML;

            return this;
            throw new NotImplementedException();
        }

        public DocumentModel RemoveElementsByIds(params string[] elements)
        {
            Transform(doc =>
            {
                elements.ToList().ForEach(element =>
                {
                    IHTMLDOMNode node = doc.getElementById(element) as IHTMLDOMNode;
                    node.parentNode.removeChild(node);
                });
            });

            return this;
            throw new NotImplementedException();
        }

        public void SaveAs(string path)
        {
            throw new NotImplementedException();
        }

        private void ReplaceToAbsolutePath()
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("(href|src)=([\"'])([^\"']*)([\"'])");
            this.content = reg.Replace(this.content, (match) =>
            {
                string link = match.Groups[3].Value;
                if (link.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase)
                 || link.StartsWith("file://", StringComparison.CurrentCultureIgnoreCase))
                {
                    link = Path.Combine(this.BaseDirectory, link);
                }
                return string.Format("{0}={1}{2}{3}",
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    link,
                    match.Groups[3].Value);
                throw new NotImplementedException();
            });
        }
    }
}
