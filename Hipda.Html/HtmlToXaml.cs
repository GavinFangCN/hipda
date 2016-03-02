﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hipda.Html
{
    public static class HtmlToXaml
    {
        /// <summary>
        /// 过滤掉不能出现在XML中的字符
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        public static string ConvertPost(int threadId, string htmlContent, Dictionary<int, string[]> postDic, int maxImageCount, ref int imageCount)
        {
            //string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            //if (deviceFamily.Equals("Windows.Mobile"))
            //{

            //}
            //else
            //{

            //}

            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace(@"<div class=""postattachlist"">", "↵");
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);

            // 移除无用的图片附加信息
            var matchsForInvalidHtml1 = new Regex(@"<div class=""t_attach"".*\n.*\n.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml1 != null && matchsForInvalidHtml1.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml1.Count; i++)
                {
                    var m = matchsForInvalidHtml1[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, string.Empty);
                }
            }

            // 移除无用的图片附加信息2
            var matchsForInvalidHtml2 = new Regex(@"<p class=""imgtitle"">\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*\n*.*").Matches(htmlContent);
            if (matchsForInvalidHtml2 != null && matchsForInvalidHtml2.Count > 0)
            {
                for (int i = 0; i < matchsForInvalidHtml2.Count; i++)
                {
                    var m = matchsForInvalidHtml2[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, string.Empty);
                }
            }

            // 替换引用链接为按钮
            var matchsForRefLink = new Regex(@"<a\s+href=""http:\/\/(www|cnc)\.hi\-pda\.com\/forum\/redirect\.php\?goto\=findpost&amp;pid\=(\d*)&amp;ptid\=(\d*)[^\""]*""[^>]*>(<img\s[^>]*>|\d+#)</a>").Matches(htmlContent);
            if (matchsForRefLink != null && matchsForRefLink.Count > 0)
            {
                for (int i = 0; i < matchsForRefLink.Count; i++)
                {
                    var m = matchsForRefLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string postIdStr = m.Groups[2].Value;
                    string threadIdStr = m.Groups[3].Value;
                    string linkContent = m.Groups[4].Value;

                    string linkXaml = string.Empty;
                    if (linkContent.StartsWith("<img"))
                    {
                        linkXaml = string.Format(@"[InlineUIContainer][c:MyRefLink2 PostId=""{0}"" ThreadId=""{1}""/][/InlineUIContainer]", postIdStr, threadIdStr);
                    }
                    else
                    {
                        linkXaml = string.Format(@"[InlineUIContainer][c:MyRefLink1 PostId=""{0}"" ThreadId=""{1}"" LinkContent=""{2}""/][/InlineUIContainer]", postIdStr, threadIdStr, linkContent);
                    }

                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换站内链接为按钮
            var matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>(.*)</a>").Matches(htmlContent);
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                for (int i = 0; i < matchsForMyLink.Count; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;
                    linkContent = Regex.Replace(linkContent, @"<[^>]*>", string.Empty);

                    string linkXaml = string.Format(@"[InlineUIContainer][c:MyLink ThreadId=""{0}"" LinkContent=""{1}""/][/InlineUIContainer]", threadIdStr, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换链接
            var matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换小字
            var matchsForSmallFont = new Regex(@"<font size=""1"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForSmallFont != null && matchsForSmallFont.Count > 0)
            {
                for (int i = 0; i < matchsForSmallFont.Count; i++)
                {
                    var m = matchsForSmallFont[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string fontText = m.Groups[1].Value;

                    string fontXaml = string.Format(@"[Span FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}""]{0}[/Span]", fontText);
                    htmlContent = htmlContent.Replace(placeHolder, fontXaml);
                }
            }

            // 替换带颜色的font标签
            var matchsForColorText = new Regex(@"<font color=""([#0-9a-zA-Z]*)"">([^<]*)</font>").Matches(htmlContent);
            if (matchsForColorText != null && matchsForColorText.Count > 0)
            {
                for (int i = 0; i < matchsForColorText.Count; i++)
                {
                    var m = matchsForColorText[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string colorName = m.Groups[1].Value.ToLower().Trim();
                    string textContent = m.Groups[2].Value;

                    string infoXaml = string.Format(@"[Span Foreground=""{1}""]{0}[/Span]", textContent, colorName);
                    if (colorName.Equals("#000") || colorName.Equals("#000000") || colorName.Equals("black") || (colorName.StartsWith("#") && colorName.Length != 4 && colorName.Length != 7))
                    {
                        infoXaml = string.Format(@"[Span]{0}[/Span]", textContent);
                    }
                    htmlContent = htmlContent.Replace(placeHolder, infoXaml);
                }
            }

            // 替换"最后编辑时间"
            var matchsForLastEditInfo = new Regex(@"<i class=""pstatus"">(.*)</i>").Matches(htmlContent);
            if (matchsForLastEditInfo != null && matchsForLastEditInfo.Count > 0)
            {
                for (int i = 0; i < matchsForLastEditInfo.Count; i++)
                {
                    var m = matchsForLastEditInfo[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string infoContent = m.Groups[1].Value.Trim();

                    string infoXaml = string.Format(@"[Run Text=""{0}"" Foreground=""{{ThemeResource SystemControlForegroundBaseLowBrush}}"" FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}""/][LineBreak/]", infoContent);
                    htmlContent = htmlContent.Replace(placeHolder, infoXaml);
                }
            }

            // 替换"引用"
            var matchsForQuote = new Regex(@"<blockquote>([\s\S]*?)<\/blockquote>").Matches(htmlContent);
            if (matchsForQuote != null && matchsForQuote.Count > 0)
            {
                for (int i = 0; i < matchsForQuote.Count; i++)
                {
                    var m = matchsForQuote[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string quoteGrid = ConvertQuote(m.Groups[1].Value.Trim(), postDic);
                    quoteGrid = $@"[/Paragraph][/RichTextBlock]{quoteGrid}[RichTextBlock xml:space=""preserve"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}""][Paragraph]";
                    htmlContent = htmlContent.Replace(placeHolder, quoteGrid);
                }
            }

            // 移除无意义图片HTML
            htmlContent = htmlContent.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            htmlContent = htmlContent.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

            #region 解析图片
            // 站内上载的图片，通过file属性解析
            MatchCollection matchsForImage1 = new Regex(@"<img[^>]*file=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage1 != null && matchsForImage1.Count > 0)
            {
                for (int i = 0; i < matchsForImage1.Count; i++)
                {
                    string placeHolderLabel = matchsForImage1[i].Groups[0].Value; // 要被替换的元素
                    string imgUrl = matchsForImage1[i].Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][c:MyImage FolderName=""{0}"" Url=""{1}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
                }
            }

            // 其他图片，通过src属性解析
            MatchCollection matchsForImage2 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage2 != null && matchsForImage2.Count > 0)
            {
                for (int i = 0; i < matchsForImage2.Count; i++)
                {
                    var m = matchsForImage2[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][c:MyImage FolderName=""{0}"" Url=""{1}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, threadId, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");

            string xamlStr = $@"<RichTextBlock xml:space=""preserve"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}""><Paragraph>{htmlContent}</Paragraph></RichTextBlock>";
            xamlStr = $@"<StackPanel xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:c=""using:Hipda.Client.Uwp.Pro.Controls"">{xamlStr}</StackPanel>";
            xamlStr = xamlStr.Replace("<Paragraph>\r\n", "<Paragraph>");
            xamlStr = xamlStr.Replace("<LineBreak/>\r\n</Paragraph>", "</Paragraph>");
            xamlStr = xamlStr.Replace("<RichTextBlock xml:space=\"preserve\" LineHeight=\"{Binding LineHeight,Source={StaticResource MyLocalSettings}}\"><Paragraph></Paragraph></RichTextBlock>", string.Empty);
            xamlStr = xamlStr.Replace("<RichTextBlock xml:space=\"preserve\" LineHeight=\"{Binding LineHeight,Source={StaticResource MyLocalSettings}}\"><Paragraph>\r\n</Paragraph></RichTextBlock>", string.Empty);
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        public static string ConvertQuote(string htmlContent, Dictionary<int, string[]> postDic)
        {
            string quoteContent, quoteInfo;
            int postId = 0;

            try
            {
                int i = htmlContent.IndexOf("<font size=\"2\">");
                if (i == -1)
                {
                    return ConvertQuote2(htmlContent);
                }

                quoteContent = htmlContent.Substring(0, i).Trim();
                quoteContent = new Regex("<[^>]*>").Replace(quoteContent, string.Empty);

                quoteInfo = htmlContent.Substring(i).Trim();

                // 获取"引用"中的 PostId
                var matchsForPostId = new Regex("PostId=\\\"([0-9]+)\\\"").Matches(quoteInfo);
                if (matchsForPostId != null && matchsForPostId.Count == 1)
                {
                    int.TryParse(matchsForPostId[0].Groups[1].ToString(), out postId);
                }
                else
                {
                    matchsForPostId = new Regex("pid=([0-9]+)").Matches(quoteInfo);
                    if (matchsForPostId != null && matchsForPostId.Count == 1)
                    {
                        int.TryParse(matchsForPostId[0].Groups[1].ToString(), out postId);
                    }
                }

                if (postId == 0)
                {
                    return ConvertQuote2(htmlContent);
                }
            }
            catch (Exception)
            {
                throw new Exception(htmlContent);
            }

            // 对于已屏蔽的用户，则连引用也不显示
            if (!postDic.ContainsKey(postId))
            {
                return string.Empty;
            }

            string[] ary = postDic[postId];
            string authorUserIdStr = ary[0];
            string authorUsername = ary[1];
            string floorNoStr = ary[2];
            string forumIdStr = ary[3];
            string forumName = ary[4];

            string xamlStr = $"<c:MyAvatarForReply Margin='-38,0,0,0' MyWidth='30' UserId='{authorUserIdStr}' Username='{authorUsername}' ForumId='{forumIdStr}' ForumName='{forumName}' HorizontalAlignment='Left' VerticalAlignment='Top'/><ContentControl Style='{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToContentControlForeground2StyleConverter}}}}'><RichTextBlock><Paragraph>{authorUsername}</Paragraph><Paragraph>{quoteContent}</Paragraph></RichTextBlock></ContentControl><TextBlock Text='{floorNoStr}' HorizontalAlignment='Right' VerticalAlignment='Top'/>";
            xamlStr = $@"<Grid Margin=""8"" Padding=""46,8,8,8"" MinWidth=""200"" Background=""{{ThemeResource SystemListLowColor}}"" BorderThickness=""1,0,0,0"" BorderBrush=""{{ThemeResource SystemControlBackgroundAccentBrush}}"">{xamlStr}</Grid>";
            xamlStr = xamlStr.Replace("<", "[").Replace(">", "]");
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        public static string ConvertQuote2(string htmlContent)
        {
            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty);
            string xamlStr = $"<ContentControl Style='{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToContentControlForeground2StyleConverter}}}}'><RichTextBlock><Paragraph>{htmlContent}</Paragraph></RichTextBlock></ContentControl>";
            xamlStr = $@"<Grid Margin=""8"" Padding=""8"" Background=""{{ThemeResource SystemListLowColor}}"" BorderThickness=""1,0,0,0"" BorderBrush=""{{ThemeResource SystemControlBackgroundAccentBrush}}"">{xamlStr}</Grid>";
            xamlStr = xamlStr.Replace("<", "[").Replace(">", "]");
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        /// <summary>
        /// 将普通字符串转换为正则表达式字符串
        /// </summary>
        /// <param name="str">普通字符串</param>
        /// <returns>正则表达式字符串</returns>
        private static string StringToRegexPattern(string str)
        {
            return str
                .Replace("^", @"\^")
                .Replace("$", @"\$")
                .Replace("|", @"\|")
                .Replace("*", @"\*")
                .Replace("+", @"\+")
                .Replace("?", @"\?")
                .Replace("[", @"\[")
                .Replace("]", @"\]")
                .Replace("(", @"\(")
                .Replace(")", @"\)");
        }

        public static string ConvertUserInfo(string htmlContent)
        {
            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);
            htmlContent = htmlContent.Replace(@"<h3 class=""blocktitle lightlink"">", @"[LineBreak/][Bold Foreground=""{ThemeResource SystemControlBackgroundAccentBrush}""]");
            htmlContent = htmlContent.Replace("</h3>", "[/Bold][LineBreak/]");
            htmlContent = htmlContent.Replace(@"<img src=""images/default/online_buddy.gif"" title=""当前在线"" class=""online_buddy"">", " \uD83D\uDCA1 "); // 当前在线小图标
            htmlContent = htmlContent.Replace(@"<img src=""images/rank/seller/0.gif"" border=""0"" class=""absmiddle"">", " \uD83D\uDC94 "); // 买家信用小图标
            htmlContent = htmlContent.Replace(@"<img src=""images/rank/buyer/0.gif"" border=""0"" class=""absmiddle"">", " \uD83D\uDC95 "); // 卖家信用小图标

            // 星星小图标
            MatchCollection matchsForStarImage = new Regex(@"<img src=""images/default/star_level1.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForStarImage != null && matchsForStarImage.Count > 0)
            {
                for (int i = 0; i < matchsForStarImage.Count; i++)
                {
                    var m = matchsForStarImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF20");
                }
            }

            // 月亮小图标
            MatchCollection matchsForMoonImage = new Regex(@"<img src=""images/default/star_level2.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForMoonImage != null && matchsForMoonImage.Count > 0)
            {
                for (int i = 0; i < matchsForMoonImage.Count; i++)
                {
                    var m = matchsForMoonImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF19");
                }
            }

            // 太阳小图标
            MatchCollection matchsForSunImage = new Regex(@"<img src=""images/default/star_level3.gif"" alt=""Rank: [0-9]*"">").Matches(htmlContent);
            if (matchsForSunImage != null && matchsForSunImage.Count > 0)
            {
                for (int i = 0; i < matchsForSunImage.Count; i++)
                {
                    var m = matchsForSunImage[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    htmlContent = htmlContent.Replace(placeHolder, "\uD83C\uDF1E");
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = $@"[Hyperlink NavigateUri=""{linkUrl}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}""]{linkContent}[/Hyperlink]";
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 移除无意义图片HTML
            htmlContent = htmlContent.Replace(@"src=""images/default/attachimg.gif""", string.Empty);
            htmlContent = htmlContent.Replace(@"src=""http://www.hi-pda.com/forum/images/default/attachimg.gif""", string.Empty);

            #region 解析图片
            // 图片，通过src属性解析
            MatchCollection matchsForImage = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage != null && matchsForImage.Count > 0)
            {
                for (int i = 0; i < matchsForImage.Count; i++)
                {
                    var m = matchsForImage[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][c:MyImage FolderName=""0"" Url=""{0}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");

            string xamlStr = string.Format(@"<RichTextBlock xml:space=""preserve"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" LineHeight=""{{Binding LineHeight,Source={{StaticResource MyLocalSettings}}}}"" xmlns:c=""using:Hipda.Client.Uwp.Pro.Controls""><Paragraph>{0}</Paragraph></RichTextBlock>", htmlContent);
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        public static string ConvertUserMessage(string htmlContent)
        {
            htmlContent = htmlContent.Replace("[", "&#8968;");
            htmlContent = htmlContent.Replace("]", "&#8971;");
            htmlContent = htmlContent.Replace("&nbsp;", " ");
            htmlContent = htmlContent.Replace("↵", "&#8629;");
            htmlContent = htmlContent.Replace("<strong>", string.Empty);
            htmlContent = htmlContent.Replace("</strong>", string.Empty);
            htmlContent = htmlContent.Replace("<br/>", "↵"); // ↵符号表示换行符
            htmlContent = htmlContent.Replace("<br />", "↵");
            htmlContent = htmlContent.Replace("<br>", "↵");
            htmlContent = htmlContent.Replace("</div>", "↵");
            htmlContent = htmlContent.Replace("</p>", "↵");
            htmlContent = htmlContent.Replace("</ul>", "↵");
            htmlContent = htmlContent.Replace("</li>", "↵");
            htmlContent = htmlContent.Replace("</td>", "↵");
            htmlContent = Regex.Replace(htmlContent, @"<span[^>]*>", string.Empty);
            htmlContent = Regex.Replace(htmlContent, @"</span>", string.Empty);

            // 替换站内链接为按钮
            MatchCollection matchsForMyLink = new Regex(@"<a\s+href=""http:\/\/www\.hi\-pda\.com\/forum\/viewthread\.php\?[^>]*&?tid\=(\d*)[^\""]*""[^>]*>(.*)</a>").Matches(htmlContent);
            if (matchsForMyLink != null && matchsForMyLink.Count > 0)
            {
                for (int i = 0; i < matchsForMyLink.Count; i++)
                {
                    var m = matchsForMyLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string threadIdStr = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;
                    linkContent = Regex.Replace(linkContent, @"<[^>]*>", string.Empty);

                    string linkXaml = string.Format(@"[InlineUIContainer][c:MyLink ThreadId=""{0}"" LinkContent=""{1}""/][/InlineUIContainer]", threadIdStr, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            // 替换链接
            MatchCollection matchsForLink = new Regex(@"<a\s+href=""([^""]*)""[^>]*>([^<#]*)</a>").Matches(htmlContent);
            if (matchsForLink != null && matchsForLink.Count > 0)
            {
                for (int i = 0; i < matchsForLink.Count; i++)
                {
                    var m = matchsForLink[i];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string linkUrl = m.Groups[1].Value;
                    string linkContent = m.Groups[2].Value;

                    if (!linkUrl.Contains(":"))
                    {
                        linkUrl = string.Format("http://www.hi-pda.com/forum/{0}", linkUrl);
                    }
                    string linkXaml = string.Format(@"[Hyperlink NavigateUri=""{0}"" Foreground=""DodgerBlue""]{1}[/Hyperlink]", linkUrl, linkContent);
                    htmlContent = htmlContent.Replace(placeHolder, linkXaml);
                }
            }

            #region 解析图片
            // 图片，通过src属性解析
            MatchCollection matchsForImage2 = new Regex(@"<img[^>]*src=""([^""]*)""[^>]*>").Matches(htmlContent);
            if (matchsForImage2 != null && matchsForImage2.Count > 0)
            {
                for (int i = 0; i < matchsForImage2.Count; i++)
                {
                    var m = matchsForImage2[i];
                    string placeHolderLabel = m.Groups[0].Value; // 要被替换的元素
                    string imgUrl = m.Groups[1].Value; // 图片URL
                    if (!imgUrl.StartsWith("http")) imgUrl = "http://www.hi-pda.com/forum/" + imgUrl;

                    string imgXaml = @"[InlineUIContainer][c:MyImage FolderName=""0"" Url=""{0}""/][/InlineUIContainer]";
                    imgXaml = string.Format(imgXaml, imgUrl);

                    htmlContent = htmlContent.Replace(placeHolderLabel, imgXaml);
                }
            }
            #endregion

            htmlContent = new Regex("<[^>]*>").Replace(htmlContent, string.Empty); // 移除所有HTML标签
            htmlContent = htmlContent.Replace("\r\n", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\r", string.Empty); // 忽略源换行
            htmlContent = htmlContent.Replace("\n", string.Empty); // 忽略源换行
            htmlContent = new Regex(@"↵{1,}").Replace(htmlContent, "↵"); // 将多个换行符合并成一个
            htmlContent = new Regex(@"^↵").Replace(htmlContent, string.Empty); // 移除行首的换行符
            htmlContent = new Regex(@"↵$").Replace(htmlContent, string.Empty); // 移除行末的换行符
            htmlContent = htmlContent.Replace("↵", "\r\n"); // 解析换行符
            htmlContent = htmlContent.Replace("[", "<");
            htmlContent = htmlContent.Replace("]", ">");

            string xamlStr = string.Format(@"<RichTextBlock xml:space=""preserve"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:c=""using:Hipda.Client.Uwp.Pro.Controls""><Paragraph>{0}</Paragraph></RichTextBlock>", htmlContent);
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        public static string ConvertSearchThreadTitle(string title, string forumName, string imageFontIcon, string fileFontIcon, string viewInfo)
        {
            string xamlStr = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToTextBlockForeground1StyleConverter}}}}"" TextWrapping=""Wrap"">[{4}] {0} <Run FontFamily=""Segoe MDL2 Assets"" Foreground=""OrangeRed"" Text=""{1}"" /> <Run FontFamily=""Segoe MDL2 Assets"" Foreground=""DeepSkyBlue"" Text=""{2}"" /> <Run Text=""{3}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /></TextBlock>";

            MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(title);
            if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
            {
                for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                {
                    var m = matchsForSearchKeywords[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string k = m.Groups[1].Value;

                    string linkXaml = string.Format(@"<Run Foreground=""Red"">{0}</Run>", k);
                    title = title.Replace(placeHolder, linkXaml);
                }
            }

            xamlStr = string.Format(xamlStr, title, imageFontIcon, fileFontIcon, viewInfo, forumName);
            return ReplaceHexadecimalSymbols(xamlStr);
        }

        public static string ConvertSearchResultSummary(string titleHtml, string forumName, string searchResultSummaryHtml, string viewInfo)
        {
            string searchResultHtml = string.Format(@"<Run>{0}</Run> <Run Text=""{1}"" Foreground=""{{ThemeResource SystemControlBackgroundAccentBrush}}"" /><LineBreak/><Span FontStyle=""Italic"" Foreground=""{{ThemeResource SystemControlForegroundBaseMediumLowBrush}}"" FontSize=""{{Binding FontSize2,Source={{StaticResource MyLocalSettings}}}}""><Run>{2}</Run></Span>", titleHtml, viewInfo, searchResultSummaryHtml);
            searchResultHtml = searchResultHtml.Replace("&", "&amp;")
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            string xamlStr = @"<TextBlock xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Style=""{{Binding FontContrastRatio,Source={{StaticResource MyLocalSettings}},Converter={{StaticResource FontContrastRatioToTextBlockForeground1StyleConverter}}}}"" TextWrapping=""Wrap"">[{1}] {0}</TextBlock>";

            MatchCollection matchsForInvalidStr = new Regex(@"\[[^\]]*\]").Matches(searchResultHtml);
            if (matchsForInvalidStr != null && matchsForInvalidStr.Count > 0)
            {
                for (int j = 0; j < matchsForInvalidStr.Count; j++)
                {
                    var m = matchsForInvalidStr[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    searchResultHtml = searchResultHtml.Replace(placeHolder, " ");
                }
            }

            MatchCollection matchsForSearchKeywords = new Regex(@"<em style=""color:red;"">([^>#]*)</em>").Matches(searchResultHtml);
            if (matchsForSearchKeywords != null && matchsForSearchKeywords.Count > 0)
            {
                for (int j = 0; j < matchsForSearchKeywords.Count; j++)
                {
                    var m = matchsForSearchKeywords[j];

                    string placeHolder = m.Groups[0].Value; // 要被替换的元素
                    string k = m.Groups[1].Value;

                    string linkXaml = string.Format(@"</Run><Run Foreground=""Red"">{0}</Run><Run>", k);
                    searchResultHtml = searchResultHtml.Replace(placeHolder, linkXaml);
                }
            }

            xamlStr = string.Format(xamlStr, searchResultHtml, forumName);
            return ReplaceHexadecimalSymbols(xamlStr);
        }
    }
}
