using DanMuJI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using DanMuJI.DLL.SignalRHub;

namespace DanMuJI.Controllers
{
    public class BiliBiliController : Controller
    {
        private static bool flag = false;
        private int TIME = 3;
        private static string API_URL = "https://api.live.bilibili.com/msg/send";
       private static int[] SLEEP_ARRAY = new int[] { 60000};
        private string[] PUTONGDANMU = new string[]
        {
            "LGD咚咚咚！",
            "中国队加油！"
        };

        // GET: BiliBili
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public  JsonResult SendMsg(string live_id,string msg, string time,string cookie)
        {
            ResultModel result = new ResultModel();
            result.code = 0;
            result.msg = "failed";

            if (string.IsNullOrEmpty(live_id))
            {
                result.msg = "直播间id不能为空";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(msg))
            {
                result.msg = "弹幕内容不能为空";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(msg))
            {
                result.msg = "Cookie不能为空";
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            try
            {
                PUTONGDANMU.Append(msg);
                flag = true;
                int times = string.IsNullOrEmpty(time) ? TIME : int.Parse(time);
                DateTime end_time = DateTime.Now.AddMinutes(times);

                string csrf = GetCSRF(cookie);
                if (string.IsNullOrEmpty(csrf))
                {
                    result.msg = "Cookie解析失败";
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                HttpClientHandler handle = new HttpClientHandler()
                {
                    UseCookies = false,
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                HttpClient client = new HttpClient(handle);
                Random random = new Random();
                SignalRHub hub = new SignalRHub();

                for (int i=1; DateTime.Now < end_time && flag;i++)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["bubble"] = "0";
                    dic["color"] = "5566168";
                    dic["mode"] = "1";
                    dic["fontsize"] = "25";
                    dic["rnd"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                    dic["roomid"] = live_id;
                    dic["csrf"] = csrf;
                    dic["csrf_token"] = csrf;
                    int danmu_index = random.Next(PUTONGDANMU.Length);
                    dic["msg"] = PUTONGDANMU[danmu_index];

                    var message = new HttpRequestMessage(HttpMethod.Post, API_URL);
                    message.Headers.Add("Accept", "*/*");
                    message.Headers.Add("Accept-Encoding", "gzip,deflate");
                    message.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
                    message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36");
                    message.Headers.Add("Cookie", cookie);

                    message.Content = new FormUrlEncodedContent(dic);
                    var response = client.SendAsync(message).Result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(response))
                    {
                        BiliBiliReturnModel return_model = JsonConvert.DeserializeObject<BiliBiliReturnModel>(response);
                        if (return_model.code == "0")
                        {
                            hub.SendMsg("success", $"第 {i} 次弹幕发送成功！弹幕内容：{PUTONGDANMU[danmu_index]}");
                            continue;
                        }
                        else if (return_model.code == "10031" || return_model.code == "10030")   //发送频繁
                        {
                            int sleep_time_index = random.Next(SLEEP_ARRAY.Length);
                            hub.SendMsg("failed", $"第 {i} 次弹幕发送失败! 失败原因: 发送频繁，暂停 {SLEEP_ARRAY[sleep_time_index] / 1000} 秒");
                            Task.Delay(SLEEP_ARRAY[sleep_time_index]).Wait();
                        }
                        else
                        {
                            ChangeFlagStatus();
                            result.code = 0;
                            result.msg = $"发送弹幕失败! 失败内容: {response}";
                            hub.SendMsg("error", $"发送弹幕失败! 失败内容: {response}");
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        hub.SendMsg("error", $"发送弹幕失败! 失败内容: {response}");
                    }
                }

                result.code = 1;
                result.msg = "成功";
                result.data = "弹幕全部发送完成";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.msg = "系统异常:" + ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CancleSend()
        {
            ResultModel result = new ResultModel();
            result.code = 0;
            result.msg = "failed";

            try
            {
                ChangeFlagStatus();
                result.code = 1;
                result.msg = "success";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public static string GetCSRF(string cookie)
        {
            if (string.IsNullOrEmpty(cookie))
                return null;

            try
            {
                string[] cookie_value = cookie.Replace(" ","").Split(';');
                foreach(var item in cookie_value)
                {
                    if (item.Contains("bili_jct"))
                    {
                        string[] key_value = item.Split('=');
                        return key_value[1];
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static void ChangeFlagStatus()
        {
            flag = false;
        }

        private static byte[] Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}