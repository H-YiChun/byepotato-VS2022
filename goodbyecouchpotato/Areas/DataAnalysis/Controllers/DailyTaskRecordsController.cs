﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using goodbyecouchpotato.Models;
using goodbyecouchpotato.Areas.DataAnalysis.ViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace goodbyecouchpotato.Areas.DataAnalysis.Controllers
{
    [Area("DataAnalysis")]
    public class DailyTaskRecordsController : Controller
    {
        private readonly GoodbyepotatoContext _context;

        public DailyTaskRecordsController(GoodbyepotatoContext context)
        {
            _context = context;
        }

        // GET: DataAnalysis/DailyTaskRecords
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Index()
        {
            //----------取搜尋時間------------------
            DateOnly today = DateOnly.FromDateTime(DateTime.Now); //將datetime轉換成dateonly
            var stratday= today.AddDays(-7);
            ViewBag.starttime=stratday.ToString("yyyy-MM-dd");
            ViewBag.startend= today.ToString("yyyy-MM-dd");

            return View();
        }
        public async Task<IActionResult> TimeTotalRecord(_TaskRecordsViewModel _TaskRecords)
        {
            //-------------計算任務完成狀況---------------------------
            int completedcount = 0;
            var result = _context.DailyTaskRecords.AsQueryable();
            result = result.Where(s => s.TrecordDate >= _TaskRecords.starttime && s.TrecordDate <= _TaskRecords.endtime);
            foreach (var record in result)
            {
                if (record.T1completed == true)  //一筆裡面有三筆，可能會有一到三個任務有完成，如果要計算總共有多少任務被完成，就要分開計數
                {
                    completedcount += 1;
                }
                if (record.T2completed == true)
                {
                    completedcount += 1;
                }
                if (record.T3completed == true)
                {
                    completedcount += 1;
                }
            }
            var transresult = result.Select(t => new _TaskRecordsViewModel
            {
                    CId=t.CId,
                    TrecordDate=t.TrecordDate,   
                    T1name=t.T1name,
                    T1completed=t.T1completed,
                    T2name=t.T2name,
                    T2completed=t.T2completed,
                    T3name=t.T3name,
                    T3completed=t.T3completed,
            });
            var distinctresult = result.GroupBy(s=>s.CId).Select(s=>s.First()); //分組之後取得每一組的第一個來去除重複
            ViewBag.personcount = distinctresult.Count(); 
            ViewBag.count =transresult.Count()*3;  //求發出的任務總數，因為一筆有3個任務，所以乘3
            ViewBag.completedcount = completedcount;
            var completedpercent= Math.Round((completedcount / (double)(transresult.Count() * 3)) * 100, 2);
            ViewBag.completedpercent = transresult.Count() * 3 > 0 ? completedpercent : 0; //如果計算出來的完成度小於等於0，就顯示為0，不然會顯示非數值
            //-------------計算任務完成狀況end---------------------------
            //---------------計算任務數據------------------------------
            var Taskresult = result;  //將按照時間篩選後的數據給新的查詢
            //因為內容是隨機在三格中某一個，所以先分別計算這三格的內容次數之後再作合併，合併之後再進行一次分組，再次計算

            var countColumn1 = Taskresult.GroupBy(t => t.T1name)
                                           .Select(g => new ContentCount { Content = g.Key, Count = g.Count(), TrueCount = g.Count(x => (x.T1completed == true)) });  //在linq裡面建立一個新的類別欄位來存放資料

            var countColumn2 = Taskresult.GroupBy(t => t.T2name)
                                               .Select(g => new ContentCount { Content = g.Key, Count = g.Count(), TrueCount = g.Count(x => (x.T2completed == true)) });

            var countColumn3 = Taskresult.GroupBy(t => t.T3name)
                                               .Select(g => new ContentCount { Content = g.Key, Count = g.Count(), TrueCount = g.Count(x => (x.T3completed == true)) });

            var countTask = countColumn1.Concat(countColumn3).Concat(countColumn2).GroupBy(s => s.Content).Select(g => new ContentCount { Content = g.Key, Count = g.Sum(x => x.Count), TrueCount = g.Sum(x => x.TrueCount) });

            ViewBag.Tasktotal = countTask;

            //---------------計算任務數據end---------------------------

            return PartialView("_TaskRecordsPartial", transresult);
        }




        public class ContentCount  //建立一個新類別來存放資料
        {
            public string Content { get; set; }
            public int Count { get; set; }
            public int TrueCount { get; set; }

            public double Percentage
            {
                get
                {
                    return Count > 0 ? Math.Round((TrueCount / (double)Count) * 100,2) : 0;
                }
            }
        }


        // GET: DataAnalysis/DailyTaskRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dailyTaskRecord = await _context.DailyTaskRecords
                .FirstOrDefaultAsync(m => m.CId == id);
            if (dailyTaskRecord == null)
            {
                return NotFound();
            }

            return View(dailyTaskRecord);
        }



        private bool DailyTaskRecordExists(int id)
        {
            return _context.DailyTaskRecords.Any(e => e.CId == id);
        }
    }
}
