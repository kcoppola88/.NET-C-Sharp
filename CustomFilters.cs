using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using KevinAuctionSite.Models;
using KevinAuctionSite.Repositories;

namespace KevinAuctionSite.Controllers.Filters
{
    //Action filter to ensure all get requests are compressed, delivered as quickly as possible
    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool allowCompression = false;
            bool.TryParse(ConfigurationManager.AppSettings["Compression"], out allowCompression);

            if (allowCompression)
            {
                HttpRequestBase request = filterContext.HttpContext.Request;

                string acceptEncoding = request.Headers["Accept-Encoding"];

                if (string.IsNullOrEmpty(acceptEncoding)) return;

                acceptEncoding = acceptEncoding.ToUpperInvariant();

                HttpResponseBase response = filterContext.HttpContext.Response;

                if (acceptEncoding.Contains("GZIP"))
                {
                    response.AppendHeader("Content-encoding", "gzip");
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("DEFLATE"))
                {
                    response.AppendHeader("Content-encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                }
            }
        }
    }

    //applied to image rendering code blocks, whitespace filter was screwing those up
    public class IgnoreWhiteSpaceFilter : ActionFilterAttribute
    {

    }

    //filters out all whitespace in HTML source
    public class WhitespaceFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            bool disabled = filterContext.ActionDescriptor.IsDefined(typeof(IgnoreWhiteSpaceFilter), true) ||
                        filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(IgnoreWhiteSpaceFilter), true);

            if (disabled)
                return;

            var response = filterContext.HttpContext.Response;

            // If it's a sitemap, just return.
            if (filterContext.HttpContext.Request.RawUrl == "/sitemap.xml") return;

            if (response.ContentType != "text/html" || response.Filter == null) return;

            response.Filter = new HelperClass(response.Filter);
        }

        private class HelperClass : Stream
        {
            private readonly Stream _base;
            StringBuilder _s = new StringBuilder();

            public HelperClass(Stream responseStream)
            {
                if (responseStream == null)
                    throw new ArgumentNullException("responseStream");
                _base = responseStream;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var html = Encoding.UTF8.GetString(buffer, offset, count);
                var reg = new Regex(@"(?<=\s)\s+(?![^<>]*</pre>)");
                html = reg.Replace(html, string.Empty);

                buffer = Encoding.UTF8.GetBytes(html);
                _base.Write(buffer, 0, buffer.Length);
            }

            #region Other Members

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead { get { return false; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override long Length { get { throw new NotSupportedException(); } }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }

            public override void Flush()
            {
                _base.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}