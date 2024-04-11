using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    public class ExamPaperController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion
            
        public static int testid = default;

        #region 学生浏览试卷
        /// <summary>
        /// 学生浏览试卷
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StudentBrowseExam(ExamPaper examPaper)
        {
            StudentUser studentUser = default;
            if(!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            if(StudentUserBLL.ConfirmStudentExam(studentUser,examPaper).isRetake == "否")
            {
                StudentUserBLL.UpdateStudentRetake(studentUser, examPaper);
            }
            //获取试卷总分
            QuestionBLL questionBLL = new QuestionBLL();
            ExamPaper exampaper = ExamPaperBLL.GetExamPaperInfo(examPaper);
            int score = 0;
       
            //试题及答案集合
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            for(int i = 0; i < SingleList.Count; i++)
            {
                score += SingleList[i].questionScore;
                SingleList[i].stu_answer = StudentUserBLL.GetStu_QuestionStuAnswer(studentUser, examPaper, SingleList[i].id).Stu_Answer;
            }
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            for (int i = 0; i < JudgeList.Count; i++)
            {
                score += JudgeList[i].questionScore;
                JudgeList[i].stu_answer = StudentUserBLL.GetStu_QuestionStuAnswer(studentUser, examPaper, JudgeList[i].id).Stu_Answer;
            }
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            for (int i = 0; i < FillList.Count; i++)
            {
                score += FillList[i].questionScore;
                FillList[i].stu_answer = StudentUserBLL.GetStu_QuestionStuAnswer(studentUser, examPaper, FillList[i].id).Stu_Answer;
            }
            exampaper.stuScore = StudentUserBLL.SearchStuScoreOne(exampaper.id, studentUser.id).isScore;
            exampaper.totalScore = score;
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(exampaper);
        } 
        #endregion

        #region 开始测试视图
        /// <summary>
        /// 开始测试视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TestPaperAnswer()
        {
            QuestionBLL questionBLL = new QuestionBLL();
            StudentUser studentUser = default;
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            int totalScore = 0;
            TestPaper testPaper = TestPaperBLL.GetTestInfoById(testid);

            List<Question> SingleList = questionBLL.GetProblemsToTest(testid, "单选题");
            for (int i = 0; i < SingleList.Count; i++ )
            {
                totalScore += SingleList[i].questionScore;
            }
            List<Question> JudgeList = questionBLL.GetProblemsToTest(testid, "判断题");
            for (int i = 0; i < JudgeList.Count; i++)
            {
                totalScore += JudgeList[i].questionScore;
            }
            List<Question> FillList = questionBLL.GetProblemsToTest(testid, "填空题");
            for (int i = 0; i < FillList.Count; i++)
            {
                totalScore += FillList[i].questionScore;
            }
            testPaper.totalScore = totalScore;
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(testPaper);
        }
        #endregion

        #region 测试卷统计分数
        /// <summary>
        /// 测试卷统计分数
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TestPaperAnswer(TestPaper testPaper)
        {
            QuestionBLL questionBLL = new QuestionBLL();
            int Studentscore = 0;

            TestPaper testpaper  = TestPaperBLL.GetTestInfoById(testPaper.id);
            //获取题
            List<Question> SingleList = questionBLL.GetProblemsToTest(testpaper.id, "单选题");
            List<Question> JudgeList = questionBLL.GetProblemsToTest(testpaper.id, "判断题");
            List<Question> FillList = questionBLL.GetProblemsToTest(testpaper.id, "填空题");
            //统计选择题
            for (int i = 1; i <= SingleList.Count; i++)
            {
                string sAnswer = Request.Form["SAnswer+" + i];//参考答案
                string radio1 = Request.Form["SingleAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateTestPaper_DetailInfo(testpaper, SingleList[i-1].id, radio1);
                if (sAnswer == radio1)
                {
                    Studentscore += SingleList[i - 1].questionScore;
                }
            }
            //统计判断题
            for (int i = 1; i <= JudgeList.Count; i++)
            {
                string jAnswer = Request.Form["JAnswer+" + i];//参考答案
                string radio2 = Request.Form["JudgeAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateTestPaper_DetailInfo(testpaper, JudgeList[i - 1].id, radio2);
                if (jAnswer == radio2)
                {
                    Studentscore += JudgeList[i - 1].questionScore;
                }
            }
            //统计填空题
            for (int i = 1; i <= FillList.Count; i++)
            {
                string fAnswer = Request.Form["FAnswer+" + i];//参考答案
                string radio3 = Request.Form["FillAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateTestPaper_DetailInfo(testpaper, FillList[i - 1].id, radio3);
                if (fAnswer == radio3)
                {
                    Studentscore += FillList[i - 1].questionScore;
                }
            }
            //保存分数到数据库中
            StudentUserBLL.SaveStudentTestScore(testpaper, Studentscore);
            return RedirectToAction("ShowTestScore", "StudentUser", testpaper);
        } 
        #endregion

        #region 随机测试选择视图
        /// <summary>
        /// 随机测试视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TestPaperToStart()
        {
            List<Question> questions = QuestionBLL.GetQuestionSubject();
            return View(questions);
        }
        #endregion

        #region 随机测试
        /// <summary>
        /// 随机测试
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TestPaperToStart(Question question)
        {
            StudentUser studentUser = default;
            if(!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            TestPaper testPaper  = default;
            string subject = question.questionSubject;
            //写入数据：test试卷与学生的关系
            if (!TestPaperBLL.AddTestPaperToDB(subject, studentUser.id))
            {
                return Content(bsJsonResult.ErrorResult("测试生成失败！！！"));
            }
            testPaper = TestPaperBLL.GetTestInfoByIdAndTestTime(studentUser.id);
            testid = testPaper.id;
            //利用随机生成标识进行测试抽题
            //单选题
            int[] itemSingleP = QuestionBLL.MakeUpProblemToTest(testPaper, "单选题", 5).ToArray();
            for (int i = 0; i < 5; i++)
            {
                ExamPaperBLL.AddTestPaperDetailToDB(testPaper, "单选题", itemSingleP[i]);
            }
            //判断题
            int[] itemJudgeP = QuestionBLL.MakeUpProblemToTest(testPaper, "判断题",5).ToArray();
            for (int i = 0; i < 5; i++)
            {
                ExamPaperBLL.AddTestPaperDetailToDB(testPaper, "判断题", itemJudgeP[i]);
            }
            //填空题
            int[] itemFillP = QuestionBLL.MakeUpProblemToTest(testPaper, "填空题", 5).ToArray();

            for (int i = 0; i < 5; i++)
            {
                ExamPaperBLL.AddTestPaperDetailToDB(testPaper, "填空题", itemFillP[i]);
            }
            return Content(bsJsonResult.SuccessResult("测试卷生成成功！！！"));
        }
        #endregion

        #region 学生已经考过的试卷
        /// <summary>
        /// 学生已经考过的试卷
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Completed_ExamPapersToStudent()
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<ExamPaper> examPapers = ExamPaperBLL.GetStu_ExamPapersToShow(studentUser);
            for (int i = 0; i < examPapers.Count; i++)
            {
                examPapers[i].teacherName = TeacherUserBLL.GetTeacherById(examPapers[i].tid).realName;
                examPapers[i].isretake = StudentUserBLL.ConfirmStudentExam( studentUser, examPapers[i]).isRetake;
            }
            if (!string.IsNullOrEmpty(sKeys))
                examPapers = examPapers.Where(p => p.examName.Contains(sKeys)).ToList();
            return View(examPapers);
        }
        #endregion

        #region 确认考试（重考）
        /// <summary>
        /// 确认考试（重考）
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        public ActionResult MakeSureToReStartExam(ExamPaper examPaper)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            studentUser.isRetake = "是";
            return Content(bsJsonResult.SuccessResult("请开始重考！！！"));
        }
        #endregion

        #region 确认考试
        /// <summary>
        /// 确认考试
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        public ActionResult MakeSureToStartExam(ExamPaper examPaper)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            studentUser.isRetake = "否";
            return Content(bsJsonResult.SuccessResult("请开始答卷！！！"));
        }
        #endregion

        #region 学生考试视图
        /// <summary>
        /// 学生考试视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StartAnswerExamPaper(ExamPaper examPaper, string name)
        {
            QuestionBLL questionBLL = new QuestionBLL();
            ExamPaper exampaper = ExamPaperBLL.GetExamPaperInfo(examPaper);
            int score = 0;

            int SingleScore = QuestionBLL.GetQuestionByType("单选题").questionScore;
            score = score + SingleScore * exampaper.SinglePNum;

            int JudgeScore = QuestionBLL.GetQuestionByType("判断题").questionScore;
            score = score + JudgeScore * exampaper.JudgePNum;

            int FillScore = QuestionBLL.GetQuestionByType("填空题").questionScore;
            score = score + FillScore * exampaper.FillPNum;

            //试卷总分
            exampaper.totalScore = score;

            //单选题集合
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(exampaper);
        }
        #endregion

        #region 学生重考考试视图
        /// <summary>
        /// 学生重考考试视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReStartAnswerExamPaper(ExamPaper examPaper, string name)
        {
            QuestionBLL questionBLL = new QuestionBLL();
            ExamPaper exampaper = ExamPaperBLL.GetExamPaperInfo(examPaper);
            int score = 0;

            int SingleScore = QuestionBLL.GetQuestionByType("单选题").questionScore;
            score = score + SingleScore * exampaper.SinglePNum;

            int JudgeScore = QuestionBLL.GetQuestionByType("判断题").questionScore;
            score = score + JudgeScore * exampaper.JudgePNum;

            int FillScore = QuestionBLL.GetQuestionByType("填空题").questionScore;
            score = score + FillScore * exampaper.FillPNum;

            //试卷总分
            exampaper.totalScore = score;

            //单选题集合
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(exampaper);
        }
        #endregion

        #region 验证信息（重考统计得分）
        /// <summary>
        /// 验证信息（重考统计得分）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReStartAnswerExamPaper(ExamPaper examPaper)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            QuestionBLL questionBLL = new QuestionBLL();
            int Studentscore = 0;

            ExamPaper exampaper = ExamPaperBLL.GetExamPaperById(examPaper.id);
            //获取题
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            //统计选择题
            for (int i = 1; i <= SingleList.Count; i++)
            {
                string sAnswer = Request.Form["SAnswer+" + i];//参考答案
                string radio1 = Request.Form["SingleAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateExamPaperDetailInfo(SingleList[i - 1].id, studentUser.id, examPaper.id,  radio1);
                if (sAnswer == radio1)
                {
                    Studentscore += SingleList[i - 1].questionScore;
                }
            }
            //统计判断题
            for (int i = 1; i <= JudgeList.Count; i++)
            {
                string jAnswer = Request.Form["JAnswer+" + i];//参考答案
                string radio2 = Request.Form["JudgeAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateExamPaperDetailInfo(JudgeList[i - 1].id, studentUser.id, examPaper.id, radio2);
                if (jAnswer == radio2)
                {
                    Studentscore += JudgeList[i - 1].questionScore;
                }
            }
            //统计填空题
            for (int i = 1; i <= FillList.Count; i++)
            {
                string fAnswer = Request.Form["FAnswer+" + i];//参考答案
                string radio3 = Request.Form["FillAnswer+" + i];//学生答案
                ExamPaperBLL.UpdateExamPaperDetailInfo(FillList[i - 1].id, studentUser.id, examPaper.id, radio3);
                if (fAnswer == radio3)
                {
                    Studentscore += FillList[i - 1].questionScore;
                }
            }
            //保存分数到数据库中
            if (studentUser.isRetake == "否")
            {
                StudentUserBLL.AddStudentScore(studentUser, exampaper, Studentscore, "否");
            }
            else if (studentUser.isRetake == "是")
            {
                StudentUserBLL.UpdateStudentScore(studentUser, exampaper, Studentscore, "是");
            }
            return RedirectToAction("SearchStuScore", "StudentUser", examPaper);
        }
        #endregion

        #region 验证信息（统计得分）
        /// <summary>
        /// 验证信息（统计得分）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StartAnswerExamPaper(ExamPaper examPaper)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            QuestionBLL questionBLL = new QuestionBLL();
            int Studentscore = 0;

            ExamPaper exampaper = ExamPaperBLL.GetExamPaperById(examPaper.id);
            //获取题
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            //统计选择题
            for (int i = 1; i <= SingleList.Count; i++)
            {
                string sAnswer = Request.Form["SAnswer+" + i];//参考答案
                string radio1 = Request.Form["SingleAnswer+" + i];//学生答案
                ExamPaperBLL.InsertExamPaperDetailInfo(SingleList[i - 1].id, studentUser.id, examPaper.id, SingleList[i-1].questionType, radio1);
                if (sAnswer == radio1)
                {
                    Studentscore += SingleList[i - 1].questionScore;
                }
            }
            //统计判断题
            for (int i = 1; i <= JudgeList.Count; i++)
            {
                string jAnswer = Request.Form["JAnswer+" + i];//参考答案
                string radio2 = Request.Form["JudgeAnswer+" + i];//学生答案
                ExamPaperBLL.InsertExamPaperDetailInfo(JudgeList[i - 1].id, studentUser.id, examPaper.id,JudgeList[i-1].questionType, radio2);
                if (jAnswer == radio2)
                {
                    Studentscore += JudgeList[i - 1].questionScore;
                }
            }
            //统计填空题
            for (int i = 1; i <= FillList.Count; i++)
            {
                string fAnswer = Request.Form["FAnswer+" + i];//参考答案
                string radio3 = Request.Form["FillAnswer+" + i];//学生答案
                ExamPaperBLL.InsertExamPaperDetailInfo(FillList[i - 1].id, studentUser.id, examPaper.id,FillList[i-1].questionType, radio3);
                if (fAnswer == radio3)
                {
                    Studentscore += FillList[i - 1].questionScore;
                }
            }
            //保存分数到数据库中
            if(studentUser.isRetake == "否")
            {
                StudentUserBLL.AddStudentScore(studentUser, exampaper, Studentscore, "否");
            }
            else if(studentUser.isRetake == "是")
            {
                StudentUserBLL.UpdateStudentScore(studentUser, exampaper, Studentscore, "是");
            }
            return RedirectToAction("SearchStuScore", "StudentUser", examPaper);
        }
        #endregion

        #region 查看试卷内容
        /// <summary>
        /// 查看试卷内容
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ScanExamPaper(ExamPaper examPaper)
        {
            QuestionBLL questionBLL = new QuestionBLL();
            ExamPaper exampaper = ExamPaperBLL.GetExamPaperInfo(examPaper);
            int score = 0;

            //单选题集合
            List<Question> SingleList = questionBLL.GetProblems(exampaper, "单选题");
            for(int i = 0; i < SingleList.Count; i++)
            {
                score += SingleList[i].questionScore;
            }
            List<Question> JudgeList = questionBLL.GetProblems(exampaper, "判断题");
            for (int i = 0; i < JudgeList.Count; i++)
            {
                score += JudgeList[i].questionScore;
            }
            List<Question> FillList = questionBLL.GetProblems(exampaper, "填空题");
            for (int i = 0; i < FillList.Count; i++)
            {
                score += FillList[i].questionScore;
            }
            exampaper.totalScore = score;
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(exampaper);
        }
        #endregion

        #region 返回试卷列表视图(未做)
        /// <summary>
        /// 返回试卷列表视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExamPaperInfoToStudent()
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }

            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<ExamPaper> examPapers = ExamPaperBLL.GetStu_ExamPapersToStart(studentUser);
            for (int i = 0; i < examPapers.Count; i++)
            {
                examPapers[i].teacherName = TeacherUserBLL.GetTeacherUserById(examPapers[i].tid).realName;
            }
            if (!string.IsNullOrEmpty(sKeys))
                examPapers = examPapers.Where(p => p.examName.Contains(sKeys)).ToList();
            return View(examPapers);
        }
        #endregion

        #region 返回试卷列表视图(管理员：不可增加，删除，只能查看)
        /// <summary>
        /// 返回试卷列表视图(管理员：不可增加，删除，只能查看)
        /// </summary>
        /// <returns></returns>
        public ActionResult ExamPaperToList(int nowPage = 1, int pageSize = 7)
        {
            TeacherUser teacherUser = default;
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<ExamPaper> examPapers = ExamPaperBLL.GetExamPaper();
            for (int i = 0; i < examPapers.Count; i++)
            {
                int tid = examPapers[i].tid;
                teacherUser = TeacherUserBLL.GetTeacherById(tid);
                examPapers[i].teacherName = teacherUser.realName;
            }
            if (!string.IsNullOrEmpty(sKeys))
                examPapers = examPapers.Where(p => p.examName.Contains(sKeys) || p.examSubject.Contains(sKeys)).ToList();
            int totalCount = examPapers.Count;
            //计算总页数
            int pageCount = Math.Max((totalCount + pageSize - 1) / pageSize, 1);

            //处理当前页为负数
            if (nowPage <= 0)
            {
                nowPage = 1;
            }
            //处理最大的页数
            if (nowPage > pageCount)
            {
                nowPage = pageCount;
            }

            ViewBag.NowSize = nowPage;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            examPapers = examPapers.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();

            return View(examPapers);
        }
        #endregion

        #region 组卷
        /// <summary>
        /// 组卷
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MakeUpExam(ExamPaper examPaper)
        {
            //查询试卷是否已组卷
            var item = ExamPaperBLL.GetExamDetailList(examPaper.id);
            if (item.Count != 0)
            {
                return Content(bsJsonResult.ErrorResult("试卷已生成！！！"));
            }
            else
            {
                ExamPaper examP = ExamPaperBLL.GetExamPaperInfo(examPaper);
                //利用随机生成标识进行试卷抽题
                //单选题
                int[] itemSingleP = QuestionBLL.MakeUpProblem(examP, "单选题", examP.SinglePNum).ToArray();
                for (int i = 0; i < examP.SinglePNum; i++)
                {
                    ExamPaperBLL.AddExamPaperDetailToDB(examPaper, "单选题", itemSingleP[i]);
                }
                //判断题
                int[] itemJudgeP = QuestionBLL.MakeUpProblem(examP, "判断题", examP.JudgePNum).ToArray();
                for (int i = 0; i < examP.JudgePNum; i++)
                {
                    ExamPaperBLL.AddExamPaperDetailToDB(examPaper, "判断题", itemJudgeP[i]);
                }
                //填空题
                int[] itemFillP = QuestionBLL.MakeUpProblem(examP, "填空题", examP.FillPNum).ToArray();

                for (int i = 0; i < examP.FillPNum; i++)
                {
                    ExamPaperBLL.AddExamPaperDetailToDB(examPaper, "填空题", itemFillP[i]);
                }
            }
            return Content(bsJsonResult.SuccessResult("组卷成功！！！"));
        }
        #endregion

        #region 创建试卷视图(老师操作)
        /// <summary>
        /// 创建试卷视图(老师操作)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddExamPaper()
        {
            List<Question> questions = QuestionBLL.GetQuestionSubject();
            return View(questions);
        }
        #endregion

        #region 创建试卷
        /// <summary>
        /// 创建试卷
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddExamPaper(ExamPaper examPaper)
        {
            return Content(CheckAddExamPaper(examPaper));
        }
        #endregion

        #region 验证返回值(创建试卷)
        /// <summary>
        /// 验证返回值(创建试卷)
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        private string CheckAddExamPaper(ExamPaper examPaper)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }

            string ExamSubject = examPaper.examSubject;
            teacherUser = TeacherUserBLL.GetTeacherById(teacherUser.id);
            examPaper.tid = teacherUser.id;

            if (string.IsNullOrEmpty(ExamSubject) || ExamSubject.Length == 0)
            {
                return bsJsonResult.ErrorResult("试卷科目必须为汉字");
            }

            if (!ExamPaperBLL.AddExamPaperInfo(examPaper))
            {
                return bsJsonResult.ErrorResult("创建试卷失败");
            }
            return bsJsonResult.SuccessResult("创建试卷成功");
        }
        #endregion

        #region 编辑试卷视图业务逻辑
        /// <summary>
        /// 编辑试卷视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditExamPaper(ExamPaper examPaper)
        {
            if (!CommDefine.IsDigital(examPaper.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditExamPaperInfo(examPaper));
        }
        #endregion

        #region 编辑试卷视图
        /// <summary>
        /// 编辑试卷视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditExamPaper(string id)
        {
            //判断值Id是否存在
            ExamPaper examPaper = ExamPaperBLL.GetExamPaperById(Convert.ToInt32(id));
            if (examPaper is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(examPaper);
        }
        #endregion

        #region 验证返回值(编辑试卷)
        /// <summary>
        /// 验证返回值(编辑试卷)
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        private string CheckEditExamPaperInfo(ExamPaper examPaper)
        {

            if (!ExamPaperBLL.EditExamPaperInfo(examPaper))
            {
                return bsJsonResult.ErrorResult("编辑试卷失败");
            }
            return bsJsonResult.SuccessResult("编辑试卷成功");
        }
        #endregion

        #region 删除试卷
        /// <summary>
        /// 删除试卷
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteExamPaper(ExamPaper examPaper)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(examPaper.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!ExamPaperBLL.DeleteExamPaperById(examPaper))
            {
                return Content(bsJsonResult.ErrorResult("删除试卷失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除试卷成功"));
        }
        #endregion

        #region 批量删除试卷
        /// <summary>
        /// 批量删除试卷
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteExamPaper(string[] ids)
        {
            int iLen = ids.Length;
            //未选中，无值
            if (ids.Length == 0)
                return Content(bsJsonResult.ErrorResult("未选中，删除失败"));
            //有值，进行校验
            for (int i = 0; i < iLen; i++)
            {
                if (!CommDefine.IsDigital(ids[i]))
                {
                    return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
                }
                else
                {
                    if (!ExamPaperBLL.MutipleDeleteExamPaperById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 更新试卷状态
        /// <summary>
        /// 更新试卷状态
        /// </summary>
        /// <param name="examPaper"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateStatus(ExamPaper examPaper)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(examPaper.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!ExamPaperBLL.UpdateExamPaperStatus(examPaper))
            {
                return Content(bsJsonResult.ErrorResult("状态更新失败"));
            }
            return Content(bsJsonResult.SuccessResult("状态更新成功"));
        }
        #endregion
    }
}