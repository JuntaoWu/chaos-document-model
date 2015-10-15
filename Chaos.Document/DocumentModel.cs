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
        public string RootDirectory { get; set; }

        private string content;
        private string workingDirectory;
        private string tempFileName;

        private DocumentModel()
        {
            this.content = string.Empty;
        }

        ~DocumentModel() { }


        //todo: Redesign the Load method to avoid mismatch method call.
        public static DocumentModel Load(string uri, string baseDirectory, string rootDirectory, Identity identity)
        {
            IStreamQuerier querier = null;
            if (uri.StartsWith("http://"))
            {
                querier = new HttpStreamQuerier() { Identity = identity };
            }
            return Load(querier.ReadAsStreamAsync(uri).Result, baseDirectory, rootDirectory);
        }

        public static DocumentModel Load(Stream stream, string baseDirectory, string rootDirectory)
        {
            StreamReader reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);

            return Load(reader.ReadToEnd(), baseDirectory, rootDirectory);
        }

        public static DocumentModel Load(string html, string baseDirectory, string rootDirectory)
        {
            DocumentModel document = new DocumentModel()
            {
                BaseDirectory = baseDirectory,
                RootDirectory = rootDirectory ?? baseDirectory,
                workingDirectory = Path.GetTempPath(),
                tempFileName = string.Format("{0}.html", Guid.NewGuid())
            };

            document.content = html;

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

            File.WriteAllText(Path.Combine(this.workingDirectory, this.tempFileName), this.content);

            return this;
        }

        public DocumentModel RemoveElementsByIds(params string[] elements)
        {
            Transform(doc =>
            {
                elements.ToList().ForEach(element =>
                {
                    IHTMLDOMNode node = doc.getElementById(element) as IHTMLDOMNode;
                    if (node != null)
                    {
                        node.parentNode.removeChild(node);
                    }
                });
            });

            return this;
        }

        public string SaveAs(string path)
        {
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.ApplicationClass();
            word.Visible = true;
            object fileName = Path.Combine(this.workingDirectory, this.tempFileName);

            Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(ref fileName);

            object saveTo = path;
            object fileFormat = Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatDocumentDefault;
            doc.SaveAs2(ref saveTo, ref fileFormat);

            word.Documents.Close();

            //todo: Release the COM object.
            return saveTo as string;
        }

        [Obsolete]
        private void ReplaceToAbsolutePath()
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("(href|src)=([\"'])([^\"']*)([\"'])");
            this.content = reg.Replace(this.content, (match) =>
            {
                string link = match.Groups[3].Value;
                //todo: Consider using Uri to help determine how to change to absolute path.
                if (!link.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase)
                 && !link.StartsWith("file://", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (link.StartsWith("/"))
                    {
                        link = Path.Combine(this.RootDirectory, link.Trim('/'));
                    }
                    else
                    {
                        link = Path.Combine(this.BaseDirectory, link);
                    }
                }
                return string.Format("{0}={1}{2}{3}",
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    link,
                    match.Groups[4].Value);
                throw new NotImplementedException();
            });
        }

        public DocumentModel ReplaceToAbsolutePath(string baseDirectory, string rootDirectory = "")
        {
            Transform(doc =>
            {
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("(href|src)=([\"'])([^\"']*)([\"'])");
                var html = doc.documentElement.outerHTML;
                //replace content;
                html = reg.Replace(html, (match) =>
                {
                    string link = match.Groups[3].Value;
                    //todo: Consider using Uri to help determine how to change to absolute path.
                    if (!link.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase)
                     && !link.StartsWith("file://", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (link.StartsWith("/"))
                        {
                            link = Path.Combine(this.RootDirectory, link.Trim('/'));
                        }
                        else
                        {
                            link = Path.Combine(this.BaseDirectory, link);
                        }
                    }
                    return string.Format("{0}={1}{2}{3}",
                        match.Groups[1].Value,
                        match.Groups[2].Value,
                        link,
                        match.Groups[4].Value);
                });
                doc.documentElement.outerHTML = html;
            });
            return this;
        }

        //todo: Find a way to replace via HTMLElements attributes
        public DocumentModel ReplaceToAbsolutePath(Func<string, string> evaluator)
        {
            Transform(doc =>
            {
                var x = doc.anchors.item() as HTMLAnchorElement;
                x.href = evaluator(x.href);
            });
            return this;
        }
    }
}
