/* --------------------------------------------------------
 * author：livezingy
 * 
 * BLOG：http://www.livezingy.com
 * 
 * Development environment：
 *      Visual Studio V2013     
 * Revision History：
   
--------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;

namespace TaoBao
{
    public partial class Form1 : Form
    {
        private HttpHelper http = new HttpHelper();
        private HttpItem item = null;
        private HttpResult result = null;
        private CookieContainer cookiesCon = new CookieContainer();
        private CookieCollection ccReturned;
        string htmlStr = "";
        string urlCode = "";
        string postStr = "";
        string tokenStr = "";
        string stStr = "";
        bool beNeedCode = false;//是否需要验证码

        private ProductPageHandle productPageHandle;
        private AuctionOperations auctionOperations;
        private OrderPageHandle orderPageHandle;
        private AuctionLog auctionLog;


        public Form1()
        {
            InitializeComponent();

            auctionLog = new AuctionLog(false);
            orderPageHandle = new OrderPageHandle(auctionLog);
           // orderPageHandleOld = new OrderPageHandleOld(auctionLog);
           // orderPageHandleQuestion = new OrderPageHandleQuestion(auctionLog);
            productPageHandle = new ProductPageHandle(auctionLog);
            auctionOperations = new AuctionOperations(auctionLog);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        //带验证码登录获取J_HToken
        private void button1_Click(object sender, EventArgs e)
        {
            postStr = "ua=" + textBox1.Text;

            postStr = postStr + "&TPL_username=" + textBox6.Text;

            postStr = postStr + "&TPL_password=&TPL_checkcode=&loginsite=0&newlogin=0&TPL_redirect_url=https%3A%2F%2Fwww.taobao.com%2F&from=tbTop&fc=default&style=default&css_style=&keyLogin=false&qrLogin=true&newMini=false&tid=&support=000001&CtrlVersion=1%2C0%2C0%2C7&loginType=3&minititle=&minipara=&umto=NaN&pstrong=&sign=&need_sign=&isIgnore=&full_redirect=&popid=&callback=&guf=&not_duplite_str=&need_user_id=&poy=&gvfdcname=10&gvfdcre=68747470733A2F2F7777772E74616F62616F2E636F6D2F&from_encoding=&sub=&";

            postStr = postStr + "TPL_password_2=" + textBox2.Text;

            postStr = postStr + "&loginASR=1&loginASRSuc=1&allp=&oslanguage=zh-CN&sr=1280*800&osVer=&naviVer=firefox%7C35";


            if (!beNeedCode)
            {
                loginIni();
            }
            else
            {
                if (!requestWithIdenCode())
                {
                    beNeedCode = false;
                }
            }
        }

        /// <summary>
        /// 用Firefox抓取登录时的数据，获取登录时的ua和加密的密码，对于同一个账号，获取一次即可。用ua和加密后的密码进行模拟登录
        /// </summary>
        /// <returns>bool，请求结果的状态码OK时，返回true；否则返回false，需要重试</returns>
        private bool loginIni()
        {
           

            item = new HttpItem()
            {
                URL = "https://login.taobao.com/member/login.jhtml",
                Method = "POST",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://login.taobao.com/member/login.jhtml",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36",
                Postdata = postStr,
                cContainer = cookiesCon,
                KeepAlive = true,
            };

            result = http.GetHtml(item);

            htmlStr = result.Html;

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (htmlStr.Contains("请输入验证码"))
                {
                    richTextBox1.Text = "此次安全验证异常，您需要输入验证码。\n";

                    Regex reg1 = new Regex(@"codeURL: ""(.*?)"",");

                    urlCode = reg1.Match(htmlStr).Groups[1].Value;

                    //获取验证码并显示
                    getIdenCode();

                    beNeedCode = true;

                    return false;
                }
                else
                {
                    richTextBox1.Text = "此次安全验证通过，正在登录。\n";

                    //若不需要输入验证码，理论上而言，此次即可得到Token
                    //从返回结果中获取J_HToken的值，并通过Token获取st的值
                    Regex tmpReg = new Regex(@"id=""J_HToken"" value=""(.*?)"" />");

                    tokenStr = tmpReg.Match(htmlStr).Groups[1].Value;

                    beNeedCode = false;

                    if (tokenStr != "")
                    {
                        //成功获取Token值后，开始获取ST码
                        return (getSTbyToken());
                    }
                    else
                    {
                        richTextBox1.Text = "获取请求失败，请您确认UA与加密密码。\n";
                        return false;
                    }
                }

                
            }
            else
            {
                richTextBox1.Text = "获取请求失败，请您确认UA与加密密码。\n";

                return false;
            }
        }

        /// <summary>
        /// 用ua与加密密码以及验证码再次请求
        /// </summary>
        private bool requestWithIdenCode()
        {
            string codeVal = "TPL_checkcode="+ textBox3.Text;

            string newPostStr = postStr.Replace("TPL_checkcode=", codeVal);

            string tmpTxtShow = richTextBox1.Text;

            item = new HttpItem()
            {
                URL = "https://login.taobao.com/member/login.jhtml",
                Method = "POST",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://login.taobao.com/member/login.jhtml",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36",
                Postdata = newPostStr,
                cContainer = cookiesCon,
                KeepAlive = true,
            };

            result = http.GetHtml(item);

            htmlStr = result.Html;

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (htmlStr.Contains("验证码错误"))
                {
                    richTextBox1.Text = tmpTxtShow + "验证码错误，请重新输入验证码。\n";

                    return false;
                }
                else
                {
                    //从返回结果中获取J_HToken的值，并通过Token获取st的值
                    Regex tmpReg = new Regex(@"id=""J_HToken"" value=""(.*?)"" />");

                    tokenStr = tmpReg.Match(htmlStr).Groups[1].Value;

                    //成功获取Token值后，开始获取ST码
                    return (getSTbyToken());
                }
            }
            else
            {
                richTextBox1.Text = tmpTxtShow + "请求错误，请重试。\n";

                return false;
            }
        }

        /// <summary>
        /// 获取st码以及top.location链接
        /// </summary>
        private bool getSTbyToken()
        {
            string tmpTxtShow = richTextBox1.Text;

            item = new HttpItem()
            {
                URL = "https://passport.alipay.com/mini_apply_st.js?site=0&token=" + tokenStr + "&callback=stCallback6",
                Method = "GET",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                ContentType = "application/x-www-form-urlencoded",
                Referer = "https://www.taobao.com/",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36",
                cContainer = cookiesCon,
                KeepAlive = true,
            };

            result = http.GetHtml(item);

            htmlStr = result.Html;

            //从返回结果中获取st的值
            Regex tmpReg = new Regex(@"""st"":""(.*?)""}");

            stStr = tmpReg.Match(htmlStr).Groups[1].Value;

            if (stStr != "")
            {                
                //用得到的st值登录淘宝
                item = new HttpItem()
                {
                    URL = "https://login.taobao.com/member/vst.htm?st=" + stStr + "&TPL_username=bebigheart",
                    Method = "GET",
                    Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                    ContentType = "application/x-www-form-urlencoded",
                    Referer = "https://login.taobao.com/",
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36",
                    cContainer = cookiesCon,
                    KeepAlive = true,
                };

                result = http.GetHtml(item);

                htmlStr = result.Html;

                tmpReg = new Regex(@"top.location = ""(.*?)"";");
                string tmpStr = tmpReg.Match(htmlStr).Groups[1].Value;

                if (tmpStr != "")
                {
                    richTextBox1.Text = tmpTxtShow + "ST码匹配成功,模拟登陆成功。";

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                richTextBox1.Text = tmpTxtShow + "ST码匹配失败,模拟登陆失败，请重试!";

                return false;
            }
        }

        /// <summary>
        /// 从验证码链接中获取
        /// </summary>
        private void getIdenCode()
        {
            item = new HttpItem()
            {
                URL = urlCode,
                Method = "GET",
                Accept = "*/*",
                Referer = "https://pin.aliyun.com/",
                ResultType = ResultType.Byte,
                cContainer = cookiesCon
            };
            result = http.GetHtml(item);

            pictureBox1.Image = getImageFromByte(result.ResultByte);
        }

        private static Image getImageFromByte(byte[] PicBytes)
        {
            MemoryStream ms = new MemoryStream(PicBytes);
            ms.Position = 0;
            Image img = Image.FromStream(ms);
            ms.Close();
            return img;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getIdenCode();
        }

        private void buyBtn_Click(object sender, EventArgs e)
        {
            string productPage = auctionOperations.GetProductPage(buyUrl.Text.Trim(),cookiesCon, out ccReturned);

            cookiesCon.Add(ccReturned);

            string orderAddress = productPageHandle.GetProductActionAddress(productPage);

            string productPostData = "activity=&auction_type=b&auto_post1=&buyer_from=&chargeTypeId=&checkCodeIds=&current_price=185.00&frm=&from=item_detail&gmtCreate=&";

            productPostData = productPostData + "";
            
        }
    }
}
