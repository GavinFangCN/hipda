﻿using Hipda.Client.Uwp.Pro.Models;
using Hipda.Client.Uwp.Pro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipda.Client.Uwp.Pro.ViewModels
{
    public class FaceIconViewModel : NotificationObject
    {
        private List<FaceItemModel> _faceIcons;

        public List<FaceItemModel> FaceIcons
        {
            get { return _faceIcons; }
            set
            {
                _faceIcons = value;
                this.RaisePropertyChanged("FaceIcons");
            }
        }

        private List<EmojiItemModel> _emojiIcons = new List<EmojiItemModel>();

        public List<EmojiItemModel> EmojiIcons
        {
            get { return _emojiIcons; }
            set
            {
                _emojiIcons = value;
                this.RaisePropertyChanged("FaceIcons");
            }
        }

        public FaceIconViewModel()
        {
            GetFaceIconsData();
        }

        private void GetFaceIconsData()
        {
            FaceIcons = new List<FaceItemModel>
                {
                    new FaceItemModel { Image = "/Assets/Faces/default_smile.png", Text = ":)"},
                    new FaceItemModel { Image = "/Assets/Faces/default_sweat.png", Text = ":sweat:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_huffy.png", Text = ":huffy:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_cry.png", Text = ":cry:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_titter.png", Text = ":titter:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_handshake.png", Text = ":handshake:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_victory.png", Text = ":victory:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_curse.png", Text = ":curse:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_dizzy.png", Text = ":dizzy:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_shutup.png", Text = ":shutup:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_funk.png", Text = ":funk:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_loveliness.png", Text = ":loveliness:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_sad.png", Text = ":("},
                    new FaceItemModel { Image = "/Assets/Faces/default_biggrin.png", Text = ":D"},
                    new FaceItemModel { Image = "/Assets/Faces/default_cool.png", Text = ":cool:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_mad.png", Text = ":mad:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_shocked.png", Text = ":o"},
                    new FaceItemModel { Image = "/Assets/Faces/default_tongue.png", Text = ":P"},
                    new FaceItemModel { Image = "/Assets/Faces/default_lol.png", Text = ":lol:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_shy.png", Text = ":shy:"},
                    new FaceItemModel { Image = "/Assets/Faces/default_sleepy.png", Value = ":sleepy:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_01.png", Value = "{:2_41:}", Text = ":酷猴1:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_02.png", Value = "{:2_42:}", Text = ":酷猴2:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_03.png", Value = "{:2_43:}", Text = ":酷猴3:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_04.png", Value = "{:2_44:}", Text = ":酷猴4:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_05.png", Value = "{:2_45:}", Text = ":酷猴5:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_06.png", Value = "{:2_46:}", Text = ":酷猴6:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_07.png", Value = "{:2_47:}", Text = ":酷猴7:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_08.png", Value = "{:2_48:}", Text = ":酷猴8:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_09.png", Value = "{:2_49:}", Text = ":酷猴9:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_10.png", Value = "{:2_50:}", Text = ":酷猴10:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_11.png", Value = "{:2_51:}", Text = ":酷猴11:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_12.png", Value = "{:2_52:}", Text = ":酷猴12:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_13.png", Value = "{:2_53:}", Text = ":酷猴13:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_14.png", Value = "{:2_54:}", Text = ":酷猴14:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_15.png", Value = "{:2_55:}", Text = ":酷猴15:"},
                    new FaceItemModel { Image = "/Assets/Faces/coolmonkey_16.png", Value = "{:2_56:}", Text = ":酷猴16:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_01.png", Value = "{:3_57:}", Text = ":呆男1:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_02.png", Value = "{:3_58:}", Text = ":呆男2:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_03.png", Value = "{:3_59:}", Text = ":呆男3:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_04.png", Value = "{:3_60:}", Text = ":呆男4:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_05.png", Value = "{:3_61:}", Text = ":呆男5:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_06.png", Value = "{:3_62:}", Text = ":呆男6:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_07.png", Value = "{:3_63:}", Text = ":呆男7:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_08.png", Value = "{:3_64:}", Text = ":呆男8:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_09.png", Value = "{:3_65:}", Text = ":呆男9:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_10.png", Value = "{:3_66:}", Text = ":呆男10:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_11.png", Value = "{:3_67:}", Text = ":呆男11:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_12.png", Value = "{:3_68:}", Text = ":呆男12:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_13.png", Value = "{:3_69:}", Text = ":呆男13:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_14.png", Value = "{:3_70:}", Text = ":呆男14:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_15.png", Value = "{:3_71:}", Text = ":呆男15:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_16.png", Value = "{:3_72:}", Text = ":呆男16:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_17.png", Value = "{:3_73:}", Text = ":呆男17:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_18.png", Value = "{:3_74:}", Text = ":呆男18:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_19.png", Value = "{:3_75:}", Text = ":呆男19:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_20.png", Value = "{:3_76:}", Text = ":呆男20:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_21.png", Value = "{:3_77:}", Text = ":呆男21:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_22.png", Value = "{:3_78:}", Text = ":呆男22:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_23.png", Value = "{:3_79:}", Text = ":呆男23:"},
                    new FaceItemModel { Image = "/Assets/Faces/grapeman_24.png", Value = "{:3_80:}", Text = ":呆男24:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_01.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522171e4cb3f0eb26c192.png[/img]", Text = ":暴漫1:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_02.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221714976bc7d3962b5d.png[/img]", Text = ":暴漫2:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_03.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522178f275b517a688ada.png[/img]", Text = ":暴漫3:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_04.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522170e1d99081ff30ca5.png[/img]", Text = ":暴漫4:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_05.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217a3afef9be5dcd55b.png[/img]", Text = ":暴漫5:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_06.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173e67ecb8c2ec2a46.png[/img]", Text = ":暴漫6:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_07.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217bbbfd48bc66f8f4a.png[/img]", Text = ":暴漫7:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_08.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173fd492dfd142a3d8.png[/img]", Text = ":暴漫8:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_09.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217dbce21054c18bbbf.png[/img]", Text = ":暴漫9:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_10.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217bb45c300842334b2.png[/img]", Text = ":暴漫10:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_11.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217832ca03e9c8dd244.png[/img]", Text = ":暴漫11:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_12.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217228e48fb9809157b.png[/img]", Text = ":暴漫12:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_13.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217d29ee7ecdccfdf84.png[/img]", Text = ":暴漫13:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_14.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221793d8be3e189f26d1.png[/img]", Text = ":暴漫14:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_15.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217e4db9fd2eae3c4c2.png[/img]", Text = ":暴漫15:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_16.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522177803b63696b5a22d.png[/img]", Text = ":暴漫16:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_17.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217715e829390952912.png[/img]", Text = ":暴漫17:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_18.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522170cab30ad86198afe.png[/img]", Text = ":暴漫18:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_19.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172c6e90bdbd2df03e.png[/img]", Text = ":暴漫19:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_20.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522173ed44c087d951af1.png[/img]", Text = ":暴漫20:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_21.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522171757582bc9aefe54.png[/img]", Text = ":暴漫21:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_22.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/1407152217a1934c5fda807e15.png[/img]", Text = ":暴漫22:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_23.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172c2407de0b4feaac.png[/img]", Text = ":暴漫23:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_24.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/140715221783463d4d07bdb661.png[/img]", Text = ":暴漫24:"},
                    new FaceItemModel { Image = "/Assets/Faces/baoman_25.png", Value = "[img=100,0]http://www.hi-pda.com/forum/attachments/day_140715/14071522172bd4de9ae043edab.png[/img]", Text = ":暴漫25:"}
                };

            foreach (var pair in Common.EmojiDic)
            {
                EmojiIcons.Add(new EmojiItemModel { Label = pair.Key, Value = pair.Value });
            }
        }
    }
}