﻿using Hipda.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media;

namespace Hipda.BackgroundTask
{
    

    public sealed class UpdateToastTask : IBackgroundTask
    {
        HttpHandle _httpClient = HttpHandle.GetInstance();
        string _formHash = string.Empty;

        

        

        

        

        async Task UpdateNoticeToastAsync(CancellationTokenSource cts)
        {
            Debug.WriteLine("====================== 执行了 UpdateNoticeToastAsync 开始");
            string url = string.Format("http://www.hi-pda.com/forum/notice.php?_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("feed"))?.ChildNodes;
            if (items == null)
            {
                Debug.WriteLine("====================== 执行了 UpdateNoticeToastAsync 结束1");
                return;
            }

            foreach (var item in items)
            {
                bool isNew = false;
                string userId = string.Empty;
                string username = string.Empty;
                string actionTime = string.Empty;
                string threadId = string.Empty;
                string threadTitle = string.Empty;
                string originalText = string.Empty;
                string actionText = string.Empty;
                string repostStr = string.Empty;
                string postId = string.Empty;

                HtmlNode userLinkNode;
                HtmlNode threadLinkNode;

                var divNode = item.ChildNodes[0];
                string nodeClass = divNode.Attributes[0].Value.Trim().ToLower();
                switch (nodeClass)
                {
                    case "f_quote":
                    case "f_reply":
                        isNew = divNode.ChildNodes[5].Name.Equals("img");
                        if (isNew)
                        {
                            userLinkNode = divNode.ChildNodes[0];
                            userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length).Split('&')[0];
                            username = userLinkNode.InnerText.Trim();

                            threadLinkNode = divNode.ChildNodes[2];
                            threadId = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/viewthread.php?from=notice&tid=".Length).Split('&')[0];
                            threadTitle = threadLinkNode.InnerText.Trim();

                            actionTime = divNode.ChildNodes[4].InnerText.Trim();

                            var actionContentNode = divNode.ChildNodes[7];
                            var buttonsNode = divNode.ChildNodes[9];

                            originalText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[0]
                                .InnerText.Trim()
                                .Replace("\r", " ")
                                .Replace("\n", " ");
                            actionText = actionContentNode.ChildNodes[0].ChildNodes[1].ChildNodes[2]
                                .InnerText.Trim()
                                .Replace("\r", " ")
                                .Replace("\n", " ")
                                .Replace(",", " ")
                                .Replace("\\\'", " ")
                                .Replace("\\\"", " ");

                            var replyLinkNode = buttonsNode.ChildNodes[0];
                            repostStr = replyLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/post.php?from=notice&action=reply&fid=2&tid=1778684&reppost=".Length).Split('&')[0];
                            var viewLinkNode = buttonsNode.ChildNodes[2];
                            postId = viewLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Split('&')[0];

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(0, actionTime);

                            // 发送
                            string noticeauthor = $"q|{userId}|{username}";
                            string noticetrimstr = $"[quote]{actionText}\r\n[size=2][color=#999999]{username} 发表于 {actionTime}[/color] [url=http://www.hi-pda.com/forum/redirect.php?goto=findpost__AND__pid={postId}__AND__ptid={threadId}][img]http://www.hi-pda.com/forum/images/common/back.gif[/img][/url][/size][/quote]\r\n    ";
                            string noticeauthormsg = actionText;
                            string _xmlForQuoteOrReply = @"<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>{0}</text>" +
                                        "<text>答复了您在《{1}》主题的贴子</text>" +
                                        "<image placement='appLogoOverride' src='{2}' hint-crop='circle'/>" +
                                        "<text>“ {3} ”</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "<actions>" +
                                    "<input id='inputPost' type='text' placeHolderContent='输入回复内容' />" +
                                    "<action content='查看' arguments='view_post={4},{5}' />" +
                                    "<action content='回复' arguments='reply_post={5},{6},{7},{8}' activationType='background' />" +
                                "</actions>" +
                                "</toast>";
                            _xmlForQuoteOrReply = string.Format(_xmlForQuoteOrReply, username, threadTitle, GetSmallAvatarUrlByUserId(Convert.ToInt32(userId)), actionText, postId, threadId, noticeauthor, noticetrimstr, noticeauthormsg);
                            SendToast(_xmlForQuoteOrReply);
                        }
                        break;
                    case "f_thread":
                        var nodes = divNode.ChildNodes;
                        isNew = nodes.Count(n => n.Name.Equals("img") && n.GetAttributeValue("alt", "").Equals("NEW")) == 1;
                        if (isNew)
                        {
                            var usernames = new List<string>();
                            var usernameNodes = nodes.Where(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("space.php?username="));
                            foreach (var n in usernameNodes)
                            {
                                usernames.Add(n.InnerText.Trim());
                            }
                            username = string.Join(",", usernames);

                            threadLinkNode = nodes.FirstOrDefault(n => n.Name.Equals("a") && n.Attributes[0].Value.StartsWith("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid="));
                            string linkUrlStr = threadLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/redirect.php?from=notice&goto=findpost&pid=".Length).Replace("ptid=", string.Empty);
                            string[] idsAry = linkUrlStr.Split('&');
                            postId = idsAry[0];
                            threadId = idsAry[1];
                            threadTitle = threadLinkNode.InnerText.Trim();
                            actionTime = nodes.FirstOrDefault(n => n.Name.Equals("em")).InnerText.Trim();

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(1, actionTime);

                            // 发送
                            string _xmlForThread = @"<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>{0}</text>" +
                                        "<text>回复了《{1}》</text>" +
                                    "</binding>" +
                                "</visual>" +
                                "<actions>" +
                                    "<action content='查看' arguments='view_post={2},{3}' />" +
                                "</actions>" +
                                "</toast>";
                            _xmlForThread = string.Format(_xmlForThread, username, threadTitle, postId, threadId);
                            SendToast(_xmlForThread);
                        }
                        break;
                    case "f_buddy":
                        isNew = divNode.ChildNodes[3].Name.Equals("img");
                        if (isNew)
                        {
                            userLinkNode = divNode.ChildNodes[0];
                            userId = userLinkNode.Attributes[0].Value.Substring("http://www.hi-pda.com/forum/space.php?from=notice&uid=".Length);
                            username = userLinkNode.InnerText.Trim();
                            actionTime = divNode.ChildNodes[2].InnerText.Trim();

                            // 保存数据，以便打开APP时还原“NEW”状态
                            SaveNoticeToastTempData(2, actionTime);

                            // 发送
                            string _xmlForBuddy = @"<toast>" +
                                "<visual>" +
                                    "<binding template='ToastGeneric'>" +
                                        "<text>{0}</text>" +
                                        "<text>添加您为好友</text>" +
                                        "<image placement='appLogoOverride' src='{1}' hint-crop='circle'/>" +
                                    "</binding>" +
                                "</visual>" +
                                "<actions>" +
                                    "<action content='加对方为好友' arguments='add_buddy={2},{3}' activationType='background' />" +
                                "</actions>" +
                                "</toast>";
                            _xmlForBuddy = string.Format(_xmlForBuddy, username, GetSmallAvatarUrlByUserId(Convert.ToInt32(userId)), userId, username);
                            SendToast(_xmlForBuddy);
                        }
                        break;
                }
            }

            Debug.WriteLine("====================== 执行了 UpdateNoticeToastAsync 结束2");
        }

