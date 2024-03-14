using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTGP
{
    public class page
    {
        private int PageId;
        public int pageId
        {
            get { return PageId; }
            set { PageId = value; }
        }
        private string Resolution;
        public string resolution
        {
            get { return Resolution; }
            set { Resolution = value; }
        }
        private string BgColor;
        public string bgColor
        {
            get { return BgColor; }
            set { BgColor = value; }
        }
        private string BgImg;
        public string bgImg
        {
            get { return BgImg; }
            set { BgImg = value; }
        }
        private string EqType;
        public string eqType
        {
            get { return EqType; }
            set { EqType = value; }
        }
        private int PageTime;
        public int pageTime
        {
            get { return PageTime; }
            set { PageTime = value; }
        }
        private string Guid;
        public string guid
        {
            get { return Guid; }
            set { Guid = value; }
        }
    }

    /// <summary>
    /// 字幕区域
    /// </summary>
    public class TextArea
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
        private int Type;
        public int type
        {
            get { return Type; }
            set { Type = value; }
        }
        private int BorderEffect;
        public int borderEffect
        {
            get { return BorderEffect; }
            set { BorderEffect = value; }
        }
        private int BorderSW;
        public int borderSW
        {
            get { return BorderSW; }
            set { BorderSW = value; }
        }
        private int BorderSpeed;
        public int borderSpeed
        {
            get { return BorderSpeed; }
            set { BorderSpeed = value; }
        }
        private int BorderType;
        public int borderType
        {
            get { return BorderType; }
            set { BorderType = value; }
        }
        private int Left;
        public int left
        {
            get { return Left; }
            set { Left = value; }
        }
        private int Top;
        public int top
        {
            get { return Top; }
            set { Top = value; }
        }
        private int Width;
        public int width
        {
            get { return Width; }
            set { Width = value; }
        }
        private int Height;
        public int height
        {
            get { return Height; }
            set { Height = value; }
        }
        

        private string Background;
        public string background
        {
            get { return Background; }
            set { Background = value; }
        }

        private string FontColor;
        public string fontColor
        {
            get { return FontColor; }
            set { FontColor = value; }
        }

        private string FontFamily;

        public string fontFamily
        {
            get { return FontFamily; }
            set { FontFamily = value; }
        }

        private int FontSize;
        public int fontSize
        {
            get { return FontSize; }
            set { FontSize = value; }
        }

        private int Italic;
        public int italic
        {
            get { return Italic; }
            set { Italic = value; }
        }

        private int Bold;
        public int bold
        {
            get { return Bold; }
            set { Bold = value; }
        }

        private int TextLine;
        public int textLine
        {
            get { return TextLine; }
            set { TextLine = value; }
        }

        private int LineHeight;
        public int lineHeight
        {
            get { return LineHeight; }
            set { LineHeight = value; }
        }



        private int PauseTime;
        public int pauseTime
        {
            get { return PauseTime; }
            set { PauseTime = value; }
        }
        private int ScrollSpeed;
        public int scrollSpeed
        {
            get { return ScrollSpeed; }
            set { ScrollSpeed = value; }
        }
        private int SiderType;
        public int siderType
        {
            get { return SiderType; }
            set { SiderType = value; }
        }

        private int TextAlign;
        public int textAlign
        {
            get { return TextAlign; }
            set { TextAlign = value; }
        }

        private string Text;
        public string text
        {
            get { return Text; }
            set { Text = value; }
        }


        private int TextShow;
        public int textShow
        {
            get { return TextShow; }
            set { TextShow = value; }
        }

        private int ColorEffect;
        public int colorEffect
        {
            get { return ColorEffect; }
            set { ColorEffect = value; }
        }

        private int TextTop;
        public int textTop
        {
            get { return TextTop; }
            set { TextTop = value; }
        }
        public int rotation { set; get; }

        private int ZIndex;
        public int zIndex
        {
            get { return ZIndex; }
            set { ZIndex = value; }
        }        
    }

    public class PicArea
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
        private int Type;
        public int type
        {
            get { return Type; }
            set { Type = value; }
        }
        private int BorderEffect;
        public int borderEffect
        {
            get { return BorderEffect; }
            set { BorderEffect = value; }
        }
        private int BorderSW;
        public int borderSW
        {
            get { return BorderSW; }
            set { BorderSW = value; }
        }
        private int BorderSpeed;
        public int borderSpeed
        {
            get { return BorderSpeed; }
            set { BorderSpeed = value; }
        }
        private int BorderType;
        public int borderType
        {
            get { return BorderType; }
            set { BorderType = value; }
        }
        private int Left;
        public int left
        {
            get { return Left; }
            set { Left = value; }
        }
        private int Top;
        public int top
        {
            get { return Top; }
            set { Top = value; }
        }
        private int Width;
        public int width
        {
            get { return Width; }
            set { Width = value; }
        }
        private int Height;
        public int height
        {
            get { return Height; }
            set { Height = value; }
        }
        private int PauseTime;
        public int pauseTime
        {
           get { return PauseTime; }
           set { PauseTime = value; }
        }
        private int Rotation;
        public int rotation
        {
            get { return Rotation; }
            set { Rotation = value; }
        }
        //老版本的特效模式
        //private int SiderType;
        //public int siderType
        //{
        //    get { return SiderType; }
        //    set { SiderType = value; }
        //}

        //新版本的特效模式
        public int picStyleIn { set; get; }
        public int picStyleInTime { set; get; }
        public int picStyleOut { set; get; }
        public int picStyleOutTime { set; get; }



        private int PicScaleType;
        public int picScaleType
        {
            get { return PicScaleType; }
            set { PicScaleType = value; }
        }


        private List<SrcGroup> srcgroup;
        public List<SrcGroup> srcGroup
        {
            get { return srcgroup; }
            set { srcgroup = value; }
        }
        private int ZIndex;
        public int zIndex
        {
            get { return ZIndex; }
            set { ZIndex = value; }
        }
        public class SrcGroup
        {
            private int id;
            public int Id
            {
                get { return id; }
                set { id = value; }
            }
            private string Src;
            public string src
            {
                get { return Src; }
                set { Src = value; }
            }
        }
    }


    public class ClockArea
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
        private int Type;
        public int type
        {
            get { return Type; }
            set { Type = value; }
        }
        private int BorderEffect;
        public int borderEffect
        {
            get { return BorderEffect; }
            set { BorderEffect = value; }
        }
        private int BorderSW;
        public int borderSW
        {
            get { return BorderSW; }
            set { BorderSW = value; }
        }
        private int BorderSpeed;
        public int borderSpeed
        {
            get { return BorderSpeed; }
            set { BorderSpeed = value; }
        }
        private int BorderType;
        public int borderType
        {
            get { return BorderType; }
            set { BorderType = value; }
        }
        private int Left;
        public int left
        {
            get { return Left; }
            set { Left = value; }
        }
        private int Top;
        public int top
        {
            get { return Top; }
            set { Top = value; }
        }
        private int Width;
        public int width
        {
            get { return Width; }
            set { Width = value; }
        }
        private int Height;
        public int height
        {
            get { return Height; }
            set { Height = value; }
        }
        private int ClockType;
        public int clockType
        {
            get { return ClockType; }
            set { ClockType = value; }
        }

        private int TimeZoneValue;
        public int timeZoneValue
        {
            get { return TimeZoneValue; }
            set { TimeZoneValue = value; }
        }
        private int ClockFormat;
        public int clockFormat
        {
            get { return ClockFormat; }
            set { ClockFormat = value; }
        }
        private string FontFamily;
        public string fontFamily
        {
            get { return FontFamily; }
            set { FontFamily = value; }
        }
        private int DateFontSize;
        public int dateFontSize
        {
            get { return DateFontSize; }
            set { DateFontSize = value; }
        }
        private string FontColor;
        public string fontColor
        {
            get { return FontColor; }
            set { FontColor = value; }
        }
        private int BoldSet;
        public int boldSet
        {
            get { return BoldSet; }
            set { BoldSet = value; }
        }
        private int Layout;
        public int layout
        {
            get { return Layout; }
            set { Layout = value; }
        }
        private int DateShow;
        public int dateShow
        {
            get { return DateShow; }
            set { DateShow = value; }
        }
        private int YearShow;
        public int yearShow
        {
            get { return YearShow; }
            set { YearShow = value; }
        }
        private int MonthShow;
        public int monthShow
        {
            get { return MonthShow; }
            set { MonthShow = value; }
        }
        private int DayShow;
        public int dayShow
        {
            get { return DayShow; }
            set { DayShow = value; }
        }
        private int WeekShow;
        public int weekShow
        {
            get { return WeekShow; }
            set { WeekShow = value; }
        }
        private int TimeShow;
        public int timeShow
        {
            get { return TimeShow; }
            set { TimeShow = value; }
        }
        private int LunarShow;
        public int lunarShow
        {
            get { return LunarShow; }
            set { LunarShow = value; }
        }
        private int DialType;
        public int dialType
        {
            get { return DialType; }
            set { DialType = value; }
        }
        private string BackColor;
        public string backColor
        {
            get { return BackColor; }
            set { BackColor = value; }
        }
        private int HourMarkType;
        public int hourMarkType
        {
            get { return HourMarkType; }
            set { HourMarkType = value; }
        }
        private int HourMarkSize;
        public int hourMarkSize
        {
            get { return HourMarkSize; }
            set { HourMarkSize = value; }
        }
        private string HourMarkColor;
        public string hourMarkColor
        {
            get { return HourMarkColor; }
            set { HourMarkColor = value; }
        }
        private int MinuteMarkType;
        public int minuteMarkType
        {
            get { return MinuteMarkType; }
            set { MinuteMarkType = value; }
        }
        private int MinuteMarkSize;
        public int minuteMarkSize
        {
            get { return MinuteMarkSize; }
            set { MinuteMarkSize = value; }
        }
        private string MinuteMarkColor;
        public string minuteMarkColor
        {
            get { return MinuteMarkColor; }
            set { MinuteMarkColor = value; }
        }
        private string HourColor;
        public string hourColor
        {
            get { return HourColor; }
            set { HourColor = value; }
        }
        private string MinuteColor;
        public string minuteColor
        {
            get { return MinuteColor; }
            set { MinuteColor = value; }
        }
        private string SecondColor;
        public string secondColor
        {
            get { return SecondColor; }
            set { SecondColor = value; }
        }
        private int DateFormat;
        public int dateFormat
        {
            get { return DateFormat; }
            set { DateFormat = value; }
        }
        private string DateColor;
        public string dateColor
        {
            get { return DateColor; }
            set { DateColor = value; }
        }
        private int WeekFormat;
        public int weekFormat
        {
            get { return WeekFormat; }
            set { WeekFormat = value; }
        }
        private string WeekColor;
        public string weekColor
        {
            get { return WeekColor; }
            set { WeekColor = value; }
        }
        private int TimeFormat;
        public int timeFormat
        {
            get { return TimeFormat; }
            set { TimeFormat = value; }
        }
        private string TimeColor;
        public string timeColor
        {
            get { return TimeColor; }
            set { TimeColor = value; }
        }
        private int WeekTimeSwap;
        public int weekTimeSwap
        {
            get { return WeekTimeSwap; }
            set { WeekTimeSwap = value; }
        }
        private string LunarColor;
        public string lunarColor
        {
            get { return LunarColor; }
            set { LunarColor = value; }
        }
        private string Text;
        public string text
        {
            get { return Text; }
            set { Text = value; }
        }
        private string TextColor;
        public string textColor
        {
            get { return TextColor; }
            set { TextColor = value; }
        }
        private int TextSize;
        public int textSize
        {
            get { return TextSize; }
            set { TextSize = value; }
        }
        private int TextBold;
        public int textBold
        {
            get { return TextBold; }
            set { TextBold = value; }
        }
        private int TextAlign;
        public int textAlign
        {
            get { return TextAlign; }
            set { TextAlign = value; }
        }
        private int ZIndex;
        public int zIndex
        {
            get { return ZIndex; }
            set { ZIndex = value; }
        }
    }



    public class StaticTextArea
    {
        private int Id;
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }
        private int Type;
        public int type
        {
            get { return Type; }
            set { Type = value; }
        }
        //private int BorderEffect;
        //public int borderEffect
        //{
        //    get { return BorderEffect; }
        //    set { BorderEffect = value; }
        //}
        //private int BorderSW;
        //public int borderSW
        //{
        //    get { return BorderSW; }
        //    set { BorderSW = value; }
        //}
        //private int BorderSpeed;
        //public int borderSpeed
        //{
        //    get { return BorderSpeed; }
        //    set { BorderSpeed = value; }
        //}
        //private int BorderType;
        //public int borderType
        //{
        //    get { return BorderType; }
        //    set { BorderType = value; }
        //}
        private int Left;
        public int left
        {
            get { return Left; }
            set { Left = value; }
        }
        private int Top;
        public int top
        {
            get { return Top; }
            set { Top = value; }
        }
        private int Width;
        public int width
        {
            get { return Width; }
            set { Width = value; }
        }
        private int Height;
        public int height
        {
            get { return Height; }
            set { Height = value; }
        }


        private string Background;
        public string background
        {
            get { return Background; }
            set { Background = value; }
        }

        public string FontColor;
        public string fontColor
        {
            get { return FontColor; }
            set { FontColor = value; }
        }

        public string FontFamily;

        public string fontFamily
        {
            get { return FontFamily; }
            set { FontFamily = value; }
        }

        public int FontSize;
        public int fontSize
        {
            get { return FontSize; }
            set { FontSize = value; }
        }

        //public int Italic;
        //public int italic
        //{
        //    get { return Italic; }
        //    set { Italic = value; }
        //}

        //public int Bold;
        //public int bold
        //{
        //    get { return Bold; }
        //    set { Bold = value; }
        //}

        //public int TextLine;
        //public int textLine
        //{
        //    get { return TextLine; }
        //    set { TextLine = value; }
        //}

 

        private string Text;
        public string text
        {
            get { return Text; }
            set { Text = value; }
        }
        private int Base64;
        public int base64
        {
            get { return Base64; }
            set { Base64 = value; }
        }
             

        private int ZIndex;
        public int zIndex
        {
            get { return ZIndex; }
            set { ZIndex = value; }
        }
    }



    public class RefreshData
    {
        private int AreaId;
        public int areaId
        {
            get { return AreaId; }
            set { AreaId = value; }
        }
        private string Text;
        public string text
        {
            get { return Text; }
            set { Text = value; }
        }
        private string FontColor;
        public string fontColor
        {
            get { return FontColor; }
            set { FontColor = value; }
        }
        private int RefreshTimes;
        public int refreshTimes
        {
            get { return RefreshTimes; }
            set { RefreshTimes = value; }
        }
        private int Base64;
        public int base64
        {
            get { return Base64; }
            set { Base64 = value; }
        }
    }

    public class RefreshPicture
    {
        private int nPageId;
        public int pageId
        {
            get { return nPageId; }
            set { nPageId = value; }
        }

        private List<PicInfo> picInfo=new List<PicInfo>();
        public List<NewTGP.RefreshPicture.PicInfo> info
        {
            get { return picInfo; }
            set { picInfo = value; }
        }
 
        public class PicInfo
        {
            private int nAreaId;
            public int areaId
            {
                get { return nAreaId; }
                set { nAreaId = value; }
            }
            private List<string> strSrc=new List<string>();
            public List<string> src
            {
                get { return strSrc; }
                set { strSrc = value; }
            }
        }
    }

    public class ProgramPlan
    {
        private Int64 CreateDate;

        public Int64 createDate
        {
            get { return CreateDate; }
            set { CreateDate = value; }
        }

        private string ProgramId;

        public string programId
        {
            get { return ProgramId; }
            set { ProgramId = value; }
        }
        private string ProgramName;

        public string programName
        {
            get { return ProgramName; }
            set { ProgramName = value; }
        }
        private int ProgramType;

        public int programType
        {
            get { return ProgramType; }
            set { ProgramType = value; }
        }
        private Int64 UpdateDate;

        public Int64 updateDate
        {
            get { return UpdateDate; }
            set { UpdateDate = value; }
        }
        private int IsDefault;

        public int isDefault
        {
            get { return IsDefault; }
            set { IsDefault = value; }
        }

        private List<Date> _Date;

        public List<Date> date
        {
            get { return _Date; }
            set { _Date = value; }
        }
        private List<int> PlanWeekDay;

        public List<int> planWeekDay
        {
            get { return PlanWeekDay; }
            set { PlanWeekDay = value; }
        }
        private List<Time> _Time;

        public List<Time> time
        {
            get { return _Time; }
            set { _Time = value; }
        }
        public class Date
        {
            private Int64 DateStart;

            public Int64 dateStart
            {
                get { return DateStart; }
                set { DateStart = value; }
            }
            private Int64 DateEnd;

            public Int64 dateEnd
            {
                get { return DateEnd; }
                set { DateEnd = value; }
            }
        }

        public class Time
        {
            private string TimeStart;

            public string timeStart
            {
                get { return TimeStart; }
                set { TimeStart = value; }
            }
            private string TimeEnd;

            public string timeEnd
            {
                get { return TimeEnd; }
                set { TimeEnd = value; }
            }
        }
    }

    public class ProgramPlanNew
    {
        private int ClearOrder;
        public int clearOrder
        {
            get { return ClearOrder; }
            set { ClearOrder = value; }
        }
        private int ClearTimer;
        public int clearTimer
        {
            get { return ClearTimer; }
            set { ClearTimer = value; }
        }
        private int ClearCut;
        public int clearCut
        {
            get { return ClearCut; }
            set { ClearCut = value; }
        }
        private List<ProgramPlan> programPlan = new List<ProgramPlan>();
        public List<ProgramPlan> program
        {
            get { return programPlan; }
            set { programPlan = value; }
        }
    }

}
