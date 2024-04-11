using ExamSystem.LogicBLL.TableBLL;
using ExamSystem.Models.TableModel;
using MVC_ExamSystem.Filter;
using System.Web.Mvc;

namespace MVC_ExamSystem.Controllers
{
    [ExamUserFilter]//在Controller的方法上添加特性标记
    public class HomeController : Controller
    {
        private readonly AdminUserBLL adminUserBLL = new AdminUserBLL();
        private readonly TeacherUserBLL teacherUserBLL = new TeacherUserBLL();
        private readonly StudentUserBLL studentUserBLL  = new StudentUserBLL();
        private readonly ExamPaperBLL examPaperBLL   = new ExamPaperBLL();
        private readonly QuestionBLL examTextQuestBLL    = new QuestionBLL();
        private readonly NoticeBLL noticeBLL  = new NoticeBLL();

        #region index
        /// <summary>
        /// index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        } 
        #endregion

        #region 管理员登录欢迎界面
        /// <summary>
        /// 管理员登录欢迎界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Welcome()
        {
            AdminUser adminUser = new AdminUser();
            if (!(Session["LoginUser"] is null))
            {
                adminUser = Session["LoginUser"] as AdminUser;
            }
            adminUser = AdminUserBLL.GetAdminUserById(adminUser.id);
            ViewBag.AdminUsers = adminUserBLL.GetAdminUsers().Count;
            ViewBag.Teachers = teacherUserBLL.GetTeacherUsers().Count;
            ViewBag.Students = studentUserBLL.GetStudentUsers().Count;
            ViewBag.ExamPapers = examPaperBLL.GetExamPapers().Count;
            ViewBag.ExamTextQuests = examTextQuestBLL.GetExamTextQuests().Count;  
            ViewBag.Notices = noticeBLL.GetSystemNoticesToCount(adminUser).Count;
            
            return View();
        }
        #endregion

        #region 教师欢迎界面
        /// <summary>
        /// 教师欢迎界面
        /// </summary>
        /// <returns></returns>
        public ActionResult WelcomeTeacher()
        {
            TeacherUser teacherUser = new TeacherUser();
            if (!(Session["LoginUser"] is null))
            {
                teacherUser = Session["LoginUser"] as TeacherUser;
            }
            teacherUser = TeacherUserBLL.GetTeacherById(teacherUser.id);
            ViewBag.Students = studentUserBLL.GetTeacher_StuList(teacherUser.id).Count;
            ViewBag.ExamPapers = examPaperBLL.GetTeacherExamPapers(teacherUser.id).Count;
            ViewBag.ExamTextQuests = examTextQuestBLL.GetExamTextQuests().Count;
            ViewBag.Notices = noticeBLL.GetStudentNoticesToCount(teacherUser).Count;
            return View();
        }
        #endregion

        #region 学生欢迎界面
        /// <summary>
        /// 学生欢迎界面
        /// </summary>
        /// <returns></returns>
        public ActionResult WelcomeStudent()
        {
            return View();
        } 
        #endregion
    }
}