        async void FirstRequestPm()
        {
            // 先请求一下，看看是否有新私信
            // 这是论坛的一种机制，否则APP过了一段时间之后不能从HTML中解析私人消息的数量
            await _httpClient.GetAsync(string.Format("http://www.hi-pda.com/forum/pm.php?checknewpm={0}&inajax=1&ajaxtarget=myprompt_check", DateTime.Now.Ticks.ToString("x")), new CancellationTokenSource());
        }

        async Task UpdatePmToastAsync(CancellationTokenSource cts)
        {
            Debug.WriteLine("====================== 执行了 UpdatePmToastAsync 开始");
            FirstRequestPm();

            // 读取数据
            string url = string.Format("http://www.hi-pda.com/forum/pm.php?filter=privatepm&_={0}", DateTime.Now.Ticks.ToString("x"));
            string htmlStr = await _httpClient.GetAsync(url, cts);

            // 实例化 HtmlAgilityPack.HtmlDocument 对象
            HtmlDocument doc = new HtmlDocument();

            // 载入HTML
            doc.LoadHtml(htmlStr);

            var items = doc.DocumentNode.Descendants().FirstOrDefault(n => n.Name.Equals("ul") && n.GetAttributeValue("class", "").Equals("pm_list"))?.ChildNodes;
            if (items == null)
            {
                Debug.WriteLine("====================== 执行了 UpdatePmToastAsync 结束1");
                return;
            }

            foreach (var item in items)
            {
                var isNew = item.ChildNodes[3].ChildNodes.Count >= 4 && item.ChildNodes[3].ChildNodes[3].Name.Equals("img");
                if (isNew)
                {
                    var citeNode = item.ChildNodes[3].ChildNodes[1];
                    var linkNode = citeNode.ChildNodes[0];
                    int userId = Convert.ToInt32(linkNode.Attributes[0].Value.Substring("space.php?uid=".Length).Split('&')[0]);
                    string username = linkNode.InnerText.Trim();
                    string lastMessageTime = item.ChildNodes[3].ChildNodes[2].InnerText.Trim();
                    string lastMessageText = item.ChildNodes[5].InnerText.Trim();

                    var flag = CheckIsExistedForPmToastTempData(userId);
                    if (flag == false)
                    {
                        // 保存数据，以便打开APP时还原“NEW”状态
                        SavePmToastTempData(userId);

                        string _xmlForPm = @"<toast>" +
                            "<visual>" +
                                "<binding template='ToastGeneric'>" +
                                    "<text>{0}</text>" +
                                    "<text>发来私信 “ {1} ”</text>" +
                                    "<image placement='appLogoOverride' src='{2}' hint-crop='circle'/>" +
                                "</binding>" +
                            "</visual>" +
                            "<actions>" +
                                "<input id='inputPm' type='text' placeHolderContent='输入回复内容' />" +
                                "<action content='查看' arguments='view_pm={3},{4}' />" +
                                "<action content='回复' arguments='reply_pm={3}' activationType='background' />" +
                            "</actions>" +
                            "</toast>";
                        _xmlForPm = string.Format(_xmlForPm, username, lastMessageText, GetSmallAvatarUrlByUserId(userId), userId, username);
                        SendToast(_xmlForPm);
                    }
                }
            }

            Debug.WriteLine("====================== 执行了 UpdatePmToastAsync 结束2");
        }

