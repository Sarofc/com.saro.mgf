//using System;

//namespace Saro
//{
//    public class TimeInfo : Singleton<TimeInfo>, IDisposable
//    {
//        private int timeZone;

//        public int TimeZone
//        {
//            get
//            {
//                return timeZone;
//            }
//            set
//            {
//                timeZone = value;
//                dt = dt1970.AddHours(TimeZone);
//            }
//        }

//        private readonly DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
//        private DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

//        public long ServerMinusClientTime { private get; set; }

//        public long FrameTime;

//        public TimeInfo()
//        {
//            FrameTime = ClientNow();
//        }

//        public void Update()
//        {
//            FrameTime = ClientNow();
//        }

//        /// <summary> 
//        /// 根据时间戳获取时间 
//        /// </summary>  
//        public DateTime ToDateTime(long timeStamp)
//        {
//            return dt.AddTicks(timeStamp * 10000);
//        }

//        /// <summary>
//        /// 获取客户端时间。线程安全
//        /// </summary>
//        /// <returns></returns>
//        public long ClientNow()
//        {
//            return (DateTime.Now.Ticks - dt1970.Ticks) / 10000;
//        }

//        public long ServerNow()
//        {
//            return ClientNow() + ServerMinusClientTime;
//        }

//        public long ClientFrameTime()
//        {
//            return FrameTime;
//        }

//        public long ServerFrameTime()
//        {
//            return FrameTime + ServerMinusClientTime;
//        }

//        public long Transition(DateTime d)
//        {
//            return (d.Ticks - dt.Ticks) / 10000;
//        }

//        public void Dispose()
//        {
//            Instance = null;
//        }
//    }
//}