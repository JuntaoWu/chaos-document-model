using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Chaos.Document;

namespace Chaos.Document.Test.Controllers
{
    public class DownloadWordDocumentController : ApiController
    {
        public FileWebResponse Get(string projectId)
        {
            string url = "";
            string baseDirectory = "";

            DocumentModel.Load(url, baseDirectory, null)
           .Transform(doc =>
            {
                doc.body.innerHTML = doc.getElementById("div").outerHTML;
            })
           .RemoveElementsByIds("")
           .SaveAs(@"D:\Result\hello.docx");

            throw new NotImplementedException();
        }
    }
}