        void UpdateBadge()
        {
            Debug.WriteLine("更新 badge 数量 开始");
            int count = GetNoticeCountFromNoticeToastTempData();
            count += GetPmCountFromPmToastTempData();

            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());
            BadgeNotification badgeNotification = new BadgeNotification(badgeXml);
            BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            badgeUpdater.Update(badgeNotification);
            Debug.WriteLine(string.Format("更新 badge 数量 {0} 结束", count));
        }

        void SendToast(string toastXml)
        {
            toastXml = ReplaceHexadecimalSymbols(toastXml);
            toastXml = ReplaceEmojiLabel(toastXml);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(toastXml);

            // 创建通知实例
            var notification = new ToastNotification(xmlDoc);

            // 单击响应
            notification.Activated += OnNotification;

            // 显示通知
            var tn = ToastNotificationManager.CreateToastNotifier();
            tn.Show(notification);
        }

        void SaveNoticeToastTempData(int noticeType, string actionTime)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            // 保存数据，以便打开APP时还原“NEW”状态
            string toastData = string.Format("{0}#{1}", noticeType, actionTime);
            if (_container.Values.ContainsKey(_dataKey))
            {
                _container.Values[_dataKey] = _container.Values[_dataKey].ToString().Trim() + "," + toastData;
            }
            else
            {
                _container.Values[_dataKey] = toastData;
            }
        }

        int GetNoticeCountFromNoticeToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "NoticeToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        void SavePmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            // 保存数据，以便打开APP时还原“NEW”状态
            string toastData = userId.ToString();
            if (_container.Values.ContainsKey(_dataKey))
            {
                _container.Values[_dataKey] = _container.Values[_dataKey].ToString().Trim() + "," + toastData;
            }
            else
            {
                _container.Values[_dataKey] = toastData;
            }
        }

        int GetPmCountFromPmToastTempData()
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);
            if (_container.Values.ContainsKey(_dataKey))
            {
                return _container.Values[_dataKey].ToString().Split(',').ToList().Count(i => i.Length > 0);
            }

            return 0;
        }

        bool CheckIsExistedForPmToastTempData(int userId)
        {
            string _containerKey = "HIPDA";
            string _dataKey = "PmToastTempData";
            var _container = ApplicationData.Current.LocalSettings.CreateContainer(_containerKey, ApplicationDataCreateDisposition.Always);

            if (_container.Values.ContainsKey(_dataKey))
            {
                var list = _container.Values[_dataKey].ToString().Split(',').ToList();
                return list.Any(u => u.Equals(userId.ToString()));
            }

            return false;
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var cancel = new CancellationTokenSource();
            taskInstance.Canceled += (s, e) => 
            {
                cancel.Cancel();
                cancel.Dispose();
            };

            var deferral = taskInstance.GetDeferral();
            try
            {
                var cts = new CancellationTokenSource();
                await LoginAsync(cts); // 先登录
                if (cts.IsCancellationRequested) return;
                await UpdateNoticeToastAsync(cts);
                await UpdatePmToastAsync(cts);
                UpdateBadge();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("出错了：" + ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnNotification(ToastNotification sender, object args)
        {
            Debug.WriteLine("[Toast] in OnNotification");
        }



        private const string TileTemplateXml = @"<tile>
  <visual branding=""nameAndLogo"">
    <binding template=""TileMedium"" hint-textStacking=""center"">
      <image src=""{{0}}"" placement=""peek"" hint-crop=""circle""/>
      <text hint-style=""captionSubtle"" hint-align=""center"">{{1}}</text>
      <text hint-style=""captionSubtle"" hint-align=""center"">{{2}}</text>
    </binding>

    <binding template=""TileWide"">
      <group>
        <subgroup hint-weight=""33"">
          <image src=""{{0}}"" hint-crop=""circle"" />
        </subgroup>
        <subgroup hint-textStacking=""center"">
          <text hint-style=""subtitleSubtle"">{{1}}</text>
          <text hint-style=""subtitleSubtle"">{{2}}</text>
        </subgroup>
      </group>
    </binding>

    <binding template=""TileLarge"" hint-textStacking=""center"">
      <group>
        <subgroup hint-weight=""1""/>
        <subgroup hint-weight=""2"">
          <image src=""{{0}}"" hint-crop=""circle""/>
        </subgroup>
        <subgroup hint-weight=""1""/>
      </group>
      <text hint-style=""title"" hint-align=""center"">{{1}}</text>
      <text hint-style=""subtitleSubtle"" hint-align=""center"">{{2}}</text>
    </binding>
  </visual>
</tile>";

        //private void UpdatePrimaryTile(List<NoticeItemModel> promptData)
        //{
        //    if (promptData == null || !promptData.Any())
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        var updater = TileUpdateManager.CreateTileUpdaterForApplication();
        //        updater.EnableNotificationQueueForWide310x150(true);
        //        updater.EnableNotificationQueueForSquare150x150(true);
        //        updater.EnableNotificationQueueForSquare310x310(true);
        //        updater.EnableNotificationQueue(true);
        //        updater.Clear();

        //        foreach (var promptItem in promptData)
        //        {
        //            var doc = new XmlDocument();
        //            if (promptItem.NoticeType == NoticeType.QuoteOrReply)
        //            {
        //                var xml = string.Format(TileTemplateXml, GetSmallAvatarUrlByUserId(Convert.ToInt32(promptItem.ActionInfo[0])), promptItem.Username, promptItem.ActionInfo[4]);
        //                doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
        //                {
        //                    ProhibitDtd = false,
        //                    ValidateOnParse = false,
        //                    ElementContentWhiteSpace = false,
        //                    ResolveExternals = false
        //                });

        //                updater.Update(new TileNotification(doc));
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        // ignored
        //    }
        //}

        async Task LoginAsync(CancellationTokenSource cts)
        {
            var _container = ApplicationData.Current.RoamingSettings.CreateContainer("AccountData", ApplicationDataCreateDisposition.Always);
            foreach (var item in _container.Values)
            {
                var acc = JsonConvert.DeserializeObject<AccountItemModel>(item.Value.ToString());
                if (acc != null && acc.IsDefault)
                {
                    var postData = new List<KeyValuePair<string, object>>();
                    postData.Add(new KeyValuePair<string, object>("username", acc.Username));
                    postData.Add(new KeyValuePair<string, object>("password", acc.Password));
                    postData.Add(new KeyValuePair<string, object>("questionid", acc.QuestionId));
                    postData.Add(new KeyValuePair<string, object>("answer", acc.Answer));

                    string loginResultMessage = await _httpClient.PostAsync("http://www.hi-pda.com/forum/logging.php?action=login&loginsubmit=yes&inajax=1", postData, cts);
                    Debug.WriteLine(string.Format("登录结果：{0}", (loginResultMessage.Contains("欢迎") && !loginResultMessage.Contains("错误") && !loginResultMessage.Contains("失败") && !loginResultMessage.Contains("非激活"))));
                    break;
                }
            }
        }

        string GetSmallAvatarUrlByUserId(int userId)
        {
            var s = new int[10];
            for (int i = 0; i < s.Length - 1; ++i)
            {
                s[i] = userId % 10;
                userId = (userId - s[i]) / 10;
            }
            return "http://www.hi-pda.com/forum/uc_server/data/avatar/" + s[8] + s[7] + s[6] + "/" + s[5] + s[4] + "/" + s[3] + s[2] + "/" + s[1] + s[0] + "_avatar_small.jpg";
        }

        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        static Dictionary<string, string> EmojiDic = new Dictionary<string, string>
        {
            { ":grinning:",     "\uD83D\uDE00" },
            { ":grin:",         "\uD83D\uDE01" },
            { ":joy:",          "\uD83D\uDE02" },
            { ":smiley:",       "\uD83D\uDE03" },
            { ":smile:",        "\uD83D\uDE04" },
            { ":sweat_smile:",  "\uD83D\uDE05" },
            { ":laughing:",     "\uD83D\uDE06" },
            { ":innocent:",     "\uD83D\uDE07" },
            { ":smiling_imp:",  "\uD83D\uDE08" },
            { ":wink:",         "\uD83D\uDE09" },
            { ":blush:",        "\uD83D\uDE0A" },
            { ":yum:",          "\uD83D\uDE0B" },
            { ":satisfied:",    "\uD83D\uDE0C" },
            { ":heart_eyes:",   "\uD83D\uDE0D" },
            { ":sunglasses:",   "\uD83D\uDE0E" },
            { ":smirk:",        "\uD83D\uDE0F" },

            { ":neutral_face:",                 "\uD83D\uDE10" },
            { ":expressionless:",               "\uD83D\uDE11" },
            { ":unamused:",                     "\uD83D\uDE12" },
            { ":sweat:",                        "\uD83D\uDE13" },
            { ":pensive:",                      "\uD83D\uDE14" },
            { ":confused:",                     "\uD83D\uDE15" },
            { ":confounded:",                   "\uD83D\uDE16" },
            { ":kissing:",                      "\uD83D\uDE17" },
            { ":kissing_heart:",                "\uD83D\uDE18" },
            { ":kissing_smiling_eyes:",         "\uD83D\uDE19" },
            { ":kissing_closed_eyes:",          "\uD83D\uDE1A" },
            { ":stuck_out_tongue:",             "\uD83D\uDE1B" },
            { ":wink2:",                        "\uD83D\uDE1C" },
            { ":stuck_out_tongue_closed_eyes:", "\uD83D\uDE1D" },
            { ":disappointed:",                 "\uD83D\uDE1E" },
            { ":worried:",                      "\uD83D\uDE1F" },

            { ":angry:",            "\uD83D\uDE20" },
            { ":rage:",             "\uD83D\uDE21" },
            { ":cry:",              "\uD83D\uDE22" },
            { ":persevere:",        "\uD83D\uDE23" },
            { ":triumph:",          "\uD83D\uDE24" },
            { ":relieved:",         "\uD83D\uDE25" },
            { ":frowning:",         "\uD83D\uDE26" },
            { ":anguished:",        "\uD83D\uDE27" },
            { ":fearful:",          "\uD83D\uDE28" },
            { ":weary:",            "\uD83D\uDE29" },
            { ":sleepy:",           "\uD83D\uDE2A" },
            { ":tired_face:",       "\uD83D\uDE2B" },
            { ":grimacing:",        "\uD83D\uDE2C" },
            { ":sob:",              "\uD83D\uDE2D" },
            { ":open_mouth:",       "\uD83D\uDE2E" },
            { ":hushed:",           "\uD83D\uDE2F" },

            { ":cold_sweat:",       "\uD83D\uDE30" },
            { ":scream:",           "\uD83D\uDE31" },
            { ":astonished:",       "\uD83D\uDE32" },
            { ":flushed:",          "\uD83D\uDE33" },
            { ":sleeping:",         "\uD83D\uDE34" },
            { ":dizzy_face:",       "\uD83D\uDE35" },
            { ":no_mouth:",         "\uD83D\uDE36" },
            { ":mask:",             "\uD83D\uDE37" },
            { ":smile_cat:",        "\uD83D\uDE38" },
            { ":joy_cat:",          "\uD83D\uDE39" },
            { ":smiley_cat:",       "\uD83D\uDE3A" },
            { ":heart_eyes_cat:",   "\uD83D\uDE3B" },
            { ":smirk_cat:",        "\uD83D\uDE3C" },
            { ":kissing_cat:",      "\uD83D\uDE3D" },
            { ":pouting_cat:",      "\uD83D\uDE3E" },
            { ":crying_cat_face:",  "\uD83D\uDE3F" },

            { ":scream_cat:",               "\uD83D\uDE40" },
            //{ "",                           "\uD83D\uDE41" },
            //{ "",                           "\uD83D\uDE42" },
            //{ "",                           "\uD83D\uDE43" },
            //{ "",                           "\uD83D\uDE44" },
            { ":no_good:",                  "\uD83D\uDE45" },
            { ":man_ok:",                   "\uD83D\uDE46" },
            { ":bow:",                      "\uD83D\uDE47" },
            { ":see_no_evil:",              "\uD83D\uDE48" },
            { ":hear_no_evil:",             "\uD83D\uDE49" },
            { ":speak_no_evil:",            "\uD83D\uDE4A" },
            { ":raised_hand:",              "\uD83D\uDE4B" },
            { ":raised_hands:",             "\uD83D\uDE4C" },
            { ":person_frowning:",          "\uD83D\uDE4D" },
            { ":person_with_pouting_face:", "\uD83D\uDE4E" },
            { ":pray:",                     "\uD83D\uDE4F" },

            //{ "",                     "\uD83D\uDC40" },
            //{ "",                     "\uD83D\uDC41" },
            //{ "",                     "\uD83D\uDC42" },
            //{ "",                     "\uD83D\uDC43" },
            //{ "",                     "\uD83D\uDC44" },
            //{ "",                     "\uD83D\uDC45" },
            //{ "",                     "\uD83D\uDC46" },
            //{ "",                     "\uD83D\uDC47" },
            //{ "",                     "\uD83D\uDC48" },
            //{ "",                     "\uD83D\uDC49" },
            { ":punch:",        "\uD83D\uDC4A" },
            { ":wave:",         "\uD83D\uDC4B" },
            { ":hand_ok:",      "\uD83D\uDC4C" },
            { ":thumbsup:",     "\uD83D\uDC4D" },
            { ":thumbsdown:",   "\uD83D\uDC4E" },
            { ":clap:",         "\uD83D\uDC4F" },

            { ":pear:",         "\uD83C\uDF50" },
            { ":peach:",         "\uD83C\uDF51" },
            { ":cherries:",         "\uD83C\uDF52" },
            { ":strawberry:",         "\uD83C\uDF53" },
            { ":hamburger:",         "\uD83C\uDF54" },
            { ":pizza:",         "\uD83C\uDF55" },
            { ":meat_on_bone:",         "\uD83C\uDF56" },
            { ":poultry_leg:",         "\uD83C\uDF57" },
            { ":rice_cracker:",         "\uD83C\uDF58" },
            { ":rice_ball:",         "\uD83C\uDF59" },
            { ":rice:",         "\uD83C\uDF5A" },
            { ":curry:",         "\uD83C\uDF5B" },
            { ":ramen:",         "\uD83C\uDF5C" },
            { ":spaghetti:",         "\uD83C\uDF5D" },
            { ":bread:",         "\uD83C\uDF5E" },
            { ":fries:",         "\uD83C\uDF5F" },

            { ":ribbon:",         "\uD83C\uDF80" },
            { ":gift:",         "\uD83C\uDF81" },
            { ":birthday:",         "\uD83C\uDF82" },
            { ":jack_o_lantern:",         "\uD83C\uDF83" },
            { ":christmas_tree:",         "\uD83C\uDF84" },
            { ":santa:",         "\uD83C\uDF85" },
            { ":fireworks:",         "\uD83C\uDF86" },
            { ":sparkler:",         "\uD83C\uDF87" },
            { ":balloon:",         "\uD83C\uDF88" },
            { ":tada:",         "\uD83C\uDF89" },
            { ":confetti_ball:",         "\uD83C\uDF8A" },
            { ":tanabata_tree:",         "\uD83C\uDF8B" },
            { ":crossed_flags:",         "\uD83C\uDF8C" },
            { ":bamboo:",         "\uD83C\uDF8D" },
            { ":dolls:",         "\uD83C\uDF8E" },
            { ":flags:",         "\uD83C\uDF8F" },

            { ":skull:",         "\uD83D\uDC80" },
            { ":information_desk_person:",         "\uD83D\uDC81" },
            { ":guardsman:",         "\uD83D\uDC82" },
            { ":dancer:",         "\uD83D\uDC83" },
            { ":lipstick:",         "\uD83D\uDC84" },
            { ":nail_care:",         "\uD83D\uDC85" },
            { ":massage:",         "\uD83D\uDC86" },
            { ":haircut:",         "\uD83D\uDC87" },
            { ":barber:",         "\uD83D\uDC88" },
            { ":syringe:",         "\uD83D\uDC89" },
            { ":pill:",         "\uD83D\uDC8A" },
            { ":kiss:",         "\uD83D\uDC8B" },
            { ":love_letter:",         "\uD83D\uDC8C" },
            { ":ring:",         "\uD83D\uDC8D" },
            { ":gem:",         "\uD83D\uDC8E" },
            { ":couplekiss:",         "\uD83D\uDC8F" },

            { ":bouquet:",         "\uD83D\uDC90" },
            { ":couple_with_heart:",         "\uD83D\uDC91" },
            { ":wedding:",         "\uD83D\uDC92" },
            { ":heartbeat:",         "\uD83D\uDC93" },
            { ":broken_heart:",         "\uD83D\uDC94" },
            { ":two_hearts:",         "\uD83D\uDC95" },
            { ":sparkling_heart:",         "\uD83D\uDC96" },
            { ":heartpulse:",         "\uD83D\uDC97" },
            { ":cupid:",         "\uD83D\uDC98" },
            { ":blue_heart:",         "\uD83D\uDC99" },
            { ":green_heart:",         "\uD83D\uDC9A" },
            { ":yellow_heart:",         "\uD83D\uDC9B" },
            { ":purple_heart:",         "\uD83D\uDC9C" },
            { ":gift_heart:",         "\uD83D\uDC9D" },
            { ":revolving_hearts:",         "\uD83D\uDC9E" },
            { ":heart_decoration:",         "\uD83D\uDC9F" },

            { ":diamond_shape_with_a_dot_inside:",         "\uD83D\uDCA0" },
            { ":bulb:",         "\uD83D\uDCA1" },
            { ":anger:",         "\uD83D\uDCA2" },
            { ":bomb:",         "\uD83D\uDCA3" },
            { ":zzz:",         "\uD83D\uDCA4" },
            { ":boom:",         "\uD83D\uDCA5" },
            { ":sweat_drops:",         "\uD83D\uDCA6" },
            { ":droplet:",         "\uD83D\uDCA7" },
            { ":dash:",         "\uD83D\uDCA8" },
            { ":shit:",         "\uD83D\uDCA9" },
            { ":muscle:",         "\uD83D\uDCAA" },
            { ":dizzy:",         "\uD83D\uDCAB" },
            { ":speech_balloon:",         "\uD83D\uDCAC" },
            { ":thought_balloon:",         "\uD83D\uDCAD" },
            { ":white_flower:",         "\uD83D\uDCAE" },
            //{ "",         "\uD83D\uDCAF" },
        };

        static string ReplaceEmojiLabel(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return string.Empty;
            }

            var matchs = new Regex(@":[a-z_]{3,30}:").Matches(txt);
            if (matchs != null && matchs.Count > 0)
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    var m = matchs[i];

                    string emojiLabel = m.Groups[0].Value; // 要被替换的标签
                    if (EmojiDic.ContainsKey(emojiLabel))
                    {
                        txt = txt.Replace(emojiLabel, EmojiDic[emojiLabel]);
                    }
                }
            }

            return txt;
        }
    }
}
