using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Chaos.Document;
using System.Web;
using System.IO;
using Chaos.Net.Infrastructure;

namespace Chaos.Document.Test.Controllers
{
    public class DownloadWordDocumentController : ApiController
    {
        public string Get(string projectId)
        {

            Uri requestUri = HttpContext.Current.Request.Url;
            Uri rootUri = new Uri(requestUri.GetLeftPart(UriPartial.Authority));
            string uriPath = HttpContext.Current.Request.Path;

            string baseDirectory = Directory.GetParent(HttpContext.Current.Request.PhysicalPath).ToString();
            string rootDirectory = Directory.GetParent(HttpContext.Current.Server.MapPath("~")).ToString();

            Uri uri = rootUri.Append(string.Format("GetProject?projectId={0}", projectId));

            //todo: Test if the Uri constructor will be more easily to use.
            //new Uri(new Uri(requestUri.GetLeftPart(UriPartial.Authority)), string.Format("GetProject?projectId={0}", projectId)).ToString();

            try
            {
                DocumentModel.Load(uri.ToString(), baseDirectory, baseDirectory, identity: null)
               .Transform(doc =>
               {
                   //todo: Ensure is not null.
                   var element = doc.getElementById("div");
                   if (element != null)
                   {
                       doc.body.innerHTML = element.outerHTML;
                   }
               })
               .RemoveElementsByIds("")
               .SaveAs(@"D:\Result\hello.docx");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            HttpContext.Current.Response.Headers.Add("Content-Type", MimeMapping.GetMimeMapping(".docx"));
            HttpContext.Current.Response.Headers.Add("Content-Disposition", string.Format("inline;filename=\"{0}\"", "hello.docx"));
            HttpContext.Current.Response.WriteFile(@"D:\Result\hello.docx");
            return "";
        }
    }
}
