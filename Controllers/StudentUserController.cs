using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.Comm.Security;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using KS.VerifyCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    public class StudentUserController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion

        #region 业务逻辑(用户)
        /// <summary>
        /// 业务逻辑(用户)
        /// </summary>
        private readonly StudentUserBLL studentUser = new StudentUserBLL();
        #endregion

        //全局变量或集合
        public static stuScore stu_score = new stuScore();
        public static List<stuScore> stuScores = new List<stuScore>();

        #region 学生浏览测试结果
        /// <summary>
        /// 学生浏览测试结果
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StudentBrowseTestContent(TestPaper testpaper)
        {
            StudentUser studentUser = default;
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            int totalScore = 0;
            QuestionBLL questionBLL = new QuestionBLL();
            TestPaper nowTestpaper = TestPaperBLL.GetTestInfoById(testpaper.id);
            //试题及答案集合
            List<Question> SingleList = questionBLL.GetProblemsToTest(nowTestpaper.id, "单选题");
            for (int i = 0; i < SingleList.Count; i++)
            {
                SingleList[i].stu_answer = StudentUserBLL.GetStu_TestQuestionStuAnswer(studentUser, nowTestpaper, SingleList[i].id).Stu_Answer;
                totalScore += SingleList[i].questionScore;
            }
            List<Question> JudgeList = questionBLL.GetProblemsToTest(nowTestpaper.id, "判断题");
            for (int i = 0; i < JudgeList.Count; i++)
            {
                JudgeList[i].stu_answer = StudentUserBLL.GetStu_TestQuestionStuAnswer(studentUser, nowTestpaper, JudgeList[i].id).Stu_Answer;
                totalScore += JudgeList[i].questionScore;
            }
            List<Question> FillList = questionBLL.GetProblemsToTest(nowTestpaper.id, "填空题");
            for (int i = 0; i < FillList.Count; i++)
            {
                FillList[i].stu_answer = StudentUserBLL.GetStu_TestQuestionStuAnswer(studentUser, nowTestpaper, FillList[i].id).Stu_Answer;
                totalScore += FillList[i].questionScore;
            }
            nowTestpaper.totalScore = totalScore;
            nowTestpaper.stuScore = StudentUserBLL.SearchStuTestScore(nowTestpaper.id).isScore;
            ViewBag.SingleProblem = SingleList;
            ViewBag.JudgeProblem = JudgeList;
            ViewBag.FillProblem = FillList;
            return View(nowTestpaper);
        }
        #endregion

        #region 删除测试记录
        /// <summary>
        /// 删除测试记录
        /// </summary>
        /// <param name="testPaper"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteTest(TestPaper testPaper)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(testPaper.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!StudentUserBLL.DeleteTestById(testPaper.id))
            {
                return Content(bsJsonResult.ErrorResult("删除测试记录失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除测试记录成功"));
        }
        #endregion

        #region 批量删除学生测试记录
        /// <summary>
        /// 批量删除学生测试记录
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteTest(string[] ids)
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
                    if (!StudentUserBLL.DeleteTestById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 测试记录视图
        /// <summary>
        /// 测试记录视图
        /// </summary>
        /// <returns></returns>
        public ActionResult StudentBrowseTest(int nowPage = 1, int pageSize = 6)
        {
            StudentUser studentUser = default;
           if(!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            List<stuScore> stuscores = default;
            stuscores = StudentUserBLL.GetStudentTestScore(studentUser);
            for(int i = 0; i< stuscores.Count; i++)
            {
                stuscores[i].stuName = studentUser.realName;
            }
            int totalCount = stuscores.Count;
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
            stuscores = stuscores.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(stuscores);
        } 
        #endregion

        #region 学生批量已读
        /// <summary>
        /// 学生批量已读
        /// </summary>
        /// <returns></returns>
        public ActionResult MutipleReadTeacherNotice(string[] ids)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            int iLen = ids.Length;
            //未选中，无值
            if (ids.Length == 0)
                return Content(bsJsonResult.ErrorResult("未选中，全部已读失败"));
            //有值，进行校验
            for (int i = 0; i < iLen; i++)
            {
                if (!CommDefine.IsDigital(ids[i]))
                {
                    return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
                }
                else
                {
                    if (!NoticeBLL.UpdateStudentReadNoticeById(Convert.ToInt32(ids[i]), studentUser.id))
                    {
                        return Content(bsJsonResult.ErrorResult("全部已读失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("全部已读成功"));
        }
        #endregion

        #region 管理员发布通告(学生接收视图)
        /// <summary>
        /// 管理员发布通告(学生接收视图)
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemPublicNoticeToStu(int nowPage = 1, int pageSize = 6)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //获取管理员发布的通告
            List<System_Stu_Notice> Notices = NoticeBLL.GetAdminNotice(studentUser.id);
            for (int i = 0; i < Notices.Count; i++)
            {
                Notices[i].Publisher = "管理员";
            }
            int totalCount = Notices.Count;
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
            Notices = Notices.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(Notices);

        }
        #endregion

        #region 教师通告
        /// <summary>
        /// 教师通告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TeacherNoitce(int nowPage = 1, int pageSize = 6)
        {
            StudentUser studentUser = default;
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            List<Teacher_Stu> teacherIds = new List<Teacher_Stu>();
            //获取当前学生的所有老师的id
            teacherIds = StudentUserBLL.GetStu_TeacherId(studentUser);
            List<System_Stu_Notice> Notices = new List<System_Stu_Notice>();
            int n = 0;
            for (int i = 0; i < teacherIds.Count; i++)
            {
                string teachername = TeacherUserBLL.GetTeacherById(teacherIds[i].tid).realName;
                List<System_Stu_Notice> notice = default;
                //获取老师的所有发布的通知
                notice = NoticeBLL.GetStudentNotice(teacherIds[i].tid,studentUser.id);
                //将所有老师的所有通知装入集合中
                for(int j = 0; j < notice.Count; j++)
                {
                    Notices.Add(notice[j]);
                    Notices[n].Publisher = teachername;
                    n++;
                }
            }
            int totalCount = Notices.Count;
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
            Notices = Notices.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(Notices);
        } 
        #endregion

        #region 显示测试分数
        /// <summary>
        /// 显示测试分数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ShowTestScore(TestPaper testPaper)
        {
            StudentUser studentUser = default;
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            int totalScore = 0;
            QuestionBLL questionBLL = new QuestionBLL();
            TestPaper nowTestpaper = TestPaperBLL.GetTestInfoById(testPaper.id);
            //试题及答案集合
            List<Question> SingleList = questionBLL.GetProblemsToTest(nowTestpaper.id, "单选题");
            for (int i = 0; i < SingleList.Count; i++)
            {
                totalScore += SingleList[i].questionScore;
            }
            List<Question> JudgeList = questionBLL.GetProblemsToTest(nowTestpaper.id, "判断题");
            for (int i = 0; i < JudgeList.Count; i++)
            {
                totalScore += JudgeList[i].questionScore;
            }
            List<Question> FillList = questionBLL.GetProblemsToTest(nowTestpaper.id, "填空题");
            for (int i = 0; i < FillList.Count; i++)
            {
                totalScore += FillList[i].questionScore;
            }
            stuScore stuscore = StudentUserBLL.SearchStuTestScore(testPaper.id);
            stuscore.totalscore = totalScore;
            return View(stuscore);
        }
        #endregion

        #region 单次试卷成绩查询
        /// <summary>
        /// 单次试卷成绩查询
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ShowStuScore()
        {
            stuScore stuscore = ExamPaperBLL.GetStuScoreById(stu_score.id);
            stuscore.examName = ExamPaperBLL.GetExamPaperById(stuscore.eid).examName;
            //stu_score.examName = ExamPaperBLL.GetExamPaperById(stu_score.eid).examName;
            return View(stuscore);
            //return View();
        }
        #endregion

        #region 学生查询所有成绩视图
        /// <summary>
        /// 学生查询所有成绩视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ShowStuALLScore(int nowPage = 1, int pageSize = 10)
        {
            List<stuScore> stuscores = stuScores;
            int totalCount = stuScores.Count;
            for(int i = 0; i < totalCount; i++)
            {
                int tid = ExamPaperBLL.GetExamPaperById(stuscores[i].eid).tid;
                stuscores[i].teachername = TeacherUserBLL.GetTeacherById(tid).realName;
            }
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
            stuscores = stuscores.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(stuscores);
        } 
        #endregion

        #region 学生所有成绩查询视图
        /// <summary>
        /// 学生所有成绩查询视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchStuToAllScore()
        {
            return View();
        }
        #endregion

        #region 学生成绩查询(学生所有试卷成绩)
        /// <summary>
        /// 学生成绩查询(学生所有试卷成绩)
        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchStuToAllScore(StudentUser studentUser)
        {
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            string UserName = Request.Form["userName"];
            string PassWord = Request.Form["passWord"].StringToMD5();
            string Imgcode = Request.Form["ImgCode"];
            //验证登录账号
            if (string.IsNullOrEmpty(UserName) || UserName.Length < 4 || UserName.Length > 14)
            {
                return Content(bsJsonResult.ErrorResult("学生用户名必须为4-14个字符"));
            }

            //验证密码
            if (string.IsNullOrEmpty(PassWord))
            {
                return Content(bsJsonResult.ErrorResult("密码不能为空"));
            }
            //验证密码
            if (string.IsNullOrEmpty(Imgcode))
            {
                return Content(bsJsonResult.ErrorResult("验证码不能为空"));
            }
            studentUser = StudentUserBLL.GetStudentUserById(studentUser.id);
            if (studentUser.userName != UserName)
                return Content(bsJsonResult.ErrorResult("用户名不存在，请重新输入"));
            if (studentUser.passWord != PassWord)
                return Content(bsJsonResult.ErrorResult("密码错误，请重新输入"));
            if (Session["CheckCode"] is null)
            {
                return Content(bsJsonResult.ErrorResult("用户不存在或密码错误，登录失败"));
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return Content(bsJsonResult.WrongCodeResult("验证码错误"));
            }
            //从数据库中取出信息
            stuScores = StudentUserBLL.SearchStuScoreAllById(studentUser.id);
            if (stuScores.Count == 0)
            {
                return Content(bsJsonResult.ErrorResult("未查询到成绩！！！"));
            }
            for(int i =0; i < stuScores.Count; i++ )
            {
                stuScores[i].examName = ExamPaperBLL.GetExamPaperById(stuScores[i].eid).examName;
            }
            return Content(bsJsonResult.SuccessResult("查询成功！！！"));
        }
        #endregion

        #region 学生成绩查询视图
        /// <summary>
        /// 学生成绩查询视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchStuScore(ExamPaper examPaper)
        {
            StudentUser studentUser = default;
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }  
            ViewBag.exam = ExamPaperBLL.GetExamPaperById(examPaper.id);
            studentUser = StudentUserBLL.GetStudentById(studentUser.id);
            return View(studentUser);
        }
        #endregion

        #region 反射实现两个类的对象之间相同属性的值的复制
        /// <summary>
        /// 反射实现两个类的对象之间相同属性的值的复制
        /// 适用于没有新建实体之间
        /// </summary>
        /// <typeparam name="D">返回的实体</typeparam>
        /// <typeparam name="S">数据源实体</typeparam>
        /// <param name="d">返回的实体</param>
        /// <param name="s">数据源实体</param>
        /// <returns></returns>
        public static D MapperToModel<D, S>(D d, S s)
        {
            try
            {
                var Types = s.GetType();//获得类型  
                var Typed = typeof(D);
                foreach (PropertyInfo sp in Types.GetProperties())//获得类型的属性字段  
                {
                    foreach (PropertyInfo dp in Typed.GetProperties())
                    {
                        if (dp.Name == sp.Name && dp.PropertyType == sp.PropertyType && dp.Name != "Error" && dp.Name != "Item")//判断属性名是否相同  
                        {
                            dp.SetValue(d, sp.GetValue(s, null), null);//获得s对象属性的值复制给d对象的属性  
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return d;
        }
        #endregion

        #region 学生成绩查询(考试后)
        /// <summary>
        /// 学生成绩查询(考试后)
        /// <summary>
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SearchStuScore(ExamPaper examPaper, StudentUser studentUser)
        {
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            examPaper.id = Convert.ToInt32(Request.Form["eid"]);
            string UserName = Request.Form["userName"];
            string PassWord = Request.Form["passWord"].StringToMD5();
            string Imgcode  = Request.Form["ImgCode"];
            //验证登录账号
            if (string.IsNullOrEmpty(UserName) || UserName.Length < 4 || UserName.Length > 14)
            {
                return Content(bsJsonResult.ErrorResult("学生用户名必须为4-14个字符"));
            }

            //验证密码
            if (string.IsNullOrEmpty(PassWord))
            {
                return Content(bsJsonResult.ErrorResult("密码不能为空"));
            }
            //验证密码
            if (string.IsNullOrEmpty(Imgcode))
            {
                return Content(bsJsonResult.ErrorResult("验证码不能为空"));
            }
            studentUser = StudentUserBLL.GetStudentUserById(studentUser.id);
            if(studentUser.userName != UserName)
                return Content(bsJsonResult.ErrorResult("用户名不存在，请重新输入"));
            if(studentUser.passWord != PassWord)
                return Content(bsJsonResult.ErrorResult("密码错误，请重新输入"));
            if (Session["CheckCode"] is null)
            {
                return Content(bsJsonResult.ErrorResult("用户不存在或密码错误，登录失败"));
            }
            else if (!Session["CheckCode"].ToString().Equals(Imgcode, StringComparison.InvariantCultureIgnoreCase))
            {
                //StringComparison.InvariantCultureIgnoreCase 忽略大小写
                //return "WrongCode";
                return Content(bsJsonResult.WrongCodeResult("验证码错误"));
            }
            //从数据库中取出信息
            if (StudentUserBLL.SearchStuScoreOne(examPaper.id,studentUser.id) == null)
            {
                return Content(bsJsonResult.ErrorResult("未查询到成绩！！！"));
            }
            stuScore stuscore = StudentUserBLL.SearchStuScoreOne(examPaper.id, studentUser.id);
            MapperToModel(stu_score, stuscore);
            return Content(bsJsonResult.SuccessResult("查询成功！！！"));
            //return RedirectToAction("ShowStuScore","StudentUser", stuscore);
        }
        #endregion

        #region 学生加入老师
        /// <summary>
        /// 学生加入老师
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StudentJoinTeacher(TeacherUser teacherUser)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //对id进行验证
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            else if (!StudentUserBLL.AddTeacher_StuInfo(teacherUser.id, studentUser.id))
            {
                return Content(bsJsonResult.ErrorResult("加入老师失败"));
            }
            return Content(bsJsonResult.SuccessResult("加入老师成功"));
        }
        #endregion

        #region 学生退出老师
        /// <summary>
        /// 学生退出老师
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult StudentQuitTeacher(TeacherUser teacherUser)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //对id进行验证
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            else if (!StudentUserBLL.QuitTeacher(teacherUser.id, studentUser.id))
            {
                return Content(bsJsonResult.ErrorResult("退出失败"));
            }
            return Content(bsJsonResult.SuccessResult("退出成功"));
        }
        #endregion

        #region 老师信息展示视图
        /// <summary>
        /// 老师信息展示视图
        /// </summary>
        /// <returns></returns>
        public ActionResult TeacherInfo()
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<TeacherUser> teacherUsers = TeacherUserBLL.GetTeacherStatusOk();
            for(int i = 0; i < teacherUsers.Count; i++)
            {
                if(StudentUserBLL.SearchStu_Teacher(teacherUsers[i],studentUser) is null)
                {
                    teacherUsers[i].isJoin = 0;
                }
                else
                {
                    teacherUsers[i].isJoin = 1;
                }
            }
            if (!string.IsNullOrEmpty(sKeys))
                teacherUsers = teacherUsers.Where(p => p.realName.Contains(sKeys)).ToList();
            return View(teacherUsers);
        }
        #endregion

        #region 母版视图
        /// <summary>
        /// 母版视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 退出登录
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            //清空Session
            Session["LoginUser"] = null;
            Session.Abandon();
            //跳转到登录界面
            return base.Redirect("/");
        }
        #endregion

        #region 登录账户信息
        /// <summary>
        /// 登录账户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountInfo()
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //从数据库中取出信息
            studentUser = StudentUserBLL.GetStudentUserById(studentUser.id);
            //视图将会拿到数据库相应信息
            return View(studentUser);
        }
        #endregion

        #region 登录账户密码
        /// <summary>
        /// 登录账户密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountPass()
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            //从数据库中取出信息
            studentUser = StudentUserBLL.GetStudentUserById(studentUser.id);
            //视图将会拿到数据库相应信息
            return View(studentUser);
        }
        #endregion

        #region 验证返回值(修改用户账号)
        /// <summary>
        /// 验证返回值(修改用户账号)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserInfo(StudentUser studentUser)
        {
            string Name = studentUser.realName;
            string Telphone = studentUser.telPhone;

            //examUser.id = ((ExamUser)(Session["LoginUser"])).id ;
            if (string.IsNullOrEmpty(Name) || Name.Length == 0)
            {
                return bsJsonResult.ErrorResult("姓名必须为汉字");
            }
            //判断是否为汉字
            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("姓名必须为汉字");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位");
            }
            //判断电话号码是否为11位
            if (!CommDefine.IsTelphone(Telphone))
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位");
            }

            if (!StudentUserBLL.UpdateStudentUser(studentUser))
            {
                return bsJsonResult.ErrorResult("修改信息失败");
            }
            return bsJsonResult.SuccessResult("修改信息成功");
        }
        #endregion

        #region 验证返回值(修改用户密码)
        /// <summary>
        /// 验证返回值(修改用户密码)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserPass(StudentUser studentUser)
        {
            string sPassword = studentUser.passWord;
            string sRePassword = studentUser.repassWord;

            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }

            if (!StudentUserBLL.UpdateStudentPass(studentUser))
            {
                return bsJsonResult.ErrorResult("修改密码失败");
            }
            return bsJsonResult.SuccessResult("修改密码成功");
        }
        #endregion

        #region 发送数据更新验证
        /// <summary>
        /// 发送数据更新验证
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccount(StudentUser studentUser)
        {
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserInfo(studentUser));
        }
        #endregion

        #region 发送数据更新验证(改密码)
        /// <summary>
        /// 发送数据更新验证(改密码)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccountPass(StudentUser studentUser)
        {
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserPass(studentUser));
        }
        #endregion

        #region 验证返回值(编辑考生密码)
        /// <summary>
        /// 验证返回值(编辑考生密码)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        private string CheckEditStudentPass(StudentUser studentUser)
        {
            string sPassword = studentUser.passWord;
            string sRePassword = studentUser.repassWord;

            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }

            if (!StudentUserBLL.UpdateStudentPass(studentUser))
            {
                return bsJsonResult.ErrorResult("编辑密码失败");
            }
            return bsJsonResult.SuccessResult("编辑密码成功");
        }
        #endregion

        #region 编辑考生密码视图界面
        /// <summary>
        /// 编辑考生密码视图界面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditStudentPass(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            StudentUser studentUser = StudentUserBLL.GetStudentById(Convert.ToInt32(id));
            if (studentUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(studentUser);
        }
        #endregion

        #region 编辑考生密码业务逻辑
        /// <summary>
        /// 编辑考生密码业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditStudentPass(StudentUser studentUser)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }

            return Content(CheckEditStudentPass(studentUser));
        }
        #endregion

        #region 编辑学生视图业务逻辑
        /// <summary>
        /// 编辑学生视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditStudent(StudentUser studentUser)
        {
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditStudentInfo(studentUser));
        }
        #endregion

        #region 编辑学生视图
        /// <summary>
        /// 编辑学生视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditStudent(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            StudentUser studentUser = StudentUserBLL.GetStudentById(Convert.ToInt32(id));
            if (studentUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(studentUser);
        }
        #endregion

        #region 验证返回值(编辑学生)
        /// <summary>
        /// 验证返回值(编辑学生)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        private string CheckEditStudentInfo(StudentUser studentUser)
        {
            string Name = studentUser.realName;
            string Telphone = studentUser.telPhone;

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("学生姓名必须为汉字,且至少2个字符");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位且1开头");
            }

            if (!StudentUserBLL.EditStudentInfo(studentUser))
            {
                return bsJsonResult.ErrorResult("学生编辑失败");
            }
            return bsJsonResult.SuccessResult("学生编辑成功");
        }
        #endregion

        #region 返回学生列表视图
        /// <summary>
        /// 返回学生列表视图
        /// </summary>
        /// <returns></returns>
        public ActionResult StudentList(int nowPage = 1, int pageSize = 7)
        {
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<StudentUser> studentUsers = StudentUserBLL.GetStudent();
            if (!string.IsNullOrEmpty(sKeys))
                studentUsers = studentUsers.Where(p => p.userName.Contains(sKeys) || p.realName.Contains(sKeys)).ToList();

            int totalCount = studentUsers.Count;
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
            studentUsers = studentUsers.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(studentUsers);
        }
        #endregion

        #region 添加学生视图
        /// <summary>
        /// 添加学生视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddStudent()
        {
            return View();
        }
        #endregion

        #region 添加学生
        /// <summary>
        /// 添加学生
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddStudent(StudentUser studentUser)
        {
            return Content(CheckAddStudent(studentUser));
        }
        #endregion

        #region 验证返回值(添加学生)
        /// <summary>
        /// 验证返回值(添加学生)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        private string CheckAddStudent(StudentUser studentUser)
        {
            string Username = studentUser.userName;
            string Name = studentUser.realName;
            string Telphone = studentUser.telPhone;
            string sPassword = studentUser.passWord;
            string sRePassword = studentUser.repassWord;

            //验证登录账号
            if (string.IsNullOrEmpty(Username) || Username.Length < 4 || Username.Length > 14)
            {
                return bsJsonResult.ErrorResult("学生账号必须为4-14个字符");
            }

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("学生姓名必须为汉字,且至少2个字符");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位且1开头");
            }
            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }
            //验证学生账号
            if (!(StudentUserBLL.ChackStudentUserName(studentUser) is null))
            {
                return bsJsonResult.ErrorResult("学生账号已经存在！请重新添加");
            }
            //验证学生手机号
            if (!(StudentUserBLL.ChackStudentTelphone(studentUser) is null))
            {
                return bsJsonResult.ErrorResult("手机号已存在，请重新输入");
            }

            if (!StudentUserBLL.AddStudentInfo(studentUser))
            {
                return bsJsonResult.ErrorResult("学生添加失败");
            }
            return bsJsonResult.SuccessResult("学生添加成功");
        }
        #endregion

        #region 添加老师的学生视图
        /// <summary>
        /// 添加老师的学生视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddTeacherStudent()
        {
            return View();
        }
        #endregion

        #region 添加当前老师的学生
        /// <summary>
        /// 添加当前老师的学生
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTeacherStudent(StudentUser studentUser)
        {
            return Content(CheckAddTeacherStudent(studentUser));
        }
        #endregion

        #region 验证返回值(添加老师的学生)
        /// <summary>
        /// 验证返回值(添加老师的学生)
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        private string CheckAddTeacherStudent(StudentUser studentUser)
        {
            TeacherUser teacherUserNow = new TeacherUser();
            string Username = studentUser.userName;
            string Name = studentUser.realName;
            string Telphone = studentUser.telPhone;
            string sPassword = studentUser.passWord;
            string sRePassword = studentUser.repassWord;

            if (!(Session["LoginUser"] is null))
            {
                teacherUserNow = Session["LoginUser"] as TeacherUser;
            }
            //验证登录账号
            if (string.IsNullOrEmpty(Username) || Username.Length < 4 || Username.Length > 14)
            {
                return bsJsonResult.ErrorResult("学生账号必须为4-14个字符");
            }

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("学生姓名必须为汉字,且至少2个字符");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位且1开头");
            }
            //验证密码
            if (string.IsNullOrEmpty(sPassword))
            {
                return bsJsonResult.ErrorResult("密码不能为空");
            }
            if (string.IsNullOrEmpty(sRePassword))
            {
                return bsJsonResult.ErrorResult("确认密码不能为空");
            }
            if (sPassword.Length < 6 || sPassword.Length > 12)
            {
                return bsJsonResult.ErrorResult("密码必须在6-12位");
            }
            if (!sPassword.Equals(sRePassword))
            {
                return bsJsonResult.ErrorResult("两次密码输入不一致");
            }
            //验证学生账号
            if (!(StudentUserBLL.ChackStudentUserName(studentUser) is null))
            {
                return bsJsonResult.ErrorResult("学生账号已经存在！请重新添加");
            }
            //验证学生手机号
            if (!(StudentUserBLL.ChackStudentTelphone(studentUser) is null))
            {
                return bsJsonResult.ErrorResult("手机号已存在，请重新输入");
            }

            if (StudentUserBLL.AddStudentInfo(studentUser))
            {
                int sid = StudentUserBLL.GetStudentByUserName(studentUser.userName).id;
                if (!StudentUserBLL.AddTeacher_StuInfo(teacherUserNow.id, sid))
                {
                    return bsJsonResult.ErrorResult("学生添加失败");
                }
            }
            return bsJsonResult.SuccessResult("学生添加成功");
        }
        #endregion

        #region 更新学生用户状态
        /// <summary>
        /// 更新学生用户状态
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateStatus(StudentUser studentUser)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!StudentUserBLL.UpdateStudentStatus(studentUser))
            {
                return Content(bsJsonResult.ErrorResult("状态更新失败"));
            }
            return Content(bsJsonResult.SuccessResult("状态更新成功"));
        }
        #endregion

        #region 删除学生
        /// <summary>
        /// 删除学生
        /// </summary>
        /// <param name="studentUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteStudentUser(StudentUser studentUser)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(studentUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!StudentUserBLL.DeleteStudentById(studentUser))
            {
                return Content(bsJsonResult.ErrorResult("删除学生失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除学生成功"));
        }
        #endregion

        #region 批量删除学生
        /// <summary>
        /// 批量删除学生
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteStudent(string[] ids)
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
                    if (!StudentUserBLL.MutipleDeleteStudentById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 显示验证码方法
        /// <summary>
        /// 显示验证码方法
        /// </summary>
        public void VerifyCode()
        {
            ImageVerifyCode imageVerify = new ImageVerifyCode();
            imageVerify.ValidateCode();
            string s = Session["CheckCode"].ToString();
        }
        #endregion
    }
}