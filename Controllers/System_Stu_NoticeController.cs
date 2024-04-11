using ExamSystem.Comm.CommHelper;
using ExamSystem.Comm.JsonHelper;
using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    public class System_Stu_NoticeController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion
 
        #region 学生批量已读管理员的通告
        /// <summary>
        /// 学生批量已读管理员的通告
        /// </summary>
        /// <returns></returns>
        public ActionResult MutipleStuReadAdminNotice(string[] ids)
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

        #region 学生已读某通知
        /// <summary>
        /// 学生已读某通知
        /// </summary>
        /// <param name="Notice"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateStudentReadNotice(System_Stu_Notice Notice)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            NoticeBLL.UpdateStudentReadNoticeById(Notice.id, studentUser.id);
            return Content(bsJsonResult.SuccessResult("已读！！！"));
        }
        #endregion

        #region 用户浏览通告（学生浏览）
        /// <summary>
        /// 用户浏览通告（学生浏览）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult StuBrowseSystemNotice(System_Stu_Notice Notice)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            System_Stu_Notice notice = NoticeBLL.GetStudentNoticeToRead(studentUser.id, Notice.id);
            return View(notice);
        }
        #endregion

        #region 学生浏览通知
        /// <summary>
        /// 学生浏览通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BrowseTeacherNotice(System_Stu_Notice Notice)
        {
            StudentUser studentUser = new StudentUser();
            if (!(Session["LoginUser"] is null))
            {
                studentUser = Session["LoginUser"] as StudentUser;
            }
            System_Stu_Notice notice = NoticeBLL.GetStudentNoticeToRead(studentUser.id,Notice.id);
            return View(notice);
        }
        #endregion

        #region 创建发布通告视图（学生）
        /// <summary>
        /// 创建发布通告视图（学生）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PublishToStudents()
        {
            return View();
        }
        #endregion

        #region 创建发布通告（学生）
        /// <summary>
        /// 创建发布通告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PublishToStudents(System_Stu_Notice Notice)
        {
            TeacherUser teacherUser  = default;
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            Notice.teacherId = teacherUser.id;
            string noticeName = Notice.NoticeName;
            string noticeContent = Notice.NoticeContent;
            if (string.IsNullOrEmpty(noticeName) || noticeName.Length == 0)
            {
                return Content(bsJsonResult.ErrorResult("请输入通知通告名称！！！"));
            }
            if (string.IsNullOrEmpty(noticeContent) || noticeContent.Length == 0)
            {
                return Content(bsJsonResult.ErrorResult("请输入通知内容！！！"));
            }
            if (!NoticeBLL.PublishNoticeStu_Add(Notice))
                return Content(bsJsonResult.ErrorResult("发布失败！！！"));
            Notice = NoticeBLL.GetStudentNoticeByIdAndByTime(teacherUser.id);
            //获取当前老师的所有学生
            List<Teacher_Stu> teacher_Stus = NoticeBLL.GetStu_NowTeacherToNotice(teacherUser.id);
            for(int i = 0; i < teacher_Stus.Count; i++)
            {
                NoticeBLL.SaveTeacherPublishToStudentDB(teacherUser.id,Notice.id,teacher_Stus[i].sid);
            }
            return Content(bsJsonResult.SuccessResult("发布成功！！！"));
        }
        #endregion

        #region 创建发布通告视图（学生和教师）
        /// <summary>
        /// 创建发布通告视图（学生和教师）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PublishToUsers()
        {
            return View();
        }
        #endregion

        #region 创建发布通告（学生和教师）
        /// <summary>
        /// 创建发布通告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PublishToUsers(System_Stu_Notice Notice)
        {
            AdminUser adminUser = default;
            if (!(Session["LoginUser"] is null))
            {
                adminUser = Session["LoginUser"] as AdminUser;
            }
            Notice.adminId = adminUser.id;
            string noticeName = Notice.NoticeName;
            string noticeContent = Notice.NoticeContent;
            if(string.IsNullOrEmpty(noticeName) || noticeName.Length == 0)
            {
                return Content(bsJsonResult.ErrorResult("请输入通知通告名称！！！"));
            }
            if(string.IsNullOrEmpty(noticeContent) || noticeContent.Length == 0)
            {
                return Content(bsJsonResult.ErrorResult("请输入通告内容！！！"));
            }
            if (!NoticeBLL.PublishNotice_Add(Notice))
                return Content(bsJsonResult.ErrorResult("发布失败！！！"));
            Notice = NoticeBLL.GetAdminNoticeByIdAndByTime(adminUser.id);
            //获取当前所有状态为1的老师
            List<TeacherUser> teacherUsers = TeacherUserBLL.GetAllTeacherStatusOK();
            //获取当前所有状态为1的学生
            List<StudentUser> studentUsers = StudentUserBLL.GetAllStudentStatusOK();
            for (int i = 0; i < teacherUsers.Count; i++)
            {
                NoticeBLL.SaveAdminPublishToTeacherDB(adminUser.id, Notice.id, teacherUsers[i].id);
            }
            for (int i = 0; i < studentUsers.Count; i++)
            {
                NoticeBLL.SaveAdminPublishToStudentDB(adminUser.id, Notice.id, studentUsers[i].id);
            }
            return Content(bsJsonResult.SuccessResult("发布成功！！！"));
        } 
        #endregion

        #region 编辑通告重新发布视图
        /// <summary>
        /// 编辑通告重新发布视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditSystemNotice(string id)
        {
            //判断值Id是否存在
            System_Stu_Notice Notice = NoticeBLL.GetSystemNoticeById(Convert.ToInt32(id));
            if (Notice is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(Notice);
        }
        #endregion

        # region 编辑通告重新发布业务逻辑
        /// <summary>
        /// 编辑通告重新发布业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditSystemNotice(System_Stu_Notice Notice)
        {
            if (!CommDefine.IsDigital(Notice.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckEditNoticeInfo(Notice));
        }
        #endregion

        #region 验证返回值(编辑通告)
        /// <summary>
        /// 验证返回值(编辑通告)
        /// </summary>
        /// <param name="Notice"></param>
        /// <returns></returns>
        private string CheckEditNoticeInfo(System_Stu_Notice Notice)
        {

            if (!NoticeBLL.EditNoticeInfo(Notice))
            {
                return bsJsonResult.ErrorResult("重新发布失败");
            }
            return bsJsonResult.SuccessResult("重新发布成功");
        }
        #endregion

        #region 删除通告（管理员的）
        /// <summary>
        /// 删除通告（管理员的）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteNotice(System_Stu_Notice Notice)
        {
            if(!CommDefine.IsDigital(Notice.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if(!NoticeBLL.DeleteSystemNoticeById(Notice.id))
            {
                return Content(bsJsonResult.ErrorResult("删除通告失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除通告成功"));
        }
        #endregion

        #region 批量删除通告(管理员)
        /// <summary>
        /// 批量删除通告(管理员)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult MutipleDeleteNotice(string[] ids)
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
                    if (!NoticeBLL.DeleteSystemNoticeById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion

        #region 删除通告（教师的）
        /// <summary>
        /// 删除通告（教师的）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteStuNotice(System_Stu_Notice Notice)
        {
            if (!CommDefine.IsDigital(Notice.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!NoticeBLL.DeleteStudentNoticeById(Notice.id))
            {
                return Content(bsJsonResult.ErrorResult("删除通告失败"));
            }
            return Content(bsJsonResult.SuccessResult("删除通告成功"));
        }
        #endregion

        #region 批量删除通告(教师的)
        /// <summary>
        /// 批量删除通告(教师的)
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult MutipleDeleteStuNotice(string[] ids)
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
                    if (!NoticeBLL.DeleteStudentNoticeById(Convert.ToInt32(ids[i])))
                    {
                        return Content(bsJsonResult.ErrorResult("批量删除失败"));
                    }
                }
            }
            return Content(bsJsonResult.SuccessResult("批量删除成功"));
        }
        #endregion
    }
}