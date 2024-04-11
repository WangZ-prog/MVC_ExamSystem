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
    public class AdminUserController : Controller
    {
        #region 返回json结果
        /// <summary>
        /// 返回json结果
        /// </summary>
        private readonly BsJsonResult bsJsonResult = new BsJsonResult();
        #endregion

        #region 管理员的通告集合视图
        /// <summary>
        /// 管理员的通告集合视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult System_NoticeToList(int nowPage = 1, int pageSize = 7)
        {
            AdminUser adminUser = new AdminUser();
            if (!(Session["LoginUser"] is null))
            {
                adminUser = Session["LoginUser"] as AdminUser;
            }
            adminUser = AdminUserBLL.GetAdminUserById(adminUser.id);
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选

            string sKeys = Request.QueryString["Keys"];
            List<System_Stu_Notice> Notices = NoticeBLL.GetSystemNotice(adminUser.id);
            for (int i = 0; i < Notices.Count; i++)
                Notices[i].Publisher = adminUser.realName;
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

        #region 登录账户信息
        /// <summary>
        /// 登录账户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountInfo()
        {
            AdminUser adminUser = new AdminUser();
            if(!(Session["LoginUser"] is null))
            {
                adminUser = Session["LoginUser"] as AdminUser;
            }
            //从数据库中取出信息
            adminUser = AdminUserBLL.GetAdminUserById(adminUser.id);
            //视图将会拿到数据库相应信息
            return View(adminUser);
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
            AdminUser adminUser = new AdminUser();
            if (!(Session["LoginUser"] is null))
            {
                adminUser = Session["LoginUser"] as AdminUser;
            }
            //从数据库中取出信息
            adminUser = AdminUserBLL.GetAdminUserById(adminUser.id);
            //视图将会拿到数据库相应信息
            return View(adminUser);
        }
        #endregion

        #region 验证返回值(修改用户账号)
        /// <summary>
        /// 验证返回值(修改用户账号)
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserInfo(AdminUser adminUser)
        {
            string Name = adminUser.realName;
            string Telphone = adminUser.telPhone;

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

            if (!AdminUserBLL.UpdateAdminUser(adminUser))
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
        /// <param name="adminUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckModifyUserPass(AdminUser adminUser)
        {
            string sPassword = adminUser.passWord;
            string sRePassword = adminUser.repassWord;

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

            if (!AdminUserBLL.UpdateAdminPass(adminUser))
            {
                return bsJsonResult.ErrorResult("修改密码失败");
            }
            return bsJsonResult.SuccessResult("修改密码成功");
        }
        #endregion

        #region 验证返回值(编辑管理员密码)
        /// <summary>
        /// 验证返回值(编辑管理员密码)
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        private string CheckEditAdminPass(AdminUser adminUser)
        {
            string sPassword = adminUser.passWord;
            string sRePassword = adminUser.repassWord;

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

            if (!AdminUserBLL.UpdateAdminPass(adminUser))
            {
                return bsJsonResult.ErrorResult("编辑密码失败");
            }
            return bsJsonResult.SuccessResult("编辑密码成功");
        }
        #endregion

        #region 编辑管理员密码视图界面
        /// <summary>
        /// 编辑管理员密码视图界面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditAdminPass(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            AdminUser adminUser = AdminUserBLL.GetAdminUserById(Convert.ToInt32(id));
            if (adminUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(adminUser);
        }
        #endregion

        #region 编辑管理员密码业务逻辑
        /// <summary>
        /// 编辑管理员密码业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditAdminPass(AdminUser adminUser)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }

            return Content(CheckEditAdminPass(adminUser));
        }
        #endregion

        #region 验证返回值(编辑管理员账号)
        /// <summary>
        /// 验证返回值(编辑管理员账号)
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        [HttpPost]
        private string CheckAdminInfo(AdminUser adminUser)
        {
            string Name = adminUser.realName;
            string Telphone = adminUser.telPhone;

            //examUser.id = ((ExamUser)(Session["LoginUser"])).id ;
            if (string.IsNullOrEmpty(Name) || Name.Length == 0)
            {
                return bsJsonResult.ErrorResult("管理员姓名必须至少2个字符");
            }
            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("管理员姓名必须为汉字");
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

            if (!AdminUserBLL.EditAdminInfo(adminUser))
            {
                return bsJsonResult.ErrorResult("管理员编辑失败");
            }
            return bsJsonResult.SuccessResult("管理员编辑成功");
        }
        #endregion

        #region 发送数据更新验证
        /// <summary>
        /// 发送数据更新验证
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccount(AdminUser adminUser)
        {
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserInfo(adminUser));
        }
        #endregion

        #region 发送数据更新验证(改密码)
        /// <summary>
        /// 发送数据更新验证(改密码)
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        public ActionResult UpdateAccountPass(AdminUser adminUser)
        {
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckModifyUserPass(adminUser));
        }
        #endregion

        #region 编辑管理员视图业务逻辑
        /// <summary>
        /// 编辑管理员视图业务逻辑
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditAdmin(AdminUser adminUser)
        {
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            return Content(CheckAdminInfo(adminUser));
        }
        #endregion

        #region 编辑管理员视图
        /// <summary>
        /// 编辑管理员视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult EditAdmin(string id)
        {
            //对Id进行验证
            if (!CommDefine.IsDigital(id))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法！！！" });
            }
            //判断值Id是否存在
            AdminUser adminUser = AdminUserBLL.GetAdminUserById(Convert.ToInt32(id));
            if (adminUser is null)
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "查询无相关信息！！！" });
            }
            return View(adminUser);
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

        #region 返回管理员列表视图
        /// <summary>
        /// 返回管理员列表视图
        /// </summary>
        /// <returns></returns>
        public ActionResult AdminList(int nowPage = 1, int pageSize = 7)
        {
            //搜索查询方法使用通过形成的List泛型集合用兰姆达表达式进行模糊删选
            string sKeys = Request.QueryString["Keys"];
            List<AdminUser> adminUsers = AdminUserBLL.GetAdmin();
            if (!string.IsNullOrEmpty(sKeys))
                adminUsers = adminUsers.Where(p => p.userName.Contains(sKeys) || p.realName.Contains(sKeys)).ToList();
            int totalCount = adminUsers.Count;
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
            adminUsers = adminUsers.OrderByDescending(c => c.id).Skip(pageSize * (nowPage - 1)).Take(pageSize).ToList();
            return View(adminUsers);
        }
        #endregion

        #region 添加管理员视图
        /// <summary>
        /// 添加管理员视图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AddAdmin()
        {
            return View();
        }
        #endregion

        #region 添加管理员
        /// <summary>
        /// 添加管理员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddAdmin(AdminUser adminUser)
        {
            return Content(CheckAddAdmin(adminUser));
        }
        #endregion

        #region 验证返回值(添加管理员)
        /// <summary>
        /// 验证返回值(添加管理员)
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        private string CheckAddAdmin(AdminUser adminUser)
        {
            string Username = adminUser.userName;
            string Name = adminUser.realName;
            string Telphone = adminUser.telPhone;
            string sPassword = adminUser.passWord;
            string sRePassword = adminUser.repassWord;

            //验证登录账号
            if (string.IsNullOrEmpty(Username) || Username.Length < 4 || Username.Length > 14)
            {
                return bsJsonResult.ErrorResult("管理员账号必须为4-14个字符");
            }

            if (!CommDefine.IsChineseChar(Name))
            {
                return bsJsonResult.ErrorResult("管理员姓名必须为汉字,且至少2个字符");
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
            if(!(AdminUserDAL.ChackUserName(adminUser) is null))
            {
                return bsJsonResult.ErrorResult("管理员账号已经存在！请重新添加");
            }
            //验证管理员手机号
            if (!(AdminUserDAL.ChackUserTelphone(adminUser) is null))
            {
                return bsJsonResult.ErrorResult("手机号已存在，请重新输入");
            }

            if(!AdminUserBLL.AddAdminUser(adminUser))
            {
                return bsJsonResult.ErrorResult("管理员添加失败");
            }
            return bsJsonResult.SuccessResult("管理员添加成功");
        }
        #endregion

        #region 更新管理员用户状态（问题：如何避免将当前已经登录的用户的状态变为0）
        /// <summary>
        /// 更新管理员用户状态
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateStatus(AdminUser adminUser)
        {
            AdminUser adminUserNow = new AdminUser();
           //对id进行验证
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!(Session["LoginUser"] is null))
            {
                adminUserNow = Session["LoginUser"] as AdminUser;
            }
            if (adminUser.id == adminUserNow.id)
                return Content(bsJsonResult.ErrorResult("当前登录用户状态不可停用"));

            if (!AdminUserBLL.UpdateAdminStatus(adminUser))
            {
                return Content(bsJsonResult.ErrorResult("状态更新失败"));
            }
            
            return Content(bsJsonResult.SuccessResult("状态更新成功"));
        }
        #endregion

        #region 删除管理员（问题：如何避免删除当前已经登录的用户）
        /// <summary>
        /// 删除管理员
        /// </summary>
        /// <param name="adminUser"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult DeleteAdminUser(AdminUser adminUser)
        {
            AdminUser adminUserNow = new AdminUser();
            //对id进行验证
            if (!CommDefine.IsDigital(adminUser.id.ToString()))
            {
                return RedirectToAction("Index", "Error", new { ErrorMessage = "参数传递不合法" });
            }
            if (!(Session["LoginUser"] is null))
            {
                adminUserNow = Session["LoginUser"] as AdminUser;
            }
            if (adminUser.id == adminUserNow.id)
                return Content(bsJsonResult.ErrorResult("当前登录用户不可删除"));

            if (!AdminUserBLL.DeleteAdminById(adminUser))
            {
                return Content(bsJsonResult.ErrorResult("删除管理员失败"));
            }
           
            return Content(bsJsonResult.SuccessResult("删除管理员成功"));
        }
        #endregion

        #region 批量删除管理员
        /// <summary>
        /// 批量删除管理员
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MutipleDeleteAdmin(string[] ids)
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
                    if(!AdminUserBLL.MutipleDeleteAdminById(Convert.ToInt32(ids[i])))
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