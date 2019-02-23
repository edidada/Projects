﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Huoyaoyuan.AdmiralRoom.API;
using Meowtrix.Collections.Generic;
using Meowtrix.ComponentModel;

namespace Huoyaoyuan.AdmiralRoom.Officer
{
    public partial class QuestManager : NotificationObject
    {
        public QuestManager()
        {
            Staff.API("api_get_member/questlist").Subscribe<getmember_questlist>(CheckQuestPage);
            Staff.API("api_req_quest/clearitemget").Subscribe(x =>
            {
                AvilableQuests.Remove(x.GetInt("api_quest_id"));
                Logger.Loggers.MaterialLogger.ForceLog = true;
            });
        }
        public readonly IDTable<int, Quest> AvilableQuests = new IDTable<int, Quest>();
        public IDTable<int, Quest> QuestInProgress { get; private set; }
        public int InProgressCount { get; private set; }
        public int AvilableCount { get; private set; }
        private int lastcheckedpage;
        private int lastcheckedfrom;
        private int lastcheckedto;
        private DateTimeOffset lastcheckedtime;
        private void CheckQuestPage(NameValueCollection req, getmember_questlist api)
        {
            int checkfrom, checkto;
            int type = req.GetInt("api_tab_id");
            CycleCount();
            if (api.api_list == null)
            {
                if (api.api_disp_page == 1) AvilableQuests.Clear();
                OnAllPropertyChanged();
                return;
            }
            checkfrom = api.api_list.First().api_no;
            checkto = api.api_list.Last().api_no;
            if (api.api_disp_page == 1) checkfrom = int.MinValue;
            else if (lastcheckedpage == api.api_disp_page - 1) checkfrom = lastcheckedto + 1;
            if (api.api_disp_page == api.api_page_count) checkto = int.MaxValue;
            else if (lastcheckedpage == api.api_disp_page + 1) checkto = lastcheckedfrom - 1;
            IEnumerable<Quest> checkremovelist = null;
            switch (type)
            {
                case 0:
                    checkremovelist = AvilableQuests;
                    break;
                case 1:
                    checkremovelist = AvilableQuests.Where(x => x.Period == QuestPeriod.Daily);
                    break;
                case 2:
                    checkremovelist = AvilableQuests.Where(x => x.Period == QuestPeriod.Weekly);
                    break;
                case 3:
                    checkremovelist = AvilableQuests.Where(x => x.Period == QuestPeriod.Monthly);
                    break;
                case 4:
                    checkremovelist = AvilableQuests.Where(x => x.Period == QuestPeriod.Once);
                    break;
            }
            if (checkremovelist != null)
                foreach (var item in checkremovelist.Where(x => x.Id >= checkfrom && x.Id <= checkto).ToList())
                    AvilableQuests.Remove(item);
            AvilableQuests.UpdateWithoutRemove(api.api_list, x => x.api_no);
            AvilableCount = api.api_count;
            InProgressCount = api.api_exec_count;
            lastcheckedpage = api.api_disp_page;
            lastcheckedfrom = checkfrom;
            lastcheckedto = checkto;
            lastcheckedtime = DateTimeOffset.UtcNow.ToOffset(QuestUpdateTime);
            UpdateInProgress();
            OnAllPropertyChanged();
        }
        private void UpdateInProgress()
        {
            List<Quest> list = AvilableQuests.Where(x => x.State == QuestState.InProgress || x.State == QuestState.Complete).ToList();
            int mistindexstart = 1001;
            while (list.Count < InProgressCount)
                list.Add(new Quest(new api_quest { api_no = mistindexstart++ }));
            list.Sort();
            QuestInProgress = new IDTable<int, Quest>(list);
        }
        private void CycleCount()
        {
            DateTimeOffset checktime = DateTimeOffset.UtcNow.ToOffset(QuestUpdateTime);
            var targets = new List<QuestTarget>();
            foreach (var quest in KnownQuests) targets.AddRange(quest.Targets);
            if (checktime.Date != lastcheckedtime.Date)
            {
                foreach (var item in AvilableQuests.Where(x => x.Period == QuestPeriod.Daily).ToList())
                    AvilableQuests.Remove(item);
                foreach (var target in targets.Where(x => x.Period == QuestPeriod.Daily))
                    target.SetProgress(0, true);
            }
            if (checktime.WeekStart() != lastcheckedtime.WeekStart())
            {
                foreach (var item in AvilableQuests.Where(x => x.Period == QuestPeriod.Weekly).ToList())
                    AvilableQuests.Remove(item);
                foreach (var target in targets.Where(x => x.Period == QuestPeriod.Weekly))
                    target.SetProgress(0, true);
            }
            if (checktime.Month != lastcheckedtime.Month)
            {
                foreach (var item in AvilableQuests.Where(x => x.Period == QuestPeriod.Monthly).ToList())
                    AvilableQuests.Remove(item);
                foreach (var target in targets.Where(x => x.Period == QuestPeriod.Monthly))
                    target.SetProgress(0, true);
            }
        }
        public static readonly TimeSpan QuestUpdateTime = TimeSpan.FromHours(4);
        public void Load()
        {
            try
            {
                using (var file = new StreamReader(@"logs\questcount.txt"))
                {
                    lastcheckedtime = new DateTimeOffset(DateTime.Parse(file.ReadLine()), QuestUpdateTime);
                    while (!file.EndOfStream)
                    {
                        string line = file.ReadLine().Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        var parts = line.Split(':');
                        var quest = KnownQuests[int.Parse(parts[0])];
                        if (quest == null) continue;
                        bool istook = bool.Parse(parts[1]);
                        quest.SetIsTook(istook);
                        var values = parts[2].Split(',');
                        for (int i = 0; i < values.Length; i++)
                            if (quest.Targets.Length > i)
                                quest.Targets[i].SetProgress(int.Parse(values[i]), true);
                    }
                }
            }
            catch { }
            CycleCount();
        }
        public void Save()
        {
            Directory.CreateDirectory("logs");
            using (var file = new StreamWriter(@"logs\questcount.txt"))
            {
                file.WriteLine(lastcheckedtime.ToOffset(QuestUpdateTime).Date.ToShortDateString());
                foreach (var quest in KnownQuests)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(quest.Id);
                    sb.Append(':');
                    sb.Append(quest.MainTarget.IsTook);
                    sb.Append(':');
                    foreach (var target in quest.Targets)
                    {
                        sb.Append(target.Progress.Current);
                        sb.Append(',');
                    }
                    sb.Remove(sb.Length - 1, 1);
                    file.WriteLine(sb);
                }
                file.Flush();
            }
        }
    }
}
