using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.DataDAL.TableDAL;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    public class TeacherUserController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion

        #region 母版视图
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 业务逻辑(用户)
        /// <summary>
        /// 业务逻辑(用户)
        /// </summary>
        private readonly TeacherUserBLL teacherUser = new TeacherUserBLL();
        #endregion

        #region 教师的学生成绩集合
        /// <summary>
        /// 教师的学生成绩集合
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Student_Tid_ScoreList(int nowPage = 1, int pageSize = 10)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            string sKeys = Request.QueryString["Keys"];
            List<stuScore> stuscores = TeacherUserBLL.GetMyStudentScore(teacherUser);
            for (int i = 0; i < stuscores.Count ;i++)
            {
                stuscores[i].stuName = StudentUserBLL.GetStudentById(stuscores[i].sid).realName;
                stuscores[i].examName = ExamPaperBLL.GetExamPaperById(stuscores[i].eid).examName;
            }
            if (!string.IsNullOrEmpty(sKeys))
                stuscores = stuscores.Where(p => p.stuName.Contains(sKeys) || p.examName.Contains(sKeys)).ToList();
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

        #region 教师批量已读管理员的通告
        /// <summary>
        /// 教师批量已读管理员的通告
        /// </summary>
        /// <returns></returns>
        public ActionResult MutipleTeaReadAdminNotice(string[] ids)
        {
            TeacherUser teacherUser  = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
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
                    if (!NoticeBLL.UpdateTeacherReadNoticeById(Convert.ToInt32(ids[i]), teacherUser.id))
                    {
                        return Content(bsJsonResult.ErrorResult("全部已读失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("全部已读成功"));
        }
        #endregion

        #region 教师已读某通知
        /// <summary>
        /// 学生已读某通知
        /// </summary>
        /// <param name="Notice"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateTeacherReadNotice(System_Stu_Notice Notice)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            NoticeBLL.UpdateTeacherReadNoticeById(Notice.id, teacherUser.id);
            return Content(bsJsonResult.SuccessResult("已读！！！"));
        }
        #endregion

        #region 用户浏览通告(教师浏览)
        /// <summary>
        /// 用户浏览通告(教师浏览)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TeaBrowseSystemNotice(System_Stu_Notice Notice)
        {
            TeacherUser teacherUser  = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            System_Stu_Notice notice = NoticeBLL.GetTeacherNoticeToRead(teacherUser.id, Notice.id);
            return View(notice);
        }
        #endregion

        #region 教师的通告集合视图
        /// <summary>
        /// 教师的通告集合视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TeacherNoticeToList(int nowPage = 1, int pageSize = 7)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            teacherUser = TeacherUserBLL.GetTeacherUserById(teacherUser.id);
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<System_Stu_Notice> Notices = NoticeBLL.GetTeacherNotice(teacherUser.id);
            for (int i = 0; i < Notices.Count; i++)
                Notices[i].Publisher = teacherUser.realName;
            if (!string.IsNullOrEmpty(sKeys))
                Notices = Notices.Where(p => p.NoticeName.Contains(sKeys) || p.NoticeContent.Contains(sKeys)).ToList();
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

        #region 管理员发布通告(教师接收视图)
        /// <summary>
        /// 管理员发布通告(教师接收视图)
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemPublicNoticeToTeacher(int nowPage = 1, int pageSize = 4)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            List<System_Stu_Notice> Notices = NoticeBLL.GetAdminNoticeToTea(teacherUser.id);
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
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            //从数据库中取出信息
            teacherUser = TeacherUserBLL.GetTeacherUserById(teacherUser.id);
            //视图将会拿到数据库相应信息
            return View(teacherUser);
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
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            //从数据库中取出信息
            teacherUser = TeacherUserBLL.GetTeacherUserById(teacherUser.id);
            //视图将会拿到数据库相应信息
            return View(teacherUser);
        }
        #endregion

        #region 验证返回值(修改用户账号)
        /// <summary>
        /// 验证返回值(修改用户账号)
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserInfo(TeacherUser teacherUser)
        {
            string Name = teacherUser.realName;
            string Telphone = teacherUser.telPhone;

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

            if (!TeacherUserBLL.UpdateTeacherUser(teacherUser))
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
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserPass(TeacherUser teacherUser)
        {
            string sPassword = teacherUser.passWord;
            string sRePassword = teacherUser.repassWord;

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

            if (!TeacherUserBLL.UpdateTeacherPass(teacherUser))
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
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccount(TeacherUser teacherUser)
        {
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserInfo(teacherUser));
        }
        #endregion

        #region 发送数据更新验证(改密码)
        /// <summary>
        /// 发送数据更新验证(改密码)
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccountPass(TeacherUser teacherUser)
        {
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserPass(teacherUser));
        }
        #endregion

        #region 验证返回值(编辑教师密码)
        /// <summary>
        /// 验证返回值(编辑教师密码)
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        private string CheckEditTeacherPass(TeacherUser teacherUser)
        {
            string sPassword = teacherUser.passWord;
            string sRePassword = teacherUser.repassWord;

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

            if (!TeacherUserBLL.UpdateTeacherPass(teacherUser))
            {
                return bsJsonResult.ErrorResult("编辑密码失败");
            }
            return bsJsonResult.SuccessResult("编辑密码成功");
        }
        #endregion

        #region 编辑教师密码视图界面
        /// <summary>
        /// 编辑教师密码视图界面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditTeacherPass(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            TeacherUser teacherUser = TeacherUserBLL.GetTeacherById(Convert.ToInt32(id));
            if (teacherUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(teacherUser);
        }
        #endregion

        #region 编辑教师密码业务逻辑
        /// <summary>
        /// 编辑教师密码业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditTeacherPass(TeacherUser teacherUser)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }

            return Content(CheckEditTeacherPass(teacherUser));
        }
        #endregion

        #region 编辑老师视图业务逻辑
        /// <summary>
        /// 编辑老师视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditTeacher(TeacherUser teacherUser)
        {
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditTeacherInfo(teacherUser));
        }
        #endregion

        #region 编辑老师视图
        /// <summary>
        /// 编辑老师视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditTeacher(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            TeacherUser teacherUser = TeacherUserBLL.GetTeacherById(Convert.ToInt32(id));
            if (teacherUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(teacherUser);
        }
        #endregion

        #region 验证返回值(编辑老师)
        /// <summary>
        /// 验证返回值(编辑老师)
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        private string CheckEditTeacherInfo(TeacherUser teacherUser)
        {
            string Name = teacherUser.realName;
            string Telphone = teacherUser.telPhone;

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("老师姓名必须为汉字,且至少2个字符");
            }
            if (string.IsNullOrEmpty(Telphone) || Telphone.Length == 0)
            {
                return bsJsonResult.ErrorResult("电话号码必须为11位且1开头");
            }

            if (!TeacherUserBLL.EditTeacherInfo(teacherUser))
            {
                return bsJsonResult.ErrorResult("老师编辑失败");
            }
            return bsJsonResult.SuccessResult("老师编辑成功");
        }
        #endregion

        #region 返回老师列表视图
        /// <summary>
        /// 返回老师列表视图
        /// </summary>
        /// <returns></returns>
        public ActionResult TeacherList(int nowPage = 1, int pageSize = 7)
        {
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<TeacherUser> teacherUsers = TeacherUserBLL.GetTeacher();
            if (!string.IsNullOrEmpty(sKeys))
                teacherUsers = teacherUsers.Where(p => p.userName.Contains(sKeys) || p.realName.Contains(sKeys)).ToList();

            int totalCount = teacherUsers.Count;
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
            teacherUsers = teacherUsers.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(teacherUsers);
        }
        #endregion

        #region 添加老师视图
        /// <summary>
        /// 添加老师视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddTeacher()
        {
            return View();
        }
        #endregion

        #region 添加老师
        /// <summary>
        /// 添加老师
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddTeacher(TeacherUser teacherUser)
        {
            return Content(CheckAddTeacher(teacherUser));
        }
        #endregion

        #region 验证返回值(添加老师)
        /// <summary>
        /// 验证返回值(添加老师)
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        private string CheckAddTeacher(TeacherUser teacherUser)
        {
            string Username = teacherUser.userName;
            string Name = teacherUser.realName;
            string Telphone = teacherUser.telPhone;
            string sPassword = teacherUser.passWord;
            string sRePassword = teacherUser.repassWord;

            //验证登录账号
            if (string.IsNullOrEmpty(Username) || Username.Length < 4 || Username.Length > 14)
            {
                return bsJsonResult.ErrorResult("老师账号必须为4-14个字符");
            }

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("老师姓名必须为汉字,且至少2个字符");
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
            //验证管理员账号
            if (!(TeacherUserDAL.ChackTeacherUserName(teacherUser) is null))
            {
                return bsJsonResult.ErrorResult("老师账号已经存在！请重新添加");
            }
            if(!(TeacherUserDAL.ChackTeacherTelphone(teacherUser) is null))
                return bsJsonResult.ErrorResult("手机号已经存在！请重新添加");
            if (!TeacherUserBLL.AddTeacherInfo(teacherUser))
            {
                return bsJsonResult.ErrorResult("老师添加失败");
            }

            return bsJsonResult.SuccessResult("老师添加成功");
        }
        #endregion

        #region 更新老师用户状态
        /// <summary>
        /// 更新老师用户状态
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateStatus(TeacherUser teacherUser)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!TeacherUserBLL.UpdateTeacherStatus(teacherUser))
            {
                return Content(bsJsonResult.ErrorResult("状态更新失败"));
            }
            return Content(bsJsonResult.SuccessResult("状态更新成功"));
        }
        #endregion

        #region 删除老师
        /// <summary>
        /// 删除老师
        /// </summary>
        /// <param name="teacherUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteTeacherUser(TeacherUser teacherUser)
        {
            //对id进行验证
            if (!CommDefine.IsDigital(teacherUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            List<ExamPaper> examPapers = ExamPaperBLL.GetTeacherExamPaper(teacherUser);
            for(int i = 0; i < examPapers.Count; i++)
            {
                //删除教师的所有试卷及相关信息
                ExamPaperBLL.DeleteExamPaperById(examPapers[i]);
            }
            if (!TeacherUserBLL.DeleteTeacherById(teacherUser))
            {
                return Content(bsJsonResult.ErrorResult("删除老师失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除老师成功"));
        }
        #endregion

        #region 批量删除老师
        /// <summary>
        /// 批量删除老师
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteTeacher(string[] ids)
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
                    TeacherUser teacherUser = TeacherUserBLL.GetTeacherById(Convert.ToInt32(ids[i]));
                    List<ExamPaper> examPapers = ExamPaperBLL.GetTeacherExamPaper(teacherUser);
                    for (int j = 0; j < examPapers.Count; j++)
                    {
                        //删除教师的所有试卷
                        ExamPaperBLL.DeleteExamPaperById(examPapers[j]);
                    }
                    if (!TeacherUserBLL.MutipleDeleteTeacherById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 返回当前老师的学生集合
        /// <summary>
        /// 返回当前老师的学生集合
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetMyStudentList(TeacherUser teacherUser, int nowPage = 1, int pageSize = 7)
        {
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            List<StudentUser> studentUsers = StudentUserBLL.GetStudentToList(teacherUser);
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

        #region 组卷查看试卷视图
        /// <summary>
        /// 组卷查看试卷视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MakeUpExamPaper()
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            teacherUser = TeacherUserBLL.GetTeacherById(teacherUser.id);
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<ExamPaper> examPapers = ExamPaperBLL.GetExamPapersToMake(teacherUser);
            for (int i = 0; i < examPapers.Count; i++)
            {
                var item = ExamPaperBLL.GetExamDetailList(examPapers[i].id);
                if (item.Count != 0)
                {
                    examPapers[i].isMake = 1;
                }
                else
                {
                    examPapers[i].isMake = 0;
                }
            }
            if (!string.IsNullOrEmpty(sKeys))
                examPapers = examPapers.Where(p => p.examName.Contains(sKeys)).ToList();
            return View(examPapers);
        }
        #endregion

        #region 返回试卷列表视图(老师的试卷)
        /// <summary>
        /// 返回试卷列表视图(老师的试卷)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExamPaperList(int nowPage = 1, int pageSize = 7)
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            teacherUser = TeacherUserBLL.GetTeacherById(teacherUser.id);
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<ExamPaper> examPapers = ExamPaperBLL.GetTeacherExamPaper(teacherUser);
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
    }
}