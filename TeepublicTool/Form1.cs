﻿using AutoUpload.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TeepublicTool
{
    public partial class Form1 : Form
    {
        private string queryString = string.Empty;
        public Form1()
        {
            InitializeComponent();

            var lines = File.ReadAllLines(@"D:\test.txt").ToList();
            List<ParamUrl> paramList = new List<ParamUrl>();
            for (int i = 0; i < lines.Count; i++)
            {
                ParamUrl param = new ParamUrl();
                string singleLine = lines[i];
                string[] temp = singleLine.Split(':');
                paramList.Add(new ParamUrl { Name = temp[0], Value = temp[1]});
            }

            queryString = GetQueryString(paramList);
            Console.WriteLine(queryString);

  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NameValueCollection nvc = new NameValueCollection();
            CustomWeb web = new CustomWeb();


            string test = web.SendRequest("https://www.teepublic.com", "GET", null, true, "");

            string loginUrl = "https://www.teepublic.com/users/sign_in";
            nvc.Clear();
            nvc.Add("utf8", "✓");
            nvc.Add("user[post_login_partial]", "");
            nvc.Add("session[email]", "deadlove_011011@yahoo.com");
            nvc.Add("session[password]", "19001560");
            nvc.Add("session[remember_me]", "0");
            nvc.Add("commit", "Login");


            //https://www.teepublic.com/users/sign_up
            test = web.SendRequest("https://www.teepublic.com/users/sign_up", "GET", null, false, "");
            string responseUrl = "";
            string result = web.SendRequest(loginUrl, "POST", "www.teepublic.com", nvc, ref responseUrl, true, "application/x-www-form-urlencoded; charset=UTF-8");

            test = web.SendRequest("https://www.teepublic.com", "GET", null, false, "");

            result = web.SendRequest("https://www.teepublic.com/design/quick_create", "GET", 
                "www.teepublic.com", null,ref responseUrl,  false, "");
            //Console.WriteLine(result);

            string referer = responseUrl;
            string publicId = "";

            string tempUrl = "";
            result = web.SendRequest(referer, "GET",
               "www.teepublic.com", null, ref tempUrl, false, "");


            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(result);

            var fileInput = doc.DocumentNode.SelectSingleNode("//input[@name='file']");
            string uploadUrl = fileInput.GetAttributeValue("data-url", string.Empty);
            dynamic objData = JsonConvert.DeserializeObject(fileInput.GetAttributeValue("data-form-data", 
                string.Empty).Replace("&quot;", "\"").Replace("{{", "{").Replace("}}", "}"));

            //result = web.SendUploadOptionRequest(uploadUrl,
            //    "OPTIONS", referer);

            nvc.Clear();
            nvc.Add("callback", objData.callback.ToString());
            nvc.Add("eager", objData.eager.ToString());
            nvc.Add("exif", objData.exif.ToString());
            nvc.Add("folder", objData.folder.ToString());
            nvc.Add("format", objData.format.ToString());
            nvc.Add("image_metadata", objData.image_metadata.ToString());
            nvc.Add("invalidate", objData.invalidate.ToString());
            nvc.Add("public_id", objData.public_id.ToString());
            nvc.Add("timestamp", objData.timestamp.ToString());
            nvc.Add("transformation", objData.transformation.ToString());
            nvc.Add("type", objData.type.ToString());
            nvc.Add("signature", objData.signature.ToString());
            nvc.Add("api_key", objData.api_key.ToString());

            result = web.HttpUploadFile(uploadUrl, @"C:\Users\RAVI\Desktop\Logo\SMITH.png",
                "file", "image/png", nvc, "api.cloudinary.com", "https://www.teepublic.com", referer);

            
            string colorLink = GetLinkFromStr(result).Replace("<a href=\"", "").Replace("\">", "");
            result = web.SendRequest(colorLink, "GET", null, false, "");

            string finalLink = GetLinkFromStr(result).Replace("<a href=\"", "").Replace("\">", "");
            result = web.SendRequest(finalLink, "GET", null, false, "");
            Console.WriteLine(result);

            string updateInfoUrl = referer.Replace("edit", "").Replace("designs", "t-shirt");

            //result = web.SendRequest(updateInfoUrl, "POST",
            // "www.teepublic.com", nvc, ref responseUrl, false, "application/x-www-form-urlencoded");

            result = web.SendUploadInfo(updateInfoUrl, "POST", "www.teepublic.com",
                "application/x-www-form-urlencoded", queryString, referer);
            Console.WriteLine(result);
        }


        private string GetLinkFromStr(string result)
        {
            Regex regex = new Regex("<a href=.+\">");
            Match match = regex.Match(result);
            if(match.Success)
            {
                return match.Groups[0].Value;
            }
            return null;
        }

        public string GetQueryString(List<ParamUrl> paramList)
        {
            //var properties = from p in obj.GetType().GetProperties()
            //                 where p.GetValue(obj, null) != null
            //                 select p.Name + "=" + WebUtility.UrlEncode(p.GetValue(obj, null).ToString());

            //return String.Join("&", properties.ToArray());

            var props = from p in paramList
                        select (p.Name)
                + "=" + WebUtility.UrlEncode(p.Value);

            return String.Join("&", props.ToArray());
            //return WebUtility.UrlEncode(
            //     string.Format("http://www.yoursite.com?{0}",
            //        string.Join("&",
            //            parameters.Select(kvp =>
            //                string.Format("{0}={1}", kvp.Key, kvp.Value)))));
        }

    }
}
